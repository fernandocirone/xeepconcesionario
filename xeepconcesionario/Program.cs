using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using xeepconcesionario.Data;
using xeepconcesionario.Models;
using xeepconcesionario.Binders;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ==============================
//  DB & servicios de infraestructura
// ==============================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Licencia de QuestPDF (Community)
QuestPDF.Settings.License = LicenseType.Community;

// Tu servicio PDF
builder.Services.AddScoped<IReceiptPdfService, ReceiptPdfService>();

// ==============================
//  Identity (UNA sola vez) + Roles
// ==============================
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        // ⚠️ Opcional (para DEV): relajar password
        // options.Password.RequireDigit = false;
        // options.Password.RequireUppercase = false;
        // options.Password.RequireNonAlphanumeric = false;
        // options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>() // ← habilita roles
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ==============================
//  MVC + Binder de decimales primero
// ==============================
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
});

// ==============================
//  Localización: números en-US y UI es-AR
// ==============================
var numberCulture = new CultureInfo("en-US"); // punto decimal
var uiCulture = new CultureInfo("es-AR"); // textos/fechas UI

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(
        culture: numberCulture, // parsing/formato
        uiCulture: uiCulture    // idioma de UI
    );

    options.SupportedCultures = new[] { numberCulture };
    options.SupportedUICultures = new[] { uiCulture };
});

var app = builder.Build();

// ==============================
//  Migraciones + SEED de admin
// ==============================
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

    const string adminRole = "Admin";
    const string adminEmail = "admin@admin.com";
    const string adminPass = "Admin123!"; // cambiá en Prod

    // Crear rol Admin si no existe
    if (!await roleManager.RoleExistsAsync(adminRole))
        await roleManager.CreateAsync(new IdentityRole(adminRole));

    // Crear usuario admin si no existe
    var user = await userManager.FindByEmailAsync(adminEmail);
    if (user == null)
    {
        user = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var create = await userManager.CreateAsync(user, adminPass);
        if (!create.Succeeded)
        {
            var errors = string.Join(" | ", create.Errors.Select(e => $"{e.Code}:{e.Description}"));
            throw new Exception($"No se pudo crear el usuario admin: {errors}");
        }
    }

    // Asegurar que el admin esté en el rol Admin
    if (!await userManager.IsInRoleAsync(user, adminRole))
        await userManager.AddToRoleAsync(user, adminRole);
}

// ==============================
//  Pipeline HTTP
// ==============================
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

// Localización (activa culturas configuradas)
app.UseRequestLocalization(
    app.Services.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>
    >().Value);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
