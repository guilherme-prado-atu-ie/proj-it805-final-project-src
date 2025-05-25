using System.Collections;
using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace eKIBRA.Web.Data;

internal static class FlashcardModelBuilder<T> where T : Flashcard
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
            .Property(p => p.Question)
            .HasMaxLength(700);
        builder.Entity<T>()
            .Property(p => p.Answer)
            .HasMaxLength(700);
        builder.Entity<T>()
            .Property(p => p.Incorrects)
            .HasConversion(
                input => JsonSerializer.Serialize(input, JsonSerializerOptions.Default),
                output => JsonSerializer.Deserialize<List<string>>(output, JsonSerializerOptions.Default) 
                          ?? new(), 
                ValueComparer.CreateDefault<List<string>>(true));
        builder.Entity<T>()
            .Property(p => p.Created);
        builder.Entity<T>()
            .Property(p => p.Modified);
        builder.Entity<T>()
            .Property(p => p.IsDeleted);

        builder.Entity<T>()
            .HasIndex(i => new { i.UserId, i.DeckId, i.Question })
            .IsUnique()
            .HasFilter("[Question] IS NOT NULL");

        builder.Entity<T>()
            .HasOne(e => e.LinkedDeck);

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