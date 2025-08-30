using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace xeepconcesionario.Binders
{
    public sealed class DecimalModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var t = context.Metadata.UnderlyingOrModelType;
            if (t == typeof(decimal) || t == typeof(decimal?))
                return new DecimalModelBinder();

            return null;
        }
    }
}
