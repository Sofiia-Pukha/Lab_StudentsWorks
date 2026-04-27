using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMVC.Domain.Entities;
using WebMVC.Infrastructure;

namespace WebMVC.Controllers;

public class ReviewsController : Controller
{
    private readonly AppDbContext _context;

    public ReviewsController(AppDbContext context)
    {
        _context = context;
    }

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
}