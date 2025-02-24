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
    public void Complete_ShouldNotModifyAnyCardTerminationProperties_WhenCardTerminationCompleteDateIsNotNull()
    {
        //Arrange
        var dateTime = Substitute.For<IDateTime>();
        dateTime.UtcNow.Returns(new DateTimeOffset(new DateTime(2025, 02, 24)));

        var termination = TestHelper.CreateInstance<CardTermination>()
            .SetProperty(x => x.CompleteDate, TerminationCompleteDate.From(dateTime));

        //Act
        termination.Complete(dateTime);

        //Assert
        termination.CompleteDate.ShouldBe(TerminationCompleteDate.From(dateTime));
        termination.Card.ShouldBeNull();
        termination.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void Complete_ShouldModifyCardTerminationProperties_WhenCardTerminationCompleteDateIsNull()
    {
        //Arrange
        var dateTime = Substitute.For<IDateTime>();
        dateTime.UtcNow.Returns(new DateTimeOffset(new DateTime(2025, 02, 24)));

        var termination = TestHelper.CreateInstance<CardTermination>()
            .SetProperty(x => x.Card, card =>
            {
                card.SetProperty(x => x.CurrentStatus, CardStatus.Active);
                card.SetProperty(x => x.RequestedStatus, CardStatus.Terminated);
            });

        //Act
        termination.Complete(dateTime);

        //Assert
        termination.Card.CurrentStatus.ShouldBe(CardStatus.Terminated);
        termination.Card.RequestedStatus.ShouldBeNull();

        var cardTerminatedDomainEvent = new CardTerminatedDomainEvent { CardTermination = termination };
        termination.AssertAllProperties(p =>
        {
            p.Expect(x => x.CardId, null);
            p.Expect(x => x.Card, termination.Card);
            p.Expect(x => x.Reason, default!);
            p.Expect(x => x.UserId, null);
            p.Expect(x => x.RequestDate, null);
            p.Expect(x => x.CompleteDate, TerminationCompleteDate.From(dateTime));
            p.Expect(x => x.DomainEvents, [cardTerminatedDomainEvent]);
        });
    }

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

    [Fact]
    public async Task Expire_ShouldNeitherCreateCardTerminationNorModifyProperties_WhenCurrentCardStatusIsTerminated()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Terminated)
            .SetProperty(x => x.RequestedStatus, null);

        //Act
        await CardTermination.Expire(card, null!, null!, CancellationToken.None);

        //Assert
        card.RequestedStatus.ShouldBeNull();
    }

    [Fact]
    public async Task Expire_ShouldCompleteRequestedCardTermination_WhenRequestedCardStatusIsTerminated()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Active)
            .SetProperty(x => x.RequestedStatus, CardStatus.Terminated);

        var termination = TestHelper.CreateInstance<CardTermination>()
            .SetProperty(x => x.Card, card);

        var cardTerminationRepository = Substitute.For<ICardTerminationRepository>();
        cardTerminationRepository.SingleAsync(Arg.Any<CardId>(), Arg.Any<CancellationToken>())
            .Returns(termination);

        var dateTime = Substitute.For<IDateTime>();
        dateTime.UtcNow.Returns(new DateTimeOffset(new DateTime(2025, 02, 24)));

        //Act
        await CardTermination.Expire(card, dateTime, cardTerminationRepository, CancellationToken.None);

        //Assert
        card.RequestedStatus.ShouldBeNull();
        card.CurrentStatus.ShouldBe(CardStatus.Terminated);
        termination.DomainEvents.ShouldHaveSingleItem();
        termination.CompleteDate.ShouldBe(TerminationCompleteDate.From(dateTime));
        await cardTerminationRepository.Received().SingleAsync(card.CardId, CancellationToken.None);
    }

    [Fact]
    public async Task Expire_ShouldCreateCompletedCardTermination_WhenNoRequestedCardTerminationIsPresent()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Active)
            .SetProperty(x => x.RequestedStatus, CardStatus.Blocked);

        var cardTerminationRepository = Substitute.For<ICardTerminationRepository>();
        cardTerminationRepository.AddCardTermination(Arg.Any<CardTermination>());

        var dateTime = Substitute.For<IDateTime>();
        dateTime.UtcNow.Returns(new DateTimeOffset(new DateTime(2025, 02, 24)));

        //Act
        await CardTermination.Expire(card, dateTime, cardTerminationRepository, CancellationToken.None);

        //Assert
        card.CurrentStatus.ShouldBe(CardStatus.Terminated);
        card.RequestedStatus.ShouldBeNull();

        var termination = cardTerminationRepository.GetFirstArgument<CardTermination>();
        cardTerminationRepository.Received().AddCardTermination(termination);

        var cardTerminatedDomainEvent = new CardTerminatedDomainEvent { CardTermination = termination };
        termination.AssertAllProperties(p =>
        {
            p.Expect(x => x.CardId, card.CardId);
            p.Expect(x => x.Card, card);
            p.Expect(x => x.Reason, TerminationReason.Expired);
            p.Expect(x => x.CompleteDate, TerminationCompleteDate.From(dateTime));
            p.Expect(x => x.RequestDate, TerminationRequestDate.From(dateTime));
            p.Expect(x => x.UserId, UserId.ExternalIssuer);
            p.Expect(x => x.DomainEvents, [cardTerminatedDomainEvent]);
        });
    }
}
