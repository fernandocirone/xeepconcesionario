namespace xeepconcesionario.Models.Dto
{
    public class ContratoDto
    {
        public int SolicitudId { get; set; }
        public int VehiculoId { get; set; }

        public string? VehiculoPatente { get; set; }
        public string? VehiculoModelo { get; set; }

        public string NombreContrato { get; set; } = "";
        public string? DescripcionContrato { get; set; }
        public int? PlazoMeses { get; set; }
        public int CantidadCuotas { get; set; }
        public decimal MontoCuota { get; set; }
        public decimal ValorTransferencia { get; set; }
    }


}
