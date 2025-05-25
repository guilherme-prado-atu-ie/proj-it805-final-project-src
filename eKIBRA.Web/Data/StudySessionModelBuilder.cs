using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eKIBRA.Web.Data;

internal static class StudySessionModelBuilder<T> where T : StudySession
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
            .Property(p => p.DeckId)
            .IsUnicode()
            .HasMaxLength(450);

        builder.Entity<T>()
            .Property(k => k.UserId)
            .IsUnicode()
            .HasMaxLength(450);

        builder.Entity<T>()
            .Property(k => k.ModifierUserId)
            .IsUnicode()
            .HasMaxLength(450);

        builder.Entity<T>()
            .Property(p => p.Version)
            .IsUnicode()
            .HasMaxLength(450)
            .IsConcurrencyToken();

        builder.Entity<T>()
            .Property(p => p.Created);
        builder.Entity<T>()
            .Property(p => p.Modified);
        builder.Entity<T>()
            .Property(p => p.IsDeleted);

        builder.Entity<T>()
            .HasIndex(i => new { i.UserId, i.DeckId, i.Status });

        builder.Entity<T>()
            .HasQueryFilter(p => !p.IsDeleted);

        builder.Entity<T>()
            .HasOne(e => e.LinkedDeck);

        builder.Entity<T>()
            .HasMany(e => e.FlashcardsProgress)
            .WithOne(e => e.LinkedStudySession as T) // maybe won't work or will fail migration
            .HasForeignKey(f => f.StudySessionId)
            .OnDelete(DeleteBehavior.NoAction);

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