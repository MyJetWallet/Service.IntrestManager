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
   Date DATE
) ON COMMIT DROP;

insert into temp_new_balances
select "WalletId", "Symbol", "NewBalance" from
    (
        select *, ROW_NUMBER() OVER (PARTITION BY "WalletId", "Symbol" ORDER BY "Timestamp" DESC) "rank"
        from balancehistory.balance_history
        where "Timestamp" < date '2021-10-01'
        order by "Timestamp" desc
    ) t where t.rank = 1;

-- stage 1
insert into temp_calculation
select hd.*, s."Apy", (hd."newbalance" * s."Apy")/365 "Amount", date '2021-10-01'
from temp_new_balances hd
join interest_manager.interestratesettings s
    on hd."walletid" = s."WalletId"
        and hd."symbol" = s."Asset"
        and (hd."newbalance" >= s."RangeFrom"
            and hd."newbalance" < s."RangeTo");

-- stage 2
insert into temp_calculation
select hd.*, s."Apy", (hd."newbalance" * s."Apy")/365 "Amount", date '2021-10-01'
from temp_new_balances hd
join interest_manager.interestratesettings s
    on hd."walletid" = s."WalletId"
        and s."Asset" = ''
        and s."RangeFrom" = 0
        and s."RangeTo" = 0
left join temp_calculation tc
    on hd."symbol" = tc.Symbol
        and hd."walletid" = tc.WalletId
where tc.WalletId IS NULL;

-- stage 3
insert into temp_calculation
select hd.*, s."Apy", (hd."newbalance" * s."Apy")/365 "Amount", date '2021-10-01'
from temp_new_balances hd
join interest_manager.interestratesettings s
    on s."WalletId" = ''
        and hd."symbol" = s."Asset"
        and (hd."newbalance" >= s."RangeFrom"
            and hd."newbalance" < s."RangeTo")
left join temp_calculation tc
    on hd."symbol" = tc.Symbol
        and hd."walletid" = tc.WalletId
where tc.WalletId IS NULL;

insert into interest_manager.interestratecalculation ("WalletId", "Symbol", "NewBalance", "Apy", "Amount", "Date")
select walletid, symbol, newbalance, apy, amount, date from temp_calculation
on conflict ("WalletId", "Symbol", "Date")
DO UPDATE
SET "NewBalance" = excluded."NewBalance",
    "Apy" = excluded."Apy",
    "Amount" = excluded."Amount";

COMMIT;