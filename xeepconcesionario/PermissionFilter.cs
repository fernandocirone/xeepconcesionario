using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace xeepconcesionario
{
    public class PermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly IAuthorizationService _authorizationService;

        public PermissionFilter(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var action = context.RouteData.Values["action"]?.ToString();
            var controller = context.RouteData.Values["controller"]?.ToString();

            if (string.IsNullOrEmpty(action) || string.IsNullOrEmpty(controller))
                return;

            // Traducimos la acción a permiso
            string? permiso = action.ToLower() switch
            {
                "index" or "details" => $"{controller}.Ver",
                "create" => $"{controller}.Crear",
                "edit" => $"{controller}.Editar",
                "delete" => $"{controller}.Borrar",
                _ => null
            };

            if (permiso != null)
            {
                try
                {
                    var result = await _authorizationService.AuthorizeAsync(context.HttpContext.User, permiso);
                    if (!result.Succeeded)
                    {
                        context.Result = new ForbidResult(); // 403 Forbidden
                    }
                }
                catch (InvalidOperationException)
                {
                    // policy no definida → ignorar y dejar pasar
                }
            }
        }
    }
}
