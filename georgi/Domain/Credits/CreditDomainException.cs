namespace Domain.Credits;

public sealed class CreditDomainException(CreditId creditId, string message) : Exception()
{
}
