using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.IntrestManager.Settings
{
    public class SettingsModel
    {
        [YamlProperty("InterestManager.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("InterestManager.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("InterestManager.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("InterestManager.MyNoSqlWriterUrl")]
        public string MyNoSqlWriterUrl { get; set; }

        [YamlProperty("InterestManager.PostgresConnectionString")]
        public string PostgresConnectionString { get; set; }
        
        [YamlProperty("InterestManager.InterestManagerTimerInSeconds")]
        public int InterestManagerTimerInSeconds { get; set; }

        [YamlProperty("InterestManager.ChangeBalanceGatewayGrpcServiceUrl")]
        public string ChangeBalanceGatewayGrpcServiceUrl { get; set; }

        [YamlProperty("InterestManager.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }

        [YamlProperty("InterestManager.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }
    }
}
