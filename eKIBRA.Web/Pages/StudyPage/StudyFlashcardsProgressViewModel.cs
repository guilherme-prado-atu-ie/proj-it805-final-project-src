using eKIBRA.Web.Data;
using eKIBRA.Web.Pages.Shared;

namespace eKIBRA.Web.Pages.StudyPage;

public class StudyFlashcardsProgressViewModel
{
    public PaginatedList<FlashcardProgress> EntityList { get; set; } = new([], 0, 0, 0);
}