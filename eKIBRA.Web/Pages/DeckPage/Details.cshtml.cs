using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.Pages.DeckPage
{
    public class DetailsModel : PageModel
    {
        private readonly ILogger<DetailsModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _user;
        private readonly SignInManager<ApplicationUser> _signin;

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;
        public Deck Input { get; set; } = default!;

        public DetailsModel(
            ILogger<DetailsModel> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _context = context;
            _user = userManager;
            _signin = signInManager;
        }

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            StatusMessage = string.Empty;

            if (id is null)
            {
                StatusMessage = MessageType.Warning
                                + "Required parameter [id] is missing.";
                return Page();
            }
            // User authenticated - validation
            if (!_signin.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            // User retrieve - validation
            var user = await _user.GetUserAsync(User);
            if (user is null)
            {
                StatusMessage = MessageType.Error + "Your account was not found. Go to [Register] page.";
                return Page();
            }
            
            var data = await _context.Decks
                .AsNoTracking()
                .Where(q => q.Id == id && q.UserId == user.Id)
                .FirstOrDefaultAsync();
            if (data is null)
            {
                StatusMessage = MessageType.Warning
                                 + "The record no longer exists.";
                return Page();
            }
            Input = data;
            return Page();

        }
    }
}
