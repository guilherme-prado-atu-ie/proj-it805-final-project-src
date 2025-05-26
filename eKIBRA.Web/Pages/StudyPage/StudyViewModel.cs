namespace eKIBRA.Web.Pages.StudyPage;

public class StudyViewModel
{
    public required string UserId { get; set; }= string.Empty;
    public required string StudySessionId { get; set; }= string.Empty;
    public required string DeckId { get; set; }= string.Empty;
    public required string FlashcardProgressId { get; set; }= string.Empty;
    public required string FlashcardId { get; set; }= string.Empty;

    public  string DeckTitle { get; set; }= string.Empty;
    public string Question { get; set; }= string.Empty;
    public string Answer { get; set; } = string.Empty;
    
    public bool ShowRevealButton { get; set; }
    public bool ShowAnwserText { get; set; }

    public required StudyViewModelCommand Command { get; set; } = StudyViewModelCommand.Get;
    
    public required Guid Version { get; set; } = Guid.Empty;
}

public enum StudyViewModelCommand
{
    Get,
    Remember,
    Forgot,
    Reveal,
}