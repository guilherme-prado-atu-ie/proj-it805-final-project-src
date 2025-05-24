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
        
        ApplicationUserModelBuilder.SetModel(builder);
        DeckModelBuilder.SetModel(builder);
        FlashcardModelBuilder.SetModel(builder);
    }
    
    public override DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Deck> Decks { get; set; }
    public DbSet<Flashcard> Flashcards { get; set; }
}
