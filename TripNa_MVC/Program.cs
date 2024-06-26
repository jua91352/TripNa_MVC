using TripNa_MVC.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
var connectionString = builder.Configuration.GetConnectionString("TripNaContext");
builder.Services.AddDbContext<TripNaContext>(x => x.UseSqlServer(connectionString));

builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=show}/{id?}");

app.Run();


static void ConfigureServices(IServiceCollection services)
{
    // 添加內存緩存服務
    services.AddMemoryCache();

    // 其他服務配置...
    services.AddControllers();
}