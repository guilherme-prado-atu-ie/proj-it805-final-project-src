using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eKIBRA.Web.Data;

internal static class TestSessionResponseModelBuilder<T> where T : TestSessionResponse
{
    private const int MaxIdLength = 450;
    private const int MaxTestSessionIdLength = 450;
    private const int MaxFlashcardIdLength = 450;
    private const int MaxResponseTypeLength = 30;

    internal static void SetModel(ModelBuilder builder)
    {
        builder.Entity<T>()
            .HasKey(k => k.Id);
        builder.Entity<T>()
            .Property(p => p.Id)
            .IsUnicode()
            .HasMaxLength(MaxIdLength);

        builder.Entity<T>()
            .Property(p => p.ResponseTimeInSeconds);

        builder.Entity<T>()
            .Property(p => p.ResponseType)
            .HasConversion<string>()
            .HasMaxLength(MaxResponseTypeLength)
            .IsRequired();

        builder.Entity<T>()
            .Property(p => p.IsDeleted);

        // Navigation properties and Foreign Keys
        builder.Entity<T>()
            .Property(p => p.TestSessionId)
            .IsUnicode()
            .HasMaxLength(MaxTestSessionIdLength);

        builder.Entity<T>()
            .HasOne(e => e.TestSession)
            .WithMany()
            .HasForeignKey(f => f.TestSessionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<T>()
            .Property(p => p.FlashcardId)
            .IsUnicode()
            .HasMaxLength(MaxFlashcardIdLength);

        builder.Entity<T>()
            .HasOne(e => e.Flashcard)
            .WithMany()
            .HasForeignKey(f => f.FlashcardId)
            .OnDelete(DeleteBehavior.NoAction);

        // Indexes and Filters
        builder.Entity<T>()
            .HasIndex(i => new { i.TestSessionId, i.FlashcardId, i.ResponseType });

        builder.Entity<T>()
            .HasQueryFilter(f => !f.IsDeleted);
    }
}