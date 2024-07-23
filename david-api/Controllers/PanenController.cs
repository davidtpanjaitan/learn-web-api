using AutoMapper;
using david_api.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System.Data;
using System.Security.Claims;
using webapp.DAL.Enum;
using webapp.DAL.Models;
using webapp.DAL.Repositories;
using webapp.DAL.Tools;

namespace david_api.Controllers
{
    [Route("api/panen")]
    public class PanenController : ControllerBase
    {
        private PanenRepository panenRepo;
        private IMapper mapper;
        public PanenController(PanenRepository panenRepo)
        {
            this.panenRepo = panenRepo;
            var mappingconfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SubmitPanenDTO, Panen>();
            });
            mapper = mappingconfig.CreateMapper();
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

        [Authorize(Roles = "admin")]
        [HttpPost("generate", Name ="GeneratePanenByLokasi")]
        public async Task<IActionResult> Generate([FromBody] GeneratePanenDTO dto)
        {
            var creator = this.User.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Name)?.Value;
            var listNewPanen = await panenRepo.GeneratePanenForLokasi(dto.idLokasi, dto.namaLokasi, dto.jumlah, creator);
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

        [Authorize]
        [HttpPut("{id}", Name = "UpdatePanen")]
        public async Task<IActionResult> UpdatePanen([FromRoute] string id, [FromBody] Panen updatedPanen)
        {
            updatedPanen.id = id;
            await panenRepo.UpdateAsync(updatedPanen);
            return new OkObjectResult(updatedPanen);
        }

        [Authorize(Roles = "petugasLokasi,admin")]
        [HttpPut("{id}/submit-lokasi")]
        public async Task<IActionResult> SubmitOnLokasi([FromRoute] string id, [FromForm] SubmitPanenDTO dto)
        {
            var panen = mapper.Map<Panen>(dto);
            panen.id = id;
            panen.gambarPanenUrl = await ImageHostService.uploadImage(dto.gambar);
            var result = await panenRepo.SubmitPanenData(panen);
            return new OkObjectResult(result);
        }

        [Authorize(Roles = "picLokasi,admin")]
        [HttpPut("{id}/approve-lokasi")]
        public async Task<IActionResult> ApproveOnLokasi([FromRoute] string id, [FromBody] ApprovalDTO dto)
        {
            var result = await panenRepo.ApprovePanenOnLokasi(id, dto.approve, dto.idApprover, dto.namaApprover);
            return new OkObjectResult(result);
        }

        [Authorize(Roles = "petugasWarehouse,admin")]
        [HttpPut("{id}/approve-warehouse")]
        public async Task<IActionResult> ApproveOnWarehouse([FromRoute] string id, [FromBody] WarehouseApprovalDTO dto)
        {
            var imageUrl = await ImageHostService.uploadImage(dto.gambar);
            var result = await panenRepo.ApprovePanenOnWarehouse(id, dto.approve, dto.idApprover, dto.namaApprover, dto.beratBaru, dto.catatan, imageUrl);
            return new OkObjectResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}/approve-done")]
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
