using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.Data;

internal static class DeckModelBuilder
{
    internal static void SetModel(ModelBuilder builder)
    {
        builder.Entity<Deck>()
            .HasKey(k => k.Id);

        builder.Entity<Deck>()
            .Property(p => p.Id)
            .IsUnicode()
            .HasMaxLength(450);

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
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Deck>()
            .HasIndex(i => new { i.UserId, i.Title })
            .IsUnique()
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

        builder.Entity<Deck>()
            .HasQueryFilter(p => !p.IsDeleted);
    }

}