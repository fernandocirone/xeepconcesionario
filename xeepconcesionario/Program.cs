using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using xeepconcesionario.Data;
using xeepconcesionario.Models;
using xeepconcesionario.Binders;
using QuestPDF.Infrastructure;
using System.Security.Claims;
using xeepconcesionario;

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
//  Identity + Roles
// ==============================
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>() // habilita roles
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ==============================
//  Authorization (Policies = permisos)
// ==============================
builder.Services.AddAuthorization(options =>
{
    // ActividadesVehiculo
    options.AddPolicy("ActividadesVehiculo.Ver", p => p.RequireClaim("Permiso", "ActividadesVehiculo.Ver"));
    options.AddPolicy("ActividadesVehiculo.Crear", p => p.RequireClaim("Permiso", "ActividadesVehiculo.Crear"));
    options.AddPolicy("ActividadesVehiculo.Editar", p => p.RequireClaim("Permiso", "ActividadesVehiculo.Editar"));
    options.AddPolicy("ActividadesVehiculo.Borrar", p => p.RequireClaim("Permiso", "ActividadesVehiculo.Borrar"));

    // Clientes
    options.AddPolicy("Clientes.Ver", p => p.RequireClaim("Permiso", "Clientes.Ver"));
    options.AddPolicy("Clientes.Crear", p => p.RequireClaim("Permiso", "Clientes.Crear"));
    options.AddPolicy("Clientes.Editar", p => p.RequireClaim("Permiso", "Clientes.Editar"));
    options.AddPolicy("Clientes.Borrar", p => p.RequireClaim("Permiso", "Clientes.Borrar"));

    // Cobros
    options.AddPolicy("Cobros.Ver", p => p.RequireClaim("Permiso", "Cobros.Ver"));
    options.AddPolicy("Cobros.Crear", p => p.RequireClaim("Permiso", "Cobros.Crear"));
    options.AddPolicy("Cobros.Editar", p => p.RequireClaim("Permiso", "Cobros.Editar"));
    options.AddPolicy("Cobros.Borrar", p => p.RequireClaim("Permiso", "Cobros.Borrar"));

    // Cuotas
    options.AddPolicy("Cuotas.Ver", p => p.RequireClaim("Permiso", "Cuotas.Ver"));
    options.AddPolicy("Cuotas.Crear", p => p.RequireClaim("Permiso", "Cuotas.Crear"));
    options.AddPolicy("Cuotas.Editar", p => p.RequireClaim("Permiso", "Cuotas.Editar"));
    options.AddPolicy("Cuotas.Borrar", p => p.RequireClaim("Permiso", "Cuotas.Borrar"));

    // Planes
    options.AddPolicy("Planes.Ver", p => p.RequireClaim("Permiso", "Planes.Ver"));
    options.AddPolicy("Planes.Crear", p => p.RequireClaim("Permiso", "Planes.Crear"));
    options.AddPolicy("Planes.Editar", p => p.RequireClaim("Permiso", "Planes.Editar"));
    options.AddPolicy("Planes.Borrar", p => p.RequireClaim("Permiso", "Planes.Borrar"));

    // Solicitudes
    options.AddPolicy("Solicitudes.Ver", p => p.RequireClaim("Permiso", "Solicitudes.Ver"));
    options.AddPolicy("Solicitudes.Crear", p => p.RequireClaim("Permiso", "Solicitudes.Crear"));
    options.AddPolicy("Solicitudes.Editar", p => p.RequireClaim("Permiso", "Solicitudes.Editar"));
    options.AddPolicy("Solicitudes.Borrar", p => p.RequireClaim("Permiso", "Solicitudes.Borrar"));

    // Configuraciones
    options.AddPolicy("Configuraciones.Ver", p => p.RequireClaim("Permiso", "Configuraciones.Ver"));
    options.AddPolicy("Configuraciones.Crear", p => p.RequireClaim("Permiso", "Configuraciones.Crear"));
    options.AddPolicy("Configuraciones.Editar", p => p.RequireClaim("Permiso", "Configuraciones.Editar"));
    options.AddPolicy("Configuraciones.Borrar", p => p.RequireClaim("Permiso", "Configuraciones.Borrar"));

    // Vehiculos
    options.AddPolicy("Vehiculos.Ver", p => p.RequireClaim("Permiso", "Vehiculos.Ver"));
    options.AddPolicy("Vehiculos.Crear", p => p.RequireClaim("Permiso", "Vehiculos.Crear"));
    options.AddPolicy("Vehiculos.Editar", p => p.RequireClaim("Permiso", "Vehiculos.Editar"));
    options.AddPolicy("Vehiculos.Borrar", p => p.RequireClaim("Permiso", "Vehiculos.Borrar"));

    // Usuarios
    options.AddPolicy("Usuarios.Ver", p => p.RequireClaim("Permiso", "Usuarios.Ver"));
    options.AddPolicy("Usuarios.Crear", p => p.RequireClaim("Permiso", "Usuarios.Crear"));
    options.AddPolicy("Usuarios.Editar", p => p.RequireClaim("Permiso", "Usuarios.Editar"));
    options.AddPolicy("Usuarios.Borrar", p => p.RequireClaim("Permiso", "Usuarios.Borrar"));

});

// ==============================
//  MVC + Binder de decimales primero
// ==============================
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
    options.Filters.Add<PermissionFilter>();
});

// ==============================
//  Localización: números en-US y UI es-AR
// ==============================
var numberCulture = new CultureInfo("en-US"); // punto decimal
var uiCulture = new CultureInfo("es-AR");     // textos/fechas UI

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
//  Migraciones + SEED de usuarios/roles
// ==============================
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var db = sp.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

    // ---- Admin ----
    const string adminRole = "Admin";
    const string adminEmail = "admin@admin.com";
    const string adminPass = "Admin123!"; // ⚠️ cambiá en producción

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

    // Asignar todos los permisos al rol Admin
    var role = await roleManager.FindByNameAsync(adminRole);
    if (role != null)
    {
        var permisos = new[]
        {
            "ActividadesVehiculo.Ver", "ActividadesVehiculo.Crear", "ActividadesVehiculo.Editar", "ActividadesVehiculo.Borrar",
            "Clientes.Ver", "Clientes.Crear", "Clientes.Editar", "Clientes.Borrar",
            "Cobros.Ver", "Cobros.Crear", "Cobros.Editar", "Cobros.Borrar",
            "Cuotas.Ver", "Cuotas.Crear", "Cuotas.Editar", "Cuotas.Borrar",
            "Planes.Ver", "Planes.Crear", "Planes.Editar", "Planes.Borrar",
            "Solicitudes.Ver", "Solicitudes.Crear", "Solicitudes.Editar", "Solicitudes.Borrar",
            "Configuraciones.Ver", "Configuraciones.Crear", "Configuraciones.Editar", "Configuraciones.Borrar",
            "Vehiculos.Ver", "Vehiculos.Crear", "Vehiculos.Editar", "Vehiculos.Borrar",
             "Usuarios.Ver", "Usuarios.Crear", "Usuarios.Editar", "Usuarios.Borrar"
        };

        var claims = await roleManager.GetClaimsAsync(role);

        foreach (var permiso in permisos)
        {
            if (!claims.Any(c => c.Type == "Permiso" && c.Value == permiso))
            {
                await roleManager.AddClaimAsync(role, new Claim("Permiso", permiso));
            }
        }
    }
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

// Localización
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
