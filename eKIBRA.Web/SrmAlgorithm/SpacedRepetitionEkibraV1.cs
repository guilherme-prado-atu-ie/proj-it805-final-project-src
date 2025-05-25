using eKIBRA.Web.Data;

namespace eKIBRA.Web.SrmAlgorithm;


public class SpacedRepetitionEKibraV1Input
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string DeckId { get; set; }
} 

public class SpacedRepetitionEKibraV1
{
    public virtual required string Id { get; set; }

    public virtual List<FlashcardProgress> CreateListOfFlashcardProgress(SpacedRepetitionEKibraV1Input input)
    {
        /*
           Id = Guid.NewGuid().           
           UserId = user.Id,
           DeckId = Input.DeckId,
         */
        
        return new();
    } 

    public virtual int GetNextInterval(FlashcardProgress flashcardProgress)
    {
        return 0;
    }
}