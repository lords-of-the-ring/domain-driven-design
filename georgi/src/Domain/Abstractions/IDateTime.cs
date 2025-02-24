namespace Domain.Abstractions;

public interface IDateTime
{
    DateTimeOffset UtcNow { get; }
}
