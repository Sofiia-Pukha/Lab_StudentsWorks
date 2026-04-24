using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebMVC.Domain.Entities;
using WebMVC.Infrastructure;

namespace WebMVC.Controllers;

public class WorksController : Controller
{
    private readonly AppDbContext _context;

    public WorksController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var works = await _context.Works
            .Include(w => w.Teacher)
            .Include(w => w.Category)
            .Include(w => w.Student)
            .ToListAsync();
        return View(works);
    }

    public IActionResult Create()
    {
        ViewBag.Teachers = new SelectList(_context.Teachers, "Id", "FullName");
        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
        ViewBag.Students = new SelectList(_context.Users, "Id", "Username");
        
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Work work)
    {
        work.CreatedAt = DateTime.UtcNow;

        if (ModelState.IsValid)
        {
            _context.Add(work);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Teachers = new SelectList(_context.Teachers, "Id", "FullName", work.TeacherId);
        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", work.CategoryId);
        ViewBag.Students = new SelectList(_context.Users, "Id", "Username", work.StudentId);
        return View(work);
    }
}