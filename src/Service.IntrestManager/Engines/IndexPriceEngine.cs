using System.Linq;
using System.Threading.Tasks;
using Service.IndexPrices.Client;
using Service.InterestManager.Postrges;
using Service.IntrestManager.Domain.Models;

namespace Service.IntrestManager.Engines
{
    public class IndexPriceEngine
    {
        private readonly IIndexPricesClient _indexPricesClient;

        public IndexPriceEngine(IIndexPricesClient indexPricesClient)
        {
            _indexPricesClient = indexPricesClient;
        }

        public async Task UpdateIndexPrices(DatabaseContext databaseContext)
        {
            var indexPrices = _indexPricesClient.GetIndexPricesAsync();
            if (!indexPrices.Any())
            {
                await Task.Delay(5000);
                indexPrices = _indexPricesClient.GetIndexPricesAsync();
            }
            if (!indexPrices.Any())
            {
                await Task.Delay(5000);
                indexPrices = _indexPricesClient.GetIndexPricesAsync();
            }
            if (indexPrices.Any())
            {
                var localIndexPrices = indexPrices.Select(e => new IndexPriceEntity()
                {
                    Asset = e.Asset,
                    PriceInUsd = e.UsdPrice
                });
                await databaseContext.UpdateIndexPrice(localIndexPrices);
            }
        }
    }
}