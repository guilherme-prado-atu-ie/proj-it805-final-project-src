using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.Pages.TestPage
{
    public class ResultsModel : PageModel
    {
        private readonly ILogger<ResultsModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _user;
        private readonly SignInManager<ApplicationUser> _signin;
        public ResultsViewModel Data { get; set; } = new();
        public TestSessionStatistics Stats { get; set; } = new();
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

                var session = await _context.TestSessions
                    .Include(s => s.LinkedDeck)
                    .FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id);

                if (session is null)
                {
                    StatusMessage = MessageType.Warning
                                    + "Test session not found.";
                }

                var allowedStatus = new[] { TestSessionStatus.Completed, TestSessionStatus.Cancelled };
                if (!allowedStatus.Contains(session.Status))
                {
                    return RedirectToPage("./Index");
                }

                Data = new ResultsViewModel
                {
                    TestSessionId = session.Id,
                    Title = session.Title,
                    Description = session.Description ?? string.Empty,
                    Status = session.Status,
                    CompletedDate = session.EndDate ?? session.Modified,
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

        private async Task<TestSessionStatistics> CalculateStatisticsAsync(TestSession session)
        {
            try
            {
                // Get all responses for this session
                var responses = await _context.TestSessionsResponse
                    .Where(r => r.TestSessionId == session.Id && !r.IsDeleted)
                    .ToListAsync();

                var totalTime = 0;
                if (session.StartDate.HasValue && session.EndDate.HasValue)
                {
                    totalTime = (int)(session.EndDate.Value - session.StartDate.Value).TotalSeconds;
                }

                var correctCount = responses.Count(r => r.ResponseType == TestSessionResponseType.Correct);
                var incorrectCount = responses.Count(r => r.ResponseType == TestSessionResponseType.Incorrect);

                var accuracy = responses.Count > 0 ?
                    Math.Round((double)correctCount / responses.Count * 100, 1) : 0;

                var avgTime = responses.Count > 0 ?
                    Math.Round(responses.Average(r => r.ResponseTimeInSeconds), 1) : 0;

                return new TestSessionStatistics
                {
                    SessionId = session.Id,
                    TotalQuestions = responses.Count,
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

        private async Task<List<QuestionResultViewModel>> GetQuestionResultsAsync(TestSession session)
        {
            try
            {
                // Get responses with flashcard data
                var responseData = await _context.TestSessionsResponse
                    .Where(r => r.TestSessionId == session.Id && !r.IsDeleted)
                    .Include(r => r.Flashcard)
                    .OrderBy(r => r.Id) // Order by response order
                    .ToListAsync();

                var results = new List<QuestionResultViewModel>();

                foreach (var response in responseData)
                {
                    if (response.Flashcard == null) continue;

                    // Determine what the user selected
                    var selectedAnswer = await GetSelectedAnswerTextAsync(response);

                    results.Add(new QuestionResultViewModel
                    {
                        QuestionText = response.Flashcard.Question,
                        SelectedAnswer = selectedAnswer,
                        CorrectAnswer = response.Flashcard.Answer,
                        IsCorrect = response.ResponseType == TestSessionResponseType.Correct,
                        TimeInSeconds = response.ResponseTimeInSeconds
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
        private async Task<string> GetSelectedAnswerTextAsync(TestSessionResponse response)
        {
            try
            {
                // If the response was correct, return the correct answer
                if (response.ResponseType == TestSessionResponseType.Correct)
                {
                    return response.Flashcard.Answer;
                }

                if (response.ResponseType == TestSessionResponseType.NotAnswered)
                {
                    return "Not Answered";
                }

                return "Incorrect";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining selected answer for response {ResponseId}", response.Id);
                return "Unknown";
            }
        }
    }
}
