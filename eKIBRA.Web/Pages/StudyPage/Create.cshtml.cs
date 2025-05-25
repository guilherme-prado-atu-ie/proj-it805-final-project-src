using System.Text.Json;
using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.Pages.StudyPage
{
    public class CreateModel : PageModel
    {
        private readonly ILogger<CreateModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _user;
        private readonly SignInManager<ApplicationUser> _signin;


        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        [BindProperty]
        public CreateViewModel Input { get; set; } = null!;

        public CreateModel(
            ILogger<CreateModel> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _context = context;
            _user = userManager;
            _signin = signInManager;
        }

        public IActionResult OnGet()
        {
            if (!_signin.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            StatusMessage = string.Empty;
            return Page();
        }

        public async Task<IActionResult> OnGetSearchDeckAsync(string search)
        {
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
            
            var inUse = _context.StudySessions
                .AsNoTracking()
                .Where(q => 
                    q.UserId == user.Id 
                    && q.Status != StudySessionStatus.Completed)
                .Select(s=> new { s.DeckId });
            
            var query = await _context.Decks
                .AsNoTracking()
                .Where(q => 
                    q.UserId == user.Id
                    && q.Title.Contains(search)
                    && !inUse
                        .Any(s => s.DeckId == q.Id))
                .Select(s => new { Title = s.Title, Description = s.Description, Display = s.Title, Value = s.Id })
                .OrderBy(o => o.Display)
                .ToListAsync();

            var json = JsonSerializer.Serialize(query);
            return new JsonResult(json);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                StatusMessage = MessageType.Error
                                + "Invalid Deck. Check the required fields or try entering new values.";
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
            // Check for existing Non-Completed Study Sessions
            var hasNonCompleted = await _context.StudySessions
                .AsNoTracking()
                .AnyAsync(q=> 
                    q.UserId == user.Id 
                    && q.DeckId == Input.DeckId
                    && q.Status != StudySessionStatus.Completed);

            if (hasNonCompleted)
            {
                StatusMessage = MessageType.Warning
                                + "You have a non-completed Study Session. Please complete it before creating a new one.";
                return Page();
            }
            
            var data = new StudySession
            {
                Id = Guid.NewGuid()
                    .ToString(),
                UserId = user.Id,
                DeckId = Input.DeckId,
                Version = Guid.NewGuid(), // to avoid concurrency issues when updating metadata
                FlashcardsProgress = []
            };

            _context.StudySessions.Add(data);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return HandleCreateException(e);
            }
            /*
             * change the behavior to stay on the page
             * and notify the user the record was created
             */
            StatusMessage = MessageType.Success + "Study Session created.";
            return Page();
        }

        public IActionResult HandleCreateException(Exception e)
        {
            if (e is DbUpdateException
                && e.InnerException
                    is SqlException { Number: 547 or 2601 or 2627 })
                /*
                 * Cannot insert a duplicate key row on 'dbo.StudySessions' because of a unique index.
                 * The duplicate key value is (deck 01).
                 *
                 * 547 - Constraint check violation
                 * 2601 - Duplicated key row error
                 * 2627 - Unique constraint error
                 */
                StatusMessage = MessageType.Warning
                                + $"Cannot create a new Study Session with selected Deck '{Input.Description}'." +
                                "The Deck is already in use by another Study Session.";
            else
                StatusMessage = MessageType.Error
                                + "Fail to create a new Study Session.";

            _logger.LogError(e, "An error occurred while creating a new Study Session.");
            return Page();
        }
    }
}
