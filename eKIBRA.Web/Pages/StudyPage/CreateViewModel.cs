using System.ComponentModel.DataAnnotations;

namespace eKIBRA.Web.Pages.StudyPage;

public sealed class CreateViewModel
{
    public string DeckId { get; set; }

    [StringLength(450)]
    [Display(Name = "Deck", Description = "Select a deck to link with a study session.")]
    public required string DeckTitle { get; set; }

    
    [StringLength(500)]
    [Display(Name = "Description", Description = "A selected deck description.")]
    public string? Description { get; set; }
}