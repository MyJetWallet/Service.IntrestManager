BEGIN TRANSACTION;

CREATE TEMPORARY TABLE temp_new_balances
(
   WalletId VARCHAR(80),
   Symbol VARCHAR(80),
   NewBalance DECIMAL
) ON COMMIT DROP;

CREATE TEMPORARY TABLE temp_calculation
(
   WalletId VARCHAR(80),
   Symbol VARCHAR(80),
   NewBalance DECIMAL,
   Apr DECIMAL,
   Amount DECIMAL,
   Date TIMESTAMP
) ON COMMIT DROP;

CREATE TEMPORARY TABLE temp_calculation_report
(
   balanceInUsd DECIMAL,
   amountInUsd DECIMAL
) ON COMMIT DROP;

insert into temp_new_balances
select t."WalletId", t."Symbol", t."NewBalance" from
    (
        select *, ROW_NUMBER() OVER (PARTITION BY bh."WalletId", "Symbol" ORDER BY "Timestamp" DESC) "rank"
        from balancehistory.balance_history bh
        where bh."Timestamp" <= timestamp '${dateArg}'
        order by bh."Timestamp" desc
    ) t
        join clientwallets.wallets cw
            on t."WalletId" = cw."WalletId"
where cw."EnableEarnProgram" = true
  and t.rank = 1
  and t."NewBalance" > 0;

-- stage 1
insert into temp_calculation
select hd.*, s."Apr", case when s."DailyLimitInUsd" > 0 then least((hd."newbalance" * s."Apr" / 100)/365, s."DailyLimitInUsd" / p."PriceInUsd") else (hd."newbalance" * s."Apr" / 100)/365 end "Amount", timestamp '${dateArg}'
from temp_new_balances hd
         join interest_manager.interestratesettings s
              on hd."walletid" = s."WalletId"
                  and hd."symbol" = s."Asset"
                  and (hd."newbalance" >= s."RangeFrom"
                      and hd."newbalance" < s."RangeTo")
         join interest_manager.indexprice as p
              on hd."symbol" = p."Asset" 
                     and p."PriceInUsd" > 0;

-- stage 2
insert into temp_calculation
select hd.*, s."Apr", case when s."DailyLimitInUsd" > 0 then least((hd."newbalance" * s."Apr" / 100)/365, s."DailyLimitInUsd" / p."PriceInUsd") else (hd."newbalance" * s."Apr" / 100)/365 end "Amount", timestamp '${dateArg}'
from temp_new_balances hd
         join interest_manager.interestratesettings s
              on hd."walletid" = s."WalletId"
                  and (s."Asset" = '' OR s."Asset" IS NULL)
                  and s."RangeFrom" = 0
                  and s."RangeTo" = 0
         left join temp_calculation tc
                   on hd."symbol" = tc.Symbol
                       and hd."walletid" = tc.WalletId
         join interest_manager.indexprice as p
              on hd."symbol" = p."Asset"
                  and p."PriceInUsd" > 0
where tc.WalletId IS NULL;

-- stage 3
insert into temp_calculation
select hd.*, s."Apr", case when s."DailyLimitInUsd" > 0 then least((hd."newbalance" * s."Apr" / 100)/365, s."DailyLimitInUsd" / p."PriceInUsd") else (hd."newbalance" * s."Apr" / 100)/365 end "DailyLimit", timestamp '${dateArg}'
from temp_new_balances hd
         join interest_manager.interestratesettings s
              on (s."WalletId" = '' OR s."WalletId" IS NULL)
                  and hd."symbol" = s."Asset"
                  and (hd."newbalance" >= s."RangeFrom"
                      and hd."newbalance" < s."RangeTo")
         left join temp_calculation tc
                   on hd."symbol" = tc.Symbol
                       and hd."walletid" = tc.WalletId
         join interest_manager.indexprice as p
              on hd."symbol" = p."Asset"
                  and p."PriceInUsd" > 0
where tc.WalletId IS NULL;

delete from interest_manager.interestratecalculation
where "Date" = timestamp '${dateArg}';

-- calculation history
insert into temp_calculation_report
select sum(sbs.balanceInUsd), sum(sbs.amountInUsd)
from (select sum(NewBalance) * p."PriceInUsd" balanceInUsd, sum(Amount) * p."PriceInUsd" amountInUsd
      from temp_calculation as c
               inner join interest_manager.indexprice as p
                          on c.Symbol = p."Asset"
      group by p."PriceInUsd") as sbs;

insert into interest_manager.calculationhistory ("CalculationDate", "CompletedDate", "WalletCount", "AmountInWalletsInUsd", "CalculatedAmountInUsd", "SettingsJson")
values ((timestamp '${dateArg}'),
        (select current_timestamp at time zone 'utc'),
        (select count(distinct WalletId) from temp_calculation),
        (COALESCE((select balanceInUsd from temp_calculation_report), 0)),
        (COALESCE((select amountInUsd from temp_calculation_report), 0)),
        (select json_agg(interestratesettings) from interest_manager.interestratesettings));

-- calculation
insert into interest_manager.interestratecalculation ("WalletId", "Symbol", "NewBalance", "Apr", "Amount", "Date", "HistoryId")
select walletid,
       symbol,
       newbalance,
       apr,
       amount,
       date,
       (select "Id" from interest_manager.calculationhistory order by "Id" desc limit 1)
from temp_calculation;

COMMIT;
