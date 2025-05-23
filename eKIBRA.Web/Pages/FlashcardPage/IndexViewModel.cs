using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;

namespace eKIBRA.Web.Pages.FlashcardPage;

public class IndexViewModel
{
    public PaginatedList<Flashcard> EntityList { get; set; } = new([], 0, 0, 0);
}