using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;
using Microsoft.AspNetCore.Identity;

namespace eKIBRA.Web.Pages.DeckPage
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
            string descriptionFilter,
            string? searchTitle,
            string? searchDescription,
            int? pageIndex)
        {
            StatusMessage = string.Empty;
            
            // User authenticated - validation
            if (!_signin.IsSignedIn(User))
            {
                RedirectToPage("/Account/Login", new { area = "Identity" });
                Data.EntityList = new PaginatedList<Deck>([], 0,0 ,0);
                return;
            }
            // User retrieve - validation
            var user = await _user.GetUserAsync(User);
            if (user is null)
            {
                StatusMessage = MessageType.Error + "Your account was not found. Go to [Register] page.";
                Data.EntityList = new PaginatedList<Deck>([], 0,0 ,0);
                return;
            }
            
            Filter.SortedBy = sortedBy;
            Filter.TitleSort = string.IsNullOrEmpty(sortedBy) ? "title_desc" : "";
            Filter.DescriptionSort = sortedBy == "Description" ? "description_desc" : "Description";
            Filter.CreatedSort = sortedBy == "Created" ? "created_desc" : "Created";
            Filter.ModifiedSort = sortedBy == "Modified" ? "modified_desc" : "Modified";

            if (searchTitle?.Length > 0 || searchDescription?.Length > 0)
            { 
                pageIndex = 1;
            }
            else
            {
                searchTitle = titleFilter;
                searchDescription = descriptionFilter;
            }

            Filter.TitleFilter = searchTitle;
            Filter.DescriptionFilter = searchDescription;

            var query = _context.Decks
                .AsNoTracking()
                .Where(q=>q.UserId == user.Id);
            
            if (!string.IsNullOrEmpty(searchTitle))
            {
                query = query.Where(q => q.Title.Contains(searchTitle));
            }
            
            if (!string.IsNullOrEmpty(searchDescription))
            {
                query = query.Where(q => q.Description != null && q.Description.Contains(searchDescription));
            }
            
            query = sortedBy switch
            {
                "Created" => query.OrderBy(q => q.Created),
                "created_desc" => query.OrderByDescending(q => q.Created),

                "Modified" => query.OrderBy(q => q.Modified),
                "modified_desc" => query.OrderByDescending(q => q.Modified),
                
                "Description" => query.OrderBy(q => q.Description),
                "description_desc" => query.OrderByDescending(q => q.Description),

                "title_desc" => query.OrderByDescending(q => q.Title),
                _ => query.OrderBy(q => q.Title),
            };
            
            Data.EntityList = await PaginatedList<Deck>.CreateAsync(
                query, pageIndex ?? 1, _pageSize);
        }
    }
}
