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

    public async Task<IActionResult> Index()
    {
        var reviews = await _context.Reviews
            .Include(r => r.Teacher)
            .Include(r => r.Work)
            .Include(r => r.Student)
            .OrderByDescending(r => r.Id) 
            .ToListAsync();
            
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
        if (ModelState.IsValid)
        {
            _context.Add(review);
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Index", "Works");
        }
        return View(review);
    }
}