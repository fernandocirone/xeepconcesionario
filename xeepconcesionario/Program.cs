using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;           // 👈 nuevo
using System.Globalization;
using xeepconcesionario.Data;
using xeepconcesionario.Models;
using xeepconcesionario.Binders;
using QuestPDF.Infrastructure; // 👈 nuevo

var builder = WebApplication.CreateBuilder(args);

// Cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddScoped<IReceiptPdfService, ReceiptPdfService>();


// Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// ✅ MVC + nuestro binder primero
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider()); // 👈 importante
});

// ✅ Números con punto (en-US) y UI en español (es-AR)
var numberCulture = new CultureInfo("en-US"); // punto decimal
var uiCulture = new CultureInfo("es-AR"); // textos/fechas UI

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture(
        culture: numberCulture,   // <-- formato para números/fechas/parsing
        uiCulture: uiCulture      // <-- recursos/idioma de la UI
    );

    options.SupportedCultures = new[] { numberCulture }; // parsing/format
    options.SupportedUICultures = new[] { uiCulture };     // textos UI
});


// (Opcional) si querés, podés quitar estas dos líneas porque UseRequestLocalization ya cubre la UI:
// var cultura = new CultureInfo("es-AR");
// CultureInfo.DefaultThreadCurrentCulture = cultura;
// CultureInfo.DefaultThreadCurrentUICulture = cultura;


var app = builder.Build();

// Ejecutar migraciones al iniciar (Prod y Dev)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Pipeline
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

app.UseRequestLocalization(                       // 👈 activar localización
    app.Services.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>().Value);

app.UseAuthentication();                           // 👈 te faltaba
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
