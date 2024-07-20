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

        public async Task<Panen> SubmitPanenData(Panen panenData)
        {
            var panen = await base.GetByIdAsync(panenData.id);
            if (panen.status == Constants.PanenStatus.GENERATED.ToString() || panen.status == Constants.PanenStatus.SUBMITTED.ToString())
            {
                panen.status = Constants.PanenStatus.SUBMITTED.ToString();
                panen.beratPanen = panenData.beratPanen;
                panen.jenisMadu = panenData.jenisMadu;
                panen.tanggalPanen = panenData.tanggalPanen;
            }
            else
            {
                throw new InvalidOperationException("Data hanya bisa diubah sebelum diapprove");
            }

            return await base.UpdateAsync(panen);
        }

        public async Task<Panen> ApprovePanenOnLokasi(string idPanen, bool approve, string idApprover, string namaApprover)
        {
            var panen = _container.GetItemLinqQueryable<Panen>().First(p => p.id == idPanen);
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

        public async Task<Panen> ApprovePanenOnWarehouse(string idPanen, bool approve, string idApprover, string namaApprover, double beratBaru, string catatan)
        {
            var panen = _container.GetItemLinqQueryable<Panen>().First(p => p.id == idPanen);
            if (panen.status == Constants.PanenStatus.PIC_APPROVED.ToString() && approve)
            {
                panen.status = Constants.PanenStatus.ARRIVED_WAREHOUSE.ToString();
                panen.idPetugasWarehouse = idApprover;
                panen.namaPetugasWarehouse = namaApprover;
                panen.beratWarehouse = beratBaru;
                panen.catatanWarehouse = catatan;
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
            var panen = _container.GetItemLinqQueryable<Panen>().First(p => p.id == idPanen);
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
