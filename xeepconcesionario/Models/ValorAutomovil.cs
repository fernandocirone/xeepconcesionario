using xeepconcesionario;
using xeepconcesionario.Models;
public class ValorPlan
{
    public int ValorPlanId { get; set; } // ✅ Clave primaria
    public int PlanId { get; set; }
    public DateTime FechaValor { get; set; }
    public decimal Valor { get; set; }

    public Plan Plan { get; set; }
}
