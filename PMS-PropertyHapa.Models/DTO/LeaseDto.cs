using Microsoft.AspNetCore.Http;
using PMS_PropertyHapa.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    public class LeaseDto
    {
        public int LeaseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsSigned { get; set; }
        public string SignatureImagePath { get; set; }
        public int? AssetId { get; set; }
        public string SelectedProperty { get; set; }
        public IEnumerable<AssetDTO> Assets { get; set; } 
        public IEnumerable<UnitDTO> SelectedUnits { get; set; }   
        public int? UnitId { get; set; }
        public string SelectedUnit { get; set; }
        public bool IsFixedTerm { get; set; }
        public bool IsMonthToMonth { get; set; }
        public bool HasSecurityDeposit { get; set; }
        public string LateFeesPolicy { get; set; }
        public string AppTenantId { get; set; }
        // Assuming TenantId is enough to link tenants for simplicity
        public int TenantId { get; set; }
        public string TenantIdValue { get; set; }
        public string Frequency { get; set; }
        public decimal? TotalRentAmount { get; set; }
        public string AddedBy { get; set; }
        public TenantModelDto Tenant { get; set; }
        // Collections for Rent Charges and Security Deposits
        public List<RentChargeDto> RentCharges { get; set; }
        public List<SecurityDepositDto> SecurityDeposits { get; set; }
        public FeeChargeDto FeeCharge { get; set; }
    }

    public class RentChargeDto
    {
        public int LeaseId { get; set; }
        public int RentChargeId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }

        public DateTime RentDate { get; set; }  

        public string RentPeriod { get; set; }
    }

    public class SecurityDepositDto
    {
        public int LeaseId { get; set; }
        public int SecurityDepositId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }

    public class FeeChargeDto
    {
        public int LeaseId { get; set; }
        public int FeeChargeId { get; set; }
        public bool ChargeLatefeeActive { get; set; } = false;
        public bool UsePropertyDefaultStructure { get; set; } = false;
        public bool SpecifyLateFeeStructure { get; set; } = false;
        public int DueDays { get; set; }
        public string Frequency { get; set; }
        public string CalculateFee { get; set; }
        public decimal Amount { get; set; }
        public int ChartAccountId { get; set; }
        public string Description { get; set; }
        public bool IsSendARemainder { get; set; } = false;
        public bool IsNotifyTenants { get; set; } = false;
        public bool IsEnableSms { get; set; } = false;
        public bool IsChargeLateFee { get; set; } = false;
        public bool IsMonthlyLimit { get; set; } = false;
        public bool IsDailyLimit { get; set; } = false;
        public bool IsMinimumBalance { get; set; } = false;
        public bool IsChargeLateFeeonSpecific { get; set; } = false;
        public DateTime FeeDate { get; set; }
    }

}
