BEGIN TRANSACTION;

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TEMPORARY TABLE temp_paid
(
   WalletId VARCHAR(80),
   Symbol VARCHAR(80),
   Date TIMESTAMP,
   Amount DECIMAl,
   State INT,
   TransactionId VARCHAR(80)
) ON COMMIT DROP;

CREATE TEMPORARY TABLE temp_paid_report
(
   amountInUsd DECIMAL
) ON COMMIT DROP;

-- temp paid
insert into temp_paid (WalletId, Symbol, Date, Amount, State, TransactionId)
select "WalletId", "Symbol", (select current_timestamp at time zone 'utc'), sum("Amount"), 1, uuid_generate_v4()
from interest_manager.interestratecalculation
where "Date" > timestamp '${dateFrom}'
  and "Date" <= timestamp '${dateTo}'
group by "WalletId", "Symbol";

delete from temp_paid
where Amount = 0
   or Amount < 0;

-- temp paid history
insert into temp_paid_report
select sum(sbs.amountInUsd)
from (select sum(Amount) * ip."PriceInUsd" amountInUsd
      from temp_paid as tp
               inner join interest_manager.indexprice as ip
                          on tp.Symbol = ip."Asset"
      group by ip."PriceInUsd") as sbs;

-- paid history
insert into interest_manager.paidhistory ("CreatedDate", "RangeFrom", "RangeTo", "WalletCount", "TotalPaidInUsd")
select
    (select current_timestamp at time zone 'utc'),
    (timestamp '${dateFrom}'),
    (timestamp '${dateTo}'),
    (select count(distinct WalletId) from temp_paid),
    (COALESCE((select amountInUsd from temp_paid_report), 0));

-- paid
insert into interest_manager.interestratepaid ("WalletId", "Symbol", "Date", "Amount", "State", "TransactionId", "HistoryId")
select
    WalletId,
    Symbol,
    Date,
    Amount,
    State,
    TransactionId,
    (select "Id" from interest_manager.paidhistory order by "Id" desc limit 1)
from temp_paid;

COMMIT;