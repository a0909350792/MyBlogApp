using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MyBlogApp.Models;

var builder = WebApplication.CreateBuilder(args);

// �]�w��Ʈw�s�u�r��å[�J DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// �K�[ Session �A��
builder.Services.AddSession(); // �u�O�d�@��

// �K�[ MVC �A��
builder.Services.AddControllersWithViews();

var app = builder.Build();

// �t�m HTTP �ШD�B�z�޹D
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// �ҥ� Session
app.UseSession(); // �T�O Session �b�o�̨ϥ�

// �]�w�{�ҩM���v
app.UseAuthentication();
app.UseAuthorization();

// �t�m����
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
