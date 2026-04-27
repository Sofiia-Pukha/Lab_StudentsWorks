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

    public async Task<IActionResult> Index(string? searchString, int? categoryId, int? departmentId, int? teacherId)
    {
        var worksQuery = _context.Works
            .Include(w => w.Category)
            .Include(w => w.Student)
            .Include(w => w.Teacher)
                .ThenInclude(t => t.Department) 
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            worksQuery = worksQuery.Where(w => w.Title.Contains(searchString));
        }

        if (categoryId.HasValue)
        {
            worksQuery = worksQuery.Where(w => w.CategoryId == categoryId.Value);
        }

        if (teacherId.HasValue)
        {
            worksQuery = worksQuery.Where(w => w.TeacherId == teacherId.Value);
        }

        if (departmentId.HasValue)
        {
            worksQuery = worksQuery.Where(w => w.Teacher.DepartmentId == departmentId.Value);
        }

        ViewBag.ReviewedWorkIds = await _context.Reviews.Select(r => r.WorkId).Distinct().ToListAsync();

        ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", categoryId);
        ViewBag.Departments = new SelectList(await _context.Departments.ToListAsync(), "Id", "Name", departmentId);
        ViewBag.Teachers = new SelectList(await _context.Teachers.ToListAsync(), "Id", "FullName", teacherId);
        ViewBag.CurrentSearch = searchString;

        return View(await worksQuery.ToListAsync());
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

        ModelState.Remove("Category");
        ModelState.Remove("Student");
        ModelState.Remove("Teacher");

        if (ModelState.IsValid)
        {
            _context.Add(work);
            await _context.SaveChangesAsync();
            return RedirectToAction("Create", "Reviews", new { workId = work.Id });
        }

        ViewBag.Teachers = new SelectList(_context.Teachers, "Id", "FullName", work.TeacherId);
        ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", work.CategoryId);
        ViewBag.Students = new SelectList(_context.Users, "Id", "Username", work.StudentId);
        return View(work);
    }
}