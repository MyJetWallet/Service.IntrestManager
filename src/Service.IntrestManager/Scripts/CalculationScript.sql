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
   Apy DECIMAL,
   Amount DECIMAL,
   Date TIMESTAMP 
) ON COMMIT DROP;

CREATE TEMPORARY TABLE temp_calculation_report
(
   balanceInUsd DECIMAL,
   amountInUsd DECIMAL
) ON COMMIT DROP;

insert into temp_new_balances
select "WalletId", "Symbol", "NewBalance" from
    (
        select *, ROW_NUMBER() OVER (PARTITION BY "WalletId", "Symbol" ORDER BY "Timestamp" DESC) "rank"
        from balancehistory.balance_history
        where "Timestamp" <= timestamp '${dateArg}'
        order by "Timestamp" desc
    ) t 
where t.rank = 1 
    and t."NewBalance" > 0
    and "WalletId" != 'SP-BrokerInterest';

-- stage 1
insert into temp_calculation
select hd.*, s."Apy", (hd."newbalance" * s."Apy" / 100)/365 "Amount", timestamp '${dateArg}'
from temp_new_balances hd
join interest_manager.interestratesettings s
    on hd."walletid" = s."WalletId"
        and hd."symbol" = s."Asset"
        and (hd."newbalance" >= s."RangeFrom"
            and hd."newbalance" < s."RangeTo");

-- stage 2
insert into temp_calculation
select hd.*, s."Apy", (hd."newbalance" * s."Apy" / 100)/365 "Amount", timestamp '${dateArg}'
from temp_new_balances hd
join interest_manager.interestratesettings s
    on hd."walletid" = s."WalletId"
        and (s."Asset" = '' OR s."Asset" IS NULL)
        and s."RangeFrom" = 0
        and s."RangeTo" = 0
left join temp_calculation tc
    on hd."symbol" = tc.Symbol
        and hd."walletid" = tc.WalletId
where tc.WalletId IS NULL;

-- stage 3
insert into temp_calculation
select hd.*, s."Apy", (hd."newbalance" * s."Apy" / 100)/365 "Amount", timestamp '${dateArg}'
from temp_new_balances hd
join interest_manager.interestratesettings s
    on (s."WalletId" = '' OR s."WalletId" IS NULL)
        and hd."symbol" = s."Asset"
        and (hd."newbalance" >= s."RangeFrom"
            and hd."newbalance" < s."RangeTo")
left join temp_calculation tc
    on hd."symbol" = tc.Symbol
        and hd."walletid" = tc.WalletId
where tc.WalletId IS NULL;

delete from interest_manager.interestratecalculation
where "Date" = timestamp '${dateArg}';

-- calculation
insert into interest_manager.interestratecalculation ("WalletId", "Symbol", "NewBalance", "Apy", "Amount", "Date")
select walletid, symbol, newbalance, apy, amount, date from temp_calculation;


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
        (select balanceInUsd from temp_calculation_report),
        (select amountInUsd from temp_calculation_report),
        (select json_agg(interestratesettings) from interest_manager.interestratesettings));

COMMIT;