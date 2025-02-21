using Domain.Abstractions;
using Domain.Credits;

namespace Domain.Accounts;

public sealed class Account : DomainEntity
{
    private Account() { }

    public required AccountId AccountId { get; init; }

    public required CreditId CreditId { get; init; }
}
