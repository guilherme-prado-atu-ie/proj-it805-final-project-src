using eKIBRA.Web.Data;

namespace eKIBRA.Web.Pages.TestPage;

public class ResultsViewModel
{
    public string TestSessionId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TestSessionStatus Status { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string DeckId { get; set; }
}
public sealed class TestSessionStatistics
{
    public string SessionId { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int IncorrectAnswers { get; set; }
    public double AccuracyPercentage { get; set; }
    public double AverageTimePerQuestion { get; set; }
    public int TotalTimeInSeconds { get; set; }

    public string FormattedTotalTime => TimeSpan.FromSeconds(TotalTimeInSeconds).ToString(@"mm\:ss");
    public string FormattedAverageTime => TimeSpan.FromSeconds(AverageTimePerQuestion).ToString(@"mm\:ss");
}

public class QuestionResultViewModel
{
    public string QuestionText { get; set; }
    public string SelectedAnswer { get; set; }
    public string CorrectAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public int TimeInSeconds { get; set; }
    public string FormattedTime => TimeSpan.FromSeconds(TimeInSeconds).ToString(@"mm\:ss");
}