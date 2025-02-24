using Application.Features.Cards.Termination.Request;
using Application.UnitTests.Abstractions;

using Domain.Abstractions;
using Domain.Cards;
using Domain.Cards.Termination;
using Domain.Users;

using NSubstitute;

using Shouldly;

using Testing.Infrastructure;

namespace Application.UnitTests.Features.Cards.Termination.Request;

public sealed class RequestCardTerminationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCorrectlyRequestNewCardTermination()
    {
        //Arrange
        var command = new RequestCardTerminationCommand
        {
            CardId = CardId.New(), TerminationReason = TerminationReason.Stolen
        };

        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CardId, command.CardId)
            .SetProperty(x => x.CurrentStatus, CardStatus.Active);
        var cardRepository = Substitute.For<ICardRepository>();
        cardRepository.SingleAsync(Arg.Any<CardId>(), Arg.Any<CancellationToken>())
            .Returns(card);

        var cardTerminationRepository = Substitute.For<ICardTerminationRepository>();
        cardTerminationRepository.AddCardTermination(Arg.Any<CardTermination>());

        var userId = UserId.New();
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.GetUserId().Returns(userId);

        var dateTime = Substitute.For<IDateTime>();
        dateTime.UtcNow.Returns(new DateTimeOffset(new DateTime(2025, 2, 2)));

        var sut = new RequestCardTerminationCommandHandler(
            MockCommandHandlerArguments.Instance,
            cardRepository,
            cardTerminationRepository,
            currentUserService,
            dateTime);

        //Act
        var result = await sut.Handle(command, CancellationToken.None);

        //Assert
        result.ShouldBeTrue();
        var termination = cardTerminationRepository.GetFirstArgument<CardTermination>();
        termination.UserId.ShouldBe(userId);
        termination.CardId.ShouldBe(command.CardId);
        termination.Reason.ShouldBe(command.TerminationReason);
        termination.RequestDate.Value.ShouldBe(new DateTimeOffset(new DateTime(2025, 2, 2)));
        currentUserService.Received().GetUserId();
        cardTerminationRepository.Received().AddCardTermination(termination);
        await cardRepository.Received().SingleAsync(command.CardId, CancellationToken.None);
    }
}
