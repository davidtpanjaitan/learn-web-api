using david_api.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using webapp.DAL.Models;
using webapp.DAL.Repositories;

namespace david_api.Controllers
{
    [Route("api/panen")]
    public class PanenController : ControllerBase
    {
        private PanenRepository panenRepo;

        public PanenController(PanenRepository panenRepo)
        {
            this.panenRepo = panenRepo;
        }

        [HttpGet(Name = "getPagedPanen")]
        public async Task<IActionResult> GetAllPanen(int pageSize, int pageNum)
        {
            var res = await panenRepo.GetAsyncPaged(pageSize, pageNum - 1);
            return new OkObjectResult(res);
        }

        [HttpPost(Name = "CreatePanen")]
        public async Task<IActionResult> Create([FromBody] Panen panen)
        {
            var newpanen = await panenRepo.CreateAsync(panen);
            return new OkObjectResult(newpanen);
        }

        [HttpPost("generate", Name ="GeneratePanenByLokasi")]
        public async Task<IActionResult> Generate([FromBody] GeneratePanenDTO dto)
        {
            var listNewPanen = await panenRepo.GeneratePanenForLokasi(dto.IdLokasi, dto.NamaLokasi, dto.Jumlah);
            return new OkObjectResult(listNewPanen);
        }

        [HttpGet("{id}", Name = "GetPanenById")]
        public async Task<IActionResult> Get( [FromRoute] string id)
        {
            Panen panen = await panenRepo.GetByIdAsync(id);
            if (panen == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(panen);
        }

        [HttpPut("{id}", Name = "UpdatePanen")]
        public async Task<IActionResult> UpdatePanen([FromRoute] string id, [FromBody] Panen updatedPanen)
        {
            updatedPanen.id = id;
            await panenRepo.UpdateAsync(updatedPanen);
            return new OkObjectResult(updatedPanen);
        }

        [HttpPut("{id}/approve-lokasi")]
        public async Task<IActionResult> ApproveOnLokasi([FromRoute] string id, [FromBody] ApprovalDTO dto)
        {
            var result = await panenRepo.ApprovePanenOnLokasi(id, dto.approve, dto.idApprover, dto.namaApprover);
            return new OkObjectResult(result);
        }
        [HttpPut("{id}/approve-warehouse")]
        public async Task<IActionResult> ApproveOnWarehouse([FromRoute] string id, [FromBody] WarehouseApprovalDTO dto)
        {
            var result = await panenRepo.ApprovePanenOnWarehouse(id, dto.approve, dto.idApprover, dto.namaApprover, dto.beratBaru, dto.catatan);
            return new OkObjectResult(result);
        }

        [HttpPut("{id}/approve-lokasi")]
        public async Task<IActionResult> ApproveByAdmin([FromRoute] string id, [FromBody] ApprovalDTO dto)
        {
            var result = await panenRepo.ApprovePanenByAdmin(id, dto.approve, dto.idApprover, dto.namaApprover);
            return new OkObjectResult(result);
        }

        [HttpDelete("{id}", Name = "DeletePanen")]
        public async Task<IActionResult> DeletePanen([FromRoute] string id)
        {
            await panenRepo.DeleteAsync(id);
            return new OkResult();
        }



    }
}
