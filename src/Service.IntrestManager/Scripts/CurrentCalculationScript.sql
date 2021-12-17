DO
$$
    BEGIN
        UPDATE interest_manager.interestratestate
        SET "CurrentEarnAmount"='0'
        where "CurrentEarnAmount" != 0;
        insert into interest_manager.interestratestate ("WalletId", "AssetId", "CurrentEarnAmount", "TotalEarnAmount")
        select "WalletId", "Symbol", SUM("Amount"), 0
        from interest_manager.interestratecalculation
        where interest_manager.interestratecalculation."Date" > timestamp '${dateArg}'
        group by "WalletId", "Symbol"
        ON CONFLICT ("WalletId", "AssetId") DO UPDATE SET "CurrentEarnAmount" = excluded."CurrentEarnAmount";
    END
$$;
