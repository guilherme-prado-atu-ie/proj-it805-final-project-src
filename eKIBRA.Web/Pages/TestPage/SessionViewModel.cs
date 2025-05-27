
using System.ComponentModel.DataAnnotations;

namespace eKIBRA.Web.Pages.TestPage;

public sealed class SessionViewModel
{
    public string TestSessionId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int CurrentFlashcard { get; set; }
    public int TotalFlashcards { get; set; }
    public int ElapsedTimeInSeconds { get; set; }
    public string Question { get; set; }
    public List<AnswerOptionViewModel> Answers { get; set; } = new();
    public string CurrentFlashcardId { get; set; }
    public DateTime QuestionStartTime { get; set; } = DateTime.UtcNow;
}

public sealed class AnswerOptionViewModel
{
    public string Id { get; set; }
    public string Text { get; set; }
    public bool IsCorrect { get; set; }
}