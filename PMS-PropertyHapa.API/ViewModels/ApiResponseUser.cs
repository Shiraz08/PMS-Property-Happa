

namespace PMS_PropertyHapa.API.ViewModels
{
    public class ApiResponseUser
    {
        public ResultUser? Result { get; set; }
        public Messages[] Messages { get; set; }
        public bool HasErrors { get; set; }
        public bool IsValid { get; set; }
        public string TextInfo { get; set; }
    }
}
