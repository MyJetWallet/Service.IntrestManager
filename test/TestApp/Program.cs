using System;
using System.Threading;
using System.Threading.Tasks;
using MyNoSqlServer.DataReader;
using ProtoBuf.Grpc.Client;
using Service.IntrestManager.Client;
using Service.IntrestManager.Domain.Models.NoSql;
using Service.IntrestManager.Grpc;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;

            var myNoSqlClient = new MyNoSqlTcpClient(() => "192.168.70.80:5125", "IntrestManagerTestApp");
            //var factory = new IntrestManagerClientFactory("http://intrestmanager-api.spot-services.svc.cluster.local:80");
            var factory = new IntrestManagerClientFactory("http://localhost:80");

            var reader =  new MyNoSqlReadRepository<InterestRateByWalletNoSql>(myNoSqlClient, InterestRateByWalletNoSql.TableName);
            var client = new InterestRateClientWithCache(reader, factory.GetInterestRateClientService());
            var service = factory.GetInterestRateClientService();
            myNoSqlClient.Start();
            while (reader.Count() == 0)
            {
                Thread.Sleep(1000);
            }
            var clientId = "df8d41ab600e416ca77218adad1cb1e8";

            var response = await client.GetInterestRatesByWalletAsync("SP-" + clientId);
            var response2 = await service.GetInterestRatesByWalletAsync(new GetInterestRatesByWalletRequest()
            {
                WalletId = $"SP-{clientId}"
            });
            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}
