using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.Data;

internal static class FlashcardProgressModelBuilder<T> where T : FlashcardProgress
{
    internal static void SetModel(ModelBuilder builder)
    {
        SetIds(builder);
        SetProperties(builder);

        builder.Entity<T>()
            .HasIndex(i => new { i.UserId, i.DeckId, i.StudySessionId, i.Sequence, i.FlashcardId });

        builder.Entity<T>()
            .HasOne(e => e.LinkedStudySession);

        builder.Entity<T>()
            .HasOne(e => e.LinkedDeck);

        builder.Entity<T>()
            .HasOne(e => e.LinkedFlashcard);

        builder.Entity<T>()
            .HasQueryFilter(p => !p.IsDeleted);
    }

    private static void SetProperties(ModelBuilder builder)
    {
        builder.Entity<T>()
            .Property(p => p.Sequence);
        builder.Entity<T>()
            .Property(p => p.Reveals);
        builder.Entity<T>()
            .Property(p => p.RevealAt);
        builder.Entity<T>()
            .Property(p => p.Remembers);
        builder.Entity<T>()
            .Property(p => p.RememberAt);
        builder.Entity<T>()
            .Property(p => p.Forgets);
        builder.Entity<T>()
            .Property(p => p.ForgetAt);
        builder.Entity<T>()
            .Property(p => p.Level);

        builder.Entity<T>()
            .Property(p => p.RevealsAcrossSessions);
        builder.Entity<T>()
            .Property(p => p.RemembersAcrossSessions);
        builder.Entity<T>()
            .Property(p => p.ForgetsAcrossSessions);
        builder.Entity<T>()
            .Property(p => p.SpacedRepetitionInterval);
        builder.Entity<T>()
            .Property(p => p.NextSpacedRepetitionInterval);

        builder.Entity<T>()
            .Property(p => p.Created);
        builder.Entity<T>()
            .Property(p => p.Modified);

        builder.Entity<T>()
            .Property(p => p.IsDeleted);
    }

    private static void SetIds(ModelBuilder builder)
    {
        builder.Entity<T>()
            .HasKey(k => k.Id);
        builder.Entity<T>()
            .Property(p => p.Id)
            .IsUnicode()
            .HasMaxLength(450);

        builder.Entity<T>()
            .Property(p => p.StudySessionId)
            .IsUnicode()
            .HasMaxLength(450);

        builder.Entity<T>()
            .Property(p => p.DeckId)
            .IsUnicode()
            .HasMaxLength(450);

        builder.Entity<T>()
            .Property(p => p.FlashcardId)
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
    }
}