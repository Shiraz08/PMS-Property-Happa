namespace PMS_PropertyHapa.Models.DTO
{
    public class TokenDTO
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public string UserName { get; set; }
        public string UserId { get; set; }

        public string PrimaryColor { get; set; }    
        public string SecondaryColor { get; set;}
        public string OrganizationName { get; set; }    
        public string OrganizationDescription { get; set; }    
        public string OrganizationLogo { get; set; }    
        public string OrganizationIcon { get; set; }    

        public int? Tid { get; set; }    
    }
}
