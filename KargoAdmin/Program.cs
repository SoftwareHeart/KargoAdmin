using KargoAdmin.Data;
using KargoAdmin.Models;
using KargoAdmin.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// DB Context ekleyin
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity servislerini ekleyin
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    // Password policy (adjust as needed)
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    // Lockout policy
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// MVC ekleyin
builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization(options =>
{
    // Sadece Admin rolündeki kullanıcılar Register sayfasına erişebilir
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
});

// Dil servisi ve session ekleyin
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
builder.Services.AddScoped<ILanguageService, LanguageService>();

// Configure form options (limit upload size)
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 5 * 1024 * 1024; // 5 MB
});

// Configure application cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
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

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Minimal security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

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

// Dil değiştirme route'u
app.MapControllerRoute(
    name: "language",
    pattern: "Language/{action=ChangeLanguage}/{id?}",
    defaults: new { controller = "Language" });
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
    name: "usefulInfoWithSlug",
    pattern: "FaydaliBilgiler/{slug}",
    defaults: new { controller = "UsefulInfo", action = "Details" });

app.MapControllerRoute(
    name: "usefulInfoById",
    pattern: "faydali-bilgiler/id/{id}",
    defaults: new { controller = "UsefulInfo", action = "Details" });

app.MapControllerRoute(
    name: "usefulInfoTag",
    pattern: "faydali-bilgiler/tag/{tag}",
    defaults: new { controller = "UsefulInfo", action = "Tag" });

app.MapControllerRoute(
    name: "usefulInfoSearch",
    pattern: "faydali-bilgiler/ara",
    defaults: new { controller = "UsefulInfo", action = "Search" });

app.MapControllerRoute(
    name: "usefulInfoIndex",
    pattern: "FaydaliBilgiler",
    defaults: new { controller = "UsefulInfo", action = "Index" });

app.MapControllerRoute(
    name: "usefulInfoAlternative",
    pattern: "faydali-bilgiler",
    defaults: new { controller = "UsefulInfo", action = "Index" });

app.MapControllerRoute(
    name: "usefulInfoAlternativeSlug",
    pattern: "faydali-bilgiler/{slug}",
    defaults: new { controller = "UsefulInfo", action = "Details" });

// Alternatif URL'ler (Türkçe karaktersiz)
app.MapControllerRoute(
    name: "usefulInfoAlternative",
    pattern: "faydali-bilgiler",
    defaults: new { controller = "UsefulInfo", action = "Index" });

app.MapControllerRoute(
    name: "usefulInfoAlternativeSlug",
    pattern: "faydali-bilgiler/{slug}",
    defaults: new { controller = "UsefulInfo", action = "Details" });
// Standart route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Aleris}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();