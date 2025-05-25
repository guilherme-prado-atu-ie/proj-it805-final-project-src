using System.ComponentModel.DataAnnotations;

namespace eKIBRA.Web.Pages.StudyPage;

public sealed class CreateViewModel
{
    public string DeckId { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    [StringLength(500)]
    [Display(Name = "Description", Description = "A deck description.")]
    public string Description { get; set; }

}