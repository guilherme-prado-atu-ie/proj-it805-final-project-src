using System.ComponentModel.DataAnnotations;

namespace eKIBRA.Web.Pages.DeckPage;

public sealed class CreateViewModel
{
    [Required(AllowEmptyStrings = false)]
    [StringLength(500)]
    [Display(Name = "Title", Description = "A unique deck title.")]
    public string Title { get; set; }
    public string? Description { get; set; }
}