using AutoMapper;
using david_api.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using webapp.DAL.Enum;
using webapp.DAL.Models;
using webapp.DAL.Repositories;
using webapp.DAL.Tools;

namespace david_api.Controllers
{
    [Route("api/produk")]
    public class ProdukController : ControllerBase
    {
        private ProdukRepository produkRepo;

        public ProdukController(ProdukRepository produkRepo)
        {
            this.produkRepo = produkRepo;
        }

        [HttpGet(Name = "getPagedProduk")]
        public async Task<IActionResult> GetAllProduk(int pageSize, int pageNum, string query)
        {
            var res = await produkRepo.GetAsyncPaged(pageSize, pageNum - 1, query);
            return new OkObjectResult(res);
        }

        [Authorize(Roles = "petugasWarehouse,admin")]
        [HttpPost(Name = "CreateProduk")]
        public async Task<IActionResult> Create([FromBody] Produk produk)
        {
            var newproduk = await produkRepo.CreateAsync(produk);
            return new OkObjectResult(newproduk);
        }

        [Authorize(Roles = "admin")]
        [HttpPost("generate", Name = "GenerateProduk")]
        public async Task<IActionResult> Generate([FromBody] GenerateProdukDTO dto)
        {
            var creator = this.User.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Name)?.Value;
            var listNewProduk = await produkRepo.GenerateProduk(dto.jumlah, creator);
            return new OkObjectResult(listNewProduk);
        }

        [HttpGet("{id}", Name = "GetProdukById")]
        public async Task<IActionResult> Get([FromRoute] string id)
        {
            Produk produk = await produkRepo.GetByIdAsync(id);
            if (produk == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(produk);
        }

        [Authorize(Roles = "petugasWarehouse,admin")]
        [HttpPut("{id}", Name = "UpdateProduk")]
        public async Task<IActionResult> UpdateProduk([FromRoute] string id, [FromBody] Produk updatedProduk)
        {
            updatedProduk.id = id;
            await produkRepo.UpdateAsync(updatedProduk);
            return new OkObjectResult(updatedProduk);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}/approve-admin")]
        public async Task<IActionResult> ApproveByAdmin([FromRoute] string id, [FromBody] ApprovalDTO dto)
        {
            var result = await produkRepo.ApproveProdukByAdmin(id, dto.approve, dto.idApprover, dto.namaApprover);
            return new OkObjectResult(result);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}", Name = "DeleteProduk")]
        public async Task<IActionResult> DeleteProduk([FromRoute] string id)
        {
            await produkRepo.DeleteAsync(id);
            return new OkResult();
        }

    }
}
