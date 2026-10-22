using Project.Models;

namespace Project.Dal.Interfaces
{
    public interface ICardDal
    {
        Task<Result<Card>> AddCard(Card card);
        Task<Result<Card>> AddCards(IReadOnlyCollection<Card> cards);
        Task<Result<Card>> GetCardsByUserAsync(int userId, bool? isPaid = null);
        Task<Result<Card>> DeleteCardAsync(int cardId, int userId);
        Task<Result<Card>> ProcessDevelopmentPaymentAsync(int userId, decimal amount);
    }
}
