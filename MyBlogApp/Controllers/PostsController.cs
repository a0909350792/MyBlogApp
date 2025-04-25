using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyBlogApp.Models;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MyBlogApp.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 顯示所有文章
        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            var posts = _context.Posts.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                posts = posts.Where(p => p.Title.Contains(search) || p.Content.Contains(search));
            }

            if (categoryId.HasValue)
            {
                posts = posts.Where(p => p.CategoryId == categoryId);
            }

            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["Search"] = search;
            ViewData["CategoryId"] = categoryId;

            return View(await posts.OrderByDescending(p => p.CreatedAt).ToListAsync());
        }

        // 顯示單一文章
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts.Include(p => p.Category).Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        // 顯示創建文章的表單
        public IActionResult Create()
        {
            ViewData["Categories"] = _context.Categories.ToList();
            return View();
        }
        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,CategoryId")] Post post)
        {
            if (ModelState.IsValid)
            {
                post.CreatedAt = DateTime.Now;  // 設置創建時間

                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));  // 重定向到文章列表頁
            }
            ViewData["Categories"] = _context.Categories.ToList();  // 保留分類資料供顯示
            return View(post);
        }




        //編輯文章
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) 
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            ViewData["Categories"] = _context.Categories.ToList();
            return View(post);
        }
        //接收編輯文章的表單提交
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,CategoryId")] Post post)
        {
            if (id != post.Id) 
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var existingPost = await _context.Posts.FindAsync(
                        post.Id);
                    if (existingPost == null)
                    {
                        return NotFound();
                    }

                    existingPost.Title = post.Title;
                    existingPost.Content = post.Content;
                    existingPost.CategoryId = post.CategoryId;
                    existingPost.UpdatedAt = DateTime.UtcNow;

                    _context.Update(existingPost);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["Categories"] = _context.Categories.ToList();
            return View(post);
        }
        [HttpPost]
        public async Task<IActionResult> AddComment(int postId, string content) 
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["CommentError"] = "留言不能為空";
                return RedirectToAction("Details", new { id = postId });
            }

            var comment = new Comment
            {
                PostId = postId,
                Content = content,
                CreatedAt = DateTime.Now
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = postId });
        }
    }
}
