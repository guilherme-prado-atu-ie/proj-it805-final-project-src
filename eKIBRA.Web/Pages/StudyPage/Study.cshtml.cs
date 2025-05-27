using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;
using eKIBRA.Web.SrmAlgorithm;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static System.String;

namespace eKIBRA.Web.Pages.StudyPage;

public class StudyModel : PageModel
{
    private readonly ILogger<StudyModel> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _user;
    private readonly SignInManager<ApplicationUser> _signin;
    private readonly ISpacedRepetitionImplementation _srm;
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
        Level = DifficultyLevel.Medium,
        
        ShowRevealButton = false,
        ShowAnwserText = false,

        VersionFeedback = Guid.Empty,
        VersionReveal = Guid.Empty,
        Command = StudyViewModelCommand.Get,
        Status = StudyViewModelStatus.Open
    };

    public StudyModel(
        ILogger<StudyModel> logger,
        ApplicationDbContext context,
        ISpacedRepetitionImplementation srm,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _logger = logger;
        _context = context;
        _srm = srm;
        _user = userManager;
        _signin = signInManager;
    }

    public async Task OnGetAsync(string? id)
    {
        StatusMessage = Empty;
        if (id is null) return;

        var user = await ValidateUserAuthentication();
        if (user is null) return;

        await StudyCommandHandlerGateway(await GetFlashcardProgress(user.Id, id));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        StatusMessage = Empty;

        if (!ModelState.IsValid)
        {
            StatusMessage = MessageType.Error
                            + "Invalid Study Flashcard." +
                            " Try open it from the Study Session list.";
            return Page();
        }

        var data =
            await GetFlashcardProgress(Input.UserId, Input.StudySessionId, Input.FlashcardProgressId);

        if (data.Item is null)
        {
            StatusMessage = MessageType.Info
                            + "Invalid Study Flashcard." +
                            " Try open it from the Study Session list.";

            Input.DeckTitle = NoResult;
            Input.ShowRevealButton = true;
            Input.NoQuestionLeft = true;
            return Page();
        }

        if (data.QuestionsProgress > data.TotalOfQuestions)
        {
            StatusMessage = MessageType.Info
                            + "No Flashcards available for this Study Session." +
                            " Please create a new Study session.";

            Input.DeckTitle = NoResult;
            Input.ShowRevealButton = true;
            Input.NoQuestionLeft = true;
            return Page();
        }

        await StudyCommandHandlerGateway(data);

        switch (Input.Command)
        {
            case StudyViewModelCommand.Get:
            case StudyViewModelCommand.Remember: // RedirectToPage("./Study", new { id = Input.StudySessionId }), //Page(),
            case StudyViewModelCommand.Forgot: //RedirectToPage("./Study", new { id = Input.StudySessionId }), //Page(),
            case StudyViewModelCommand.Reveal:
            default:
                return Page();         //RedirectToPage("/StudyPage/Study", new { id = Input.StudySessionId }) //Page(),
        }

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

    private async Task StudyCommandHandlerGateway(Navigation data)
    {
        switch (Input.Command)
        {
            case StudyViewModelCommand.Get:
                GetStudyCommandHandler(data);
                break;
            case StudyViewModelCommand.Remember:
                await RememberCommandHandler(data);
                break;
            case StudyViewModelCommand.Forgot:
                await ForgotCommandHandler(data);
                break;
            case StudyViewModelCommand.Reveal:
                await RevealCommandHandler(data);
                break;
            default:
                GetStudyCommandHandler(data);
                break;
        }
    }

    private void GetStudyCommandHandler(Navigation data)
    {
        var (userId, studySessionId, questionProgress, totalOfQuestions, item) = data;
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
                QuestionsProgress = 0,
                TotalOfQuestions = 0,
                Answer = NoResult,
                Question = NoResult,

                NoQuestionLeft = true,

                ShowRevealButton = true,
                ShowAnwserText = false,
                Level = DifficultyLevel.Medium,

                Status = StudyViewModelStatus.Open,
                Command = StudyViewModelCommand.Get,

                VersionReveal = Guid.Empty,
                VersionFeedback = Guid.Empty,
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
            QuestionsProgress = questionProgress,
            TotalOfQuestions = totalOfQuestions,

            NoQuestionLeft = questionProgress == 0 && totalOfQuestions == 0,

            Question = item.LinkedFlashcard.Question,
            Answer = item.LinkedFlashcard.Answer,
            Level = item.Level,

            ShowRevealButton = showReveal,
            ShowAnwserText = !showReveal,

            Status = questionProgress > totalOfQuestions
                ? StudyViewModelStatus.Completed
                : StudyViewModelStatus.InProgress,

            Command = StudyViewModelCommand.Get,

            VersionFeedback = item.Version,
            VersionReveal = item.Version,
        };
    }

    private async Task RevealCommandHandler(Navigation data)
    {
        var (userId, studySessionId,
            questionProgress, totalOfQuestions, item) = data;

        if (item is null || questionProgress > totalOfQuestions) { return; }

        Input.UserId = userId;
        Input.StudySessionId = studySessionId;

        var version = Guid.NewGuid();
        var revealAt = DateTime.UtcNow;
        var reveals = item.Reveals + 1;
        var revealsAcrossSessions = item.RevealsAcrossSessions + 1;
        var modifiedUserId = userId;
        var modified = revealAt;

        try
        {
            var affected = await _context.FlashcardsProgress
                .Where(q =>
                    q.UserId == userId
                    && q.Id == Input.FlashcardProgressId
                    && q.Version == Input.VersionReveal)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(v => v.Reveals, reveals)
                    .SetProperty(v => v.RevealAt, revealAt)
                    .SetProperty(v => v.RevealsAcrossSessions, revealsAcrossSessions)
                    .SetProperty(v => v.Modified, modified)
                    .SetProperty(v => v.ModifierUserId, modifiedUserId)
                    .SetProperty(v => v.Version, version));

            if (affected == 0)
            {
                StatusMessage = MessageType.Info
                                + "The answer was revealed." +
                                " Click on 'Remember' or 'Forget' for next the Flashcard.";

            }

            /*
             * Intentionally not updating 'Input.VersionReveal' with the new version.
             * Needed to avoid updating records when accidentally or intentionally post-backing
             * the page through refresh - e.g., pressing F5 or reloading.  
             */

            Input = new StudyViewModel
            {
                // VersionReveal = version,
                VersionFeedback = version,

                DeckId = item.LinkedDeck.Id,
                FlashcardId = item.LinkedFlashcard.Id,
                FlashcardProgressId = item.Id,

                DeckTitle = item.LinkedDeck.Title,

                QuestionsProgress = questionProgress,
                TotalOfQuestions = totalOfQuestions,

                Question = item.LinkedFlashcard.Question,
                Answer = item.LinkedFlashcard.Answer,
                Level = Input.Level,

                Status = questionProgress > totalOfQuestions
                    ? StudyViewModelStatus.Completed
                    : StudyViewModelStatus.InProgress,

                ShowAnwserText = true,
                ShowRevealButton = false,

                UserId = userId,
                StudySessionId = studySessionId,
                Command = StudyViewModelCommand.Reveal,
            };
        }
        catch (Exception e)
        {
            HandleCreateException(e);
        }
    }

    private async Task RememberCommandHandler(Navigation data)
    {
        var (userId, studySessionId,
            questionProgress, totalOfQuestions, item) = data;

        if (item is null || questionProgress > totalOfQuestions) { return; }

        var version = Guid.NewGuid();
        var remembersAt = DateTime.UtcNow;
        var remembers = item.Remembers + 1;
        var remembersAcrossSessions = item.RemembersAcrossSessions + 1;
        var level = Input.Level;
        var modifiedUserId = userId;
        var modified = remembersAt;
        var isStudyCompleted = totalOfQuestions == questionProgress;
        var studyStatus = isStudyCompleted
            ? StudySessionStatus.Completed
            : StudySessionStatus.InProgress;

        try
        {
            var affected = await _context.FlashcardsProgress
                .Where(q =>
                    q.UserId == userId
                    && q.Id == Input.FlashcardProgressId
                    && q.Version == Input.VersionFeedback)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(p => p.Remembers, remembers)
                    .SetProperty(p => p.RememberAt, remembersAt)
                    .SetProperty(p => p.RemembersAcrossSessions, remembersAcrossSessions)
                    .SetProperty(p => p.Level, level)
                    .SetProperty(p => p.Modified, modified)
                    .SetProperty(p => p.ModifierUserId, modifiedUserId)
                    .SetProperty(p => p.Version, version));

            await _context.StudySessions
                .Where(q =>
                    q.UserId == userId
                    && q.Id == studySessionId
                    && q.Status != studyStatus)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(p => p.Modified, modified)
                    .SetProperty(p => p.ModifierUserId, modifiedUserId)
                    .SetProperty(p => p.Status, studyStatus)
                    .SetProperty(p => p.Version, Guid.NewGuid()));

            if (affected == 0)
            {
                StatusMessage = MessageType.Info
                                + "The flashcard is already mark as `Remember`." +
                                " Please wait until the next Flashcard automatically appears.";
                /*
                 * Intentionally not updating 'Input.VersionReveal' and 'Input.VersionFeedback' with the new version.
                 * Needed to avoid updating records when accidentally or intentionally post-backing
                 * the page through refresh - e.g., pressing F5 or reloading.
                 */

                Input = new StudyViewModel
                {
                    DeckId = item.LinkedDeck.Id,
                    FlashcardId = item.LinkedFlashcard.Id,
                    FlashcardProgressId = item.Id,

                    DeckTitle = item.LinkedDeck.Title,

                    QuestionsProgress = questionProgress,
                    TotalOfQuestions = totalOfQuestions,

                    Question = item.LinkedFlashcard.Question,
                    Answer = item.LinkedFlashcard.Answer,
                    Level = Input.Level,

                    Status = questionProgress > totalOfQuestions
                        ? StudyViewModelStatus.Completed
                        : StudyViewModelStatus.InProgress,

                    ShowAnwserText = true,
                    ShowRevealButton = false,

                    UserId = userId,
                    StudySessionId = studySessionId,
                    Command = StudyViewModelCommand.Remember,
                };
                return;
            }

            // move next
            StatusMessage = Empty;
            Input.Command = StudyViewModelCommand.Get;
            await StudyCommandHandlerGateway(await GetFlashcardProgress(userId, studySessionId));
        }
        catch (Exception e)
        {
            HandleCreateException(e);
        }
    }

    private async Task ForgotCommandHandler(Navigation data)
    {
        var (userId, studySessionId,
            questionProgress, totalOfQuestions, item) = data;

        if (item is null || questionProgress > totalOfQuestions) { return; }

        Input.UserId = userId;
        Input.StudySessionId = studySessionId;

        var version = Guid.NewGuid();
        var forgetAt = DateTime.UtcNow;
        var forgets = item.Forgets + 1;
        var forgetsAcrossSessions = item.ForgetsAcrossSessions + 1;
        var level = Input.Level;
        var modifiedUserId = userId;
        var modified = forgetAt;
        var isStudyCompleted = totalOfQuestions == questionProgress;
        var studyStatus = isStudyCompleted
            ? StudySessionStatus.Completed
            : StudySessionStatus.InProgress;

        // Moves to random position for next iteration
        var sequence = Random.Shared.Next(1, totalOfQuestions);

        try
        {
            var affected = await _context.FlashcardsProgress
                .Where(q =>
                    q.UserId == userId
                    && q.Id == Input.FlashcardProgressId
                    && q.Version == Input.VersionFeedback)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(p => p.Sequence, sequence)
                    .SetProperty(p => p.Forgets, forgets)
                    .SetProperty(p => p.ForgetAt, forgetAt)
                    .SetProperty(p => p.ForgetsAcrossSessions, forgetsAcrossSessions)
                    .SetProperty(p => p.Level, level)
                    .SetProperty(p => p.Modified, modified)
                    .SetProperty(p => p.ModifierUserId, modifiedUserId)
                    .SetProperty(p => p.Version, version));

            await _context.StudySessions
                .Where(q =>
                    q.UserId == userId
                    && q.Id == studySessionId
                    && q.Status != studyStatus)
                .ExecuteUpdateAsync(set => set
                    .SetProperty(p => p.Modified, modified)
                    .SetProperty(p => p.ModifierUserId, modifiedUserId)
                    .SetProperty(p => p.Status, studyStatus)
                    .SetProperty(p => p.Version, Guid.NewGuid()));


            if (affected == 0)
            {
                StatusMessage = MessageType.Info
                                + "The flashcard is already mark as `Forgot`." +
                                " Please wait until the next Flashcard automatically appears.";

                /*
                 * Intentionally not updating 'Input.VersionReveal' and 'Input.VersionFeedback' with the new version.
                 * Needed to avoid updating records when accidentally or intentionally post-backing
                 * the page through refresh - e.g., pressing F5 or reloading.
                 */

                Input = new StudyViewModel
                {
                    // VersionReveal = version,
                    // VersionFeedback = version,

                    DeckId = item.LinkedDeck.Id,
                    FlashcardId = item.LinkedFlashcard.Id,
                    FlashcardProgressId = item.Id,

                    DeckTitle = item.LinkedDeck.Title,

                    QuestionsProgress = questionProgress,
                    TotalOfQuestions = totalOfQuestions,

                    Question = item.LinkedFlashcard.Question,
                    Answer = item.LinkedFlashcard.Answer,
                    Level = Input.Level,

                    Status = isStudyCompleted
                        ? StudyViewModelStatus.Completed
                        : StudyViewModelStatus.InProgress,

                    ShowAnwserText = true,
                    ShowRevealButton = false,

                    UserId = userId,
                    StudySessionId = studySessionId,
                    Command = StudyViewModelCommand.Forgot,
                };

                return;
            }

            // move next
            StatusMessage = Empty;
            Input.Command = StudyViewModelCommand.Get;
            await StudyCommandHandlerGateway(await GetFlashcardProgress(userId, studySessionId));
        }
        catch (Exception e)
        {
            HandleCreateException(e);
        }
    }

    private async Task<Navigation> GetFlashcardProgress(string userId, string studySessionId, string? flashcardProgressId = null)
    {
        //_srm.CreateListOfFlashcardProgress();
        
        var totalOfQuestions = await _context.FlashcardsProgress
            .AsNoTracking()
            .Where(q =>
                q.UserId == userId
                && q.StudySessionId == studySessionId)
            .CountAsync();

        var questionsProgress = await _context.FlashcardsProgress
            .AsNoTracking()
            .Where(q =>
                q.UserId == userId
                && q.StudySessionId == studySessionId
                && q.Remembers > 0)
            .CountAsync() + 1;

        var query = _context.FlashcardsProgress
            .AsNoTracking()
            .Include(i => i.LinkedDeck)
            .Include(i => i.LinkedFlashcard)
            .Where(q =>
                q.UserId == userId
                && q.StudySessionId == studySessionId);

        if (!IsNullOrWhiteSpace(flashcardProgressId))
        {
            query = query.Where(q =>
                q.Id == flashcardProgressId);
        }
        else
        {
            query = query.Where(q =>
                    q.Remembers == 0)
                .OrderBy(q => q.Sequence);
            //.ThenByDescending(q => q.Reveals);
        }

        var flashcardProgress = await query.FirstOrDefaultAsync();

        // if zero progress set to the "first question"
        return new Navigation(
            userId,
            studySessionId,
            questionsProgress > 0
                ? questionsProgress
                : 1,
            totalOfQuestions,
            flashcardProgress);
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
                            + "Cannot update the current Flashcard. Please try again later.";
        else
            StatusMessage = MessageType.Error
                            + "Fail to update the current Flashcard. Please try again later.";

        _logger.LogError(e, "An error occurred while updating a record during the Study Session.");
        return Page();
    }

}