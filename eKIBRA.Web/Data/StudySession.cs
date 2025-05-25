using System.ComponentModel.DataAnnotations;

namespace eKIBRA.Web.Data;

public class StudySession
{
    public virtual required string Id { get; set; }
    public virtual required string DeckId { get; set; }
    public virtual required string UserId { get; set; }
    public virtual StudySessionStatus Status { get; set; } = StudySessionStatus.Created;
    public virtual required List<FlashcardProgress> FlashcardsProgress { get; set; } = [];
    public virtual bool IsDeleted { get; set; }
    public virtual DateTime Created { get; set; } = DateTime.UtcNow;
    public virtual DateTime? Modified { get; set; }
    public virtual string? ModifierUserId { get; set; }
    public virtual Guid Version { get; set; }
    public virtual ApplicationUser? User { get; set; }
    public virtual ApplicationUser? ModifierUser { get; set; }
    public virtual Deck LinkedDeck { get; set; } = null!;
}

/// <summary>
/// Used to control Study Session status
/// </summary>
public enum StudySessionStatus
{
    [Display(Name = "Created", Description = "Created")]
    Created,
    [Display(Name = "In Progress", Description = "In Progress")]
    InProgress,
    [Display(Name = "Completed", Description = "Completed")]
    Completed,
}