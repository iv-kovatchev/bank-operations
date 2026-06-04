using BankOperations.DTOs.BankAccounts;
using BankOperations.Entities;
using BankOperations.Enums;
using BankOperations.Exceptions;
using BankOperations.Mappers.BankAccounts;
using BankOperations.Repositories.BankAccounts;
using BankOperations.Repositories.Clients;

namespace BankOperations.Services.BankAccounts;

public class BankAccountService : IBankAccountService
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IClientRepository _clientRepository;

    public BankAccountService(IBankAccountRepository bankAccountRepository, IClientRepository clientRepository)
    {
        _bankAccountRepository = bankAccountRepository;
        _clientRepository = clientRepository;
    }

    public async Task<IEnumerable<BankAccountResponseDto>> GetAllByClientIdAsync(Guid clientId)
    {
        var client = await _clientRepository.GetByIdAsync(clientId)
            ?? throw new NotFoundException("Client", clientId);

        var accounts = await _bankAccountRepository.GetAllByClientIdAsync(clientId);
        return accounts.Select(BankAccountMapper.ToDto);
    }

    public async Task<BankAccountResponseDto> OpenAccountAsync(Guid clientId, CreateBankAccountDto dto, Guid createdByUserId)
    {
        var client = await _clientRepository.GetByIdAsync(clientId)
            ?? throw new NotFoundException("Client", clientId);

        if (await _bankAccountRepository.ExistsByIbanAsync(dto.IBAN))
            throw new ConflictException($"IBAN {dto.IBAN} already exists.");

        var account = new BankAccount
        {
            ClientId = clientId,
            IBAN = dto.IBAN,
            Balance = dto.InitialBalance,
            Status = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdByUserId
        };

        await _bankAccountRepository.AddAsync(account);
        await _bankAccountRepository.SaveChangesAsync();

        return BankAccountMapper.ToDto(account);
    }

    public async Task CloseAccountAsync(Guid id)
    {
        var account = await _bankAccountRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("BankAccount", id);

        account.Status = AccountStatus.Closed;
        await _bankAccountRepository.UpdateAsync(account);
        await _bankAccountRepository.SaveChangesAsync();
    }

}
