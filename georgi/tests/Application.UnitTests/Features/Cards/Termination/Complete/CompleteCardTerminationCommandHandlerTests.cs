using Application.Features.Cards.Termination.Complete;
using Application.UnitTests.Abstractions;

using Domain.Abstractions;
using Domain.Cards;
using Domain.Cards.Termination;

using NSubstitute;

using Shouldly;

using Testing.Infrastructure;

namespace Application.UnitTests.Features.Cards.Termination.Complete;

public sealed class CompleteCardTerminationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCorrectlyCompleteCardTermination()
    {
        //Arrange
        var command = new CompleteCardTerminationCommand { CardId = CardId.New() };

        var termination = TestHelper.CreateInstance<CardTermination>()
            .SetProperty(x => x.Card, card =>
            {
                card.SetProperty(x => x.CurrentStatus, CardStatus.Active);
                card.SetProperty(x => x.RequestedStatus, CardStatus.Terminated);
            });

        var cardTerminationRepository = Substitute.For<ICardTerminationRepository>();
        cardTerminationRepository.SingleAsync(Arg.Any<CardId>(), Arg.Any<CancellationToken>())
            .Returns(termination);

        var dateTime = Substitute.For<IDateTime>();
        dateTime.UtcNow.Returns(new DateTimeOffset(new DateTime(2025, 02, 27)));

        var sut = new CompleteCardTerminationCommandHandler(
            MockCommandHandlerArguments.Instance,
            cardTerminationRepository,
            dateTime);

        //Act
        var result = await sut.Handle(command, CancellationToken.None);

        //Assert
        result.ShouldBeTrue();
        termination.CompleteDate!.Value.ShouldBe(new DateTimeOffset(new DateTime(2025, 02, 27)));
        termination.Card.CurrentStatus.ShouldBe(CardStatus.Terminated);
        termination.Card.RequestedStatus.ShouldBeNull();
        await cardTerminationRepository.Received().SingleAsync(command.CardId, CancellationToken.None);
    }
}
