using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using xeepconcesionario.Data;
using xeepconcesionario.Models;

public class EditUserModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public EditUserModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public List<SelectListItem> TiposUsuario { get; set; }

    public class InputModel
    {
        public string Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string? NombreCompleto { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }

        public List<int> TiposUsuarioIds { get; set; } = new();

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password), Compare("Password")]
        public string? ConfirmPassword { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var user = await _userManager.Users
            .Include(u => u.TiposUsuario)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound();

        Input = new InputModel
        {
            Id = user.Id,
            Email = user.Email,
            NombreCompleto = user.NombreCompleto,
            Telefono = user.Telefono,
            Direccion = user.Direccion,
            TiposUsuarioIds = user.TiposUsuario.Select(t => t.TipoUsuarioId).ToList()
        };

        TiposUsuario = await _context.TiposUsuario
            .OrderBy(t => t.Nombretipousuario)
            .Select(t => new SelectListItem
            {
                Value = t.TipousuarioId.ToString(),
                Text = t.Nombretipousuario
            })
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            TiposUsuario = await _context.TiposUsuario
                .OrderBy(t => t.Nombretipousuario)
                .Select(t => new SelectListItem
                {
                    Value = t.TipousuarioId.ToString(),
                    Text = t.Nombretipousuario
                })
                .ToListAsync();
            return Page();
        }

        var user = await _userManager.Users
            .Include(u => u.TiposUsuario)
            .FirstOrDefaultAsync(u => u.Id == Input.Id);

        if (user == null)
            return NotFound();

        // Datos básicos
        user.Email = Input.Email;
        user.UserName = Input.Email;
        user.NombreCompleto = Input.NombreCompleto;
        user.Telefono = Input.Telefono;
        user.Direccion = Input.Direccion;

        // Tipos de usuario
        user.TiposUsuario.Clear();
        foreach (var tipoId in Input.TiposUsuarioIds)
        {
            user.TiposUsuario.Add(new ApplicationUserTipoUsuario
            {
                UserId = user.Id,
                TipoUsuarioId = tipoId
            });
        }

        // Guardar cambios de datos
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        // Cambiar contraseña si se ingresó
        if (!string.IsNullOrEmpty(Input.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passResult = await _userManager.ResetPasswordAsync(user, token, Input.Password);
            if (!passResult.Succeeded)
            {
                foreach (var error in passResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }
        }

        return RedirectToPage("Index"); // Página de lista de usuarios
    }
}
