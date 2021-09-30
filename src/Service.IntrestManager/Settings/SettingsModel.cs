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
    }
}
