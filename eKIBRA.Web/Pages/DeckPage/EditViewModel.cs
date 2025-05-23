using System.ComponentModel.DataAnnotations;

namespace eKIBRA.Web.Pages.DeckPage;

public class EditViewModel
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(500)]
    [Display(Name = "Title", Description = "A unique deck title.")]
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Modified { get; set; }
    public string? Id { get; set; }
}