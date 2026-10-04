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

        public async Task<Result<Card>> AddCard(CardDto cardDto, int userId)
        {
            if (userId <= 0 || cardDto == null || cardDto.PresentId <= 0)
            {
                return new Result<Card> { Success = false, Message = "A valid present is required.", Data = null };
            }

            var card = _mapper.Map<Card>(cardDto);
            card.CreatedBy = userId;
            card.IsActive = true;
            card.IsDeleted = false;
            card.IsPaid = false;
            return await _cardDal.AddCard(card);
        }
        public Task<Result<Card>> GetCardsByUserAsync(int userId, bool? isPaid = null) =>
            userId > 0
                ? _cardDal.GetCardsByUserAsync(userId, isPaid)
                : Task.FromResult(new Result<Card> { Success = false, Message = "Invalid user id.", Data = null });

        public Task<Result<Card>> DeleteCardAsync(int cardId, int userId) =>
            cardId > 0 && userId > 0
                ? _cardDal.DeleteCardAsync(cardId, userId)
                : Task.FromResult(new Result<Card> { Success = false, Message = "Invalid card or user id.", Data = null });

        public Task<Result<Card>> ProcessPaymentAsync(int userId) =>
            userId > 0
                ? _cardDal.ProcessPaymentAsync(userId)
                : Task.FromResult(new Result<Card> { Success = false, Message = "Invalid user id.", Data = null });
    }
}