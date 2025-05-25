using System.ComponentModel.DataAnnotations;

namespace eKIBRA.Web.Data;

public class FlashcardProgress
{
    public virtual required string Id { get; set; }
    public virtual required string StudySessionId { get; set; }

    public virtual required string UserId { get; set; }
    public virtual required string DeckId { get; set; }
    public virtual required string FlashcardId { get; set; }

    #region Core Metadata for SRM (Spaced Repetition Method Implementation)

    /// <summary>
    /// Used to display the Flashcards during the Study Session
    /// It can shift the Flashcards order allowing retries 
    /// </summary>
    public virtual required int Sequence { get; set; }

    /// <summary>
    /// Holds how many times the Flashcard was revealed
    /// during the Study Session
    /// </summary>
    public virtual required int Reveals { get; set; }

    public virtual DateTime? RevealAt { get; set; }

    /// <summary>
    /// Holds how many times the Flashcard was remembered
    /// during the Study Session (it should be 0 or 1)
    /// </summary>
    public virtual required int Remembers { get; set; }

    public virtual DateTime? RememberAt { get; set; }

    /// <summary>
    /// Holds how many times the Flashcard was forgotten
    /// during the Study Session (it should be 0 or n)
    /// </summary>
    public virtual required int Forgets { get; set; }

    public virtual DateTime? ForgetAt { get; set; }

    /// <summary>
    /// Holds the Difficulty Level, where automatically calculated
    /// or informed by the Learner, default is Medium  
    /// </summary>
    public virtual required DifficultyLevel Level { get; set; } = DifficultyLevel.Medium;

    /// <summary>
    /// Holds how many times the Flashcard was revealed
    /// across all past sessions, including the current.
    /// Copied from the previous Study Session
    /// or Zero when it is the 1st Study Session.
    /// Incremented and captured during
    /// the current Study Session from UI's feedback.
    /// </summary>
    public virtual required int RevealsAcrossSessions { get; set; }

    /// <summary>
    /// Holds how many times the Flashcard was remembered
    /// across all past sessions, including the current.
    /// Copied from the previous Study Session
    /// or Zero when it is the 1st Study Session.
    /// Incremented and captured during
    /// the current Study Session from UI's feedback.
    /// </summary>
    public virtual required int RemembersAcrossSessions { get; set; }

    /// <summary>
    /// Holds how many times the Flashcard was forgotten
    /// across all past sessions, including the current.
    /// Copied from the previous Study Session
    /// or Zero when it is the 1st Study Session.
    /// Incremented and captured during
    /// the current Study Session from UI's feedback.
    /// </summary>
    public virtual required int ForgetsAcrossSessions { get; set; }

    /// <summary>
    /// Holds the used Spaced Repetition interval used during the session 
    /// </summary>
    public virtual required int SpacedRepetitionInterval { get; set; }

    /// <summary>
    /// Holds the copy of the Spaced Repetition interval to be used on the next Study Session 
    /// </summary>
    public virtual required int NextSpacedRepetitionInterval { get; set; }

    #endregion

    public virtual DateTime Created { get; set; } = DateTime.UtcNow;
    public virtual DateTime? Modified { get; set; }
    public virtual string? ModifierUserId { get; set; }

    public virtual bool IsDeleted { get; set; }
    public virtual Guid Version { get; set; }

    public virtual StudySession? LinkedStudySession { get; set; }
    public virtual Deck? LinkedDeck { get; set; }
    public virtual Flashcard? LinkedFlashcard { get; set; }

    public virtual ApplicationUser? User { get; set; }
    public virtual ApplicationUser? ModifierUser { get; set; }
}

public enum DifficultyLevel
{
    [Display(Name = "Easy", Description = "Easy")]
    Easy,
    [Display(Name = "Medium", Description = "Medium")]
    Medium,
    [Display(Name = "Hard", Description = "Hard")]
    Hard,
}