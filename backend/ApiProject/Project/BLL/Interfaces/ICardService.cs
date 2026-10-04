using Project.Dto;
using Project.Models;

namespace Project.Bll.Interfaces
{
    public interface ICardService
    {
        Task<Result<Card>> AddCard(CardDto cardDto, int userId);
        Task<Result<Card>> GetCardsByUserAsync(int userId, bool? isPaid = null);
        Task<Result<Card>> DeleteCardAsync(int cardId, int userId);
        Task<Result<Card>> ProcessPaymentAsync(int userId);
    }
}