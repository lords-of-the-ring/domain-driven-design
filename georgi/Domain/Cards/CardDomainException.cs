namespace Domain.Cards;

public sealed class CardDomainException(CardId cardId, string message) : Exception
{
}
