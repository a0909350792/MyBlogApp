using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MyBlogApp.Models;

var builder = WebApplication.CreateBuilder(args);

// 設定資料庫連線字串並加入 DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 添加 Session 服務
builder.Services.AddSession(); // 只保留一次

// 添加 MVC 服務
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 配置 HTTP 請求處理管道
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 啟用 Session
app.UseSession(); // 確保 Session 在這裡使用

// 設定認證和授權
app.UseAuthentication();
app.UseAuthorization();

// 配置路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
