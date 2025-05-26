using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.Pages.FlashcardPage
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
        public Flashcard Input { get; set; } = null!;

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

            var data = await _context.Flashcards
                .AsNoTracking()
                .Include(i => i.LinkedDeck)
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

            var data = await _context.Flashcards
                .Where(q => q.Id == id && q.UserId == user.Id)
                .FirstOrDefaultAsync();
            if (data is null)
            {
                StatusMessage = MessageType.Warning
                                 + "The record no longer exists.";
                return Page();
            }

            // check if the deck is in use by any study session
            var inUse = await _context.StudySessions
                .AsNoTracking()
                .Include(i => i.FlashcardsProgress)
                .Where(q =>
                    q.UserId == user.Id
                    && q.DeckId == data.DeckId
                    && q.Status != StudySessionStatus.Completed
                    && q.FlashcardsProgress.Any(p => p.FlashcardId == data.Id))
                .AnyAsync();

            if (inUse)
            {
                StatusMessage = MessageType.Warning
                                + "Cannot change a Flashcard's Deck while any Study Session is not Completed.";
                return Page();
            }

            // replacing the title with a Guid to avoid duplicate key error for soft-deleted items
            data.Modified = DateTime.UtcNow;
            data.ModifierUserId = user.Id;
            data.Question = "Deleted " + data.Id;
            data.Answer = "Deleted " + data.Id;
            data.Incorrects = [];
            data.IsDeleted = true;

            await _context.SaveChangesAsync();

            Input = null!;

            StatusMessage = MessageType.Success
                             + "The record was removed successfully.";
            return Page();
        }
    }
}
