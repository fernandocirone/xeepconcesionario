using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using System.Globalization;
using NuGet.Configuration;

public record ReciboCobroDto(
    int CobroId,
    DateTime Fecha,
    string Cliente,
    string? DocumentoCliente,
    string? SolicitudInfo,
    decimal Importe,
    decimal? SaldoAnterior,
    decimal? SaldoActual,
    string? Observacion,
    string? Usuario
);

public interface IReceiptPdfService
{
    byte[] Generar(ReciboCobroDto dto, byte[]? logoBytes = null);
}

public class ReceiptPdfService : IReceiptPdfService
{
    private static readonly CultureInfo Ar = new("es-AR");

    public byte[] Generar(ReciboCobroDto dto, byte[]? logoBytes = null)
    {


        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(25);
                page.DefaultTextStyle(ts => ts.FontSize(10));
                page.Header().Element(c =>
                {
                    c.Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("RECIBO DE COBRO")
                                .Bold().FontSize(16);
                            col.Item().Text($"Nº {dto.CobroId}").SemiBold();
                            col.Item().Text($"Fecha: {dto.Fecha:dd/MM/yyyy HH:mm}");
                        });

                        if (logoBytes is not null)
                        {
                            row.ConstantItem(90).Image(logoBytes);
                        }
                    });
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(6);

                    col.Item().Background(Colors.Grey.Lighten4).Padding(8).Column(c2 =>
                    {
                        c2.Item().Text($"Cliente: {dto.Cliente}").SemiBold();
                        if (!string.IsNullOrWhiteSpace(dto.DocumentoCliente))
                            c2.Item().Text($"Documento: {dto.DocumentoCliente}");
                        if (!string.IsNullOrWhiteSpace(dto.SolicitudInfo))
                            c2.Item().Text($"Solicitud: {dto.SolicitudInfo}");
                    });

                    col.Item().PaddingTop(6).Table(t =>
                    {
                        t.ColumnsDefinition(cd =>
                        {
                            cd.RelativeColumn(3);
                            cd.RelativeColumn(2);
                        });

                        t.Cell().Element(HeaderCell).Text("Concepto");
                        t.Cell().Element(HeaderCell).AlignRight().Text("Importe");

                        t.Cell().Element(NormalCell).Text("Pago recibido");
                        t.Cell().Element(NormalCell).AlignRight().Text(dto.Importe.ToString("C", Ar));


                        static IContainer HeaderCell(IContainer c) =>
                            c.Background(Colors.Grey.Lighten3).Padding(5).DefaultTextStyle(x => x.SemiBold());
                        static IContainer NormalCell(IContainer c) =>
                            c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4);
                        static IContainer FadedCell(IContainer c) =>
                          c.DefaultTextStyle(x => x.FontColor(Colors.Grey.Darken2))
                           .PaddingVertical(3);

                    });

                    if (!string.IsNullOrWhiteSpace(dto.Observacion))
                        col.Item().PaddingTop(10).Text($"Obs: {dto.Observacion}");

                    if (!string.IsNullOrWhiteSpace(dto.Usuario))
                        col.Item().PaddingTop(10).Text($"Atendido por: {dto.Usuario}");

                    col.Item().PaddingTop(14).Text("¡Gracias por su pago!")
                        .AlignCenter();
                });

                page.Footer().AlignCenter().Text(txt =>
                {
                    txt.Span("Este comprobante no reemplaza la factura. ");
                    txt.Span("Sist. xeepconcesionario").Italic();
                });
            });
        }).GeneratePdf();

        return bytes;
    }
}
