using KargoAdmin.Data;
using KargoAdmin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DB Context ekleyin
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity servislerini ekleyin
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// MVC ekleyin
builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization(options =>
{
    // Sadece Admin rolündeki kullanıcılar Register sayfasına erişebilir
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
});
var app = builder.Build();


// Seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Seed datası oluşturulurken hata oluştu.");
    }
}

// Configure the HTTP request pipeline...
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Ana sayfa kullanıcı tarafına yönlendirir
app.MapControllerRoute(
    name: "public",
    pattern: "",
    defaults: new { controller = "Public", action = "Index" });

// Diğer public sayfalar
app.MapControllerRoute(
    name: "publicPages",
    pattern: "public/{action}/{id?}",
    defaults: new { controller = "Public" });

// Blog sayfaları için SEO dostu URL'ler
app.MapControllerRoute(
    name: "blogWithSlug",
    pattern: "blog/{slug}",
    defaults: new { controller = "PublicBlog", action = "Details" });

app.MapControllerRoute(
    name: "blogById",
    pattern: "blog/id/{id}",
    defaults: new { controller = "PublicBlog", action = "Details" });

app.MapControllerRoute(
    name: "blogTag",
    pattern: "blog/tag/{tag}",
    defaults: new { controller = "PublicBlog", action = "Tag" });

app.MapControllerRoute(
    name: "blogIndex",
    pattern: "blog",
    defaults: new { controller = "PublicBlog", action = "Index" });
app.MapControllerRoute(
    name: "about",
    pattern: "about",
    defaults: new { controller = "Public", action = "About" });

app.MapControllerRoute(
    name: "services",
    pattern: "services",
    defaults: new { controller = "Public", action = "Services" });

app.MapControllerRoute(
    name: "contact",
    pattern: "contact",
    defaults: new { controller = "Public", action = "Contact" });
// Admin panel giriş URL'si
app.MapControllerRoute(
    name: "adminLogin",
    pattern: "admin/login",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "adminBlog",
    pattern: "admin/blog/{action=Index}/{id?}",
    defaults: new { controller = "Blog", action = "Index" });

// Standart route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Public}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();