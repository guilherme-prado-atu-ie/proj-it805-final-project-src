using System.ComponentModel.DataAnnotations;

namespace eKIBRA.Web.Data;

public class TestSession
{
    public virtual required string Id { get; set; }

    public virtual string Title { get; set; }

    public virtual string? Description { get; set; }

    public virtual required TestSessionStatus Status { get; set; } = TestSessionStatus.Created;

    public virtual DateTime? StartDate { get; set; }

    public virtual DateTime? EndDate { get; set; }

    public virtual DateTime Created { get; set; } = DateTime.UtcNow;

    public virtual DateTime? Modified { get; set; }

    public virtual bool IsDeleted { get; set; }

    // Navigation properties / Foreign Keys
    public virtual required string DeckId { get; set; }

    public virtual Deck? LinkedDeck { get; set; }

    public virtual required string UserId { get; set; }

    public virtual ApplicationUser? User { get; set; }
}

public enum TestSessionStatus
{
    [Display(Name = "Created", Description = "Created")]
    Created,
    [Display(Name = "In Progress", Description = "In Progress")]
    InProgress,
    [Display(Name = "Completed", Description = "Completed")]
    Completed,
    [Display(Name = "Cancelled", Description = "Cancelled")]
    Cancelled,
}