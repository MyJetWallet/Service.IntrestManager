using System.Runtime.Serialization;

namespace Service.IntrestManager.Grpc
{
    [DataContract]
    public class SyncDbAndNoSqlResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
    }
}