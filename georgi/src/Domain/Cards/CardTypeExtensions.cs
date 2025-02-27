namespace Domain.Cards;

public static class CardTypeExtensions
{
    public static bool SupportsPlastic(this CardType type) => type switch
    {
        CardType.Plastic => true,
        CardType.Virtual => false,
        CardType.VirtualWithPlastic => true,
        _ => throw new NotSupportedException($"Card type is not supported.")
    };
}
