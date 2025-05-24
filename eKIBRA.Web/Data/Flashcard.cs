namespace eKIBRA.Web.Data;

public class Flashcard
{
    public virtual required string Id { get; set; }
    public virtual required string DeckId { get; set; }
    public virtual required string UserId { get; set; }
    public virtual required string Question { get; set; }
    public virtual required string Answer { get; set; }
    public virtual required List<string> Incorrects { get; set; } = [];
    public virtual Deck? LinkedDeck { get; set; }
    public virtual DateTime Created { get; set; } = DateTime.UtcNow;
    public virtual DateTime? Modified { get; set; }
    public virtual string? ModifierUserId { get; set; }
    public virtual bool IsDeleted { get; set; }
    public virtual ApplicationUser? User { get; set; }
    public virtual ApplicationUser? ModifierUser { get; set; }

    public virtual string GetIncorrect(int index)
    {
        return Incorrects.Count > index ? Incorrects[index] : string.Empty;
    }
}