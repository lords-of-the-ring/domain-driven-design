using Application.Features.Cards.Issuance.Complete;
using Application.UnitTests.Abstractions;

using Domain.Abstractions;
using Domain.Cards;
using Domain.Cards.Issuance;

using NSubstitute;

using Shouldly;

using Testing.Infrastructure;

namespace Application.UnitTests.Features.Cards.Issuance.Complete;

public sealed class CompleteCardIssuanceCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCorrectlyCompleteCardIssuance()
    {
        //Arrange
        var command = new CompleteCardIssuanceCommand
        {
            CardId = CardId.New(),
            CardPan = CardPan.From("1234****5678"),
            CardExpiryDate = CardExpiryDate.From(new DateTimeOffset(new DateTime(2029, 5, 1)))
        };

        var dateTime = Substitute.For<IDateTime>();
        dateTime.UtcNow.Returns(new DateTimeOffset(new DateTime(2025, 02, 27)));

        var cardIssuance = TestHelper.CreateInstance<CardIssuance>()
            .SetProperty(x => x.Card, card =>
            {
                card.SetProperty(x => x.CurrentStatus, CardStatus.Requested);
                card.SetProperty(x => x.RequestedStatus, CardStatus.Issued);
            });

        var cardIssuanceRepository = Substitute.For<ICardIssuanceRepository>();
        cardIssuanceRepository.SingleAsync(Arg.Any<CardId>(), Arg.Any<CancellationToken>())
            .Returns(cardIssuance);

        var sut = new CompleteCardIssuanceCommandHandler(
            MockCommandHandlerArguments.Instance,
            dateTime,
            cardIssuanceRepository);

        //Act
        var result = await sut.Handle(command, CancellationToken.None);

        //Assert
        result.ShouldBeTrue();

        cardIssuance.CardPan.ShouldBe(command.CardPan);
        cardIssuance.CardExpiryDate.ShouldBe(command.CardExpiryDate);
        cardIssuance.CompleteDate!.Value.ShouldBe(new DateTimeOffset(new DateTime(2025, 02, 27)));

        await cardIssuanceRepository.Received().SingleAsync(command.CardId, CancellationToken.None);
    }
}
