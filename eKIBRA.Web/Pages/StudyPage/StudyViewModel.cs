using eKIBRA.Web.Data;

namespace eKIBRA.Web.Pages.StudyPage;

public class StudyViewModel
{
    public required string UserId { get; set; } = string.Empty;
    public required string StudySessionId { get; set; } = string.Empty;
    public required string DeckId { get; set; } = string.Empty;
    public required string FlashcardProgressId { get; set; } = string.Empty;
    public required string FlashcardId { get; set; } = string.Empty;
    public string DeckTitle { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public bool NoQuestionLeft { get; set; }
    public bool ShowRevealButton { get; set; }
    public bool ShowAnwserText { get; set; }
    public int TotalOfQuestions { get; set; }
    public int QuestionsProgress { get; set; }
    public required StudyViewModelCommand Command { get; set; } = StudyViewModelCommand.Get;
    public Guid VersionReveal { get; set; } = Guid.Empty;
    public Guid VersionFeedback { get; set; } = Guid.Empty;
    public required StudyViewModelStatus Status { get; set; } = StudyViewModelStatus.Open;
    
    public required DifficultyLevel Level { get; set; } = DifficultyLevel.Medium;
}   
public enum StudyViewModelStatus
{
    Open,
    InProgress,
    Completed,
}
public enum StudyViewModelCommand
{
    Get,
    Remember,
    Forgot,
    Reveal,
}

public record Navigation(
    string UserId,
    string StudySessionId,
    int QuestionsProgress,
    int TotalOfQuestions,
    FlashcardProgress? Item)
{
    public string UserId { get; } = UserId;
    public string StudySessionId { get; } = StudySessionId;
    public int QuestionsProgress { get; } = QuestionsProgress;
    public int TotalOfQuestions { get; } = TotalOfQuestions;
    public FlashcardProgress? Item { get; } = Item;
}