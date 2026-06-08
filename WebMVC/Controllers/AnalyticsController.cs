using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebMVC.Infrastructure;

namespace WebMVC.Controllers;

[AllowAnonymous] 
public class AnalyticsController : Controller
{
    private readonly AppDbContext _context;

    public AnalyticsController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetTeacherRatingsData()
    {
        var dbData = await _context.Reviews
            .Include(r => r.Work)
            .ThenInclude(w => w.Teacher)
            .Where(r => r.Work != null && r.Work.Teacher != null)
            .GroupBy(r => r.Work.Teacher)
            .Select(g => new
            {
                TeacherName = g.Key.FullName,
                RawAverage = g.Average(r => (double)r.Rating),
                ReviewCount = g.Count()
            })
            .ToListAsync();

        var finalData = dbData
            .Select(d => new
            {
                TeacherName = d.TeacherName,
                AverageRating = Math.Round(d.RawAverage, 2),
                ReviewCount = d.ReviewCount
            })
            .OrderByDescending(t => t.AverageRating)
            .Take(10)
            .ToList();

        return Json(finalData);
    }

    [HttpGet]
    public async Task<IActionResult> GetTopicPopularityData()
    {
        var data = await _context.Categories
            .Select(c => new
            {
                TopicName = c.Name,
                WorksCount = _context.Works.Count(w => w.CategoryId == c.Id)
            })
            .Where(c => c.WorksCount > 0)
            .OrderByDescending(c => c.WorksCount)
            .ToListAsync();

        return Json(data);
    }

    [HttpGet]
    public async Task<IActionResult> GetGradeChancesData()
    {
        var dbData = await _context.Works
            .Include(w => w.Teacher)
            .ThenInclude(t => t.Department)
            // Шукаємо Grade і відкидаємо роботи без оцінок (Grade != null)
            .Where(w => w.Teacher != null && w.Teacher.Department != null && w.Grade != null)
            .GroupBy(w => w.Teacher.Department)
            .Select(g => new
            {
                DepartmentName = g.Key.Name,
                // Використовуємо Grade для розрахунку (знак ! каже програмі, що значення точно є)
                RawAverage = g.Average(w => (double)w.Grade!) 
            })
            .ToListAsync();

        var finalData = dbData
            .Select(d => new
            {
                DepartmentName = d.DepartmentName,
                AverageScore = Math.Round(d.RawAverage, 2)
            })
            .OrderByDescending(d => d.AverageScore)
            .ToList();

        return Json(finalData);
    }
}