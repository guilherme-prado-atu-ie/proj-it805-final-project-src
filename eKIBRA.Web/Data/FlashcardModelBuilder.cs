using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace eKIBRA.Web.Data;

internal static class FlashcardModelBuilder
{
    internal static void SetModel(ModelBuilder builder)
    {
        builder.Entity<Flashcard>()
            .HasKey(k => k.Id);
        builder.Entity<Flashcard>()
            .Property(p => p.Id)
            .IsUnicode()
            .HasMaxLength(450);

        builder.Entity<Flashcard>()
            .Property(k => k.UserId)
            .HasMaxLength(450);

        builder.Entity<Flashcard>()
            .Property(p => p.Question)
            .HasMaxLength(4000);
        builder.Entity<Flashcard>()
            .Property(p => p.Answer)
            .HasMaxLength(4000);
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
            .HasIndex(i => new { i.UserId, i.DeckId, i.Question })
            .IsUnique()
            .HasFilter("[Question] IS NOT NULL");

        builder.Entity<Flashcard>()
            .HasOne(e => e.LinkedDeck);

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

        builder.Entity<Flashcard>()
            .HasQueryFilter(p => !p.IsDeleted);
    }

}