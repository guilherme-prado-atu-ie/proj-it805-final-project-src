using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.Pages.StudyPage;

public class StudyModel : PageModel
{
    private readonly ILogger<StudyModel> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _user;
    private readonly SignInManager<ApplicationUser> _signin;

    [TempData] public string StatusMessage { get; set; } = string.Empty;

    [BindProperty] public FlashcardProgress Input { get; set; } = null!;

    public StudyModel(
        ILogger<StudyModel> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _logger = logger;
        _context = context;
        _user = userManager;
        _signin = signInManager;
    }

    public async Task OnGetAsync(string? id)
    {
        StatusMessage = string.Empty;

        // User authenticated - validation
        if (!_signin.IsSignedIn(User))
        {
            RedirectToPage("/Account/Login", new { area = "Identity" });
            Input = null!;
            return;
        }

        // User retrieve - validation
        var user = await _user.GetUserAsync(User);
        if (user is null)
        {
            StatusMessage = MessageType.Error + "Your account was not found. Go to [Register] page.";
            Input = null!;
            return;
        }

        var item = await _context.FlashcardsProgress
            .AsNoTracking()
            .Include(i => i.LinkedDeck)
            .Include(i => i.LinkedFlashcard)
            .Where(q => q.StudySessionId == id && q.UserId == user.Id)
            .FirstOrDefaultAsync();

        Input = item ?? null!;

        // add logic to retrieve flashcards here
    }
    
    

    /*
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

        var incorrects = new[]
                { Input.IncorrectOne, Input.IncorrectTwo, Input.IncorrectThree, Input.IncorrectFour }
            .Where(q => !string.IsNullOrWhiteSpace(q))
            .ToList();

        var data = new Flashcard
        {
            Id = Guid.NewGuid()
                .ToString(),
            UserId = user.Id,
            DeckId = Input.DeckId,
            Question = Input.Question,
            Answer = Input.Anwser,
            Incorrects = incorrects
        };

        _context.Flashcards.Add(data);
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
         #1#
        StatusMessage = MessageType.Success + "Flashcard created.";
        return Page();
    }

    public IActionResult HandleCreateException(Exception e)
    {
        if (e is DbUpdateException
            && e.InnerException
                is SqlException { Number: 547 or 2601 or 2627 })
            /*
             * Cannot insert a duplicate key row on 'dbo.Flashcards' because of a unique index.
             * The duplicate key value is (flashcard 01).
             *
             * 547 - Constraint check violation
             * 2601 - Duplicated key row error
             * 2627 - Unique constraint error
             #1#
            StatusMessage = MessageType.Warning
                            + $"Cannot create a new Flashcard. Question '{Input.Question}' is duplicated.";
        else
            StatusMessage = MessageType.Error
                            + "Fail to create a new Flashcard.";

        _logger.LogError(e, "An error occurred while creating a new Flashcard.");
        return Page();
    }*/
}