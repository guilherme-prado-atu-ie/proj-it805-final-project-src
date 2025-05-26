namespace eKIBRA.Web.Data;

public class Deck
{
    public virtual required string Id { get; set; }
    public virtual required string UserId { get; set; }
    public virtual required string Title { get; set; }
    public virtual string? Description { get; set; }
    public virtual DateTime Created { get; set; } = DateTime.UtcNow;
    public virtual DateTime? Modified { get; set; }
    public virtual string? ModifierUserId { get; set; }
    public virtual bool IsDeleted { get; set; }
    public virtual List<Flashcard> Flashcards { get; set; } = [];
    public virtual ApplicationUser? User { get; set; }
    public virtual ApplicationUser? ModifierUser { get; set; }
}