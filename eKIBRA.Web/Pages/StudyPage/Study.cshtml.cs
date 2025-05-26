using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static System.String;

namespace eKIBRA.Web.Pages.StudyPage;

using Navigation = (string userId, string studySessionId, FlashcardProgress? item);

public class StudyModel : PageModel
{
    private readonly ILogger<StudyModel> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _user;
    private readonly SignInManager<ApplicationUser> _signin;
    private const string NoResult = "No result";
    [TempData] public string StatusMessage { get; set; } = Empty;

    [BindProperty] 
    public StudyViewModel Input { get; set; } = new()
    {
        UserId = Empty,
        StudySessionId = Empty,
        DeckId = Empty,
        FlashcardProgressId = Empty,
        FlashcardId = Empty,
        
        DeckTitle = Empty,
        Question = Empty,
        Answer = Empty,
        
        ShowRevealButton = false,
        ShowAnwserText = false,
        
        Version = Guid.Empty,
        Command = StudyViewModelCommand.Get
    };

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
        StatusMessage = Empty;
        if (id is null) return;
        
        var user = await ValidateUserAuthentication();
        if(user is null) return;
        
        StudyCommandHandlerGateway(await GetFlashcardProgress(user.Id, id));
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            StatusMessage = MessageType.Error
                            + "Invalid Deck. Check the required fields or try entering new values.";
            return Page();
        }
        
        var data = 
            await GetFlashcardProgress(userId: Input.UserId, studySessionId: Input.StudySessionId);
        
        // if diff can use to logic avoid recount
        // Input.Version
        // item.Version
        
        if (data.item is null)
        {
            // msg do somethings
            return Page();
        }
        
        StudyCommandHandlerGateway(data);
        
        return Page();
    }

    private async Task<ApplicationUser?> ValidateUserAuthentication()
    {
        // User authenticated - validation
        if (!_signin.IsSignedIn(User))
        {
            RedirectToPage("/Account/Login", new { area = "Identity" });
            Input = null!;
            return null;
        }
        
        // User retrieve - validation
        var user = await _user.GetUserAsync(User);
        if (user is not null) return user;
        StatusMessage = MessageType.Error + "Your account was not found. Go to [Register] page.";
        Input = null!;
        return null;
    }
    
    private void StudyCommandHandlerGateway(Navigation data)
    {
        switch (Input.Command)
        {
            case StudyViewModelCommand.Get:
                GetStudyCommandHandler(data);
                break;
            case StudyViewModelCommand.Remember:
                break;
            case StudyViewModelCommand.Forgot:
                break;
            case StudyViewModelCommand.Reveal:
                RevealCommandHandler(data);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void GetStudyCommandHandler(Navigation data)
    {
        var (userId, studySessionId, item) = data;
        if (item is null)
        {
            Input = new StudyViewModel
            {
                UserId = userId,
                StudySessionId = studySessionId,
                
                DeckId = Empty,
                FlashcardProgressId = Empty,
                FlashcardId = Empty,

                DeckTitle = NoResult,
                Answer = NoResult,
                Question = NoResult,
                
                ShowRevealButton = true,
                ShowAnwserText = false,
                
                Version = Guid.Empty,
                Command = StudyViewModelCommand.Get
            };
            return;
        }
        var showReveal = item is { Remembers: 0 };
        Input = new StudyViewModel
        {
            UserId = userId,
            StudySessionId = studySessionId,
            DeckId = item.LinkedDeck.Id,
            FlashcardId = item.LinkedFlashcard.Id,
            FlashcardProgressId = item.Id,

            DeckTitle = item.LinkedDeck.Title,
            Question = item.LinkedFlashcard.Question,
            Answer = item.LinkedFlashcard.Answer,

            ShowRevealButton = showReveal,
            ShowAnwserText = !showReveal,
            
            Version = item.Version,
            
            Command = StudyViewModelCommand.Get
        };
    }

    private void RevealCommandHandler(Navigation data)
    {
        var (userId, studySessionId, item) = data;
        Input.UserId = userId;
        Input.StudySessionId = studySessionId;                
        
        if (item is null) { return; }
        
        Input.DeckId = item.LinkedDeck.Id;
        Input.FlashcardId = item.LinkedFlashcard.Id;
        Input.FlashcardProgressId = item.Id;
                
        Input.DeckTitle = item.LinkedDeck.Title;
        Input.Question = item.LinkedFlashcard.Question;
        Input.Answer = item.LinkedFlashcard.Answer;
    }


    private async Task<Navigation> GetFlashcardProgress(string userId, string studySessionId)
    {
        var item = await _context.FlashcardsProgress
            .AsNoTracking()
            .Include(i => i.LinkedDeck)
            .Include(i => i.LinkedFlashcard)
            .Where(q => 
                q.UserId == userId
                && q.StudySessionId == studySessionId
                && q.Remembers == 0)
            .OrderBy(q => q.Sequence)
            .ThenByDescending(q=> q.Reveals)
            .FirstOrDefaultAsync();
        
        return new Navigation(userId, studySessionId, item);
    }


}