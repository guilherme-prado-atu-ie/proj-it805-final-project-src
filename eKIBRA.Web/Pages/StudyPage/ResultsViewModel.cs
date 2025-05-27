using eKIBRA.Web.Data;

namespace eKIBRA.Web.Pages.StudyPage;

public class ResultsViewModel
{
    public string StudySessionId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public StudySessionStatus Status { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string DeckId { get; set; }
}
public sealed class StudySessionStatistics
{
    public string SessionId { get; set; }
    public int TotalQuestions { get; set; }

    public int SumOfReveals { get; set; }
    public int RevealsAcrossSessions { get; set; }
    public int SumOfRemember { get; set; }
    public int RemembersAcrossSessions { get; set; }
    public int SumOfForgot { get; set; }
    public int ForgotAcrossSessions { get; set; }

    public double MemoryRetention { get; set; }

    public double MemoryRetentionAcrossSessions { get; set; }

    public double AvgRepetitionInterval { get; set; }
    public double AvgNextRepetitionInterval { get; set; }
    public double AverageTimePerQuestion { get; set; }
    public int TotalTimeInSeconds { get; set; }
}

public class QuestionResultViewModel
{
    public string QuestionText { get; set; } = string.Empty;
    public string RememberForgetSummary { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;

    public int Forgot { get; set; }

    public int Reveal { get; set; }

    public DifficultyLevel Level { get; set; }

    public int Remember { get; set; }

    public int RemembersAcrossSessions { get; set; }
    public int ForgotAcrossSessions { get; set; }
    public int RevealsAcrossSessions { get; set; }

    public double RepetitionInterval { get; set; }
    public double NextRepetitionInterval { get; set; }


    public bool HasRememberAtFirst { get; set; }
    public int TimeInSeconds { get; set; }
    public string FormattedTime => TimeSpan.FromSeconds(TimeInSeconds).ToString(@"mm\:ss");
}