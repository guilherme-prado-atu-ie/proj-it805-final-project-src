using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.Pages.DeckPage
{
    public class EditModel : PageModel
    {
        private readonly ILogger<EditModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _user;
        private readonly SignInManager<ApplicationUser> _signin;
        
        [TempData]
        public string StatusMessage { get; set; } = string.Empty;
        [BindProperty]
        public EditViewModel Input { get; set; } = null!;

        public EditModel(
            ILogger<EditModel> logger,
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
                .Where(q => 
                    q.Id == id && q.UserId == user.Id)
                .Select(s=> 
                    new EditViewModel {
                        Id = s.Id, Title = s.Title, Description = s.Description, 
                        Created = s.Created, Modified = s.Modified})
                .FirstOrDefaultAsync();
            
            if (data is null)
            {
                StatusMessage =  MessageType.Warning 
                                 + "The record no longer exists.";
                return Page();
            }
            
            Input = data;
            return Page();

        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
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
            
            var data = await _context.Decks
                .FirstOrDefaultAsync(fd=> 
                    fd.Id == Input.Id && fd.UserId == user.Id);
            if (data is null)
            {
                StatusMessage =  MessageType.Warning 
                                 + "The record no longer exists.";
                return Page();
            }
            
            data.Title = Input.Title;
            data.Description = Input.Description;
            data.Modified = DateTime.UtcNow;
            data.ModifierUserId = user.Id;
            
            try
            {
                await _context.SaveChangesAsync();
                Input.Modified = data.Modified;
            }
            catch (Exception e)
            {
                return HandleCreateException(e);
            } 
            /*
             * change the behavior to stay on the page
             * and notify the user the record was updated
             */
            StatusMessage = MessageType.Success + "Deck updated.";
            return Page();
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
                                + $"Cannot update this Deck. Title '{Input.Title}' is duplicated.";
            else
                StatusMessage = MessageType.Error
                                + "Fail to update the existing Deck.";
            
            _logger.LogError(e, "An error occurred while updating a Deck.");
            return Page();
        }
    }
}
