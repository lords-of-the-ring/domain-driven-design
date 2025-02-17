namespace Domain.Credits;

public sealed class Credit
{
    public required CreditStatus Status { get; init; }

    public required CreditType Type { get; init; }
}

public enum CreditType
{
    Regular = 1,
}

public enum CreditStatus
{
    Active = 1,
}
