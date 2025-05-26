using System.ComponentModel.DataAnnotations;

namespace eKIBRA.Web.Pages.FlashcardPage;

public sealed class CreateViewModel
{
    [StringLength(450)]
    [Display(Name = "Deck", Description = "Select a deck to link the flashcard.")]
    public required string DeckTitle { get; set; }

    public string DeckId { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(700)]
    [Display(Name = "Question", Description = "A unique flashcard question.")]
    public string Question { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(700)]
    [Display(Name = "Anwser", Description = "A flashcard answer.")]
    public string Anwser { get; set; }

    [StringLength(700), MaxLength(700)]
    [Display(Name = "Incorrect (1)", Description = "An incorrect answer to the question (optional).")]
    public string? IncorrectOne { get; set; }

    [StringLength(700), MaxLength(700, ErrorMessage = "Incorrect (2) must be a maximum of 700 characters.")]
    [Display(Name = "Incorrect (2)", Description = "An incorrect answer to the question (optional).")]
    public string? IncorrectTwo { get; set; }

    [StringLength(700), MaxLength(700, ErrorMessage = "Incorrect (3) must be a maximum of 700 characters.")]
    [Display(Name = "Incorrect (3)", Description = "An incorrect answer to the question (optional).")]
    public string? IncorrectThree { get; set; }

    [StringLength(700), MaxLength(700, ErrorMessage = "Incorrect (4) must be a maximum of 700 characters.")]
    [Display(Name = "Incorrect (4)", Description = "An incorrect answer to the question (optional).")]
    public string? IncorrectFour { get; set; }
}