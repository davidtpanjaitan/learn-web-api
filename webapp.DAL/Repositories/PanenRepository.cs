using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using webapp.DAL.DTO;
using webapp.DAL.Models;
using static webapp.DAL.Models.Produk;

namespace webapp.DAL.Repositories
{
    public class PanenRepository : BaseRepository<Panen>
    {
        public PanenRepository(CosmosClient client, string databaseName) : base(client, databaseName)
        {
        }

        private string GeneratePanenId(int index)
        {
            return "PN"+DateTime.Now.ToString("yyyyMMddHHmmss") + index.ToString("D3");
        }

        public override async Task<PagedResult<Panen>> GetAsyncPaged(int pageSize, int pageNumber, string query = "")
        {

            query = query.ToLower();
            var dbquery = _container.GetItemQueryIterator<Panen>(
                new QueryDefinition($"SELECT * FROM c WHERE LOWER(c.status) LIKE '%{query}%' OR LOWER(c.namaLokasi) LIKE '%{query}%' OR LOWER(c.jenisMadu) LIKE '%{query}%' OR LOWER(c.catatanWarehouse) LIKE '%{query}%'"
                + $" OR LOWER(c.namaPetugasPanen) LIKE '%{query}%' OR LOWER(c.namaPICPanen) LIKE '%{query}%' OR LOWER(c.namaPetugasWarehouse) LIKE '%{query}%' OR LOWER(c.namaAdmin) LIKE '%{query}%' OR LOWER(c.id) LIKE '%{query}%'"
                + $" ORDER BY c.createdDate DESC OFFSET {pageNumber * pageSize} LIMIT {pageSize}"),
                requestOptions: new QueryRequestOptions { MaxItemCount = pageSize, PartitionKey = new PartitionKey(partitionKey) }
            );

            List<Panen> results = new List<Panen>();

            FeedResponse<Panen> response = await dbquery.ReadNextAsync();
            results.AddRange(response);

            return new PagedResult<Panen>
            {
                Items = results,
                TotalCount = await base.GetTotalCountAsync()
            };
        }

        public async Task<List<Panen>> GeneratePanenForLokasi(string idLokasi, string namaLokasi, int number, string creator ="")
        {
            var batch = _container.CreateTransactionalBatch(new PartitionKey(""));
            var result = new List<Panen>();
            for (int i = 0; i < number; i++)
            {
                var panen = new Panen
                {
                    id = GeneratePanenId(i),
                    idLokasi = idLokasi,
                    namaLokasi = namaLokasi,
                    status = Constants.PanenStatus.GENERATED.ToString(),
                    createdBy = creator
                };
                result.Add(panen);
                batch.CreateItem(panen);
            }
            await batch.ExecuteAsync();
            return result;
        }

        public async Task<Panen> SubmitPanenData(Panen panenData)
        {
            var panen = await base.GetByIdAsync(panenData.id);
            if (panen.status == Constants.PanenStatus.GENERATED.ToString() || panen.status == Constants.PanenStatus.SUBMITTED.ToString())
            {
                panen.status = Constants.PanenStatus.SUBMITTED.ToString();
                panen.beratPanen = panenData.beratPanen;
                panen.jenisMadu = panenData.jenisMadu;
                panen.tanggalPanen = panenData.tanggalPanen;
                panen.namaPetugasPanen = panenData.namaPetugasPanen;
                panen.idPetugasPanen = panenData.idPetugasPanen;
                if (panenData.gambarPanenUrl != null && panenData.gambarPanenUrl.Length > 0)
                {
                    panen.gambarPanenUrl = panenData.gambarPanenUrl;
                }
                panen.jumlahDirigen = panenData.jumlahDirigen;
                panen.jumlahDrum = panenData.jumlahDrum;
            }
            else
            {
                throw new InvalidOperationException("Data hanya bisa diubah sebelum diapprove");
            }

            return await base.UpdateAsync(panen);
        }

        public async Task<Panen> ApprovePanenOnLokasi(string idPanen, bool approve, string idApprover, string namaApprover)
        {
            var panen = await GetByIdAsync(idPanen);
            if (panen.status == Constants.PanenStatus.SUBMITTED.ToString() && approve)
            {
                panen.status = Constants.PanenStatus.PIC_APPROVED.ToString();
                panen.idPICPanen = idApprover;
                panen.namaPICPanen = namaApprover;
                await _container.UpsertItemAsync(panen);
            }
            else
            {
                throw new InvalidOperationException("PIC hanya bisa approve setelah data diisi");
            }
            return panen;
        }

        public async Task<Panen> ApprovePanenOnWarehouse(string idPanen, bool approve, string idApprover, string namaApprover, double beratBaru, string catatan, string gambarWarehouseUrl)
        {
            var panen = await GetByIdAsync(idPanen);
            if (panen.status == Constants.PanenStatus.PIC_APPROVED.ToString() && approve)
            {
                panen.status = Constants.PanenStatus.ARRIVED_WAREHOUSE.ToString();
                panen.idPetugasWarehouse = idApprover;
                panen.namaPetugasWarehouse = namaApprover;
                panen.beratWarehouse = beratBaru;
                panen.catatanWarehouse = catatan;
                panen.tanggalWarehouse = DateTime.Now;
                if (gambarWarehouseUrl != null && gambarWarehouseUrl.Length > 0) 
                { 
                    panen.gambarWarehouseUrl = gambarWarehouseUrl;
                }
                await _container.UpsertItemAsync(panen);
            } 
            else
            {
                throw new InvalidOperationException("Warehouse hanya bisa menerima panen yang diapprove pic");
            }
            return panen;
        }

        public async Task<Panen> ApprovePanenByAdmin(string idPanen, bool approve, string idApprover, string namaApprover)
        {
            var panen = await GetByIdAsync(idPanen);
            if (panen.status == Constants.PanenStatus.ARRIVED_WAREHOUSE.ToString() && approve)
            {
                panen.status = Constants.PanenStatus.ADMIN_CONFIRMED.ToString();
                panen.idAdmin = idApprover;
                panen.namaAdmin = namaApprover;
                panen.beratSisa = panen.beratWarehouse;
                await _container.UpsertItemAsync(panen);
            } else
            {
                throw new InvalidOperationException("Admin hanya bisa konfirmasi panen yang sudah dilaporkan di warehouse");
            }
            return panen;
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

        public async Task UpdateListPanen(List<PanenItem> panenList)
        {
            var batch = _container.CreateTransactionalBatch(new PartitionKey(""));
            foreach (var  panen in panenList)
            {
                await UsePanenForMixing(panen.id, panen.berat, batch);
            }
            await batch.ExecuteAsync();
        }

        public async Task UsePanenForMixing(string panenId, double useAmount, TransactionalBatch? batch = null)
        {
            var panen = await GetByIdAsync(panenId);
            panen.beratSisa -= useAmount;
            if (panen.beratSisa < 0)
                throw new InvalidOperationException("Berat dipakai lebih dari berat sisa");
            if (batch != null)
                batch.UpsertItem(panen);
            else
                await UpdateAsync(panen);
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
