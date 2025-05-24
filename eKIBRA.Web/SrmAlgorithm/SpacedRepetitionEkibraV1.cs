using eKIBRA.Web.Data;

namespace eKIBRA.Web.SrmAlgorithm;

public class SpacedRepetitionEKibraV1
{
    public virtual required string Id { get; set; }

    public virtual int GetNextInterval(FlashcardProgress flashcardProgress)
    {
        return 0;
    }
}