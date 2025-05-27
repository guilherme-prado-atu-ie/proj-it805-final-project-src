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

        var deck = await _context.Decks
            .AsNoTracking()
            .Include(i => i.Flashcards)
            .Where(q => q.UserId == input.UserId && q.Id == input.DeckId)
            .FirstOrDefaultAsync();

        if (deck is null)
        {
            _logger.LogWarning("Deck not found.");
            return [];
        }

        if (deck.Flashcards is { Count: 0 })
        {
            _logger.LogWarning("Deck has no flashcards.");
            return [];
        }

        var sequence = 0;
        var created = DateTime.UtcNow;
        var listOfNewFlashcardProgress = deck
            .Flashcards
            .Select(item => 
                new FlashcardProgress
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
                    NextSpacedRepetitionInterval = 1,
                    Version = Guid.NewGuid(),
                })
            .ToList();

        /*
         * track the last study session and bring the list of studied flashcard progress
         */
        var lastStudyDeck = await _context.StudySessions
            .AsNoTracking()
            .Include(i=> i.FlashcardsProgress)
            .Where(q =>
                q.UserId == input.UserId
                && q.DeckId == input.DeckId
                && q.Status == StudySessionStatus.Completed
                && q.Modified != null)
            .Select(s => new { s.Id, s.Modified, s.FlashcardsProgress })
            .OrderByDescending(q => q.Modified)
            .FirstOrDefaultAsync();
        
        if (lastStudyDeck is null)
        {
            _logger.LogWarning("Previous Study Session not found.");
            return listOfNewFlashcardProgress;
        }

        /*
         * copy and evaluate the srm data from the last study session
         */
        foreach (var item in lastStudyDeck.FlashcardsProgress
                     .Where(q=> q.Modified != null))
        {
            // deck can have new flashcards not studied before; those will have the SRM interval of (1 day)
            var newFlashcard = listOfNewFlashcardProgress
                .FirstOrDefault(q => q.FlashcardId == item.FlashcardId);

            // set the default values for the new flashcards
            if (newFlashcard is null) continue;
            
            newFlashcard.ForgetsAcrossSessions = item.ForgetsAcrossSessions;
            newFlashcard.RemembersAcrossSessions = item.RemembersAcrossSessions;
            newFlashcard.RevealsAcrossSessions = item.RevealsAcrossSessions;
            
            newFlashcard.SpacedRepetitionInterval = item.SpacedRepetitionInterval;
            newFlashcard.NextSpacedRepetitionInterval = item.NextSpacedRepetitionInterval;

            /*
             * Check if the next interval is due, so it will be recalculated otherwise keep the existing interval.
             */
            var srmIntervalNext = 
                item.NextSpacedRepetitionInterval > 0
                    ? item.NextSpacedRepetitionInterval
                    : 1;
            if (DateTime.UtcNow.Date <= item.Modified?.AddDays(srmIntervalNext).Date) 
                continue;
            
            /*
             * Shift the SRM interval according to (A. C. Mace) - Double Intervals 
             * Shift the SRM interval according to (Leitner System) - Lower Intervals for difficulty levels. 
             * The sequence where flashcards are harder to remember will be at the top of the list.
             * Decrease the interval based on the difficulty level
             * 
             * Easy - higher interval (3x)
             * Medium - normal interval (double)
             * Hard - lower interval (1)
             */
                
            switch (item.Level)
            {
                case DifficultyLevel.Easy:
                    newFlashcard.SpacedRepetitionInterval = srmIntervalNext;
                    newFlashcard.NextSpacedRepetitionInterval = srmIntervalNext * 3;
                    break;
                case DifficultyLevel.Medium:
                    newFlashcard.SpacedRepetitionInterval = srmIntervalNext;
                    newFlashcard.NextSpacedRepetitionInterval = srmIntervalNext * 2;
                    break;
                case DifficultyLevel.Hard:
                    newFlashcard.SpacedRepetitionInterval = srmIntervalNext;
                    newFlashcard.NextSpacedRepetitionInterval = srmIntervalNext / 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        // appy the new order by and recalculate the sequence based on the new SRM inputs.


        var orderBySrmRule = listOfNewFlashcardProgress
            .OrderByDescending(o => o.SpacedRepetitionInterval)
            .ThenBy(o => o.Level)
            .ThenBy(o => o.ForgetsAcrossSessions);
        
        sequence = 0;
        foreach (var item in orderBySrmRule)
        {
            item.Sequence = ++sequence;
        }
        
        return listOfNewFlashcardProgress;
    }

    public virtual int GetNextInterval(FlashcardProgress flashcardProgress)
    {
        return 0;
    }
}