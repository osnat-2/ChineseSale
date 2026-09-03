using AutoMapper;
using Project.BLL.Interfaces;
using Project.DAL.Interfaces;
using Project.Models;
using Project.Models.ModelsDTO;
using Project.Validators;
using System.Threading.Tasks;

namespace Project.BLL
{
    public class PresentService : IPresentService
    {
        IPresentDal _presentDal;
        IMapper _mapper;
        public PresentService(IPresentDal presentDal, IMapper mapper)
        {
            _presentDal = presentDal;
            _mapper = mapper;
        }

        public async Task<Result<Present>> AddPresentAsync(PresentDto presentDto)
        {
            if (presentDto == null)
            {
                return new Result<Present>
                {
                    Success = false,
                    Message = "Present data is null",
                    Data = null
                };
            }

            if (!Validator.ValidName(presentDto.Name) || presentDto.Price <= 0 || presentDto.Quantity < 0 || presentDto.CategoryId <= 0 || presentDto.DonorId <= 0)
            {
                return new Result<Present>
                {
                    Success = false,
                    Message = "Invalid present data",
                    Data = null
                };
            }

            // Check for duplicate name
            var duplicateCheck = await _presentDal.PresentNameExistsAsync(presentDto.Name);
            if (duplicateCheck != null && duplicateCheck.Success && duplicateCheck.Data.FirstOrDefault())
            {
                return new Result<Present>
                {
                    Success = false,
                    Message = $"A present with name '{presentDto.Name}' already exists",
                    Data = null
                };
            }

            var present = _mapper.Map<Present>(presentDto);
            return await _presentDal.AddPresentAsync(present);
        }

        public async Task<Result<Present>> GetAllPresentsAsync(bool onlyActive = true)
        {
            return await _presentDal.GetAllPresentsAsync(onlyActive);
        }

        public async Task<Result<Present>> GetPresentByIdAsync(int id)
        {
            if (id <= 0)
            {
                return new Result<Present>
                {
                    Success = false,
                    Message = "Invalid present id",
                    Data = null
                };
            }

            return await _presentDal.GetPresentByIdAsync(id);
        }

        public async Task<Result<Present>> DeletePresentAsync(int presentId)
        {
            if (presentId <= 0)
            {
                return new Result<Present>
                {
                    Success = false,
                    Message = "Invalid present id",
                    Data = null
                };
            }

            return await _presentDal.DeletePresentAsync(presentId);
        }

        public async Task<Result<Present>> UpdatePresentAsync(int id, PresentDto presentDto)
        {
            if (id <= 0 || presentDto == null)
            {
                return new Result<Present>
                {
                    Success = false,
                    Message = "Invalid present id or data",
                    Data = null
                };
            }

            if (!Validator.ValidName(presentDto.Name) || presentDto.Price <= 0 || presentDto.Quantity < 0 || presentDto.CategoryId <= 0 || presentDto.DonorId <= 0)
            {
                return new Result<Present>
                {
                    Success = false,
                    Message = "Invalid present data",
                    Data = null
                };
            }

            // Check for duplicate name (excluding current present)
            var duplicateCheck = await _presentDal.PresentNameExistsAsync(presentDto.Name, id);
            if (duplicateCheck != null && duplicateCheck.Success && duplicateCheck.Data.FirstOrDefault())
            {
                return new Result<Present>
                {
                    Success = false,
                    Message = $"A present with name '{presentDto.Name}' already exists",
                    Data = null
                };
            }

            var present = _mapper.Map<Present>(presentDto);
            present.Id = id;
            return await _presentDal.UpdatePresentAsync(present);
        }

        public async Task<Result<Present>> GetPresentsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new Result<Present>
                {
                    Success = false,
                    Message = "Invalid name",
                    Data = null
                };
            }

            return await _presentDal.GetPresentsByNameAsync(name);
        }

        public async Task<Result<Present>> GetPresentsByDonorNameAsync(string donorName)
        {
            if (string.IsNullOrWhiteSpace(donorName))
            {
                return new Result<Present>
                {
                    Success = false,
                    Message = "Invalid donor name",
                    Data = null
                };
            }

            return await _presentDal.GetPresentsByDonorNameAsync(donorName);
        }

        public async Task<Result<Present>> GetPresentsByBuyerCountAsync(int buyerCount)
        {
            if (buyerCount < 0)
            {
                return new Result<Present>
                {
                    Success = false,
                    Message = "Invalid buyer count",
                    Data = null
                };
            }

            return await _presentDal.GetPresentsByBuyerCountAsync(buyerCount);
        }

        public async Task<Result<Present>> GetPresentsByPriceAsync()
        {
            return await _presentDal.GetPresentsByPriceAsync();
        }

        public async Task<Result<Present>> GetPresentsByCategoryAsync()
        {
            return await _presentDal.GetPresentsByCategoryAsync();
        }

        public async Task<Result<Present>> GetAllPresentsWithWinnersAsync()
        {
            return await _presentDal.GetAllPresentsWithWinnersAsync();
        }
    }
}