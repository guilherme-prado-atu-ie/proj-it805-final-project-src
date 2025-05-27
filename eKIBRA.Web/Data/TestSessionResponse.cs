using System.ComponentModel.DataAnnotations;

namespace eKIBRA.Web.Data;

public class TestSessionResponse
{
    public virtual required string Id { get; set; }

    public virtual required TestSessionResponseType ResponseType { get; set; } = TestSessionResponseType.NotAnswered;

    public virtual int ResponseTimeInSeconds { get; set; }
    public virtual bool IsDeleted { get; set; }

    // Navigation properties / Foreign Keys
    public virtual required string TestSessionId { get; set; }

    public virtual TestSession? TestSession { get; set; }

    public virtual required string FlashcardId { get; set; }

    public virtual Flashcard? Flashcard { get; set; }
}

public enum TestSessionResponseType
{
    [Display(Name = "Correct", Description = "Correct")]
    Correct,
    [Display(Name = "Incorrect", Description = "Incorrect")]
    Incorrect,
    [Display(Name = "Not Answered", Description = "Not Answered")]
    NotAnswered
}