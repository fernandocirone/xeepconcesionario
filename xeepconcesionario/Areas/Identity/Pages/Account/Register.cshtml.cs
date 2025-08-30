// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using xeepconcesionario.Data;
using xeepconcesionario.Models;

namespace xeepconcesionario.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        // 👇 Combos / datos auxiliares para la vista
        public List<SelectListItem> TiposUsuario { get; set; } = new List<SelectListItem>();
        public List<ApplicationUser> UsuariosExistentes { get; set; } = new List<ApplicationUser>();

        public class InputModel
        {
            [Required, EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar contraseña")]
            [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
            public string ConfirmPassword { get; set; }

            // 👇 Nuevo: selección del tipo funcional del usuario (catálogo TipoUsuario)
            [Required]
            [Display(Name = "Tipos de usuario")]
            public List<int> TiposUsuarioIds { get; set; } = new();


            // Campos opcionales del perfil
            [Display(Name = "Nombre completo")]
            public string NombreCompleto { get; set; }

            [Display(Name = "Teléfono")]
            public string Telefono { get; set; }

            [Display(Name = "Dirección")]
            public string Direccion { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            await LoadCombosAsync();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
            {
                await LoadCombosAsync();
                return Page();
            }

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                NombreCompleto = Input.NombreCompleto,
                Telefono = Input.Telefono,
                Direccion = Input.Direccion,
                TiposUsuario = Input.TiposUsuarioIds
                    .Select(id => new ApplicationUserTipoUsuario
                    {
                        TipoUsuarioId = id
                    }).ToList()

            };

            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            await LoadCombosAsync();
            return Page();
        }

        // --- Helpers privados ---

        private async Task LoadCombosAsync()
        {
            // Catálogo de tipos de usuario (reemplaza Vendedores/Supervisores/JefesVentas antiguos)
            TiposUsuario = await _context.TiposUsuario
                .AsNoTracking()
                .OrderBy(t => t.Nombretipousuario)
                .Select(t => new SelectListItem
                {
                    Value = t.TipousuarioId.ToString(),
                    Text = t.Nombretipousuario
                })
                .ToListAsync();

            // Listado de usuarios existentes (si lo mostrás en la vista)
            UsuariosExistentes = await _userManager.Users
                .AsNoTracking()
                .Include(u => u.TiposUsuario)
                    .ThenInclude(tu => tu.TipoUsuario) // 👈 Cargar el TipoUsuario real
                .OrderBy(u => u.Email)
                .ToListAsync();

        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
                throw new NotSupportedException("The default UI requires a user store with email support.");
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
