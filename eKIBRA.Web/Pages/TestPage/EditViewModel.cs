using System.ComponentModel.DataAnnotations;
using eKIBRA.Web.Data;

namespace eKIBRA.Web.Pages.TestPage;

public sealed class EditViewModel
{
    public string? Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Modified { get; set; }
    public string DeckId { get; set; }

    [StringLength(450)]
    [Display(Name = "Deck", Description = "Deck associated to the test session.")]
    public required string DeckTitle { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(700)]
    [Display(Name = "Title", Description = "Test session title.")]
    public string Title { get; set; }

    [Required(AllowEmptyStrings = false)]
    [StringLength(700)]
    [Display(Name = "Description", Description = "Test session description.")]
    public string Description { get; set; }

    [Required]
    [EnumDataType(typeof(TestSessionStatus))]
    [Display(Name = "Status", Description = "Current status of the test session.")]
    public TestSessionStatus Status { get; set; }

}