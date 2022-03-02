using System;
using System.Diagnostics;
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
            var factory = new IntrestManagerClientFactory("http://intrestmanager-api.spot-services.svc.cluster.local:80");
            //var factory = new IntrestManagerClientFactory("http://localhost:80");

            var reader =  new MyNoSqlReadRepository<InterestRateByWalletNoSql>(myNoSqlClient, InterestRateByWalletNoSql.TableName);
            var client = new InterestRateClientWithCache(reader, factory.GetInterestRateClientService());
            var service = factory.GetInterestRateClientService();
            myNoSqlClient.Start();
            while (reader.Count() == 0)
            {
                Thread.Sleep(1000);
            }
            var clientId = "6b2cf78bd97942cd8a602ef921807450";

            var sw = new Stopwatch();
            sw.Start();
            Console.WriteLine("Start");
            var response = await client.GetInterestRatesByWalletAsync("SP-" + clientId);
            /*
            var response2 = await service.GetInterestRatesByWalletAsync(new GetInterestRatesByWalletRequest()
             
            {
                WalletId = $"SP-{clientId}"
            });
            */
            sw.Stop();
            Console.WriteLine($"End. {sw.Elapsed}");
            Console.ReadLine();
        }
    }
}
