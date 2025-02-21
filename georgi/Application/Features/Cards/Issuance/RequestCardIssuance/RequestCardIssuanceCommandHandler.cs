using Application.Abstractions;

using Domain.Abstractions;
using Domain.Accounts;
using Domain.Cards;
using Domain.Cards.Issuance;
using Domain.Credits;
using Domain.Users;

namespace Application.Features.Cards.Issuance.RequestCardIssuance;

public sealed class RequestCardIssuanceCommandHandler(
    CommandHandlerArguments arguments,
    ICreditRepository creditRepository,
    IAccountRepository accountRepository,
    IAccountBlockInfoRepository accountBlockInfoRepository,
    ILastAccountCardIssuanceRepository lastAccountCardIssuanceRepository,
    ICurrentUserService currentUserService,
    IDateTime dateTime,
    ICardIssuanceRepository cardIssuanceRepository
) : CommandHandler<RequestCardIssuanceCommand, CardId>(arguments)
{
    protected override async Task<CardId> Execute(RequestCardIssuanceCommand command,
        CancellationToken cancellationToken)
    {
        var account = await accountRepository.SingleAsync(command.AccountId, cancellationToken);

        var credit = await creditRepository.SingleAsync(account.CreditId, cancellationToken);

        var accountBlockInfo = await accountBlockInfoRepository.LoadAccountBlockInfo(command.AccountId,
            cancellationToken);

        var lastCard = await lastAccountCardIssuanceRepository.Load(command.AccountId, cancellationToken);

        var userId = currentUserService.GetUserId();

        var cardId = CardIssuance.Request(
            userId,
            credit,
            account.AccountId,
            lastCard,
            accountBlockInfo,
            command.CardType,
            command.CardIssuerId,
            dateTime,
            cardIssuanceRepository);

        return cardId;
    }
}
