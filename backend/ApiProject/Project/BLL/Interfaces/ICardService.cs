using Project.Models;
using Project.Models.ModelsDTO;

namespace Project.BLL.Interfaces
{
    public interface ICardService
    {
        Task<Result<Card>> AddCard(CardDto cardDto);
        //Task<Result<Card>> GetMostExpensiveCardsAsync();
        //Task<Result<User>> GetAllCardBuyersAsync();
        //Task<Result<Card>> GetCardsByUserAsync(int userId);
        //Task<Result<Card>> GetUnpaidCardsAsync(int userId);
        //Task<Result<Card>> GetCardsByQuantityAsync();
        //Task<Result<Card>> DeleteCardAsync(int cardId);
        //Task<Result<Card>> ProcessPaymentForUserAsync(int userId);
        //Task<Result<Card>> GetCardsWithPresentsAsync();
    }
}