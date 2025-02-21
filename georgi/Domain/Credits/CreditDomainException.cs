namespace Domain.Credits;

public sealed class CreditDomainException(CreditId creditId, string reason)
    : Exception($"An unexpected error for credit with id '{creditId.Value}' has occurred: {reason}.")
{
    public CreditId CreditId { get; } = creditId;

    public string Reason { get; } = reason;
}
