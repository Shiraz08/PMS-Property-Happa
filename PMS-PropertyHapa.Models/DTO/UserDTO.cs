namespace PMS_PropertyHapa.Models.DTO
{
    public class UserDTO
    {
        public long userID { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string IP { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public string email { get; set; }
        public string AccountNumber { get; set; }
        public string userShift { get; set; }
        public string phoneNumber { get; set; }
        public string userType { get; set; }
        public bool active { get; set; }
        public bool beginner { get; set; }
        public bool blacklist { get; set; }
        public bool advanced { get; set; }
        public DateTime? activeDate { get; set; }
        public string branch { get; set; }
        public string subCategory { get; set; }
        public string category { get; set; }
        public int loginCount { get; set; }
        public int attempts { get; set; }
        public DateTime nowLogin { get; set; }
        public DateTime createdOn { get; set; }
        public string BasicSalary { get; set; }
        public string Incentive { get; set; }
        public string MinSaleTarget { get; set; }
        public string StaffTravelAllowance { get; set; }
        public bool StaffOverTime { get; set; }
        public string StaffAnnualBonus { get; set; }
        public bool StaffAnnualBonusAllowed { get; set; }

        //Fecilities
        public string AllUsersAutoFacility { get; set; }
        public bool AllUsersAutoFacilityAllowed { get; set; }
        public string AllUsersPhoneFacility { get; set; }
        public bool AllUsersPhoneFacilityAllowed { get; set; }
        public string AllUsersInternetFacility { get; set; }
        public bool AllUsersInternetFacilityAllowed { get; set; }
        public string AllUsersHomeRentFacility { get; set; }
        public bool AllUsersHomeRentFacilityAllowed { get; set; }
        public string AllUsersEOBIFacility { get; set; }
        public bool AllUsersEOBIFacilityAllowed { get; set; }
        public string AllUsersTransportFacility { get; set; }
        public bool AllUsersTransportFacilityAllowed { get; set; }

        //Fines and Penalties
        public string AllUsersAbsentFine { get; set; }
        public bool AllUsersAbsentFineAllowed { get; set; }
        public string AllUsersLateComingsFine { get; set; }
        public bool AllUsersLateComingsFineAllowed { get; set; }
        public long ParentId { get; set; }
    }
}
