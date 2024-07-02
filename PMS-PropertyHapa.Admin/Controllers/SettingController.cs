using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PMS_PropertyHapa.MigrationsFiles.Data;
using PMS_PropertyHapa.Models.DTO;
using PMS_PropertyHapa.Models.Entities;
using System.Security.Claims;

namespace PMS_PropertyHapa.Admin.Controllers
{
    public class SettingController : Controller
    {
        private ApiDbContext _context;


        public SettingController(ApiDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult FAQ()
        {
            return View();
        }

        public IActionResult VideoTutorial()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveFAQ([FromBody] FAQ model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId == null)
                {
                    return Json(new { success = false, message = "User is not logged in." });
                }

                var faq = _context.FAQs.FirstOrDefault(x => x.FAQId == model.FAQId);

                if (faq == null)
                    faq = new FAQ();

                faq.Question = model.Question;
                faq.Answer = model.Answer;

                if (faq.FAQId > 0)
                {

                    faq.ModifiedBy = userId;
                    faq.ModifiedDate = DateTime.Now;
                    _context.FAQs.Update(faq);
                }
                else
                {
                    faq.AddedBy = userId;
                    faq.AddedDate = DateTime.Now;
                    _context.FAQs.Add(faq);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "FAQ added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding FAQ: " + ex.Message });
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetFAQById(int id)
        {

            try
            {
                var result = await (from faq in _context.FAQs
                                    where faq.FAQId == id
                                    select new FAQ
                                    {
                                        FAQId = faq.FAQId,
                                        Question = faq.Question,
                                        Answer = faq.Answer,

                                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (result == null)
                {
                    return Json(new { success = false, message = "FAQ not found." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error retrieving FAQ data." + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFAQ(int id)
        {
            try
            {
                var faq = await _context.FAQs.FindAsync(id);
                if (faq == null)
                {
                    return Json(new { success = false, message = "FAQ not found." });
                }

                faq.IsDeleted = true;
                _context.FAQs.Update(faq);
                var saveResult = await _context.SaveChangesAsync();

                if (saveResult > 0)
                {
                    return Ok(new { success = true, message = "FAQ deleted successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete FAQ." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting FAQ: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFAQs()
        {

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await (from faq in _context.FAQs
                                    where faq.IsDeleted != true && faq.AddedBy == userId
                                    select new FAQ
                                    {
                                        FAQId = faq.FAQId,
                                        Question = faq.Question,
                                        Answer = faq.Answer,

                                    })
                    .AsNoTracking()
                    .ToListAsync();

                if (!result.Any())
                {
                    return Json(new { success = false, message = "No FAQs found." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error retrieving FAQ data." + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveVideoTutorial([FromBody] VideoTutorial model)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId == null)
                {
                    return Json(new { success = false, message = "User is not logged in." });
                }

                var vt = _context.VideoTutorial.FirstOrDefault(x => x.TutorialId == model.TutorialId);

                if (vt == null)
                    vt = new VideoTutorial();

                vt.Title = model.Title;
                vt.VideoLink = model.VideoLink;

                if (vt.TutorialId > 0)
                {

                    vt.ModifiedBy = userId;
                    vt.ModifiedDate = DateTime.Now;
                    _context.VideoTutorial.Update(vt);
                }
                else
                {
                    vt.AddedBy = userId;
                    vt.AddedDate = DateTime.Now;
                    _context.VideoTutorial.Add(vt);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Video Tutorial added successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding Video Tutorial: " + ex.Message });
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetVideoTutorialById(int id)
        {

            try
            {
                var result = await (from vt in _context.VideoTutorial
                                    where vt.TutorialId == id
                                    select new VideoTutorial
                                    {
                                        TutorialId = vt.TutorialId,
                                        Title = vt.Title,
                                        VideoLink = vt.VideoLink,

                                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (result == null)
                {
                    return Json(new { success = false, message = "Video Tutorial not found." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error retrieving Video Tutorial data." + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVideoTutorial(int id)
        {
            try
            {
                var vt = await _context.VideoTutorial.FindAsync(id);
                if (vt == null)
                {
                    return Json(new { success = false, message = "Video Tutorial not found." });
                }

                vt.IsDeleted = true;
                _context.VideoTutorial.Update(vt);
                var saveResult = await _context.SaveChangesAsync();

                if (saveResult > 0)
                {
                    return Ok(new { success = true, message = "Video Tutorial deleted successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to delete Video Tutorial." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting Video Tutorial: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVideoTutorials()
        {

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var result = await (from vt in _context.VideoTutorial
                                    where vt.IsDeleted != true && vt.AddedBy == userId
                                    select new VideoTutorial
                                    {
                                        TutorialId = vt.TutorialId,
                                        Title = vt.Title,
                                        VideoLink = vt.VideoLink,

                                    })
                    .AsNoTracking()
                    .ToListAsync();

                if (!result.Any())
                {
                    return Json(new { success = false, message = "No Video Tutorials found." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error retrieving Video Tutorial data." + ex.Message });
            }
        }
    }
}
