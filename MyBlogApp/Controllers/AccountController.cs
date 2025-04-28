using Microsoft.AspNetCore.Mvc;
using MyBlogApp.Models;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.CSharp.Syntax;
namespace MyBlogApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }
        //註冊頁面
        public IActionResult Register()
        {
            return View();
        }
        //處理註冊
        [HttpPost]
        public async Task<IActionResult> Register(string userName, string fullName, string password)
        {
            // 檢查用戶名是否已存在
            if (_context.Users.Any(u => u.UserName == userName))
            {
                TempData["ErrorMessage"] = "用戶名已存在！";
                return View();
            }

            // 創建新用戶並保存
            var user = new User
            {
                UserName = userName,
                FullName = fullName,
                PasswordHash = HashPassword(password),
                JoinedDate = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 註冊成功，重定向到登入頁面
            TempData["SuccessMessage"] = "註冊成功，請登入！";
            return RedirectToAction("Account", "Login");
        }

        //登入頁面
        public IActionResult Login()
        {
            // 檢查使用者是否已經登入
            if (HttpContext.Session.GetString("UserId") != null)
            {
                // 如果已經登入，跳轉到文章頁面
                return RedirectToAction("Index", "Posts");
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password)
        {
            // 先取得對應使用者的資料
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);

            // 如果使用者不存在或密碼不正確
            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                // 顯示錯誤訊息
                ViewData["ErrorMessage"] = "帳號或密碼錯誤";
                return View();
            }

            // 登入成功，這裡可以用 Session 或 Cookie 記錄登入狀態
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.UserName);

            // 重定向至文章頁面
            return RedirectToAction("Create", "Posts");
        }


        //登出
        public IActionResult Logout() 
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        private bool VerifyPassword(string password, string storedPasswordHash)
        {
            // 將輸入的密碼進行雜湊，並與存儲的雜湊密碼進行比對
            var passwordHash = HashPassword(password);
            return passwordHash == storedPasswordHash;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }


    }
}

    
    

