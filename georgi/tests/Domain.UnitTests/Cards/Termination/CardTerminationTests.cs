using Domain.Abstractions;
using Domain.Cards;
using Domain.Cards.Termination;
using Domain.Users;

using NSubstitute;

using Shouldly;

using Testing.Infrastructure;

namespace Domain.UnitTests.Cards.Termination;

public sealed class CardTerminationTests
{
    [Fact]
    public void Request_ShouldThrowException_WhenCurrentCardStatusIsTerminated()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CardId, CardId.New())
            .SetProperty(x => x.CurrentStatus, CardStatus.Terminated);

        //Act
        var exception =
            Should.Throw<CardDomainException>(() => CardTermination.Request(card, null!, default!, null!, null!));

        //Assert
        exception.CardId.ShouldBe(card.CardId);
        exception.Reason.ShouldBe(CardTermination.Errors.CurrentCardStatusCannotBeTerminated);
    }

    [Fact]
    public void Request_ShouldThrowException_WhenRequestedCardStatusIsTerminated()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CardId, CardId.New())
            .SetProperty(x => x.CurrentStatus, CardStatus.Active)
            .SetProperty(x => x.RequestedStatus, CardStatus.Terminated);

        //Act
        var exception =
            Should.Throw<CardDomainException>(() => CardTermination.Request(card, null!, default!, null!, null!));

        //Assert
        exception.CardId.ShouldBe(card.CardId);
        exception.Reason.ShouldBe(CardTermination.Errors.RequestedCardStatusCannotBeTerminated);
    }

    [Fact]
    public void Request_ShouldThrowException_WhenTerminationReasonIsExpired()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CardId, CardId.New())
            .SetProperty(x => x.CurrentStatus, CardStatus.Active)
            .SetProperty(x => x.RequestedStatus, CardStatus.Blocked);

        //Act
        var exception = Should.Throw<CardDomainException>(() =>
            CardTermination.Request(card, null!, TerminationReason.Expired, null!, null!));

        //Assert
        exception.CardId.ShouldBe(card.CardId);
        exception.Reason.ShouldBe(CardTermination.Errors.TerminationReasonExpiredIsNotAllowed);
    }

    [Theory]
    [InlineData(TerminationReason.Lost)]
    [InlineData(TerminationReason.Stolen)]
    [InlineData(TerminationReason.Destroyed)]
    public void Request_ShouldCreateNewCardTermination_WhenAllValidationsSucceed(TerminationReason terminationReason)
    {
        //Arrange
        var userId = UserId.New();
        var dateTime = Substitute.For<IDateTime>();
        dateTime.UtcNow.Returns(new DateTimeOffset(new DateTime(2025, 02, 24)));
        var cardTerminationRepository = Substitute.For<ICardTerminationRepository>();
        cardTerminationRepository.AddCardTermination(Arg.Any<CardTermination>());

        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CardId, CardId.New())
            .SetProperty(x => x.CurrentStatus, CardStatus.Blocked)
            .SetProperty(x => x.RequestedStatus, null);

        //Act
        CardTermination.Request(card, userId, terminationReason, dateTime, cardTerminationRepository);

        //Assert
        var termination = cardTerminationRepository.GetFirstArgument<CardTermination>();
        cardTerminationRepository.Received().AddCardTermination(termination);

        card.CurrentStatus.ShouldBe(CardStatus.Blocked);
        card.RequestedStatus.ShouldBe(CardStatus.Terminated);

        var cardTerminationRequestedDomainEvent =
            new CardTerminationRequestedDomainEvent { CardTermination = termination };

        termination.AssertAllProperties(p =>
        {
            p.Expect(x => x.CardId, card.CardId);
            p.Expect(x => x.Card, card);
            p.Expect(x => x.Reason, terminationReason);
            p.Expect(x => x.UserId, userId);
            p.Expect(x => x.RequestDate, TerminationRequestDate.From(dateTime));
            p.Expect(x => x.CompleteDate, null);
            p.Expect(x => x.DomainEvents, [cardTerminationRequestedDomainEvent]);
        });
    }
}
