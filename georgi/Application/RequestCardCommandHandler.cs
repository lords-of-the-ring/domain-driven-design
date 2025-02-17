using Application.Abstractions;

using Domain.Abstractions;
using Domain.Accounts;
using Domain.Cards;
using Domain.Cards.Issuance;
using Domain.Credits;
using Domain.Users;

namespace Application;

public sealed class RequestCardCommandHandler(
    ICreditRepository creditRepository,
    IAccountRepository accountRepository,
    IAccountBlockInfoRepository accountBlockInfoRepository,
    ILastAccountCardRepository lastAccountCardRepository,
    ICurrentUserService currentUserService,
    IDateTime dateTime,
    ICardIssuanceRepository cardIssuanceRepository
) : CommandHandler<RequestCardCommand, CardId>
{
    protected override async Task<CardId> Execute(RequestCardCommand command, CancellationToken cancellationToken)
    {
        var account = await accountRepository.FindAsync(command.AccountId, cancellationToken);

        var credit = await creditRepository.FindAsync(account.CreditId, cancellationToken);

        var accountBlockInfo = await accountBlockInfoRepository.LoadAccountBlockInfo(command.AccountId,
            cancellationToken);

        var lastAccountCard = await lastAccountCardRepository.LoadLastAccountCard(command.AccountId,
            cancellationToken);

        var cardIssuance = CardIssuance.Request(
            currentUserService.GetUserId(),
            credit,
            account.AccountId,
            lastAccountCard,
            accountBlockInfo,
            command.CardType,
            command.CardIssuerId,
            dateTime);

        cardIssuanceRepository.AddCardIssuance(cardIssuance);

        return cardIssuance.CardId;
    }
}
