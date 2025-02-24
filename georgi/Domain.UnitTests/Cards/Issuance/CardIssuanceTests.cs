using Domain.Abstractions;
using Domain.Accounts;
using Domain.Cards;
using Domain.Cards.Issuance;
using Domain.Cards.Issuers;
using Domain.Credits;
using Domain.Users;

using NSubstitute;

using Shouldly;

using Testing.Abstractions;

namespace Domain.UnitTests.Cards.Issuance;

public sealed class CardIssuanceTests
{
    [Fact]
    public void Complete_ShouldNotModifyCardIssuanceProperties_WhenCompleteDateIsNotNull()
    {
        //Arrange
        var completeDate = TestHelper.CreateInstance<IssuanceCompleteDate>();
        var cardIssuance = TestHelper.CreateInstance<CardIssuance>()
            .SetProperty(x => x.CompleteDate, completeDate);

        //Act
        cardIssuance.Complete(null!, null!, null!);

        //Assert
        cardIssuance.AssertAllProperties(p =>
        {
            p.Expect(x => x.CardId, null);
            p.Expect(x => x.Card, null);
            p.Expect(x => x.RequestDate, null);
            p.Expect(x => x.CompleteDate, completeDate);
            p.Expect(x => x.UserId, null);
            p.Expect(x => x.CardExpiryDate, null);
            p.Expect(x => x.CardPan, null);
            p.Expect(x => x.DomainEvents, []);
        });
    }

    [Fact]
    public void Complete_ShouldModifyCardIssuanceProperties_WhenCompleteDateIsNull()
    {
        //Arrange
        var card = TestHelper.CreateInstance<Card>();
        card.SetProperty(c => c.CardId, CardId.New());
        card.SetProperty(c => c.CurrentStatus, CardStatus.Requested);
        card.SetProperty(c => c.RequestedStatus, CardStatus.Issued);

        var cardIssuance = TestHelper.CreateInstance<CardIssuance>()
            .SetProperty(x => x.CardId, card.CardId)
            .SetProperty(x => x.Card, card);

        var dateTime = Substitute.For<IDateTime>();
        dateTime.UtcNow.Returns(new DateTimeOffset(new DateTime(2025, 07, 23)));

        var pan = TestHelper.CreateInstance<CardPan>();
        var expiryDate = TestHelper.CreateInstance<CardExpiryDate>();

        //Act
        cardIssuance.Complete(dateTime, expiryDate, pan);

        //Assert
        var cardIssuedDomainEvent = new CardIssuedDomainEvent { Card = card };

        cardIssuance.AssertAllProperties(p =>
        {
            p.Expect(x => x.CardId, card.CardId);
            p.Expect(x => x.Card, card);
            p.Expect(x => x.RequestDate, null);
            p.Expect(x => x.CompleteDate, IssuanceCompleteDate.From(dateTime));
            p.Expect(x => x.UserId, null);
            p.Expect(x => x.CardExpiryDate, expiryDate);
            p.Expect(x => x.CardPan, pan);
            p.Expect(x => x.DomainEvents, [cardIssuedDomainEvent]);
        });

        card.CurrentStatus.ShouldBe(CardStatus.Issued);
        card.RequestedStatus.ShouldBeNull();
    }

    [Fact]
    public void Request_ShouldThrowException_WhenRequestingInitialCardIssuanceAndCreditStatusIsNotActive()
    {
        //Arrange
        var credit = TestHelper.CreateInstance<Credit>()
            .SetProperty(x => x.Status, CreditStatus.Closed)
            .SetProperty(x => x.CreditId, CreditId.New());

        var lastCardIssuance = TestHelper.CreateInstance<LastAccountCardIssuance>();

        //Act
        var exception = Should.Throw<CreditDomainException>(() =>
        {
            CardIssuance.Request(
                null!,
                credit,
                null!,
                lastCardIssuance,
                null!,
                default!,
                null!,
                null!,
                null!);
        });

        //Assert
        exception.CreditId.ShouldBe(credit.CreditId);
        exception.Reason.ShouldBe(CardIssuance.Errors.CreditStatusMustBeActive);
    }

    [Fact]
    public void Request_ShouldThrowException_WhenRequestingInitialCardIssuanceAndCreditTypeIsNotRegular()
    {
        //Arrange
        var credit = TestHelper.CreateInstance<Credit>()
            .SetProperty(x => x.Status, CreditStatus.Active)
            .SetProperty(x => x.Type, CreditType.Declined)
            .SetProperty(x => x.CreditId, CreditId.New());

        var lastCardIssuance = TestHelper.CreateInstance<LastAccountCardIssuance>();

        //Act
        var exception = Should.Throw<CreditDomainException>(() =>
        {
            CardIssuance.Request(
                null!,
                credit,
                null!,
                lastCardIssuance,
                null!,
                default!,
                null!,
                null!,
                null!);
        });

        //Assert
        exception.CreditId.ShouldBe(credit.CreditId);
        exception.Reason.ShouldBe(CardIssuance.Errors.CreditTypeMustBeRegular);
    }

    [Fact]
    public void Request_ShouldThrowException_WhenPendingAccountBlocksArePresent()
    {
        //Arrange
        var credit = TestHelper.CreateInstance<Credit>()
            .SetProperty(x => x.Status, CreditStatus.Active)
            .SetProperty(x => x.Type, CreditType.Regular)
            .SetProperty(x => x.CreditId, CreditId.New());

        var accountBlockInfo = TestHelper.CreateInstance<AccountBlockInfo>()
            .SetProperty(x => x.HasPendingBlocks, true);

        var lastCardIssuance = TestHelper.CreateInstance<LastAccountCardIssuance>();

        var accountId = AccountId.New();

        //Act
        var exception = Should.Throw<AccountDomainException>(() =>
        {
            CardIssuance.Request(
                null!,
                credit,
                accountId,
                lastCardIssuance,
                accountBlockInfo,
                default!,
                null!,
                null!,
                null!);
        });

        //Assert
        exception.AccountId.ShouldBe(accountId);
        exception.Reason.ShouldBe(CardIssuance.Errors.PendingAccountBlocksArePresent);
    }

    [Fact]
    public void Request_ShouldCreateNewCardIssuance_WhenAllValidationsSucceed()
    {
        //Arrange
        var credit = TestHelper.CreateInstance<Credit>()
            .SetProperty(x => x.Status, CreditStatus.Active)
            .SetProperty(x => x.Type, CreditType.Regular)
            .SetProperty(x => x.CreditId, CreditId.New());

        var lastCardIssuance = TestHelper.CreateInstance<LastAccountCardIssuance>()
            .SetProperty(x => x.Value, value =>
            {
                value.SetProperty(v => v!.CardId, CardId.New());
            });

        var accountBlockInfo = TestHelper.CreateInstance<AccountBlockInfo>()
            .SetProperty(x => x.HasPendingBlocks, false);

        var userId = UserId.New();
        var accountId = AccountId.New();
        const CardType cardType = CardType.VirtualWithPlastic;
        var cardIssuerId = CardIssuerId.From(123);
        var dateTime = Substitute.For<IDateTime>();
        dateTime.UtcNow.Returns(new DateTimeOffset(new DateTime(2025, 07, 23)));
        var cardIssuanceRepository = Substitute.For<ICardIssuanceRepository>();
        cardIssuanceRepository.AddCardIssuance(Arg.Any<CardIssuance>());

        //Act
        CardIssuance.Request(
            userId,
            credit,
            accountId,
            lastCardIssuance,
            accountBlockInfo,
            cardType,
            cardIssuerId,
            dateTime,
            cardIssuanceRepository);

        //Assert
        var cardIssuance = (CardIssuance)cardIssuanceRepository.ReceivedCalls().Single().GetArguments()[0]!;

        var cardIssuanceRequestedDomainEvent = new CardIssuanceRequestedDomainEvent { CardIssuance = cardIssuance };

        cardIssuance.AssertAllProperties(p =>
        {
            p.Expect(x => x.CardId, cardIssuance.Card.CardId);
            p.Expect(x => x.Card, cardIssuance.Card);
            p.Expect(x => x.RequestDate, IssuanceRequestDate.From(dateTime));
            p.Expect(x => x.CompleteDate, null);
            p.Expect(x => x.UserId, userId);
            p.Expect(x => x.CardExpiryDate, null);
            p.Expect(x => x.CardPan, null);
            p.Expect(x => x.DomainEvents, [cardIssuanceRequestedDomainEvent]);
        });

        cardIssuance.Card.AssertAllProperties(p =>
        {
            p.Expect(x => x.CardId, cardIssuance.CardId);
            p.Expect(x => x.AccountId, accountId);
            p.Expect(x => x.Type, cardType);
            p.Expect(x => x.IssuerId, cardIssuerId);
            p.Expect(x => x.CurrentStatus, CardStatus.Requested);
            p.Expect(x => x.RequestedStatus, CardStatus.Issued);
            p.Expect(x => x.DomainEvents, []);
        });
    }
}
