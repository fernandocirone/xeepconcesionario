using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using xeepconcesionario.Models;

namespace xeepconcesionario.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // EXISTENTES
        public DbSet<ApplicationUserTipoUsuario> ApplicationUserTiposUsuario { get; set; }
        public DbSet<TipoUsuario> TiposUsuario => Set<TipoUsuario>();
        public DbSet<Solicitud> Solicitudes { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Plan> Planes { get; set; }
        public DbSet<Cuota> Cuotas { get; set; }
        public DbSet<Cobro> Cobros { get; set; }
        public DbSet<Contrato> Contratos { get; set; }
        public DbSet<Estado> Estados { get; set; }
        public DbSet<Provincia> Provincias { get; set; }
        public DbSet<Region> Regiones { get; set; }
        public DbSet<Localidad> Localidades { get; set; }
        public DbSet<Sucursal> Sucursales { get; set; }
        public DbSet<CondicionVenta> CondicionesVenta { get; set; }
        public DbSet<TipoBaja> TiposBaja { get; set; }
        public DbSet<ActividadSolicitud> ActividadesSolicitud { get; set; }
        public DbSet<EstadoActividad> EstadosActividad { get; set; }

        // NUEVOS
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<ActividadVehiculo> ActividadesVehiculo { get; set; }
        public DbSet<TipoActividadVehiculo> TiposActividadVehiculo { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
            builder.Properties<DateTime>().HaveConversion<DateTimeUnspecifiedConverter>();
            builder.Properties<DateTime?>().HaveConversion<NullableDateTimeUnspecifiedConverter>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // PKs
            modelBuilder.Entity<Solicitud>().HasKey(e => e.SolicitudId);
            modelBuilder.Entity<Cliente>().HasKey(e => e.ClienteId);
            modelBuilder.Entity<Plan>().HasKey(e => e.PlanId);
            modelBuilder.Entity<Cuota>().HasKey(e => e.CuotaId);
            modelBuilder.Entity<Cobro>().HasKey(e => e.CobroId);
            modelBuilder.Entity<Contrato>().HasKey(e => e.ContratoId);
            modelBuilder.Entity<Estado>().HasKey(e => e.EstadoId);
            modelBuilder.Entity<Provincia>().HasKey(e => e.ProvinciaId);
            modelBuilder.Entity<Region>().HasKey(e => e.RegionId);
            modelBuilder.Entity<Localidad>().HasKey(e => e.LocalidadId);
            modelBuilder.Entity<CondicionVenta>().HasKey(e => e.CondicionVentaId);
            modelBuilder.Entity<TipoBaja>().HasKey(e => e.TipoBajaId);
            modelBuilder.Entity<Sucursal>().HasKey(e => e.Id);

            // --- NUEVOS PKs ---
            modelBuilder.Entity<Vehiculo>().HasKey(v => v.Id);
            modelBuilder.Entity<ActividadVehiculo>().HasKey(av => av.Id);
            modelBuilder.Entity<TipoActividadVehiculo>().HasKey(t => t.Id);

            // TipoUsuario ↔ ApplicationUser (1..N)
            modelBuilder.Entity<ApplicationUserTipoUsuario>()
                .HasKey(ut => new { ut.UserId, ut.TipoUsuarioId });

            modelBuilder.Entity<ApplicationUserTipoUsuario>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.TiposUsuario)
                .HasForeignKey(ut => ut.UserId);

            modelBuilder.Entity<ApplicationUserTipoUsuario>()
                .HasOne(ut => ut.TipoUsuario)
                .WithMany(t => t.Usuarios)
                .HasForeignKey(ut => ut.TipoUsuarioId);

            // Cobro ↔ Cuota
            modelBuilder.Entity<Cobro>()
                .HasOne(c => c.Cuota)
                .WithMany(q => q.Cobros)
                .HasForeignKey(c => c.CuotaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Solicitud ↔ ApplicationUser (varias FKs)
            modelBuilder.Entity<Solicitud>()
                .HasOne(s => s.Vendedor)
                .WithMany()
                .HasForeignKey(s => s.VendedorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Solicitud>()
                .HasOne(s => s.Supervisor)
                .WithMany()
                .HasForeignKey(s => s.SupervisorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Solicitud>()
                .HasOne(s => s.JefeVentas)
                .WithMany()
                .HasForeignKey(s => s.JefeVentasUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Solicitud>()
                .HasOne(s => s.Usuario)
                .WithMany()
                .HasForeignKey(s => s.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Semilla de TiposUsuario
            modelBuilder.Entity<TipoUsuario>().HasData(
                new TipoUsuario { TipousuarioId = 1, Nombretipousuario = "Vendedor" },
                new TipoUsuario { TipousuarioId = 2, Nombretipousuario = "Supervisor" },
                new TipoUsuario { TipousuarioId = 3, Nombretipousuario = "Jefe de Ventas" }
            );

            // Localización
            // Cliente -> Localidad (N..1)
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Localidad)
                .WithMany(l => l.Clientes)
                .HasForeignKey(c => c.LocalidadId)
                .OnDelete(DeleteBehavior.Restrict);

            // Localidad -> Provincia (N..1)
            modelBuilder.Entity<Localidad>()
                .HasOne(l => l.Provincia)
                .WithMany(p => p.Localidades)
                .HasForeignKey(l => l.ProvinciaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Localidad -> Region (N..1)
            modelBuilder.Entity<Localidad>()
                .HasOne(l => l.Region)
                .WithMany(r => r.Localidades)
                .HasForeignKey(l => l.RegionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cobro>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // --------- NUEVAS RELACIONES (Vehículos) ---------

            // Vehiculo ↔ Actividades (1..N)
            modelBuilder.Entity<ActividadVehiculo>()
                .HasOne(av => av.Vehiculo)
                .WithMany(v => v.Actividades)
                .HasForeignKey(av => av.VehiculoId)
                .OnDelete(DeleteBehavior.Restrict); // evita borrar actividades por cascada (ajusta a gusto)

            // ActividadVehiculo ↔ TipoActividadVehiculo (N..1)
            modelBuilder.Entity<ActividadVehiculo>()
                .HasOne(av => av.TipoActividadVehiculo)
                .WithMany() // lookup sin colección
                .HasForeignKey(av => av.TipoActividadVehiculoId)
                .OnDelete(DeleteBehavior.Restrict);

            // ActividadVehiculo ↔ Sucursal (N..1 opcional)
            modelBuilder.Entity<ActividadVehiculo>()
                .HasOne(av => av.Sucursal)
                .WithMany()
                .HasForeignKey(av => av.SucursalId)
                .OnDelete(DeleteBehavior.Restrict);

            // ActividadVehiculo ↔ Usuario (N..1 requerido)
            modelBuilder.Entity<ActividadVehiculo>()
                .HasOne(av => av.Usuario)
                .WithMany()
                .HasForeignKey(av => av.UsuarioId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Índice único opcional para Patente (PostgreSQL: filtra nulos)
            modelBuilder.Entity<Vehiculo>()
                .HasIndex(v => v.Patente)
                .IsUnique()
                .HasFilter("\"Patente\" IS NOT NULL");

            // Ejemplo de índice útil en actividades por fecha
            modelBuilder.Entity<ActividadVehiculo>()
                .HasIndex(av => new { av.VehiculoId, av.Fecha });
        }

        // Converters concretos
        private sealed class DateTimeUnspecifiedConverter : ValueConverter<DateTime, DateTime>
        {
            public DateTimeUnspecifiedConverter()
                : base(
                    v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified))
            { }
        }

        private sealed class NullableDateTimeUnspecifiedConverter : ValueConverter<DateTime?, DateTime?>
        {
            public NullableDateTimeUnspecifiedConverter()
                : base(
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Unspecified) : v,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Unspecified) : v)
            { }
        }
    }
}
