using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eKIBRA.Web.Data;

internal static class TestSessionModelBuilder<T> where T : TestSession
{
    private const int MaxIdLength = 450;
    private const int MaxUserIdLength = 450;
    private const int MaxDeckIdLength = 450;
    private const int MaxTitleLength = 500;
    private const int MaxDescriptionLength = 700;
    private const int MaxStatusLength = 30;

    internal static void SetModel(ModelBuilder builder)
    {
        builder.Entity<T>()
            .HasKey(k => k.Id);
        builder.Entity<T>()
            .Property(p => p.Id)
            .IsUnicode()
            .HasMaxLength(MaxIdLength);

        builder.Entity<T>()
            .Property(p => p.Title)
            .HasMaxLength(MaxTitleLength);

        builder.Entity<T>()
            .Property(p => p.Description)
            .HasMaxLength(MaxDescriptionLength);

        builder.Entity<T>()
            .Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(MaxStatusLength)
            .IsRequired();

        builder.Entity<T>()
            .Property(p => p.StartDate);

        builder.Entity<T>()
            .Property(p => p.EndDate);

        builder.Entity<T>()
            .Property(p => p.Created);

        builder.Entity<T>()
            .Property(p => p.Modified);

        builder.Entity<T>()
            .Property(p => p.IsDeleted);

        // Navigation properties and Foreign Keys
        builder.Entity<T>()
            .Property(p => p.DeckId)
            .IsUnicode()
            .HasMaxLength(MaxDeckIdLength);

        builder.Entity<T>()
            .HasOne(e => e.LinkedDeck)
            .WithMany()
            .HasForeignKey(f => f.DeckId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<T>()
            .Property(k => k.UserId)
            .IsUnicode()
            .HasMaxLength(MaxUserIdLength);

        builder.Entity<T>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Indexes and Filters
        builder.Entity<T>()
            .HasIndex(i => new { i.UserId, i.DeckId, i.Status });

        builder.Entity<T>()
            .HasQueryFilter(f => !f.IsDeleted);
    }
}