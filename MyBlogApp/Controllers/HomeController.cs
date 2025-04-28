using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBlogApp.Models;

namespace MyBlogApp.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string search, int? categoryId)
    {
        var posts = _context.Posts.Include(p => p.Category).AsQueryable();

        // 搜尋
        if (!string.IsNullOrEmpty(search))
        {
            posts = posts.Where(p => p.Title.Contains(search) || p.Content.Contains(search));
        }

        // 篩選分類
        if (categoryId.HasValue)
        {
            posts = posts.Where(p => p.CategoryId == categoryId.Value);
        }

        // 傳遞篩選條件到視圖
        ViewBag.Categories = await _context.Categories.ToListAsync();
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;

        // 如果沒有搜尋到文章，設置提示訊息
        var postList = await posts.OrderByDescending(p => p.CreatedAt).ToListAsync();
        if (!postList.Any())
        {
            ViewBag.NoResults = "沒有找到符合條件的文章。";
        }
        return View(await posts.OrderByDescending(p => p.CreatedAt).ToListAsync());
    }

    // 加在你的 HomeController 裡
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var post = await _context.Posts
            .Include(p => p.Category)
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (post == null)
        {
            return NotFound();
        }

        return View(post);  // 會去找 Views/Home/Details.cshtml
    }

}

