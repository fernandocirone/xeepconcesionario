using System.ComponentModel.DataAnnotations;

namespace xeepconcesionario.Models
{
    public enum TipoVehiculo
    {
        AUTO = 1,
        MOTO = 2
    }

    public enum TipoPlan
    {
        [Display(Name = "Cuotas Fijas")]
        CUOTASFIJAS = 1,

        [Display(Name = "Plan de Ahorro")]
        PLANDEAHORRO = 2
    }
}
