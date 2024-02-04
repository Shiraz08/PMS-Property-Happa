using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PMS_PropertyHapa.Admin.Data;
using PMS_PropertyHapa.Shared.Dapper;

namespace PMS_PropertyHapa.Admin.Controllers
{
    public class LookUpController : Controller
    {
        private PropertyHapaAdminContext _context;
        private readonly IDapper _dapper;
        public LookUpController(PropertyHapaAdminContext context, IDapper dapper)
        {
            _context = context;
            _dapper = dapper;
        }
        [HttpGet]
        public List<SelectListItem> AspnetRolesDropDown()
        {
            var countrylist = _context.Roles.ToList();
            var selectList = new List<SelectListItem>();
            foreach (var element in countrylist)
            {
                selectList.Add(new SelectListItem
                {
                    Value = element.Id.ToString(),
                    Text = element.Name
                });
            }
            return selectList;
        }
    }
}
