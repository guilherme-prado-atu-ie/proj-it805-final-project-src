using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;

namespace eKIBRA.Web.Pages.StudyPage;

public class IndexViewModel
{
    public PaginatedList<StudySession> EntityList { get; set; } = new([], 0, 0, 0);
}