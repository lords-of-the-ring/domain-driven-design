namespace Domain.Users;

public sealed record UserId
{
    public static readonly UserId ExternalIssuer = new() { Value = Guid.Parse("019528f3-17e3-7ed5-9ab6-8619a8551086") };

    private UserId() { }

    public required Guid Value { get; init; }

    public static UserId New() => new() { Value = Guid.CreateVersion7() };
}
