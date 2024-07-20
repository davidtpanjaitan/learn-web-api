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
        public async Task<List<Produk>> GenerateProduk(int number, string creator)
        {
            var batch = _container.CreateTransactionalBatch(new PartitionKey(""));
            var result = new List<Produk>();
            for (int i = 0; i < number; i++)
            {
                var produk = new Produk
                {
                    id = GenerateProdukId(i),
                    status = Constants.ProdukStatus.GENERATED.ToString(),
                    createdBy = creator
                };
                result.Add(produk);
                batch.CreateItem(produk);
            }
            await batch.ExecuteAsync();
            return result;
        }

        public override async Task<Produk> UpdateAsync(Produk item)
        {
            var dbItem = await base.GetByIdAsync(item.id);
            if (dbItem.status == Constants.ProdukStatus.GENERATED.ToString())
            {
                dbItem.status = Constants.ProdukStatus.SUBMITTED.ToString();
            }
            return await base.UpdateAsync(dbItem);
        }

        public async Task<Produk> ApproveProdukByAdmin(string idProduk, bool approve, string idApprover, string namaApprover)
        {
            var produk = _container.GetItemLinqQueryable<Produk>().First(p => p.id == idProduk);
            
            if (produk.status == Constants.ProdukStatus.SUBMITTED.ToString() && approve)
            {
                produk.status = Constants.ProdukStatus.ADMIN_APPROVED.ToString();
                produk.idAdmin = idApprover;
                produk.namaAdmin = namaApprover;
                await _container.UpsertItemAsync(produk);
            }
            else
            {
                throw new InvalidOperationException("Admin hanya bisa approve setelah data diisi");
            }
            return produk;
        }
    }
}
