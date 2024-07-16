using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public async Task<List<Panen>> GeneratePanenForLokasi(string idLokasi, string namaLokasi, int number)
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
                };
                result.Add(panen);
                batch.CreateItem(panen);
            }
            await batch.ExecuteAsync();
            return result;
        }

        public async Task<Panen> ApprovePanenOnLokasi(string idPanen, bool approve, string idApprover, string namaApprover)
        {
            var panen = _container.GetItemLinqQueryable<Panen>().First(p => p.id == idPanen);
            panen.idPICPanen = idApprover;
            panen.namaPICPanen = namaApprover;
            if (panen.status == Constants.PanenStatus.SUBMITTED.ToString() && approve)
            {
                panen.status = Constants.PanenStatus.PIC_APPROVED.ToString();
            }
            await _container.UpsertItemAsync(panen);
            return panen;
        }

        public async Task<Panen> ApprovePanenOnWarehouse(string idPanen, bool approve, string idApprover, string namaApprover, double beratBaru, string catatan)
        {
            var panen = _container.GetItemLinqQueryable<Panen>().First(p => p.id == idPanen);
            panen.idPetugasWarehouse = idApprover;
            panen.namaPetugasWarehouse = namaApprover;
            panen.beratWarehouse = beratBaru;
            panen.catatanWarehouse = catatan;
            if (panen.status == Constants.PanenStatus.PIC_APPROVED.ToString() && approve)
            {
                panen.status = Constants.PanenStatus.ARRIVED_WAREHOUSE.ToString();
            }
            await _container.UpsertItemAsync(panen);
            return panen;
        }

        public async Task<Panen> ApprovePanenByAdmin(string idPanen, bool approve, string idApprover, string namaApprover)
        {
            var panen = _container.GetItemLinqQueryable<Panen>().First(p => p.id == idPanen);
            panen.idAdmin = idApprover;
            panen.namaAdmin = namaApprover;
            if (panen.status == Constants.PanenStatus.ARRIVED_WAREHOUSE.ToString() && approve)
            {
                panen.status = Constants.PanenStatus.ADMIN_CONFIRMED.ToString();
            }
            await _container.UpsertItemAsync(panen);
            return panen;
        }
    }
}
