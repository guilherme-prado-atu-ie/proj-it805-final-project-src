using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.Pages.FlashcardPage
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _user;
        private readonly SignInManager<ApplicationUser> _signin;

        [TempData]
        public string StatusMessage { get; set; } = string.Empty;
        public IndexViewModel Data { get; set; } = new();
        public IndexViewModelFilter Filter { get; set; } = new();

        private readonly int _pageSize;

        public IndexModel(
            ILogger<IndexModel> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _user = userManager;
            _signin = signInManager;
            _pageSize = configuration.GetValue("PageSize", 10);
        }

        public async Task OnGetAsync(
            string sortedBy,
            string titleFilter,
            string questionFilter,
            string? searchTitle,
            string? searchQuestion,
            int? pageIndex)
        {
            StatusMessage = string.Empty;

            // User authenticated - validation
            if (!_signin.IsSignedIn(User))
            {
                RedirectToPage("/Account/Login", new { area = "Identity" });
                Data.EntityList = new PaginatedList<Flashcard>([], 0, 0, 0);
                return;
            }
            // User retrieve - validation
            var user = await _user.GetUserAsync(User);
            if (user is null)
            {
                StatusMessage = MessageType.Error + "Your account was not found. Go to [Register] page.";
                Data.EntityList = new PaginatedList<Flashcard>([], 0, 0, 0);
                return;
            }

            Filter.SortedBy = sortedBy;
            Filter.TitleSort = string.IsNullOrEmpty(sortedBy) ? "title_desc" : "";
            Filter.QuestionSort = sortedBy == "question" ? "question_desc" : "question";
            Filter.AnwserSort = sortedBy == "answer" ? "answer_desc" : "answer";
            Filter.CreatedSort = sortedBy == "created" ? "created_desc" : "created";
            Filter.ModifiedSort = sortedBy == "modified" ? "modified_desc" : "modified";

            if (searchTitle?.Length > 0 || searchQuestion?.Length > 0)
            {
                pageIndex = 1;
            }
            else
            {
                searchTitle = titleFilter;
                searchQuestion = questionFilter;
            }

            Filter.TitleFilter = searchTitle;
            Filter.QuestionFilter = searchQuestion;

            var query = _context.Flashcards
                .AsNoTracking()
                .Include(q => q.LinkedDeck)
                .Where(q => q.UserId == user.Id);

            if (!string.IsNullOrEmpty(searchTitle))
            {
                query = query.Where(q => q.LinkedDeck.Title.Contains(searchTitle));
            }

            if (!string.IsNullOrEmpty(searchQuestion))
            {
                query = query.Where(q => q.Question.Contains(searchQuestion));
            }

            query = sortedBy switch
            {
                "created" => query.OrderBy(q => q.Created),
                "created_desc" => query.OrderByDescending(q => q.Created),

                "modified" => query.OrderBy(q => q.Modified),
                "modified_desc" => query.OrderByDescending(q => q.Modified),

                "question" => query.OrderBy(q => q.Question),
                "question_desc" => query.OrderByDescending(q => q.Question),
                
                "answer" => query.OrderBy(q => q.Answer),
                "answer_desc" => query.OrderByDescending(q => q.Answer),

                "title_desc" => query.OrderByDescending(q => q.LinkedDeck.Title),
                _ => query.OrderBy(q => q.LinkedDeck.Title),
            };

            Data.EntityList = await PaginatedList<Flashcard>.CreateAsync(
                query, pageIndex ?? 1, _pageSize);
        }
    }
}
