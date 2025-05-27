using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ApplicationUserModelBuilder<ApplicationUser>.SetModel(builder);
        DeckModelBuilder<Deck>.SetModel(builder);
        FlashcardModelBuilder<Flashcard>.SetModel(builder);
        StudySessionModelBuilder<StudySession>.SetModel(builder);
        FlashcardProgressModelBuilder<FlashcardProgress>.SetModel(builder);
        TestSessionModelBuilder<TestSession>.SetModel(builder);
        TestSessionResponseModelBuilder<TestSessionResponse>.SetModel(builder);
    }

    public override DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Deck> Decks { get; set; }
    public DbSet<Flashcard> Flashcards { get; set; }
    public DbSet<StudySession> StudySessions { get; set; }
    public DbSet<FlashcardProgress> FlashcardsProgress { get; set; }

    public DbSet<TestSession> TestSessions { get; set; }

    public DbSet<TestSessionResponse> TestSessionsResponse { get; set; }

}
