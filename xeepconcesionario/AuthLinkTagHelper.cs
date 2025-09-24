using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace xeepconcesionario
{
    [HtmlTargetElement("auth-link")]
    public class AuthLinkTagHelper : TagHelper
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthLinkTagHelper(
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Nombre de la policy que debe cumplir el usuario.
        /// </summary>
        public string Policy { get; set; } = string.Empty;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var user = _httpContextAccessor.HttpContext!.User;
            var result = await _authorizationService.AuthorizeAsync(user, Policy);

            if (!result.Succeeded)
            {
                // No autorizado → no renderiza nada
                output.SuppressOutput();
            }
            else
            {
                // Autorizado → quita el tag <auth-link> y deja solo el contenido
                output.TagName = null;
                output.TagMode = TagMode.StartTagAndEndTag;
            }
        }
    }
}
