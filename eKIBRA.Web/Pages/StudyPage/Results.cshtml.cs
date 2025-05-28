using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;

namespace eKIBRA.Web.Pages.StudyPage
{
    public class ResultsModel : PageModel
    {
        private readonly ILogger<ResultsModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _user;
        private readonly SignInManager<ApplicationUser> _signin;
        public ResultsViewModel Data { get; set; } = new();
        public StudySessionStatistics Stats { get; set; } = new();
        public List<QuestionResultViewModel> QuestionResults { get; set; } = new();

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;

        public ResultsModel(
            ILogger<ResultsModel> logger,
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
            try
            {
                StatusMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(id))
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
                    StatusMessage = MessageType.Error
                                    + "Your account was not found. Go to [Register] page.";
                    return Page();
                }

                var session = await _context.StudySessions
                    .Include(s => s.LinkedDeck)
                    .FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id);

                if (session is null)
                {
                    StatusMessage = MessageType.Warning
                                    + "Study session not found.";
                    return Page();
                }

                if (session.Status != StudySessionStatus.Completed)
                {
                    return RedirectToPage("./Index");
                }

                Data = new ResultsViewModel
                {
                    StudySessionId = session.Id,
                    Title = session.LinkedDeck.Title,
                    Description = session.LinkedDeck.Description ?? string.Empty,
                    Status = session.Status,
                    CompletedDate = session.Modified,
                    DeckId = session.DeckId
                };

                Stats = await CalculateStatisticsAsync(session);
                QuestionResults = await GetQuestionResultsAsync(session);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading test session {Id}", id);
                StatusMessage = MessageType.Error
                                + "Error loading test session";
                return Page();
            }
        }

        private async Task<StudySessionStatistics> CalculateStatisticsAsync(StudySession session)
        {
            try
            {
                // Get all responses for this session
                var cardProgress = await _context.FlashcardsProgress
                    .Where(r =>
                        r.StudySessionId == session.Id)
                    .ToListAsync();

                var totalTime = 0;
                if (session.Modified.HasValue)
                {
                    totalTime = (int)(session.Created - session.Modified.Value).TotalSeconds;
                }

                var sumOfReveals = cardProgress.Sum(r => r.Reveals);
                var sumOfRemember = cardProgress.Sum(r => r.Remembers);
                var sumOfForgot = cardProgress.Sum(r => r.Forgets);

                var memoryRetentionSimplify = sumOfForgot > 0 ?
                    Math.Round(cardProgress.Count / (double)sumOfReveals * 100, 1) : 100;
                /*
                 * as a question can be remembered only once and a session will be completed only if
                 * all questions were remembered, we can calculate the memory retention across sessions
                 */
                var sumOfRememberAcrossSessions = cardProgress.Sum(r => r.RemembersAcrossSessions);
                var sumOfRevealsAcrossSessions = cardProgress.Sum(r => r.RevealsAcrossSessions);

                var memoryRetentionAcrossSessionsSimplify = sumOfForgot > 0 ?
                    Math.Round(sumOfRememberAcrossSessions / (double)sumOfRevealsAcrossSessions * 100, 1) : 100;

                var avgTime = Math.Round(
                    cardProgress.Average(r => r.RevealAt?.Ticks - r.Modified?.Ticks) ?? 1, 1);

                var avgSpacedRepetitionSession = Math.Round(cardProgress
                    .Average(r => r.SpacedRepetitionInterval), 1);

                var avgSpacedRepetitionNextSession = Math.Round(cardProgress
                    .Average(r => r.NextSpacedRepetitionInterval), 1);

                return new StudySessionStatistics
                {
                    SessionId = session.Id,
                    TotalQuestions = cardProgress.Count,

                    SumOfReveals = sumOfReveals,
                    RevealsAcrossSessions = sumOfRevealsAcrossSessions,

                    SumOfRemember = sumOfRemember,
                    RemembersAcrossSessions = sumOfRememberAcrossSessions,

                    SumOfForgot = sumOfForgot,
                    ForgotAcrossSessions = cardProgress.Sum(r => r.ForgetsAcrossSessions),

                    MemoryRetention = memoryRetentionSimplify,
                    MemoryRetentionAcrossSessions = memoryRetentionAcrossSessionsSimplify,

                    AvgRepetitionInterval = avgSpacedRepetitionSession,
                    AvgNextRepetitionInterval = avgSpacedRepetitionNextSession,

                    AverageTimePerQuestion = avgTime,
                    TotalTimeInSeconds = totalTime
                };
            }
            catch (Exception ex)
            {
                StatusMessage = MessageType.Error
                                + "Error calculating statistics.";

                _logger.LogError(ex, "Error calculating statistics for session {SessionId}", session.Id);
                throw;
            }
        }

        private async Task<List<QuestionResultViewModel>> GetQuestionResultsAsync(StudySession session)
        {
            try
            {
                // Get responses with flashcard data
                var responseData = await _context.FlashcardsProgress
                    .Where(r => r.StudySessionId == session.Id && !r.IsDeleted)
                    .Include(r => r.LinkedFlashcard)
                    .OrderBy(r => r.Id) // Order by response order
                    .ToListAsync();

                var results = new List<QuestionResultViewModel>();

                foreach (var response in responseData)
                {
                    // Determine what the user selected
                    var rememberForgetSummary = GetRememberForgetSummary(response);

                    results.Add(new QuestionResultViewModel
                    {
                        QuestionText = response.LinkedFlashcard.Question,
                        Answer = response.LinkedFlashcard.Answer,

                        Level = response.Level,

                        Reveal = response.Reveals,
                        RevealsAcrossSessions = response.RevealsAcrossSessions,

                        Remember = response.Remembers,
                        RemembersAcrossSessions = response.RemembersAcrossSessions,

                        Forgot = response.Forgets,
                        ForgotAcrossSessions = response.ForgetsAcrossSessions,

                        RepetitionInterval = response.SpacedRepetitionInterval,
                        NextRepetitionInterval = response.NextSpacedRepetitionInterval,

                        RememberForgetSummary = rememberForgetSummary,
                        HasRememberAtFirst = response.Forgets == 0,

                        TimeInSeconds = 1
                    });
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting question results for session {SessionId}", session.Id);
                throw;
            }
        }
        private string GetRememberForgetSummary(FlashcardProgress response)
        {
            try
            {
                var forget = response.Forgets > 1 ? $"{response.Forgets} times" : "once";
                return response.Reveals - response.Remembers == 0
                    ? "Remember at first!"
                    : $"Rememberd after {response.Reveals} Reveals.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining selected answer for studied flashcard {ResponseId}", response.Id);
                return "Unknown";
            }
        }
    }
}
