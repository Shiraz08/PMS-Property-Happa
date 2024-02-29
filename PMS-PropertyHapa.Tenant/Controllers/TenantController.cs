using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Tenant.Controllers
{
    public class TenantController : Controller
    {
        private readonly ApiDbContext _db;

        public TenantController(ApiDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult GetTenant(string tenantId)
        {
            if (string.IsNullOrEmpty(tenantId))
                return BadRequest("Tenant ID is required.");

            var tenant = _db.Tenant.FirstOrDefault(t => t.AppTenantId == Guid.Parse(tenantId));
            if (tenant != null)
                return Json(new { data = tenant });
            else
                return Json(new { data = new TenantModelDto() });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TenantModelDto tenantDto)
        {
            tenantDto.AppTenantId = Guid.Parse(tenantDto.AppTid);
            var result = await CreateTenantAsync(tenantDto);
            if (result)
                return Json(new { success = true, message = "Tenant added successfully" });
            else
                return Json(new { success = false, message = "Failed to add tenant" });
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] TenantModelDto tenantDto)
        {
            tenantDto.AppTenantId = Guid.Parse(tenantDto.AppTid);
            var result = await UpdateTenantAsync(tenantDto);
            if (result)
                return Json(new { success = true, message = "Tenant updated successfully" });
            else
                return Json(new { success = false, message = "Failed to update tenant" });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string tenantId)
        {
            var result = await DeleteTenantAsync(tenantId);
            if (result)
                return Json(new { success = true, message = "Tenant deleted successfully" });
            else
                return Json(new { success = false, message = "Failed to delete tenant" });
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddTenant()
        {
            var model = new TenantModelDto();
            return View(model);
        }


        public async Task<bool> CreateTenantAsync(TenantModelDto tenantDto)
        {
            var newTenant = new PMS_PropertyHapa.Models.Entities.Tenant
            {
                FirstName = tenantDto.FirstName,
                LastName = tenantDto.LastName,
                EmailAddress = tenantDto.EmailAddress,
                PhoneNumber = tenantDto.PhoneNumber,
                EmergencyContactInfo = tenantDto.EmergencyContactInfo,
                LeaseAgreementId = tenantDto.LeaseAgreementId,
                TenantNationality = tenantDto.TenantNationality,
                Gender = tenantDto.Gender,
                DOB = tenantDto.DOB,
                VAT = tenantDto.VAT,
                Status = true,
                LegalName = tenantDto.LegalName,
                Account_Name = tenantDto.Account_Name,
                Account_Holder = tenantDto.Account_Holder,
                Account_IBAN = tenantDto.Account_IBAN,
                Account_Swift = tenantDto.Account_Swift,
                Account_Bank = tenantDto.Account_Bank,
                Account_Currency = tenantDto.Account_Currency,
                AppTenantId = tenantDto.AppTenantId,
                Address = tenantDto.Address,
                Address2 = tenantDto.Address2,
                Locality = tenantDto.Locality,
                Region = tenantDto.Region,
                PostalCode = tenantDto.PostalCode,
                Country = tenantDto.Country,
                CountryCode = tenantDto.CountryCode
            };

            await _db.Tenant.AddAsync(newTenant);

            var result = await _db.SaveChangesAsync();

            return result > 0;
        }


        public async Task<bool> UpdateTenantAsync(TenantModelDto tenantDto)
        {
            var tenant = await _db.Tenant.FirstOrDefaultAsync(t => t.TenantId == tenantDto.TenantId);
            if (tenant == null) return false;

            tenant.FirstName = tenantDto.FirstName;
            tenant.LastName = tenantDto.LastName;
            tenant.EmailAddress = tenantDto.EmailAddress;
            tenant.PhoneNumber = tenantDto.PhoneNumber;
            tenant.EmergencyContactInfo = tenantDto.EmergencyContactInfo;
            tenant.LeaseAgreementId = tenantDto.LeaseAgreementId;
            tenant.TenantNationality = tenantDto.TenantNationality;
            tenant.Gender = tenantDto.Gender;
            tenant.DOB = tenantDto.DOB;
            tenant.VAT = tenantDto.VAT;
            tenant.Status = true;
            tenant.LegalName = tenantDto.LegalName;
            tenant.Account_Name = tenantDto.Account_Name;
            tenant.Account_Holder = tenantDto.Account_Holder;
            tenant.Account_IBAN = tenantDto.Account_IBAN;
            tenant.Account_Swift = tenantDto.Account_Swift;
            tenant.Account_Bank = tenantDto.Account_Bank;
            tenant.Account_Currency = tenantDto.Account_Currency;
            tenant.AppTenantId = tenantDto.AppTenantId;
            tenant.Address = tenantDto.Address;
            tenant.Address2 = tenantDto.Address2;
            tenant.Locality = tenantDto.Locality;
            tenant.Region = tenantDto.Region;
            tenant.PostalCode = tenantDto.PostalCode;
            tenant.Country = tenantDto.Country;
            tenant.CountryCode = tenantDto.CountryCode;

            _db.Tenant.Update(tenant);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }


        public async Task<bool> DeleteTenantAsync(string tenantId)
        {
            var tenant = await _db.Tenant.FirstOrDefaultAsync(t => t.TenantId == Convert.ToInt32(tenantId));
            if (tenant == null) return false;

            _db.Tenant.Remove(tenant);
            var result = await _db.SaveChangesAsync();
            return result > 0;
        }

    }
}
