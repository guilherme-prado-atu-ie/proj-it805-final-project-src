using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.Data;

internal static class DeckModelBuilder<T> where T : Deck
{
    internal static void SetModel(ModelBuilder builder)
    {
        builder.Entity<T>()
            .HasKey(k => k.Id);

        builder.Entity<T>()
            .Property(p => p.Id)
            .IsUnicode()
            .HasMaxLength(450);

        builder.Entity<T>()
            .Property(p => p.IsDeleted);

        builder.Entity<T>()
            .Property(p => p.Title)
            .HasMaxLength(500);
        builder.Entity<T>()
            .Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Entity<T>()
            .Property(p => p.Created);
        builder.Entity<T>()
            .Property(p => p.UserId);

        builder.Entity<T>()
            .Property(p => p.Modified);
        builder.Entity<T>()
            .Property(p => p.ModifierUserId);

        builder.Entity<T>()
            .HasMany(e => e.Flashcards)
            .WithOne(e => e.LinkedDeck as T)
            .HasForeignKey(f => f.DeckId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<T>()
            .HasIndex(i => new { i.UserId, i.Title })
            .IsUnique()
            .HasFilter("[Title] IS NOT NULL");

        builder.Entity<T>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<T>()
            .HasOne(e => e.ModifierUser)
            .WithMany()
            .HasForeignKey(f => f.ModifierUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<T>()
            .HasQueryFilter(p => !p.IsDeleted);
    }

}