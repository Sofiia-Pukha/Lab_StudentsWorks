using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; 
using WebMVC.Domain.Entities;
using WebMVC.Infrastructure;
using System.Security.Claims;

namespace WebMVC.Controllers;

public class ReviewsController : Controller
{
    private readonly AppDbContext _context;

    public ReviewsController(AppDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(int? workId)
    {
        var query = _context.Reviews
            .Include(r => r.Teacher)
            .Include(r => r.Work)
            .Include(r => r.Student)
            .AsQueryable();

        if (workId.HasValue)
        {
            query = query.Where(r => r.WorkId == workId.Value);
        }

        var reviews = await query.OrderByDescending(r => r.Id).ToListAsync();
            
        return View(reviews);
    }

    [Authorize(Roles = "StudentAuthor,Admin")]
    public IActionResult Create(int workId)
    {
        var work = _context.Works
            .Include(w => w.Teacher)
            .FirstOrDefault(w => w.Id == workId);

        if (work == null) return NotFound();

        ViewBag.WorkTitle = work.Title;
        ViewBag.TeacherName = work.Teacher?.FullName;
        
        var review = new Review 
        { 
            WorkId = workId, 
            TeacherId = work.TeacherId,
            StudentId = work.StudentId 
        };

        return View(review);
    }

    [Authorize(Roles = "StudentAuthor,Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Review review)
    {
        ModelState.Remove("Work");
        ModelState.Remove("Student");
        ModelState.Remove("Teacher");

        if (ModelState.IsValid)
        {
            _context.Add(review);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Index", "Works");
        }

        var work = await _context.Works
            .Include(w => w.Teacher)
            .FirstOrDefaultAsync(w => w.Id == review.WorkId);
            
        if (work != null)
        {
            ViewBag.WorkTitle = work.Title;
            ViewBag.TeacherName = work.Teacher?.FullName;
        }

        return View(review);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var review = await _context.Reviews.FindAsync(id);
        if (review == null) return NotFound();

        return View(review);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Review review)
    {
        if (id != review.Id) return NotFound();

        var reviewInDb = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (reviewInDb == null) return NotFound();

        ModelState.Remove("Work");
        ModelState.Remove("Student");
        ModelState.Remove("Teacher");

        if (ModelState.IsValid)
        {
            try
            {
                if (reviewInDb.Comment != review.Comment)
                {
                    int adminId = GetCurrentAdminId();
                    if (adminId != 0)
                    {
                        _context.AdminLogs.Add(new AdminLog {
                            AdminId = adminId,
                            TargetTable = "Reviews",
                            TargetColumn = "Comment",
                            OldValue = reviewInDb.Comment,
                            NewValue = review.Comment,
                            ActionTime = DateTime.UtcNow
                        });
                    }

                    reviewInDb.Comment = review.Comment;
                    
                    _context.Update(reviewInDb);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReviewExists(review.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index), new { workId = reviewInDb.WorkId }); 
        }
        return View(review);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var review = await _context.Reviews.FirstOrDefaultAsync(m => m.Id == id);
        if (review == null) return NotFound();

        int currentWorkId = review.WorkId;

        int adminId = GetCurrentAdminId();
        if (adminId != 0)
        {
            _context.AdminLogs.Add(new AdminLog {
                AdminId = adminId,
                TargetTable = "Reviews",
                TargetColumn = "Deleted",
                OldValue = $"Видалено відгук студента. Текст: '{review.Comment}' (Оцінка: {review.Rating})",
                NewValue = "-",
                ActionTime = DateTime.UtcNow
            });
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        
        return RedirectToAction(nameof(Index), new { workId = currentWorkId });
    }

    private bool ReviewExists(int id)
    {
        return _context.Reviews.Any(e => e.Id == id);
    }

    private int GetCurrentAdminId()
    {
        var claimIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(claimIdStr) && int.TryParse(claimIdStr, out int parsedId))
        {
            return parsedId;
        }
        
        var fallbackAdmin = _context.Users.FirstOrDefault(u => u.Role == UserRole.Admin);
        return fallbackAdmin?.Id ?? 0;
    }
}