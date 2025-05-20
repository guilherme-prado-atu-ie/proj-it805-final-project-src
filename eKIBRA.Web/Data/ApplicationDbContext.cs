using System.Text.Json;
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

        #region Application User

        builder.Entity<ApplicationUser>()
            .Property(e => e.IsDeleted);

        #endregion

        #region Deck

        builder.Entity<Deck>()
            .HasKey(k => k.Id);

        builder.Entity<Deck>()
            .Property(p => p.IsDeleted);

        builder.Entity<Deck>()
            .Property(p => p.Title)
            .HasMaxLength(500);
        builder.Entity<Deck>()
            .Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Entity<Deck>()
            .Property(p => p.Created);
        builder.Entity<Deck>()
            .Property(p => p.UserId);

        builder.Entity<Deck>()
            .Property(p => p.Modified);
        builder.Entity<Deck>()
            .Property(p => p.ModifierUserId);

        builder.Entity<Deck>()
            .HasMany(e => e.Flashcards)
            .WithOne(e => e.LinkedDeck)
            .HasForeignKey(f => f.DeckId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Deck>()
            .HasIndex(i => i.Title)
            .IsUnique()
            .HasDatabaseName("DeckTitle")
            .HasFilter("[Title] IS NOT NULL");

        builder.Entity<Deck>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Deck>()
            .HasOne(e => e.ModifierUser)
            .WithMany()
            .HasForeignKey(f => f.ModifierUserId)
            .OnDelete(DeleteBehavior.NoAction);


        #endregion

        #region Flashcard

        builder.Entity<Flashcard>()
            .HasKey(k => k.Id);
        builder.Entity<Flashcard>()
            .Property(k => k.UserId)
            .HasMaxLength(450);
        builder.Entity<Flashcard>()
            .Property(p => p.Question);
        builder.Entity<Flashcard>()
            .Property(p => p.Answer);
        builder.Entity<Flashcard>()
            .Property(p => p.Incorrects)
            .HasConversion(
                input => JsonSerializer.Serialize(input, JsonSerializerOptions.Default),
                output => JsonSerializer.Deserialize<List<string>>(output, JsonSerializerOptions.Default) ?? new());
        builder.Entity<Flashcard>()
            .Property(p => p.Created);
        builder.Entity<Flashcard>()
            .Property(p => p.Modified);
        builder.Entity<Flashcard>()
            .Property(p => p.IsDeleted);

        builder.Entity<Flashcard>()
            .HasIndex(i => i.Question)
            .IsUnique()
            .HasDatabaseName("QuestionText")
            .HasFilter("[Question] IS NOT NULL");

        builder.Entity<Flashcard>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Flashcard>()
            .HasOne(e => e.ModifierUser)
            .WithMany()
            .HasForeignKey(f => f.ModifierUserId)
            .OnDelete(DeleteBehavior.NoAction);

        #endregion
    }

    public override DbSet<ApplicationUser> Users { get; set; }
    public DbSet<Deck> Decks { get; set; }
    public DbSet<Flashcard> Flashcards { get; set; }
}
