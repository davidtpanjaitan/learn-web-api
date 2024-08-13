using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using webapp.DAL.DTO;
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

        public override async Task<PagedResult<Produk>> GetAsyncPaged(int pageSize, int pageNumber, string query = "")
        {
            query = query.ToLower();
            var dbquery = _container.GetItemQueryIterator<Produk>(
                new QueryDefinition($"SELECT * FROM c WHERE LOWER(c.nama) LIKE '%{query}%' OR LOWER(c.status) LIKE '%{query}%' OR LOWER(c.namaPetugasMixing) LIKE '%{query}%' OR LOWER(c.namaAdmin) LIKE '%{query}%' OR LOWER(c.id) LIKE '%{query}%'"
                + $" ORDER BY c.createdDate DESC OFFSET {pageNumber * pageSize} LIMIT {pageSize}"),
                requestOptions: new QueryRequestOptions { MaxItemCount = pageSize, PartitionKey = new PartitionKey(partitionKey) }
            );

            List<Produk> results = new List<Produk>();

            FeedResponse<Produk> response = await dbquery.ReadNextAsync();
            results.AddRange(response);

            return new PagedResult<Produk>
            {
                Items = results,
                TotalCount = await base.GetTotalCountAsync()
            };
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
            if (dbItem.status == Constants.ProdukStatus.GENERATED.ToString() || dbItem.status == Constants.ProdukStatus.SUBMITTED.ToString())
            {
                dbItem.status = Constants.ProdukStatus.SUBMITTED.ToString();
                dbItem.nama = item.nama;
                dbItem.tanggal = item.tanggal;
                dbItem.idPetugasMixing = item.idPetugasMixing;
                dbItem.namaPetugasMixing = item.namaPetugasMixing;
                dbItem.listPanen = item.listPanen;
            }

            return await base.UpdateAsync(dbItem);
        }

        public async Task<Produk> ApproveProdukByAdmin(string idProduk, bool approve, string idApprover, string namaApprover)
        {
            var produk = await GetByIdAsync(idProduk);
            
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

        protected override async Task<int> GetTotalCountAsync()
        {
            var countQuery = _container.GetItemQueryIterator<int>(
                new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE c.status <> 'GENERATED'")
            );

            int totalCount = 0;
            while (countQuery.HasMoreResults)
            {
                FeedResponse<int> response = await countQuery.ReadNextAsync();
                totalCount += response.FirstOrDefault();
            }

            return totalCount;
        }

        public async Task<StatistikResult> GetStatistik()
        {
            var result = new StatistikResult();
            result.LifetimeItemCount = await GetTotalCountAsync();
            using (var monthCountQueryIterator = _container.GetItemQueryIterator<Stats>(
                new QueryDefinition("SELECT COUNT(1) AS ItemCount, c.createdDateYearMonth AS Month FROM (SELECT c.id, SUBSTRING(c.createdDate, 0, 7) AS createdDateYearMonth FROM c WHERE c.status <> 'GENERATED') c GROUP BY c.createdDateYearMonth")
                ))
            {
                while (monthCountQueryIterator.HasMoreResults)
                {
                    var response = await monthCountQueryIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        result.MonthlyStats.Add(item);
                    }
                }
            }

            return result;
        }
    }
}
