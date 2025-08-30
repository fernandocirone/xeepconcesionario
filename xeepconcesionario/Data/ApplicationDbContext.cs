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

        public DbSet<ApplicationUserTipoUsuario> ApplicationUserTiposUsuario { get; set; }
        public DbSet<TipoUsuario> TiposUsuario => Set<TipoUsuario>();
        public DbSet<Solicitud> Solicitudes { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Plan> Planes { get; set; }
        public DbSet<ValorPlan> ValoresPlan { get; set; }
        public DbSet<Cuota> Cuotas { get; set; }
        public DbSet<Cobro> Cobros { get; set; }
        public DbSet<Contrato> Contratos { get; set; }
        public DbSet<Estado> Estados { get; set; }
        public DbSet<Provincia> Provincias { get; set; }
        public DbSet<Region> Regiones { get; set; }
        public DbSet<Localidad> Localidades { get; set; }
        public DbSet<CondicionVenta> CondicionesVenta { get; set; }
        public DbSet<Cobrador> Cobradores { get; set; }
        public DbSet<TipoBaja> TiposBaja { get; set; }

        public DbSet<ActividadSolicitud> ActividadesSolicitud { get; set; }
        public DbSet<EstadoActividad> EstadosActividad { get; set; }


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
            modelBuilder.Entity<ValorPlan>().HasKey(e => e.ValorPlanId);
            modelBuilder.Entity<Cuota>().HasKey(e => e.CuotaId);
            modelBuilder.Entity<Cobro>().HasKey(e => e.CobroId);
            modelBuilder.Entity<Contrato>().HasKey(e => e.ContratoId);
            modelBuilder.Entity<Estado>().HasKey(e => e.EstadoId);
            modelBuilder.Entity<Provincia>().HasKey(e => e.ProvinciaId);
            modelBuilder.Entity<Region>().HasKey(e => e.RegionId);
            modelBuilder.Entity<Localidad>().HasKey(e => e.LocalidadId);
            modelBuilder.Entity<CondicionVenta>().HasKey(e => e.CondicionVentaId);
            modelBuilder.Entity<Cobrador>().HasKey(e => e.CobradorId);
            modelBuilder.Entity<TipoBaja>().HasKey(e => e.TipoBajaId);

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

            modelBuilder.Entity<Cobro>()
                .HasOne(c => c.Cuota)
                .WithMany(q => q.Cobros)
                .HasForeignKey(c => c.CuotaId)   // 👈 aclaramos que la FK es CuotaId
                .OnDelete(DeleteBehavior.Cascade);

            // Solicitud ↔ ApplicationUser (3 FKs diferentes)
            modelBuilder.Entity<Solicitud>()
                .HasOne(s => s.Vendedor)
                .WithMany() // sin colección inversa
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

            // Opcional: semilla de tipos
            modelBuilder.Entity<TipoUsuario>().HasData(
                new TipoUsuario { TipousuarioId = 1, Nombretipousuario = "Vendedor" },
                new TipoUsuario { TipousuarioId = 2, Nombretipousuario = "Supervisor" },
                new TipoUsuario { TipousuarioId = 3, Nombretipousuario = "Jefe de Ventas" }
            );

            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Localidad).WithMany(l => l.Clientes)
                .HasForeignKey(c => c.LocalidadId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Localidad>()
                .HasOne(l => l.Provincia).WithMany(p => p.Localidades)
                .HasForeignKey(l => l.ProvinciaId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Localidad>()
                .HasOne(l => l.Region).WithMany(r => r.Localidades)
                .HasForeignKey(l => l.RegionId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ValorPlan>()
                .HasOne(v => v.Plan).WithMany()
                .HasForeignKey(v => v.PlanId).OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Cobro>()
                .HasOne(c => c.Usuario)
                .WithMany()
                .HasForeignKey(c => c.UsuarioId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);


            // Índice opcional
            // modelBuilder.Entity<Cuota>()
            //     .HasIndex(c => new { c.SolicitudId, c.NumeroCuota })
            //     .IsUnique();
        }

        // Converters concretos (pueden ir en archivos separados si querés)
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
