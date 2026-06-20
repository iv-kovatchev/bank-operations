using BankOperations.DTOs.BankAccounts;
using BankOperations.Entities;
using BankOperations.Enums;
using BankOperations.Exceptions;
using BankOperations.Mappers.BankAccounts;
using BankOperations.Repositories.BankAccounts;
using BankOperations.Repositories.Clients;
using BankOperations.Services.ActivityLogs;

namespace BankOperations.Services.BankAccounts;

public class BankAccountService : IBankAccountService
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IActivityLogService _activityLogService;

    public BankAccountService(
        IBankAccountRepository bankAccountRepository,
        IClientRepository clientRepository,
        IActivityLogService activityLogService)
    {
        _bankAccountRepository = bankAccountRepository;
        _clientRepository = clientRepository;
        _activityLogService = activityLogService;
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
        var client = await _clientRepository.GetByIdWithDetailsAsync(clientId)
            ?? throw new NotFoundException("Client", clientId);

        if (!client.User.IsActive)
            throw new ValidationException("Cannot open an account for an inactive client.");

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

        await _activityLogService.LogAsync(createdByUserId, "OpenAccount", "BankAccount", account.Id, $"Opened account {dto.IBAN}");

        return BankAccountMapper.ToDto(account);
    }

    public async Task CloseAccountAsync(Guid id, Guid requestingUserId, bool isAdmin)
    {
        var account = await _bankAccountRepository.GetByIdWithClientAsync(id)
            ?? throw new NotFoundException("BankAccount", id);

        if (!isAdmin && account.Client.CreatedByUserId != requestingUserId)
            throw new UnauthorizedException("You do not have access to this account.");

        account.Status = AccountStatus.Closed;
        await _bankAccountRepository.UpdateAsync(account);
        await _bankAccountRepository.SaveChangesAsync();

        await _activityLogService.LogAsync(requestingUserId, "CloseAccount", "BankAccount", id, $"Closed account {id}");
    }

    public async Task DeleteAccountAsync(Guid id)
    {
        var account = await _bankAccountRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("BankAccount", id);

        await _bankAccountRepository.DeleteAsync(account.Id);
    }

    public async Task<BankAccountResponseDto> DepositAsync(Guid id, decimal amount, Guid requestingUserId, bool isAdmin)
    {
        var account = await _bankAccountRepository.GetByIdWithClientAsync(id)
            ?? throw new NotFoundException("BankAccount", id);

        if (!isAdmin && account.Client.CreatedByUserId != requestingUserId)
            throw new UnauthorizedException("You do not have access to this account.");

        if (account.Status != AccountStatus.Active)
            throw new ValidationException("Cannot deposit to a closed account.");

        account.Balance += amount;
        await _bankAccountRepository.UpdateAsync(account);
        await _bankAccountRepository.SaveChangesAsync();

        await _activityLogService.LogAsync(requestingUserId, "Deposit", "BankAccount", id, $"Deposited {amount} to account {id}");

        return BankAccountMapper.ToDto(account);
    }

    public async Task<BankAccountResponseDto> WithdrawAsync(Guid id, decimal amount, Guid requestingUserId, bool isAdmin)
    {
        var account = await _bankAccountRepository.GetByIdWithClientAsync(id)
            ?? throw new NotFoundException("BankAccount", id);

        if (!isAdmin && account.Client.CreatedByUserId != requestingUserId)
            throw new UnauthorizedException("You do not have access to this account.");

        if (account.Status != AccountStatus.Active)
            throw new ValidationException("Cannot withdraw from a closed account.");

        if (account.Balance < amount)
            throw new ValidationException("Insufficient funds.");

        account.Balance -= amount;
        await _bankAccountRepository.UpdateAsync(account);
        await _bankAccountRepository.SaveChangesAsync();

        await _activityLogService.LogAsync(requestingUserId, "Withdraw", "BankAccount", id, $"Withdrew {amount} from account {id}");

        return BankAccountMapper.ToDto(account);
    }

}
