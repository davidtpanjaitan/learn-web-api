﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using System.Security.Claims;
using webapp.DAL.Enum;
using webapp.DAL.Models;
using webapp.DAL.Repositories;

namespace david_api.Controllers
{
    [Route("api/lokasi")]
    public class LokasiController : ControllerBase
    {
        private LokasiRepository lokasiRepo;

        public LokasiController(LokasiRepository lokasiRepo)
        {
            this.lokasiRepo = lokasiRepo;
        }

        [HttpGet(Name = "getAllLokasi")]
        public async Task<IActionResult> GetAllLokasi()
        {
            var res = await lokasiRepo.GetAllAsync();
            return new OkObjectResult(res);
        }

        [Authorize(Roles="admin")]
        [HttpPost(Name = "CreateLokasi")]
        public async Task<IActionResult> Create([FromBody] Lokasi lokasi)
        {
            var creator = this.User.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Name)?.Value;
            var newlokasi = await lokasiRepo.CreateAsync(lokasi, creator);
            return new OkObjectResult(newlokasi);
        }


        [HttpGet("{id}", Name = "GetLokasiById")]
        public async Task<IActionResult> Get( [FromRoute] string id)
        {
            Lokasi lokasi = await lokasiRepo.GetByIdAsync(id);
            if (lokasi == null)
            {
                return new NotFoundResult();
            }
            return new OkObjectResult(lokasi);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}", Name = "UpdateLokasi")]
        public async Task<IActionResult> UpdateLokasi([FromRoute] string id, [FromBody] Lokasi updatedLokasi)
        {
            updatedLokasi.id = id;
            await lokasiRepo.UpdateAsync(updatedLokasi);
            return new OkObjectResult(updatedLokasi);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}", Name = "DeleteLokasi")]
        public async Task<IActionResult> DeleteLokasi([FromRoute] string id)
        {
            await lokasiRepo.DeleteAsync(id);
            return new OkResult();
        }

    }
}
