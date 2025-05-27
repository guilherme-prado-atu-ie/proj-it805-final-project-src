using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

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
                                    + "Test session not found.";
                }

                var allowedStatus = new[] { StudySessionStatus.Completed };
                if (!allowedStatus.Contains(session.Status))
                {
                    return RedirectToPage("./Index");
                }

                Data = new ResultsViewModel
                {
                    StudySessionId = session.Id,
                    Title = session.LinkedDeck.Title,
                    Description = session.LinkedDeck.Description ?? string.Empty,
                    Status = session.Status,
                    CompletedDate =  session.Modified,
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
                    .Where(r => r.StudySessionId == session.Id && !r.IsDeleted)
                    .ToListAsync();

                var totalTime = 0;
                if (session.Modified.HasValue)
                {
                    totalTime = (int)(session.Created - session.Modified.Value).TotalSeconds;
                }

                var correctCount = cardProgress.Count(r => r.Remembers > 0);
                var incorrectCount = cardProgress.Count(r => r.Forgets > 0);
                
                var accuracy = cardProgress.Count > 0 ?
                    Math.Round((double)correctCount / cardProgress.Count * 100, 1) : 0;

                /*var avgTime = cardProgress.Count > 0 ?
                    Math.Round(
                        cardProgress.Average(r => (
                        (r.RevealAt.HasValue 
                            ? r.RevealAt.Value 
                            : r.Created) 
                        - (r.RevealAt.HasValue ? r.RevealAt.Value : r.Created)
                        ).TotalSeconds(), 1) : 0;*/
                
                /*var avgTime = cardProgress.Count > 0 ?
                    Math.Round(cardProgress.Average(r => (r.RememberAt??r.Forgets).Value.Ticks), 1) : 0;*/
                var avgTime = 1;

                return new StudySessionStatistics
                {
                    SessionId = session.Id,
                    TotalQuestions = cardProgress.Count,
                    CorrectAnswers = correctCount,
                    IncorrectAnswers = incorrectCount,
                    AccuracyPercentage = accuracy,
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
                    if (response.LinkedFlashcard == null) continue;

                    // Determine what the user selected
                    var selectedAnswer = await GetSelectedAnswerTextAsync(response);

                    results.Add(new QuestionResultViewModel
                    {
                        QuestionText = response.LinkedFlashcard.Question,
                        SelectedAnswer = selectedAnswer,
                        CorrectAnswer = response.LinkedFlashcard.Answer,
                        IsCorrect = true,
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
        private async Task<string> GetSelectedAnswerTextAsync(FlashcardProgress response)
        {
            try
            {
                // If the response was correct, return the correct answer
                if (response.Remembers > 0)
                {
                    return response.LinkedFlashcard.Answer;
                }

                if (response.Forgets > 0)
                {
                    return "Forgot";
                }

                return "Incorrect";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining selected answer for studied flashcard {ResponseId}", response.Id);
                return "Unknown";
            }
        }
    }
}
