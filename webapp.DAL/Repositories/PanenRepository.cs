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
            var dbquery = _container.GetItemQueryIterator<Panen>(
                new QueryDefinition($"SELECT * FROM c WHERE c.status LIKE '%{query}%' OR c.namaLokasi LIKE '%{query}%' OR c.jenisMadu LIKE '%{query}%' OR c.catatanWarehouse LIKE '%{query}%'"
                + $" OR c.namaPetugasPanen LIKE '%{query}%' OR c.namaPICPanen LIKE '%{query}%' OR c.namaPetugasWarehouse LIKE '%{query}%' OR c.namaAdmin LIKE '%{query}%' OR c.id LIKE '%{query}%'"
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
                panen.gambarPanenUrl = panenData.gambarPanenUrl;
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
                panen.gambarWarehouseUrl = gambarWarehouseUrl;
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
                await _container.UpsertItemAsync(panen);
            } else
            {
                throw new InvalidOperationException("Admin hanya bisa konfirmasi panen yang sudah dilaporkan di warehouse");
            }
            return panen;
        }
    }
}
