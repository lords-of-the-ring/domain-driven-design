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
    public void Request_ShouldThrowException_WhenRequestingCardIssuanceRenewalAndPendingAccountBlocksArePresent()
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
            .SetProperty(x => x.HasPendingBlocks, true);

        //Act
        var exception = Should.Throw<CardDomainException>(() =>
        {
            CardIssuance.Request(
                null!,
                credit,
                null!,
                lastCardIssuance,
                accountBlockInfo,
                default!,
                null!,
                null!,
                null!);
        });

        //Assert
        exception.CardId.ShouldBe(lastCardIssuance.Value!.CardId);
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
