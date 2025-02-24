using Domain.Cards;
using Domain.Cards.Issuance;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Cards.Issuance;

public sealed class CardIssuanceConfiguration : IEntityTypeConfiguration<CardIssuance>
{
    public void Configure(EntityTypeBuilder<CardIssuance> builder)
    {
        builder.HasKey(x => x.CardId);

        builder
            .HasOne<Card>()
            .WithOne()
            .HasForeignKey<CardIssuance>(x => x.CardId);
    }
}
