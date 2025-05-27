using System.Text;
using System.Text.Json;
using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace eKIBRA.Web.Pages.FlashcardPage
{
    public class FlashcardDto
    {
        public string? Question { get; set; }
        public string? Answer { get; set; }
        public string? IncorrectOne { get; set; }
        public string? IncorrectTwo { get; set; }
        public string? IncorrectThree { get; set; }
        public string? IncorrectFour { get; set; }

        public bool IsDuplicated { get; set; }
    }

    public class BulkImportModel : PageModel
    {
        private readonly ILogger<BulkImportModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _user;
        private readonly SignInManager<ApplicationUser> _signin;

        public bool IsDataLoaded { get; set; } = false;
        [TempData] public string StatusMessage { get; set; } = string.Empty;

        [BindProperty]
        public BulkImportViewModel Input { get; set; } = new()
        { DeckId = string.Empty, DeckTitle = string.Empty, CsvFile = null, ValidationErrors = [] };

        public BulkImportModel(
            ILogger<BulkImportModel> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _context = context;
            _user = userManager;
            _signin = signInManager;
        }

        public IActionResult OnGet()
        {
            if (!_signin.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            IsDataLoaded = false;
            StatusMessage = string.Empty;
            HttpContext.Session.SetString("CsvImportData", JsonSerializer.Serialize(new List<FlashcardDto>()));

            return Page();
        }

        public async Task<IActionResult> OnGetSearchDeckAsync(string search)
        {
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
            var query = await _context.Decks
                .AsNoTracking()
                .Where(q =>
                    q.UserId == user.Id
                    && q.Title.Contains(search))
                .Select(s => new { Title = s.Title, Description = s.Description, Display = s.Title, Value = s.Id })
                .OrderBy(o => o.Title)
                .ToListAsync();

            var json = JsonSerializer.Serialize(query);
            return new JsonResult(json);
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (Input.DeckId.IsNullOrEmpty())
            {
                StatusMessage = MessageType.Error
                                + "Invalid Import. Check the required fields or try entering new values.";
                return Page();
            }

            if (!_signin.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            //Retrieve CsvData if it was persisted (e.g., from Session)
            var serializedData = HttpContext.Session.GetString("CsvImportData");
            if (!string.IsNullOrEmpty(serializedData))
            {
                Input.Flashcards = JsonSerializer.Deserialize<List<FlashcardDto>>(serializedData) ?? new List<FlashcardDto>();
            }

            if (Input.Flashcards == null || !Input.Flashcards.Any())
            {
                StatusMessage = "No data to save. Please import a CSV file first.";
                return Page();
            }

            // User retrieve - validation
            var user = await _user.GetUserAsync(User);
            if (user is null)
            {
                StatusMessage = MessageType.Error + "Your account was not found. Go to [Register] page.";
                return Page();
            }

            var newFlashcards = new List<Flashcard>();

            foreach (var flashcard in Input.Flashcards)
            {
                if (flashcard.Question.IsNullOrEmpty() || flashcard.Answer.IsNullOrEmpty() || flashcard.IsDuplicated)
                {
                    continue;
                }

                var incorrects = new[]
                        { flashcard.IncorrectOne, flashcard.IncorrectTwo, flashcard.IncorrectThree, flashcard.IncorrectFour }
                    .Where(q => !string.IsNullOrWhiteSpace(q))
                    .ToList();

                var data = new Flashcard
                {
                    Id = Guid.NewGuid()
                        .ToString(),
                    UserId = user.Id,
                    DeckId = Input.DeckId,
                    Question = flashcard.Question!,
                    Answer = flashcard.Answer!,
                    Incorrects = incorrects,
                };

                newFlashcards.Add(data);
            }

            await _context.Flashcards.AddRangeAsync(newFlashcards);

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
             */
            StatusMessage = MessageType.Success + "Flashcards created.";
            return Page();
        }

        public async Task<IActionResult> OnPostImportAsync()
        {
            if (!ModelState.IsValid)
            {
                StatusMessage = MessageType.Error
                                + "Invalid Import. Check the required fields or try entering new values.";
                return Page();
            }

            if (!_signin.IsSignedIn(User))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            if (Input.CsvFile is null || Input.CsvFile.Length is 0)
            {
                StatusMessage = MessageType.Warning + "File is empty.";
                return Page();
            }

            if (!Input.CsvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                StatusMessage = MessageType.Warning + "Please select a valid CSV file.";
                return Page();
            }

            if (Input.CsvFile.Length > 10 * 1024 * 1024) // 10 MB
            {
                StatusMessage = MessageType.Warning + "File size exceeds the 10MB limit.";
                return Page();
            }

            // User retrieve - validation
            var user = await _user.GetUserAsync(User);
            if (user is null)
            {
                StatusMessage = MessageType.Error + "Your account was not found. Go to [Register] page.";
                return Page();
            }

            var userFlashcards = await _context.Flashcards.AsNoTracking().Where(q =>
                    q.UserId == user.Id && q.DeckId == Input.DeckId)
                .Select(s => new FlashcardDto { Question = s.Question, Answer = s.Answer })
                .ToListAsync();

            Input.Flashcards = new List<FlashcardDto>();

            try
            {
                using (var reader = new StreamReader(Input.CsvFile.OpenReadStream()))
                {
                    string? headerLine = await reader.ReadLineAsync();
                    if (headerLine == null)
                    {
                        StatusMessage = MessageType.Error + "Error: CSV file is empty or has no header.";
                        return Page();
                    }

                    //Header validation
                    string[] expectedHeaders = { "Question", "Answer", "IncorrectOne", "IncorrectTwo", "IncorrectThree", "IncorrectFour" };
                    string[] actualHeaders = headerLine.Split(',').Select(h => h.Trim().Replace("\"", "")).ToArray();
                    if (!expectedHeaders.SequenceEqual(actualHeaders, StringComparer.OrdinalIgnoreCase))
                    {
                        StatusMessage = MessageType.Error + "CSV header does not match the expected format.";
                        return Page();
                    }

                    string? line;
                    int lineNumber = 1;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        lineNumber++;
                        if (string.IsNullOrWhiteSpace(line)) continue; // Skip empty lines

                        string[] values = ParseCsvLine(line);

                        if (values.Length < 2) // At least Question and Answer
                        {
                            StatusMessage = $"Warning: Skipping line {lineNumber} due to insufficient columns.";
                            continue;
                        }

                        //Alert if the question already exists in the user's flashcards
                        var question = values.ElementAtOrDefault(0)?.Trim();
                        var isDuplicated = userFlashcards.Any(f => f.Question == question);

                        var flashcard = new FlashcardDto
                        {
                            Question = values.ElementAtOrDefault(0)?.Trim(),
                            Answer = values.ElementAtOrDefault(1)?.Trim(),
                            IncorrectOne = values.ElementAtOrDefault(2)?.Trim(),
                            IncorrectTwo = values.ElementAtOrDefault(3)?.Trim(),
                            IncorrectThree = values.ElementAtOrDefault(4)?.Trim(),
                            IncorrectFour = values.ElementAtOrDefault(5)?.Trim(),
                            IsDuplicated = isDuplicated
                        };

                        // Skip if question is duplicated on the csv.
                        var csvDuplication = Input.Flashcards.Any(f => f.Question == flashcard.Question);
                        if (!csvDuplication)
                        {
                            Input.Flashcards.Add(flashcard);
                        }
                    }
                }

                if (Input.Flashcards.Any())
                {
                    IsDataLoaded = true;
                    HttpContext.Session.SetString("CsvImportData",
                        JsonSerializer.Serialize(Input.Flashcards));

                    StatusMessage = MessageType.Success
                         + $"Successfully parsed {Input.Flashcards.Count} records from the CSV. Review and click 'Save'.";
                }
                else
                {
                    StatusMessage = MessageType.Warning + "No data found in the CSV file or all lines were skipped.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = MessageType.Error + $"Error processing CSV file: {ex.Message}";
            }

            return Page();
        }

        private string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var currentField = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    // Handle double quotes within a quoted field (e.g., "field with ""quotes"" inside")
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }
            fields.Add(currentField.ToString()); // Add the last field
            return fields.Select(f => f.Trim()).ToArray();
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
                 */
                StatusMessage = MessageType.Warning
                                + $"Cannot create a new Flashcard. Question is duplicated.";
            else
                StatusMessage = MessageType.Error
                                + "Fail to create a new Flashcard.";

            _logger.LogError(e, "An error occurred while creating a new Flashcard.");
            return Page();
        }
    }
}