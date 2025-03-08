using Application.Abstractions;

using Domain.Cards.Issuance;
using Domain.Cards.Plastics;

namespace Application.Features.Cards.Plastics.CardIssuanceRequested;

public sealed class CardIssuanceRequestedDomainEventHandler(
    ICardPlasticRepository cardPlasticRepository
) : IDomainEventHandler<CardIssuanceRequestedDomainEvent>
{
    public Task Handle(CardIssuanceRequestedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        CardPlastic.Create(domainEvent.CardIssuance.Card, cardPlasticRepository);
        return Task.CompletedTask;
    }
}
