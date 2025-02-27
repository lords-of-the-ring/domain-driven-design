namespace Domain.Abstractions;

public abstract record ValueObject<T>
{
    public required T Value { get; init; }
}
