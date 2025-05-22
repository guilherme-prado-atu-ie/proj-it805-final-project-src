using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;

namespace eKIBRA.Web.Pages.DeckPage;

public class IndexViewModel
{
    public PaginatedList<Deck> EntityList { get; set; } = new([], 0, 0, 0);
}