namespace eKIBRA.Web.Pages.TestPage;

public class IndexViewModelFilter
{
    public string TitleSort { get; set; }
    public string DescriptionSort { get; set; }
    public string CreatedSort { get; set; }
    public string ModifiedSort { get; set; }

    public string StatusSort { get; set; }
    public string? TitleFilter { get; set; }
    public string? DescriptionFilter { get; set; }
    public string? StatusFilter { get; set; }
    public string SortedBy { get; set; }
}