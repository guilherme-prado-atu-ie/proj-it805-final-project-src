using eKIBRA.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.SrmAlgorithm;

public interface ISpacedRepetitionImplementation
{
    Task<List<FlashcardProgress>> CreateListOfFlashcardProgress(StudySessionParam input);
}

public class StudySessionParam
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string DeckId { get; set; }
}

public class SpacedRepetitionEKibraV1 : ISpacedRepetitionImplementation
{
    private readonly ILogger<SpacedRepetitionEKibraV1> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _user;
    private readonly SignInManager<ApplicationUser> _signin;

    public SpacedRepetitionEKibraV1(
        ILogger<SpacedRepetitionEKibraV1> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _logger = logger;
        _context = context;
        _user = userManager;
        _signin = signInManager;
    }

    public virtual async Task<List<FlashcardProgress>> CreateListOfFlashcardProgress(StudySessionParam input)
    {
        /*
         * check for new flashcards
         * check for existing study session, for flashcards
         * copy data from previous study session
         * use to set the next interval
         */

        /*
           Id = Guid.NewGuid().           
           UserId = user.Id,
           DeckId = Input.DeckId,
         */

        var tmp = await _context.Decks
            .AsNoTracking()
            .Include(i => i.Flashcards)
            .Where(q => q.UserId == input.UserId && q.Id == input.DeckId)
            .FirstOrDefaultAsync();

        if (tmp is null)
        {
            _logger.LogWarning("Deck not found.");
            return [];
        }

        if (tmp.Flashcards is { Count: 0 })
        {
            _logger.LogWarning("Deck has no flashcards.");
            return [];
        }

        var listOfFlashcardProgress = new List<FlashcardProgress>();
        var sequence = 0;
        var created = DateTime.UtcNow;

        foreach (var item in tmp.Flashcards)
        {
            listOfFlashcardProgress.Add(new FlashcardProgress
            {
                Id = Guid.NewGuid().ToString(),
                StudySessionId = input.Id,
                DeckId = item.DeckId,
                FlashcardId = item.Id,
                UserId = input.UserId,
                Created = created,
                Sequence = ++sequence,
                Forgets = 0,
                ForgetsAcrossSessions = 0,
                Remembers = 0,
                RemembersAcrossSessions = 0,
                Reveals = 0,
                RevealsAcrossSessions = 0,
                Level = DifficultyLevel.Medium,
                SpacedRepetitionInterval = 0,
                NextSpacedRepetitionInterval = 0,
                Version = Guid.NewGuid(),
            });
        }

        return listOfFlashcardProgress;
    }

    public virtual int GetNextInterval(FlashcardProgress flashcardProgress)
    {
        return 0;
    }
}