using Application.Features.Cards.Issuance.Request;
using Application.UnitTests.Abstractions;

using Domain.Abstractions;
using Domain.Accounts;
using Domain.Cards;
using Domain.Cards.Issuance;
using Domain.Cards.Issuers;
using Domain.Credits;
using Domain.Users;

using NSubstitute;

using Shouldly;

using Testing.Infrastructure;

namespace Application.UnitTests.Features.Cards.Issuance.Request;

public sealed class RequestCardIssuanceCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCorrectlyRequestNewCardIssuance()
    {
        //Arrange
        var command = new RequestCardIssuanceCommand
        {
            AccountId = AccountId.New(),
            CardIssuerId = CardIssuerId.From(123),
            CardType = CardType.VirtualWithPlastic
        };

        var credit = TestHelper.CreateInstance<Credit>()
            .SetProperty(x => x.Status, CreditStatus.Active)
            .SetProperty(x => x.Type, CreditType.Regular);
        var creditRepository = Substitute.For<ICreditRepository>();
        creditRepository.SingleAsync(Arg.Any<CreditId>(), Arg.Any<CancellationToken>())
            .Returns(credit);

        var account = TestHelper.CreateInstance<Account>()
            .SetProperty(x => x.CreditId, CreditId.New())
            .SetProperty(x => x.AccountId, command.AccountId);
        var accountRepository = Substitute.For<IAccountRepository>();
        accountRepository.SingleAsync(Arg.Any<AccountId>(), Arg.Any<CancellationToken>())
            .Returns(account);

        var accountBlockInfo = TestHelper.CreateInstance<AccountBlockInfo>()
            .SetProperty(x => x.HasPendingBlocks, false);
        var accountBlockInfoRepository = Substitute.For<IAccountBlockInfoRepository>();
        accountBlockInfoRepository.LoadAccountBlockInfo(Arg.Any<AccountId>(), Arg.Any<CancellationToken>())
            .Returns(accountBlockInfo);

        var lastAccountCardIssuance = TestHelper.CreateInstance<LastAccountCardIssuance>();
        var lastAccountCardIssuanceRepository = Substitute.For<ILastAccountCardIssuanceRepository>();
        lastAccountCardIssuanceRepository.Load(Arg.Any<AccountId>(), Arg.Any<CancellationToken>())
            .Returns(lastAccountCardIssuance);

        var userId = UserId.New();
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.GetUserId().Returns(userId);

        var dateTime = Substitute.For<IDateTime>();
        dateTime.UtcNow.Returns(new DateTimeOffset(new DateTime(2025, 2, 2)));

        var cardIssuanceRepository = Substitute.For<ICardIssuanceRepository>();
        cardIssuanceRepository.AddCardIssuance(Arg.Any<CardIssuance>());

        var sut = new RequestCardIssuanceCommandHandler(
            MockCommandHandlerArguments.Instance,
            creditRepository,
            accountRepository,
            accountBlockInfoRepository,
            lastAccountCardIssuanceRepository,
            currentUserService,
            dateTime,
            cardIssuanceRepository);

        //Act
        var result = await sut.Handle(command, CancellationToken.None);

        //Assert
        var cardIssuance = cardIssuanceRepository.GetFirstArgument<CardIssuance>();
        result.ShouldBe(cardIssuance.CardId);
        cardIssuance.Card.AccountId.ShouldBe(command.AccountId);
        cardIssuance.UserId.ShouldBe(userId);
        cardIssuance.CardId.ShouldBe(cardIssuance.Card.CardId);
        cardIssuance.RequestDate.Value.ShouldBe(new DateTimeOffset(new DateTime(2025, 2, 2)));

        currentUserService.Received().GetUserId();
        await accountRepository.Received().SingleAsync(command.AccountId, CancellationToken.None);
        await creditRepository.Received().SingleAsync(account.CreditId, CancellationToken.None);
        await accountBlockInfoRepository.Received().LoadAccountBlockInfo(command.AccountId, CancellationToken.None);
        await lastAccountCardIssuanceRepository.Received().Load(command.AccountId, CancellationToken.None);
    }
}
