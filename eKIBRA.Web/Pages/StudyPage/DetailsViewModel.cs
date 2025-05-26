using eKIBRA.Web.Data;

namespace eKIBRA.Web.Pages.StudyPage;

public class DetailsViewModel
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public StudySessionStatus Status { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Modified { get; set; }
}