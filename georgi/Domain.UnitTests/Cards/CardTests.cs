using Domain.Accounts;
using Domain.Cards;
using Domain.Cards.Issuers;

using Shouldly;

using Testing.Abstractions;

namespace Domain.UnitTests.Cards;

public sealed class CardTests
{
    [Fact]
    public void Create_ShouldReturnNewCardInstance()
    {
        //Arrange
        var accountId = AccountId.New();
        const CardType cardType = CardType.VirtualWithPlastic;
        var cardIssuerId = CardIssuerId.From(123);

        //Act
        var card = Card.Create(accountId, cardType, cardIssuerId);

        //Assert
        card.AssertAllProperties(p =>
        {
            p.Expect(x => x.CardId, card.CardId);
            p.Expect(x => x.AccountId, accountId);
            p.Expect(x => x.Type, cardType);
            p.Expect(x => x.IssuerId, cardIssuerId);
            p.Expect(x => x.CurrentStatus, CardStatus.Requested);
            p.Expect(x => x.RequestedStatus, null);
            p.Expect(x => x.DomainEvents, []);
        });
    }

    [Fact]
    public void CompleteStatusChange_ShouldNotModifyAnyCardProperties_WhenCurrentStatusIsTerminated()
    {
        //Arrange
        const CardStatus notRealCardStatus = (CardStatus)123;
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Terminated)
            .SetProperty(x => x.RequestedStatus, notRealCardStatus);

        //Act
        card.CompleteStatusChange(CardStatus.Active);

        //Assert
        card.CurrentStatus.ShouldBe(CardStatus.Terminated);
        card.RequestedStatus.ShouldBe(notRealCardStatus);
    }

    [Fact]
    public void CompleteStatusChange_ShouldThrowException_WhenRequestedStatusIsNullAndExpectedStatusIsNotTerminated()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Active)
            .SetProperty(x => x.RequestedStatus, null)
            .SetProperty(x => x.CardId, CardId.New());

        //Act
        var exception = Should.Throw<CardDomainException>(() => card.CompleteStatusChange(CardStatus.Blocked));

        //Assert
        exception.CardId.ShouldBe(card.CardId);
        exception.Reason.ShouldBe(Card.Errors.ExpectedStatusMustBeTerminated);
    }

    [Fact]
    public void
        CompleteStatusChange_ShouldSetCurrentStatusToTerminated_WhenRequestedStatusIsNullAndExpectedStatusIsTerminated()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Blocked)
            .SetProperty(x => x.RequestedStatus, null);

        //Act
        card.CompleteStatusChange(CardStatus.Terminated);

        //Assert
        card.CurrentStatus.ShouldBe(CardStatus.Terminated);
        card.RequestedStatus.ShouldBeNull();
    }

    [Fact]
    public void
        CompleteStatusChange_ShouldSetCurrentStatusToTerminatedAndRequestedStatusToNull_WhenBothRequestedStatusAndExpectedStatusAreTerminated()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Issued)
            .SetProperty(x => x.RequestedStatus, CardStatus.Terminated);

        //Act
        card.CompleteStatusChange(CardStatus.Terminated);

        //Assert
        card.CurrentStatus.ShouldBe(CardStatus.Terminated);
        card.RequestedStatus.ShouldBeNull();
    }

    [Fact]
    public void
        CompleteStatusChange_ShouldThrowException_WhenRequestedStatusIsTerminatedAndExpectedStatusIsNotTerminatedButExpectedStatusIsNotCompatibleWithCurrentStatus()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Issued)
            .SetProperty(x => x.RequestedStatus, CardStatus.Terminated)
            .SetProperty(x => x.CardId, CardId.New());

        //Act
        var exception = Should.Throw<CardDomainException>(() => card.CompleteStatusChange(CardStatus.Blocked));

        //Assert
        exception.CardId.ShouldBe(card.CardId);
        exception.Reason.ShouldBe(Card.Errors.NextStatusMustBeActiveWhenCurrentStatusIsIssued);
    }

    [Fact]
    public void
        CompleteStatusChange_ShouldSetCurrentStatusToExpectedStatus_WhenRequestedStatusIsTerminatedAndExpectedStatusIsNotTerminated()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Active)
            .SetProperty(x => x.RequestedStatus, CardStatus.Terminated);

        //Act
        card.CompleteStatusChange(CardStatus.Blocked);

        //Assert
        card.CurrentStatus.ShouldBe(CardStatus.Blocked);
        card.RequestedStatus.ShouldBe(CardStatus.Terminated);
    }

    [Fact]
    public void
        CompleteStatusChange_ShouldSetCurrentStatusToTerminatedAndRequestedStatusToNull_WhenRequestedStatusIsNotTerminatedAndExpectedStatusIsTerminated()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Blocked)
            .SetProperty(x => x.RequestedStatus, CardStatus.Active);

        //Act
        card.CompleteStatusChange(CardStatus.Terminated);

        //Assert
        card.CurrentStatus.ShouldBe(CardStatus.Terminated);
        card.RequestedStatus.ShouldBeNull();
    }

    [Fact]
    public void CompleteStatusChange_ShouldThrowException_WhenRequestedStatusAndExpectedStatusAreNotTheSame()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Requested)
            .SetProperty(x => x.RequestedStatus, CardStatus.Issued)
            .SetProperty(x => x.CardId, CardId.New());

        //Act
        var exception = Should.Throw<CardDomainException>(() => card.CompleteStatusChange(CardStatus.Active));

        //Assert
        exception.CardId.ShouldBe(card.CardId);
        exception.Reason.ShouldBe(Card.Errors.ExpectedStatusMustBeTheSameAsRequestedStatus);
    }

    [Fact]
    public void
        CompleteStatusChange_ShouldThrowException_WhenExpectedStatusIsTheSameAsRequestedStatusButExpectedStatusIsNotCompatibleWithCurrentStatus()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Issued)
            .SetProperty(x => x.RequestedStatus, CardStatus.Blocked)
            .SetProperty(x => x.CardId, CardId.New());

        //Act
        var exception = Should.Throw<CardDomainException>(() => card.CompleteStatusChange(CardStatus.Blocked));

        //Assert
        exception.CardId.ShouldBe(card.CardId);
        exception.Reason.ShouldBe(Card.Errors.NextStatusMustBeActiveWhenCurrentStatusIsIssued);
    }

    [Fact]
    public void
        CompleteStatusChange_ShouldSetCurrentStatusToExpectedStatusAndRequestedStatusToNull_WhenExpectedStatusIsTheSameAsRequestedStatusAndExpectedStatusIsCompatibleWithCurrentStatus()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Active)
            .SetProperty(x => x.RequestedStatus, CardStatus.Blocked);

        //Act
        card.CompleteStatusChange(CardStatus.Blocked);

        //Assert
        card.CurrentStatus.ShouldBe(CardStatus.Blocked);
        card.RequestedStatus.ShouldBeNull();
    }

    [Fact]
    public void ValidateStatusCompatibility_ShouldThrowException_WhenCurrentStatusIsTerminated()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Terminated)
            .SetProperty(x => x.CardId, CardId.New());

        //Act
        var exception = Should.Throw<CardDomainException>(() =>
            card.ValidateStatusCompatibility(CardStatus.Active));

        //Arrange
        exception.CardId.ShouldBe(card.CardId);
        exception.Reason.ShouldBe(Card.Errors.CurrentStatusCannotBeTerminated);
    }

    [Fact]
    public void ValidateStatusCompatibility_ShouldThrowException_WhenNextStatusIsTerminated()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Blocked)
            .SetProperty(x => x.CardId, CardId.New());

        //Act
        var exception = Should.Throw<CardDomainException>(() =>
            card.ValidateStatusCompatibility(CardStatus.Terminated));

        //Arrange
        exception.CardId.ShouldBe(card.CardId);
        exception.Reason.ShouldBe(Card.Errors.NextStatusCannotBeTerminated);
    }

    [Fact]
    public void ValidateStatusCompatibility_ShouldThrowException_WhenCurrentStatusIsRequestedButNextStatusIsNotIssued()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Requested)
            .SetProperty(x => x.CardId, CardId.New());

        //Act
        var exception = Should.Throw<CardDomainException>(() =>
            card.ValidateStatusCompatibility(CardStatus.Active));

        //Arrange
        exception.CardId.ShouldBe(card.CardId);
        exception.Reason.ShouldBe(Card.Errors.NextStatusMustBeIssuedWhenCurrentStatusIsRequested);
    }

    [Fact]
    public void ValidateStatusCompatibility_ShouldThrowException_WhenCurrentStatusIsIssuedButNextStatusIsNotActive()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Issued)
            .SetProperty(x => x.CardId, CardId.New());

        //Act
        var exception = Should.Throw<CardDomainException>(() =>
            card.ValidateStatusCompatibility(CardStatus.Blocked));

        //Arrange
        exception.CardId.ShouldBe(card.CardId);
        exception.Reason.ShouldBe(Card.Errors.NextStatusMustBeActiveWhenCurrentStatusIsIssued);
    }

    [Fact]
    public void ValidateStatusCompatibility_ShouldThrowException_WhenCurrentStatusIsActiveButNextStatusIsNotBlocked()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Active)
            .SetProperty(x => x.CardId, CardId.New());

        //Act
        var exception = Should.Throw<CardDomainException>(() =>
            card.ValidateStatusCompatibility(CardStatus.Issued));

        //Arrange
        exception.CardId.ShouldBe(card.CardId);
        exception.Reason.ShouldBe(Card.Errors.NextStatusMustBeBlockedWhenCurrentStatusIsActive);
    }

    [Fact]
    public void ValidateStatusCompatibility_ShouldThrowException_WhenCurrentStatusIsBlockedButNextStatusIsNotActive()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>()
            .SetProperty(x => x.CurrentStatus, CardStatus.Blocked)
            .SetProperty(x => x.CardId, CardId.New());

        //Act
        var exception = Should.Throw<CardDomainException>(() =>
            card.ValidateStatusCompatibility(CardStatus.Issued));

        //Arrange
        exception.CardId.ShouldBe(card.CardId);
        exception.Reason.ShouldBe(Card.Errors.NextStatusMustBeActiveWhenCurrentStatusIsBlocked);
    }
}
