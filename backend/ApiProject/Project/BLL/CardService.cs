using AutoMapper;
using Project.Bll.Interfaces;
using Project.Dal.Interfaces;
using Project.Dto;
using Project.Models;

namespace Project.Bll
{
    public class CardService : ICardService
    {
        private readonly ICardDal _cardDal;
        private readonly IMapper _mapper;

        public CardService(ICardDal cardDal, IMapper mapper)
        {
            _cardDal = cardDal;
            _mapper = mapper;
        }

        public async Task<Result<Card>> AddCard(CardDto cardDto)
        {
            if (cardDto != null && cardDto.PresentId > 0)
            {
                var card = _mapper.Map<Card>(cardDto);
                return await _cardDal.AddCard(card);
            }

            return new Result<Card>
            {
                Success = false,
                Message = "PresentId is required.",
                Data = null
            };
        }
        //
        //public async Task<Result<Card>> GetCardsWithPresentsAsync()
        //{
        //    return await _cardDAL.GetCardsByAllPresentsAsync();
        //}

        //public async Task<Result<Card>> GetMostExpensiveCardsAsync()
        //{
        //    return await _cardDAL.GetMostExpensiveCardsAsync();
        //}

        //public async Task<Result<User>> GetAllCardBuyersAsync()
        //{
        //    return await _cardDAL.GetAllCardBuyersAsync();
        //}

        //public async Task<Result<Card>> GetCardsByQuantityAsync()
        //{
        //    return await _cardDAL.GetCardsByQuantityAsync();
        //}

        //public async Task<Result<Card>> DeleteCardAsync(int cardId)
        //{
        //    if (cardId <= 0)
        //    {
        //        return new Result<Card>
        //        {
        //            Success = false,
        //            Message = "Invalid card id",
        //            Data = null
        //        };
        //    }

        //    return await _cardDAL.DeleteCardAsync(cardId);
        //}

        //public async Task<Result<Card>> ProcessPaymentForUserAsync(int userId)
        //{
        //    if (userId <= 0)
        //    {
        //        return new Result<Card>
        //        {
        //            Success = false,
        //            Message = "Invalid user id",
        //            Data = null
        //        };
        //    }

        //    return await _cardDAL.ProcessPaymentForUserAsync(userId);
        //}

        //public async Task<Result<Card>> GetCardsByUserAsync(int userId)
        //{
        //    if (userId <= 0)
        //    {
        //        return new Result<Card>
        //        {
        //            Success = false,
        //            Message = "Invalid user id",
        //            Data = null
        //        };
        //    }

        //    return await _cardDAL.GetCardsByUserAsync(userId);
        //}

        //public async Task<Result<Card>> GetUnpaidCardsAsync(int userId)
        //{
        //    if (userId <= 0)
        //    {
        //        return new Result<Card>
        //        {
        //            Success = false,
        //            Message = "Invalid user id",
        //            Data = null
        //        };
        //    }

        //    return await _cardDAL.GetUnpaidCardsAsync(userId);
        //}
    }
}