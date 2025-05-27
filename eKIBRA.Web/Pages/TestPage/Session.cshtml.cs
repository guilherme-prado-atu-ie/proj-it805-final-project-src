using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.Pages.TestPage
{
    public class SessionModel : PageModel
    {
        private readonly ILogger<SessionModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _user;
        private readonly SignInManager<ApplicationUser> _signin;

        [BindProperty] public SessionViewModel Data { get; set; } = new();

        [BindProperty]
        public string SelectedAnswerId { get; set; }
        public string StatusMessage { get; set; } = string.Empty;


        public SessionModel(
            ILogger<SessionModel> logger,
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

                var allowedStatus = new[] { TestSessionStatus.Created, TestSessionStatus.InProgress };
                if (!allowedStatus.Contains(session.Status))
                {
                    return RedirectToPage("./Results", new { id = session.Id });
                }

                // If the session is created, change its status to InProgress
                if (session!.Status == TestSessionStatus.Created)
                {
                    session.Status = TestSessionStatus.InProgress;
                    session.StartDate = DateTime.UtcNow;
                    session.Modified = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                await LoadCurrentQuestionAsync(session);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading test session {Id}", id);
                StatusMessage = "Error loading test session";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostSubmitAnswerAsync(DateTime startTime)
        {
            try
            {
                StatusMessage = string.Empty;

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
                    .FirstOrDefaultAsync(s => s.Id == Data.TestSessionId);

                if (session is null)
                {
                    StatusMessage = MessageType.Warning
                                    + "Test session not found.";

                    return Page();
                }

                // Calculate response time
                var responseTime = (int)(DateTime.UtcNow - startTime).TotalSeconds;

                // Get current flashcard to check answer
                var currentFlashcard = await _context.Flashcards
                    .FirstOrDefaultAsync(f => f.Id == Data.CurrentFlashcardId);

                if (currentFlashcard is null)
                {
                    StatusMessage = MessageType.Warning
                                    + "Flashcard not found.";
                    return Page();
                }

                var isCorrect = IsAnswerCorrect(SelectedAnswerId);

                // Create response record
                var response = new TestSessionResponse
                {
                    Id = Guid.NewGuid().ToString(),
                    TestSessionId = Data.TestSessionId,
                    FlashcardId = Data.CurrentFlashcardId,
                    ResponseType = isCorrect ? TestSessionResponseType.Correct : TestSessionResponseType.Incorrect,
                    ResponseTimeInSeconds = responseTime
                };

                _context.TestSessionsResponse.Add(response);
                await _context.SaveChangesAsync();

                // Check if there are more questions
                var hasMoreQuestions = await HasMoreQuestionsAsync(Data.TestSessionId);

                if (hasMoreQuestions)
                {
                    // Move to next question
                    return RedirectToPage(new { id = Data.TestSessionId });
                }

                // Complete the session
                session.Status = TestSessionStatus.Completed;
                session.EndDate = DateTime.UtcNow;
                session.Modified = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return RedirectToPage("/TestPage/Results", new { id = Data.TestSessionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting answer for session {Id}", Data.TestSessionId);
                StatusMessage = "Error submitting answer";
                await ReloadCurrentQuestion();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostExitTestAsync()
        {
            try
            {
                var session = await _context.TestSessions
                    .FirstOrDefaultAsync(s => s.Id == Data.TestSessionId);

                if (session != null)
                {
                    // Mark session as cancelled
                    session.Status = TestSessionStatus.Cancelled;
                    session.EndDate = DateTime.UtcNow;
                    session.Modified = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                // Redirect to results
                return RedirectToPage("/TestPage/Results", new { sessionId = Data.TestSessionId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exiting test session {SessionId}", Data.TestSessionId);
                StatusMessage = "Error exiting test session";
                return Page();
            }
        }

        private bool IsAnswerCorrect(string selectedAnswerId)
        {
            return selectedAnswerId.StartsWith("correct_");
        }
        private async Task<bool> HasMoreQuestionsAsync(string sessionId)
        {
            var session = await _context.TestSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsDeleted);

            if (session == null) return false;

            var totalFlashcards = await _context.Flashcards
                .CountAsync(f => f.DeckId == session.DeckId && !f.IsDeleted);

            var answeredCount = await _context.TestSessionsResponse
                .CountAsync(r => r.TestSessionId == sessionId && !r.IsDeleted);

            return answeredCount < totalFlashcards;
        }

        private async Task ReloadCurrentQuestion()
        {
            try
            {
                var session = await _context.TestSessions
                    .Include(s => s.LinkedDeck)
                    .FirstOrDefaultAsync(s => s.Id == Data.TestSessionId && !s.IsDeleted);

                if (session != null)
                {
                    await LoadCurrentQuestionAsync(session);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading current question");
            }
        }

        private async Task LoadCurrentQuestionAsync(TestSession session)
        {
            // Get current flashcard (next unanswered one)
            var currentFlashcard = await GetCurrentFlashcardAsync(session.Id, session.DeckId);

            if (currentFlashcard == null)
            {
                // No more questions - redirect to results
                Response.Redirect($"/TestPage/Results?id={session.Id}");
                return;
            }

            // Get session progress
            var answeredCount = await _context.TestSessionsResponse
                .CountAsync(r => r.TestSessionId == session.Id);

            var totalFlashcards = await _context.Flashcards
                .CountAsync(f => f.DeckId == session.DeckId);

            Data = new SessionViewModel
            {
                TestSessionId = session.Id,
                Title = session.Title,
                Description = session.Description ?? string.Empty,
                CurrentFlashcard = answeredCount + 1,
                TotalFlashcards = totalFlashcards,
                ElapsedTimeInSeconds = session.StartDate.HasValue
                    ? (int)(DateTime.UtcNow - session.StartDate.Value).TotalSeconds
                    : 0,
                Question = currentFlashcard.Question,
                CurrentFlashcardId = currentFlashcard.Id,
                Answers = await GenerateAnswerOptionsAsync(currentFlashcard),
                QuestionStartTime = DateTime.UtcNow
            };
        }

        private async Task<Flashcard?> GetCurrentFlashcardAsync(string sessionId, string deckId)
        {
            // Get all flashcards for the deck
            var allFlashcards = await _context.Flashcards
                .Where(f => f.DeckId == deckId)
                .OrderBy(f => f.Id)
                .ToListAsync();

            // Get already answered flashcard IDs
            var answeredFlashcardIds = await _context.TestSessionsResponse
                .Where(r => r.TestSessionId == sessionId)
                .Select(r => r.FlashcardId)
                .ToListAsync();

            // Find first unanswered flashcard
            var currentFlashcard = allFlashcards
                .FirstOrDefault(f => !answeredFlashcardIds.Contains(f.Id));

            return currentFlashcard;
        }

        private async Task<List<AnswerOptionViewModel>> GenerateAnswerOptionsAsync(Flashcard flashcard)
        {
            var answers = new List<AnswerOptionViewModel>();

            // Add correct answer
            answers.Add(new AnswerOptionViewModel
            {
                Id = $"correct_{flashcard.Id}",
                Text = flashcard.Answer,
                IsCorrect = true
            });

            //Get incorrect answers from the flashcard
            foreach (var incorrect in flashcard.Incorrects)
            {
                if (!string.IsNullOrWhiteSpace(incorrect))
                {
                    answers.Add(new AnswerOptionViewModel
                    {
                        Id = $"incorrect_{Guid.NewGuid()}",
                        Text = incorrect,
                        IsCorrect = false
                    });
                }
            }

            if (answers.Count < 5)
            {
                // If not enough incorrect answers, get other flashcards from the same deck for incorrect answers.
                var otherFlashcards = await _context.Flashcards
                    .Where(f => f.DeckId == flashcard.DeckId && f.Id != flashcard.Id && !f.IsDeleted)
                    .Take(4)
                    .ToListAsync();

                foreach (var other in otherFlashcards)
                {
                    answers.Add(new AnswerOptionViewModel
                    {
                        Id = $"incorrect_{Guid.NewGuid()}",
                        Text = other.Answer,
                        IsCorrect = false
                    });
                }
            }

            // If not enough incorrect answers, add some generic ones
            while (answers.Count < 5)
            {
                answers.Add(new AnswerOptionViewModel
                {
                    Id = $"incorrect_{Guid.NewGuid()}",
                    Text = $"Option {answers.Count}",
                    IsCorrect = false
                });
            }

            // Shuffle the answers
            var random = new Random();
            return answers.OrderBy(x => random.Next()).Take(5).ToList();
        }

        public IActionResult HandleCreateException(Exception e)
        {
            if (e is DbUpdateException
                && e.InnerException
                    is SqlException { Number: 547 or 2601 or 2627 })
                /*
                 * Cannot insert duplicate key row in object 'dbo.Flashcards' with unique index 'FlashcardTitle'.
                 * The duplicate key value is (Flashcard 01).
                 *
                 * 547 - Constraint check violation
                 * 2601 - Duplicated key row error
                 * 2627 - Unique constraint error
                 */
                StatusMessage = MessageType.Warning
                                + $"Cannot update this Test session: '{Data.Title}' is duplicated.";
            else
                StatusMessage = MessageType.Error
                                + "Fail to update the existing Test session.";

            _logger.LogError(e, "An error occurred while updating a Test session.");
            return Page();
        }
    }
}