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
    name: "aleris",
    pattern: "",
    defaults: new { controller = "Aleris", action = "Index" });

// Diğer public sayfalar
app.MapControllerRoute(
    name: "AlerisPages",
    pattern: "Aleris/{action}/{id?}",
    defaults: new { controller = "Aleris" });

// Blog sayfaları için SEO dostu URL'ler
app.MapControllerRoute(
    name: "blogWithSlug",
    pattern: "Blog/{slug}",
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
    pattern: "Blog",
    defaults: new { controller = "PublicBlog", action = "Index" });
app.MapControllerRoute(
    name: "about",
    pattern: "about",
    defaults: new { controller = "Aleris", action = "About" });

app.MapControllerRoute(
    name: "contact",
    pattern: "contact",
    defaults: new { controller = "Aleris", action = "Contact" });
// Admin panel giriş URL'si
app.MapControllerRoute(
    name: "adminLogin",
    pattern: "admin/login",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "adminBlog",
    pattern: "admin/blog/{action=Index}/{id?}",
    defaults: new { controller = "Blog", action = "Index" });
// Service detail pages routes
app.MapControllerRoute(
    name: "landTransport",
    pattern: "karayolu-tasimaciligi",
    defaults: new { controller = "Aleris", action = "LandTransport" });

app.MapControllerRoute(
    name: "airTransport",
    pattern: "havayolu-tasimaciligi",
    defaults: new { controller = "Aleris", action = "AirTransport" });

app.MapControllerRoute(
    name: "seaTransport",
    pattern: "denizyolu-tasimaciligi",
    defaults: new { controller = "Aleris", action = "SeaTransport" });

app.MapControllerRoute(
    name: "storageDistribution",
    pattern: "depolama-dagitim",
    defaults: new { controller = "Aleris", action = "StorageDistribution" });

// Alternative routes for service details
app.MapControllerRoute(
    name: "servicesKarayolu",
    pattern: "Aleris/Servisler/KaraYolu",
    defaults: new { controller = "Aleris", action = "LandTransport" });

app.MapControllerRoute(
    name: "servicesHavayolu",
    pattern: "Aleris/Servisler/HavaYolu",
    defaults: new { controller = "Aleris", action = "AirTransport" });

app.MapControllerRoute(
    name: "servicesDenizyolu",
    pattern: "Aleris/Servisler/DenizYolu",
    defaults: new { controller = "Aleris", action = "SeaTransport" });

app.MapControllerRoute(
    name: "servicesDepolama",
    pattern: "Aleris/Servisler/DepolamaDagitim",
    defaults: new { controller = "Aleris", action = "StorageDistribution" });

app.MapControllerRoute(
    name: "usefulInfoTag",
    pattern: "faydali-bilgiler/kategori/{tag}",
    defaults: new { controller = "PublicBlog", action = "UsefulInfoTag" });

app.MapControllerRoute(
    name: "usefulInfoSearch",
    pattern: "faydali-bilgiler/ara",
    defaults: new { controller = "PublicBlog", action = "SearchUsefulInfo" });

app.MapControllerRoute(
    name: "usefulInfoIndex",
    pattern: "faydali-bilgiler",
    defaults: new { controller = "PublicBlog", action = "UsefulInfo" });

// Alternatif route'lar
app.MapControllerRoute(
    name: "usefulInfoAlt",
    pattern: "useful-info",
    defaults: new { controller = "PublicBlog", action = "UsefulInfo" });
// Standart route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Aleris}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();