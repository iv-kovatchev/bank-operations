using BankOperations.DTOs.CreditServices;
using BankOperations.Exceptions;
using BankOperations.Mappers.Credits;
using BankOperations.Repositories.CreditServices;
using CreditServiceEntity = BankOperations.Entities.CreditService;

namespace BankOperations.Services.CreditServices;

public class CreditServiceService : ICreditServiceService
{
    private readonly ICreditServiceRepository _creditServiceRepository;

    public CreditServiceService(ICreditServiceRepository creditServiceRepository)
    {
        _creditServiceRepository = creditServiceRepository;
    }

    public async Task<IEnumerable<CreditServiceResponseDto>> GetAllAsync()
    {
        var creditServices = await _creditServiceRepository.GetAllAsync();

        return creditServices.Select(CreditMapper.ToDto);
    }

    public async Task<CreditServiceResponseDto> GetByIdAsync(Guid id)
    {
        var creditService = await _creditServiceRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("CreditService", id);

        return CreditMapper.ToDto(creditService);
    }

    public async Task<CreditServiceResponseDto> CreateAsync(CreateCreditServiceDto dto)
    {
        if (await _creditServiceRepository.ExistsByNameAsync(dto.Name))
            throw new ConflictException("Credit service with this name already exists.");

        var creditService = new CreditServiceEntity
        {
            Name = dto.Name,
            Type = dto.Type,
            InterestRate = dto.InterestRate,
            MaxAmount = dto.MaxAmount,
            MaxTermMonths = dto.MaxTermMonths
        };

        await _creditServiceRepository.AddAsync(creditService);
        await _creditServiceRepository.SaveChangesAsync();

        return CreditMapper.ToDto(creditService);
    }

    public async Task<CreditServiceResponseDto> UpdateAsync(Guid id, UpdateCreditServiceDto dto)
    {
        var creditService = await _creditServiceRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("CreditService", id);

        var nameExists = await _creditServiceRepository.ExistsByNameAsync(dto.Name);
        
        if (dto.Name != creditService.Name && nameExists)
            throw new ConflictException("Credit service with this name already exists.");

        creditService.Name = dto.Name;
        creditService.Type = dto.Type;
        creditService.InterestRate = dto.InterestRate;
        creditService.MaxAmount = dto.MaxAmount;
        creditService.MaxTermMonths = dto.MaxTermMonths;

        await _creditServiceRepository.UpdateAsync(creditService);
        await _creditServiceRepository.SaveChangesAsync();

        return CreditMapper.ToDto(creditService);
    }

    public async Task DeleteAsync(Guid id)
    {
        var creditService = await _creditServiceRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("CreditService", id);

        await _creditServiceRepository.DeleteAsync(creditService.Id);
        await _creditServiceRepository.SaveChangesAsync();
    }
}
