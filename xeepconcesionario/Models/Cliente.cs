using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using xeepconcesionario.Models;

public class Cliente
{
    public int ClienteId { get; set; }
    public string ApellidoYNombre { get; set; }
    public string Dni { get; set; }
    public string? TelefonoFijo { get; set; }
    public string? TelefonoCelular { get; set; }
    public string? Mail { get; set; }

    [Display(Name = "F. Nacimiento")]
    [DataType(DataType.Date)]
    [Column(TypeName = "timestamp")]
    public DateTime? FechaNacimiento { get; set; }
    public string? Direccion { get; set; }
    public string? Nacionalidad { get; set; }
    public int? LocalidadId { get; set; }
    public string? Barrio { get; set; }
    public string? TipoVivienda { get; set; }
    public bool? TieneTarjetaCredito { get; set; } = false;
    public string? Sexo { get; set; }
    public string? EstadoCivil { get; set; }
    public string? Ocupacion { get; set; }
    public string? Empresa { get; set; }
    public string? DomicilioLaboral { get; set; }
    public string? Cargo { get; set; }
    public decimal? IngresosMensuales { get; set; }
    public string? TipoOcupacion { get; set; }
    public string? RazonSocial { get; set; }

    public Localidad? Localidad { get; set; }
    public ICollection<Solicitud>? Solicitudes { get; set; }
}