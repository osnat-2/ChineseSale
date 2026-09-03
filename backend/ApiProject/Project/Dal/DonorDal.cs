using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.DAL.Interfaces;
using Project.Models;
using Project.Models.ModelsDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.DAL
{
    public class DonorDal : IDonorDal
    {
        private readonly AppDBContext dbContext;
        private readonly ILogger<DonorDal> logger;

        public DonorDal(AppDBContext dbContext, ILogger<DonorDal> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        public async Task<Result<Donor>> GetAllDonorsAsync(bool onlyActive = true)
        {
            try
            {
                var query = dbContext.Donor
                    .Include(d => d.Presents)
                    .AsQueryable();

                if (onlyActive)
                {
                    query = query.Where(d => d.IsActive == true);
                }
                

                var donors = await query.ToListAsync();

                return new Result<Donor>
                {
                    Success = true,
                    Message = "Fetched donors successfully.",
                    Data = donors
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching donors.");
                return new Result<Donor>
                {
                    Success = false,
                    Message = "Error fetching donors: " + ex.Message,
                    Data = Enumerable.Empty<Donor>()
                };
            }
        }

        public async Task<Result<Donor>> GetDonorByIdAsync(int id)
        {
            try
            {
                var donor = await dbContext.Donor
                    .Include(d => d.Presents)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (donor == null)
                {
                    return new Result<Donor>
                    {
                        Success = false,
                        Message = $"Donor with ID {id} not found.",
                        Data = Enumerable.Empty<Donor>()
                    };
                }

                return new Result<Donor>
                {
                    Success = true,
                    Message = "Fetched donor successfully.",
                    Data = new List<Donor> { donor }
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error fetching donor by id {id}.");
                return new Result<Donor>
                {
                    Success = false,
                    Message = $"Error fetching donor by id {id}: {ex.Message}",
                    Data = Enumerable.Empty<Donor>()
                };
            }
        }

        public async Task<Result<Donor>> AddDonorAsync(Donor donor)
        {
            try
            {
                await dbContext.Donor.AddAsync(donor);
                await dbContext.SaveChangesAsync();

                return new Result<Donor>
                {
                    Success = true,
                    Message = $"Donor with email {donor.Email} added successfully.",
                    Data = new List<Donor> { donor }
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error adding donor with email {donor.Email}.");
                return new Result<Donor>
                {
                    Success = false,
                    Message = $"Error adding donor with email {donor.Email}: {ex.Message}",
                    Data = Enumerable.Empty<Donor>()
                };
            }
        }

        public async Task<Result<Donor>> UpdateDonorAsync(Donor donor)
        {
            try
            {
                var existing = await dbContext.Donor.FindAsync(donor.Id);
                if (existing == null)
                {
                    return new Result<Donor>
                    {
                        Success = false,
                        Message = $"Donor with ID {donor.Id} not found.",
                        Data = Enumerable.Empty<Donor>()
                    };
                }

                existing.Name = donor.Name;
                existing.Email = donor.Email;
                existing.Phone = donor.Phone;
                existing.IsActive = donor.IsActive;
                existing.UpdatedAt = donor.UpdatedAt;
                existing.CreatedBy = donor.CreatedBy;
                existing.CreatedAt = donor.CreatedAt;

                dbContext.Donor.Update(existing);
                await dbContext.SaveChangesAsync();

                return new Result<Donor>
                {
                    Success = true,
                    Message = $"Donor with ID {donor.Id} updated successfully.",
                    Data = new List<Donor> { existing }
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error updating donor with ID {donor.Id}.");
                return new Result<Donor>
                {
                    Success = false,
                    Message = $"Error updating donor with ID {donor.Id}: {ex.Message}",
                    Data = Enumerable.Empty<Donor>()
                };
            }
        }

        public async Task<Result<Donor>> SoftDeleteDonorAsync(int id)
        {
            try
            {
                var donor = await dbContext.Donor.FindAsync(id);
                if (donor == null)
                {
                    return new Result<Donor>
                    {
                        Success = false,
                        Message = $"Donor with ID {id} not found.",
                        Data = Enumerable.Empty<Donor>()
                    };
                }

                donor.IsActive = false;
                donor.UpdatedAt = DateTime.UtcNow;
                dbContext.Donor.Update(donor);
                await dbContext.SaveChangesAsync();

                return new Result<Donor>
                {
                    Success = true,
                    Message = $"Donor with ID {id} deactivated successfully.",
                    Data = new List<Donor> { donor }
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deactivating donor with ID {id}.");
                return new Result<Donor>
                {
                    Success = false,
                    Message = $"Error deactivating donor with ID {id}: {ex.Message}",
                    Data = Enumerable.Empty<Donor>()
                };
            }
        }

        public async Task<Result<Donor>> GetDonorsByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return new Result<Donor>
                    {
                        Success = false,
                        Message = "Email filter is required.",
                        Data = Enumerable.Empty<Donor>()
                    };
                }

                var normalized = email.Trim().ToLower();
                var donors = await dbContext.Donor
                    .Include(d => d.Presents)
                    .Where(d => d.Email.ToLower().Contains(normalized))
                    .ToListAsync();

                return new Result<Donor>
                {
                    Success = true,
                    Message = $"Fetched donors matching email '{email}' successfully.",
                    Data = donors
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error fetching donors by email {email}.");
                return new Result<Donor>
                {
                    Success = false,
                    Message = $"Error fetching donors by email {email}: {ex.Message}",
                    Data = Enumerable.Empty<Donor>()
                };
            }
        }

        public async Task<Result<Donor>> GetDonorsByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new Result<Donor>
                    {
                        Success = false,
                        Message = "Name filter is required.",
                        Data = Enumerable.Empty<Donor>()
                    };
                }

                var normalized = name.Trim().ToLower();
                var donors = await dbContext.Donor
                    .Include(d => d.Presents)
                    .Where(d => d.Name.ToLower().Contains(normalized))
                    .ToListAsync();

                return new Result<Donor>
                {
                    Success = true,
                    Message = $"Fetched donors matching name '{name}' successfully.",
                    Data = donors
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error fetching donors by name {name}.");
                return new Result<Donor>
                {
                    Success = false,
                    Message = $"Error fetching donors by name {name}: {ex.Message}",
                    Data = Enumerable.Empty<Donor>()
                };
            }
        }

        public async Task<Result<Donor>> GetDonorsByPresentNameAsync(string presentName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(presentName))
                {
                    return new Result<Donor>
                    {
                        Success = false,
                        Message = "Present name filter is required.",
                        Data = Enumerable.Empty<Donor>()
                    };
                }

                var normalized = presentName.Trim().ToLower();
                var donors = await dbContext.Donor
                    .Include(d => d.Presents)
                    .Where(d => dbContext.Present.Any(p => p.DonorId == d.Id && p.Name.ToLower().Contains(normalized)))
                    .ToListAsync();

                return new Result<Donor>
                {
                    Success = true,
                    Message = $"Fetched donors for present name '{presentName}' successfully.",
                    Data = donors
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error fetching donors by present name {presentName}.");
                return new Result<Donor>
                {
                    Success = false,
                    Message = $"Error fetching donors by present name '{presentName}': {ex.Message}",
                    Data = Enumerable.Empty<Donor>()
                };
            }
        }

        public async Task<bool> ExistingEmailAsync(string email, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return false;
                }

                var normalized = email.Trim().ToLower();
                return await dbContext.Donor
                    .AnyAsync(d => d.Email.ToLower() == normalized && (!excludeId.HasValue || d.Id != excludeId.Value));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error checking existing email {email}.");
                return true;
            }
        }

        public async Task<bool> DonorHasPresentsAsync(int donorId)
        {
            try
            {
                return await dbContext.Present.AnyAsync(p => p.DonorId == donorId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error checking presents for donor id {donorId}.");
                return true;
            }
        }
    }
}