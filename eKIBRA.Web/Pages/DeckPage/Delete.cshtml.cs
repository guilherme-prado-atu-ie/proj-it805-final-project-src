using eKIBRA.Web.Data;
using eKIBRA.Web.Data.Migrations;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.Pages.DeckPage
{
    public class DeleteModel : PageModel
    {
        private readonly ILogger<DeleteModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _user;
        private readonly SignInManager<ApplicationUser> _signin;

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;
        [BindProperty]
        public Deck Input { get; set; } = null!;

        public DeleteModel(
            ILogger<DeleteModel> logger,
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
                .FirstOrDefaultAsync(fd =>
                    fd.Id == id && fd.UserId == user.Id);

            if (data is null)
            {
                StatusMessage = MessageType.Warning
                                 + "The record no longer exists.";
                return Page();
            }

            Input = data;
            return Page();

        }

        public async Task<IActionResult> OnPostAsync(string? id)
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
                .Include(sq => sq.Flashcards)
                .FirstOrDefaultAsync(fd =>
                    fd.Id == id && fd.UserId == user.Id);
            if (data is null)
            {
                StatusMessage = MessageType.Warning
                                 + "The record no longer exists.";
                return Page();
            }

            foreach (var flashcard in data.Flashcards)
            {
                // replacing the Question with a Guid to avoid duplicate key error for soft-deleted items
                flashcard.Question = "Deleted " + flashcard.Id;
                flashcard.IsDeleted = true;
            }

            // replacing the title with a Guid to avoid duplicate key error for soft-deleted items
            data.Title = "Deleted " + data.Id;
            data.IsDeleted = true;

            await _context.SaveChangesAsync();

            Input = null!;

            StatusMessage = MessageType.Success
                             + "The record was removed successfully.";
            return Page();
        }
    }
}
