using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.IntrestManager.Settings
{
    public class SettingsModel
    {
        [YamlProperty("IntrestManager.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("IntrestManager.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("IntrestManager.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }
    }
}
