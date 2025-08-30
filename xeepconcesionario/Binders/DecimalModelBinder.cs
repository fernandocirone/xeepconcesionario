using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace xeepconcesionario.Binders
{
    public sealed class DecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));

            var val = ctx.ValueProvider.GetValue(ctx.ModelName);
            if (val == ValueProviderResult.None)
                return Task.CompletedTask;

            ctx.ModelState.SetModelValue(ctx.ModelName, val);

            var raw = val.FirstValue?.Trim();
            if (string.IsNullOrEmpty(raw))
            {
                // Para decimal? permitir null
                if (ctx.ModelType == typeof(decimal?))
                    ctx.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            // Normalización tolerante:
            // - quitamos espacios y separadores de miles
            // - detectamos el último separador (.,) como separador decimal
            // - convertimos a invariant "1234.56"
            raw = raw.Replace(" ", "")
                     .Replace("\u00A0", ""); // NBSP

            // Si tiene ambos, el último define el separador decimal
            int lastDot = raw.LastIndexOf('.');
            int lastComma = raw.LastIndexOf(',');

            char? decimalSep = null;
            if (lastDot >= 0 || lastComma >= 0)
                decimalSep = (lastDot > lastComma) ? '.' : ',';

            if (decimalSep.HasValue)
            {
                // remover todos los separadores de miles (el opuesto al decimal)
                char thousandsSep = decimalSep.Value == '.' ? ',' : '.';
                raw = raw.Replace(thousandsSep.ToString(), "");
                // y unificar el decimal a punto
                if (decimalSep.Value == ',')
                    raw = raw.Replace(',', '.');
            }

            if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var dec))
            {
                if (ctx.ModelType == typeof(decimal))
                    ctx.Result = ModelBindingResult.Success(dec);
                else // decimal?
                    ctx.Result = ModelBindingResult.Success((decimal?)dec);
            }
            else
            {
                ctx.ModelState.TryAddModelError(ctx.ModelName, "Número inválido.");
            }

            return Task.CompletedTask;
        }
    }
}
