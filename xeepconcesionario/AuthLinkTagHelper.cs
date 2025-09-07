using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public string Policy { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var user = _httpContextAccessor.HttpContext!.User;
            var result = await _authorizationService.AuthorizeAsync(user, Policy);

            if (!result.Succeeded)
            {
                output.SuppressOutput(); // oculta el link
            }
        }
    }

}
