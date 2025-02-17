using Domain.Cards;
using Domain.Credits;

namespace Domain.Accounts;

public sealed class Account
{
    public required AccountId AccountId { get; init; }

    public required CreditId CreditId { get; init; }
}
