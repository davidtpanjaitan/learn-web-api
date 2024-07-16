using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using webapp.DAL.Models;

namespace webapp.DAL.Repositories
{
    public class ProdukRepository : BaseRepository<Produk>
    {
        public ProdukRepository(CosmosClient client, string databaseName) : base(client, databaseName)
        {
        }
        private string GenerateProdukId(int index)
        {
            return "PR" + DateTime.Now.ToString("yyyyMMddHHmmss") + index.ToString("D3");
        }
        public async Task<List<Produk>> GenerateProduk(int number)
        {
            var batch = _container.CreateTransactionalBatch(new PartitionKey(""));
            var result = new List<Produk>();
            for (int i = 0; i < number; i++)
            {
                var produk = new Produk
                {
                    id = GenerateProdukId(i),
                    status = Constants.ProdukStatus.GENERATED.ToString(),
                };
                result.Add(produk);
                batch.CreateItem(produk);
            }
            await batch.ExecuteAsync();
            return result;
        }

        public async Task<Produk> ApproveProdukByAdmin(string idProduk, bool approve, string idApprover, string namaApprover)
        {
            var produk = _container.GetItemLinqQueryable<Produk>().First(p => p.id == idProduk);
            produk.idAdmin = idApprover;
            produk.namaAdmin = namaApprover;
            if (produk.status == Constants.ProdukStatus.SUBMITTED.ToString() && approve)
            {
                produk.status = Constants.ProdukStatus.ADMIN_APPROVED.ToString();
            }
            await _container.UpsertItemAsync(produk);
            return produk;
        }
    }
}
