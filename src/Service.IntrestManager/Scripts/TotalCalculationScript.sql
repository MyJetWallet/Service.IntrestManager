DO
$$
    BEGIN
        insert into interest_manager.interestratestate ("WalletId", "AssetId", "CurrentEarnAmount", "TotalEarnAmount")
        select "WalletId", "Symbol", 0, SUM("Amount")
        from interest_manager.interestratepaid
        where "State" = 2
        group by "WalletId", "Symbol"
        ON CONFLICT ("WalletId", "AssetId") DO UPDATE SET "TotalEarnAmount" = excluded."TotalEarnAmount";
    END
$$;
