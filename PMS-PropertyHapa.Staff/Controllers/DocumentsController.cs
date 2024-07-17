using Microsoft.AspNetCore.Mvc;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using PMS_PropertyHapa.Staff.Services.IServices;

namespace PMS_PropertyHapa.Staff.Controllers
{
    public class DocumentsController : Controller
    {
        private readonly IAuthService _authService;

        public DocumentsController(IAuthService authService)
        {
            _authService = authService;
        }
        public async Task<IActionResult> Documents()
        {
            return View();
        }

        public async Task<IActionResult> GetDocuments()
        {
            IEnumerable<DocumentsDto> documents = new List<DocumentsDto>();
            documents = await _authService.GetDocumentsAsync();
            var currenUserId = Request?.Cookies["userId"]?.ToString();
            if (currenUserId != null)
            {
                documents = documents.Where(s => s.AddedBy == currenUserId);
            }
            return Ok(documents);
        }

      
        [HttpPost]
        public async Task<IActionResult> SaveDocument(DocumentsDto document)
        {
            if (document == null)
            {
                return Json(new { success = false, message = "Received data is null." });
            }

            document.AddedBy = Request?.Cookies["userId"]?.ToString();
            bool isSuccess = await _authService.SaveDocumentAsync(document);

            if (isSuccess)
            {
                return Json(new { success = true, message = "Document added successfully" });
            }
            else
            {
                return Json(new { success = false, message = "Failed to save document." });
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            await _authService.DeleteDocumentAsync(id);
            return Json(new { success = true, message = "Document deleted successfully" });
        }

        public async Task<IActionResult> GetDocumentById(int id)
        {
            DocumentsDto document = await _authService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return StatusCode(500, "Document request not found");
            }
            return Ok(document);
        }
        
        public async Task<IActionResult> GetDocumentByAsset(int assetId)
        {
            var invoices = await _authService.GetDocumentByAssetAsync(assetId);
            return Ok(invoices);
        }

    }
}
