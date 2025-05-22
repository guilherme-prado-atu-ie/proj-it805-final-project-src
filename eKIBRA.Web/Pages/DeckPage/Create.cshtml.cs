using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.Pages.DeckPage
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
        public CreateViewModel Input { get; set; } = new();

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
            var data = new Deck
            {
                Id = Guid.NewGuid()
                    .ToString(),
                UserId = user.Id, 
                Title = Input.Title,
                Description = Input.Description,
            };

            _context.Decks.Add(data);
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
            StatusMessage = MessageType.Success + "Deck created.";
            return Page();
            // return RedirectToPage("./Index");
        }

        public IActionResult HandleCreateException(Exception e)
        {
            if (e is DbUpdateException 
                && e.InnerException 
                    is SqlException { Number: 547 or 2601 or 2627 })
                    /*
                     * Cannot insert duplicate key row in object 'dbo.Decks' with unique index 'DeckTitle'.
                     * The duplicate key value is (deck 01).
                     *
                     * 547 - Constraint check violation
                     * 2601 - Duplicated key row error
                     * 2627 - Unique constraint error
                     */
                StatusMessage = MessageType.Warning
                                + $"Cannot create a new Deck. Title '{Input.Title}' is duplicated.";
            else
                StatusMessage = MessageType.Error
                                + "Fail to create a new Deck.";
            
            _logger.LogError(e, "An error occurred while creating a new Deck.");
            return Page();
        }
    }
}
