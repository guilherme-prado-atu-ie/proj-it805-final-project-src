using System.ComponentModel.DataAnnotations;
using eKIBRA.Web.Data;

namespace eKIBRA.Web.Pages.FlashcardPage;

public sealed class BulkImportViewModel
{
    [StringLength(450)]
    [Display(Name = "Deck", Description = "Select a deck to link the flashcard.")]
    public required string DeckTitle { get; set; }

    public string DeckId { get; set; }

    [Display(Name = "2. Select CSV File:")]
    public IFormFile? CsvFile { get; set; }

    public List<FlashcardDto>? Flashcards { get; set; } = [];
    public List<string>? ValidationErrors { get; set; }
}