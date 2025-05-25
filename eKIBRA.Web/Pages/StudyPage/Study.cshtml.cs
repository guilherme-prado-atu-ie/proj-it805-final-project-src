using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.Pages.StudyPage;

public class StudyModel : PageModel
{
    private readonly ILogger<StudyModel> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _user;
    private readonly SignInManager<ApplicationUser> _signin;

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    [BindProperty]
    public FlashcardProgress Input { get; set; } = null!;
    public StudyFlashcardsProgressViewModel Data { get; set; } = new();
    public StudyFlashcardsProgressViewModelFilter Filter { get; set; } = new();

    private const int _pageSize = 1;

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
        /*
         * page size is set in appsettings.json
         * however, it is not used in this page
         * configuration.GetValue("PageSize", 1)
         */
    }

    public async Task OnGetAsync(
        string? id,
        string sortedBy,
        string titleFilter,
        string statusFilter,
        string? searchTitle,
        string? searchStatus,
        int? pageIndex)
    {
        StatusMessage = string.Empty;

        // User authenticated - validation
        if (!_signin.IsSignedIn(User))
        {
            RedirectToPage("/Account/Login", new { area = "Identity" });
            Data.EntityList = new PaginatedList<FlashcardProgress>([], 0, 0, 0);
            return;
        }

        // User retrieve - validation
        var user = await _user.GetUserAsync(User);
        if (user is null)
        {
            StatusMessage = MessageType.Error + "Your account was not found. Go to [Register] page.";
            Data.EntityList = new PaginatedList<FlashcardProgress>([], 0, 0, 0);
            return;
        }

        var query = _context.FlashcardsProgress
            .AsNoTracking()
            .Include(i => i.LinkedDeck)
            .Include(i => i.LinkedFlashcard)
            .Where(q => q.StudySessionId == id && q.UserId == user.Id);

        Data.EntityList = await PaginatedList<FlashcardProgress>.CreateAsync(
            query, pageIndex ?? 1, _pageSize);

        Input = Data.EntityList is { Count: 0 }
            ? null!
            : Data.EntityList[0];



        // add logic to retrieve flashcards here
    }
}