using MyJetWallet.Sdk.Postgres;

namespace Service.InterestManager.Postrges.DesignTime
{
    public class ContextFactory : MyDesignTimeContextFactory<DatabaseContext>
    {
        public ContextFactory() : base(options => new DatabaseContext(options))
        {

        }
    }
}