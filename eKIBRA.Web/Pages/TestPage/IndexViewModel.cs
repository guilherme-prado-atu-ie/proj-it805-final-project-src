using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;

namespace eKIBRA.Web.Pages.TestPage;

public class IndexViewModel
{
    public PaginatedList<TestSession> EntityList { get; set; } = new([], 0, 0, 0);
}