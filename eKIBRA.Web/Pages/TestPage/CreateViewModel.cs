using System.ComponentModel.DataAnnotations;

namespace eKIBRA.Web.Pages.TestPage;

public sealed class CreateViewModel
{
    [StringLength(450)]
    [Display(Name = "Deck", Description = "Select a deck to link the flashcard.")]
    public required string DeckTitle { get; set; }
    public string DeckId { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(500)]
    [Display(Name = "Title", Description = "A unique deck title.")]
    public string Title { get; set; }

    public string? Description { get; set; }
}