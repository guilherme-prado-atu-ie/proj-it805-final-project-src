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

    public virtual Guid Version { get; set; }

    public virtual Deck? LinkedDeck { get; set; }
}

/// <summary>
/// Used to control Study Session status
/// </summary>
public enum StudySessionStatus
{
    Created,
    InProgress,
    Completed,
}