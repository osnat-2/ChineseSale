using AutoMapper;
using Project.BLL.Interfaces;
using Project.DAL.Interfaces;
using Project.Models;
using Project.Models.ModelsDTO;
using Project.Validators;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BLL
{
    public class DonorService : IDonorService
    {
        private readonly IDonorDal _donorDAL;
        private readonly IMapper _mapper;

        public DonorService(IDonorDal donorDal, IMapper mapper)
        {
            _donorDAL = donorDal;
            _mapper = mapper;
        }

        public async Task<Result<Donor>> GetAllDonorsAsync(bool onlyActive = true)
        {
            return await _donorDAL.GetAllDonorsAsync(onlyActive);
        }

        public async Task<Result<Donor>> GetDonorByIdAsync(int id)
        {
            if (id <= 0)
            {
                return new Result<Donor>
                {
                    Success = false,
                    Message = "Invalid donor ID.",
                    Data = null
                };
            }

            return await _donorDAL.GetDonorByIdAsync(id);
        }

        public async Task<Result<Donor>> AddDonorAsync(DonorDto donorDto, int createdById)
        {
            if (donorDto == null)
            {
                return new Result<Donor>
                {
                    Success = false,
                    Message = "Donor data is required.",
                    Data = null
                };
            }

            if (!Validator.ValidateData(donorDto.Name, donorDto.Email, donorDto.Phone))
            {
                return new Result<Donor>
                {
                    Success = false,
                    Message = "Name, email and phone must be valid.",
                    Data = null
                };
            }

            if (await _donorDAL.ExistingEmailAsync(donorDto.Email))
            {
                return new Result<Donor>
                {
                    Success = false,
                    Message = "A donor with this email already exists.",
                    Data = null
                };
            }

            var donor = _mapper.Map<Donor>(donorDto);
            donor.IsActive = true;
            donor.CreatedAt = DateTime.UtcNow;
            donor.CreatedBy = createdById;
            donor.UpdatedAt = null;

            return await _donorDAL.AddDonorAsync(donor);
        }

        public async Task<Result<Donor>> UpdateDonorAsync(int id, DonorDto donorDto, int updatedById)
        {
            if (id <= 0)
            {
                return new Result<Donor>
                {
                    Success = false,
                    Message = "Invalid donor ID.",
                    Data = null
                };
            }

            if (donorDto == null)
            {
                return new Result<Donor>
                {
                    Success = false,
                    Message = "Donor data is required.",
                    Data = null
                };
            }

            if (!Validator.ValidateData(donorDto.Name, donorDto.Email, donorDto.Phone))
            {
                return new Result<Donor>
                {
                    Success = false,
                    Message = "Name, email and phone must be valid.",
                    Data = null
                };
            }

            if (await _donorDAL.ExistingEmailAsync(donorDto.Email, id))
            {
                return new Result<Donor>
                {
                    Success = false,
                    Message = "Another donor with this email already exists.",
                    Data = null
                };
            }

            var existingResponse = await _donorDAL.GetDonorByIdAsync(id);
            var existingDonor = existingResponse?.Data?.FirstOrDefault();
            if (existingDonor == null || !existingResponse.Success)
            {
                return new Result<Donor>
                {
                    Success = false,
                    Message = $"Donor with ID {id} not found.",
                    Data = null
                };
            }

            existingDonor.Name = donorDto.Name;
            existingDonor.Email = donorDto.Email;
            existingDonor.Phone = donorDto.Phone;
            existingDonor.UpdatedAt = DateTime.UtcNow;

            return await _donorDAL.UpdateDonorAsync(existingDonor);
        }

        public async Task<Result<Donor>> DeleteDonorAsync(int id)
        {
            if (id <= 0)
            {
                return new Result<Donor>
                {
                    Success = false,
                    Message = "Invalid donor ID.",
                    Data = null
                };
            }

            if (await _donorDAL.DonorHasPresentsAsync(id))
            {
                return new Result<Donor>
                {
                    Success = false,
                    Message = "Cannot delete donor with active presents.",
                    Data = null
                };
            }

            return await _donorDAL.SoftDeleteDonorAsync(id);
        }

        public Task<Result<Donor>> GetDonorsByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Task.FromResult(new Result<Donor>
                {
                    Success = false,
                    Message = "Email is required.",
                    Data = null
                });
            }

            return _donorDAL.GetDonorsByEmailAsync(email);
        }

        public Task<Result<Donor>> GetDonorsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Task.FromResult(new Result<Donor>
                {
                    Success = false,
                    Message = "Name is required.",
                    Data = null
                });
            }

            return _donorDAL.GetDonorsByNameAsync(name);
        }

        public Task<Result<Donor>> GetDonorsByPresentNameAsync(string presentName)
        {
            if (string.IsNullOrWhiteSpace(presentName))
            {
                return Task.FromResult(new Result<Donor>
                {
                    Success = false,
                    Message = "Present name is required.",
                    Data = null
                });
            }

            return _donorDAL.GetDonorsByPresentNameAsync(presentName);
        }
    }
}