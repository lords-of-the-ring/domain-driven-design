namespace Domain.Credits;

public sealed class Credit
{
    private Credit() { }

    public required CreditId CreditId { get; init; }

    public required CreditStatus Status { get; init; }

    public required CreditType Type { get; init; }

    public static Credit Create() =>
        new() { CreditId = CreditId.New(), Status = CreditStatus.Active, Type = CreditType.Regular };
}
