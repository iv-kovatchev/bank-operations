using BankOperations.Data;
using BankOperations.DTOs.Credits;
using BankOperations.DTOs.Credits.ConsumerCredits;
using BankOperations.DTOs.Credits.MortgageCredits;
using BankOperations.Entities;
using BankOperations.Entities.Credits;
using BankOperations.Enums;
using BankOperations.Exceptions;
using BankOperations.Mappers.Credits;
using BankOperations.Repositories.Clients;
using BankOperations.Repositories.Credits;
using BankOperations.Repositories.CreditServices;
using BankOperations.Services.ActivityLogs;
using Microsoft.EntityFrameworkCore;
using CreditServiceEntity = BankOperations.Entities.CreditService;

namespace BankOperations.Services.Credits;

public class CreditService : ICreditService
{
    private readonly ICreditRepository _creditRepository;
    private readonly ICreditServiceRepository _creditServiceRepository;
    private readonly IClientRepository _clientRepository;
    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLogService;

    public CreditService(
        ICreditRepository creditRepository,
        ICreditServiceRepository creditServiceRepository,
        IClientRepository clientRepository,
        ApplicationDbContext context,
        IActivityLogService activityLogService)
    {
        _creditRepository = creditRepository;
        _creditServiceRepository = creditServiceRepository;
        _clientRepository = clientRepository;
        _context = context;
        _activityLogService = activityLogService;
    }

    public async Task<IEnumerable<CreditResponseDto>> GetAllByClientIdAsync(Guid clientId, Guid requestingUserId, bool isAdmin)
    {
        var credits = await _creditRepository.GetAllByClientIdAsync(clientId);

        if (!isAdmin)
            credits = credits.Where(c => c.Client != null && c.Client.CreatedByUserId == requestingUserId);

        return credits.Select(CreditMapper.ToDto);
    }

    public async Task<CreditResponseDto> GetByIdAsync(Guid id, Guid requestingUserId, bool isAdmin)
    {
        var credit = await _creditRepository.GetByIdWithDetailsAsync(id)
            ?? throw new NotFoundException("Credit", id);

        if (!isAdmin && credit.Client.CreatedByUserId != requestingUserId)
            throw new UnauthorizedException("You do not have access to this credit.");

        return CreditMapper.ToDto(credit);
    }

    public async Task<CreditResponseDto> GrantConsumerCreditAsync(CreateConsumerCreditDto dto, Guid createdByUserId)
    {
        var creditService = await _creditServiceRepository.GetByIdAsync(dto.CreditServiceId)
            ?? throw new NotFoundException("CreditService", dto.CreditServiceId);

        ValidateCreditServiceLimits(creditService, dto.Amount, dto.TermMonths);

        var client = await _clientRepository.GetByIdWithDetailsAsync(dto.ClientId)
            ?? throw new NotFoundException("Client", dto.ClientId);

        if (!client.User.IsActive)
            throw new ValidationException("Cannot grant credit to an inactive client.");

        var credit = new ConsumerCredit
        {
            ClientId = dto.ClientId,
            CreditServiceId = dto.CreditServiceId,
            Amount = dto.Amount,
            TermMonths = dto.TermMonths,
            Status = CreditStatus.Active,
            CreatedByUserId = createdByUserId,
            Purpose = dto.Purpose
        };

        await _creditRepository.AddAsync(credit);
        await _creditRepository.SaveChangesAsync();

        var plan = GenerateRepaymentPlan(credit, creditService);
        await _context.RepaymentPlans.AddAsync(plan);
        await _creditRepository.SaveChangesAsync();

        await _activityLogService.LogAsync(createdByUserId, "GrantConsumerCredit", "Credit", credit.Id, $"Granted consumer credit of {dto.Amount}");

        return CreditMapper.ToDto(credit);
    }

    public async Task<CreditResponseDto> GrantMortgageCreditAsync(CreateMortgageCreditDto dto, Guid createdByUserId)
    {
        var creditService = await _creditServiceRepository.GetByIdAsync(dto.CreditServiceId)
            ?? throw new NotFoundException("CreditService", dto.CreditServiceId);

        ValidateCreditServiceLimits(creditService, dto.Amount, dto.TermMonths);

        var client = await _clientRepository.GetByIdWithDetailsAsync(dto.ClientId)
            ?? throw new NotFoundException("Client", dto.ClientId);

        if (!client.User.IsActive)
            throw new ValidationException("Cannot grant credit to an inactive client.");

        var credit = new MortgageCredit
        {
            ClientId = dto.ClientId,
            CreditServiceId = dto.CreditServiceId,
            Amount = dto.Amount,
            TermMonths = dto.TermMonths,
            Status = CreditStatus.Active,
            CreatedByUserId = createdByUserId,
            PropertyAddress = dto.PropertyAddress,
            PropertyType = dto.PropertyType
        };

        await _creditRepository.AddAsync(credit);
        await _creditRepository.SaveChangesAsync();

        var plan = GenerateRepaymentPlan(credit, creditService);
        await _context.RepaymentPlans.AddAsync(plan);
        await _creditRepository.SaveChangesAsync();

        await _activityLogService.LogAsync(createdByUserId, "GrantMortgageCredit", "Credit", credit.Id, $"Granted mortgage credit of {dto.Amount}");

        return CreditMapper.ToDto(credit);
    }

    public async Task<CreditResponseDto> UpdateConsumerCreditAsync(Guid id, UpdateConsumerCreditDto dto, Guid requestingUserId, bool isAdmin)
    {
        var cc = await ValidateAndPrepareUpdateAsync<ConsumerCredit>(id, requestingUserId, isAdmin);

        var creditService = cc.CreditService;

        if (dto.CreditServiceId != cc.CreditServiceId)
        {
            creditService = await _creditServiceRepository.GetByIdAsync(dto.CreditServiceId)
                ?? throw new NotFoundException("CreditService", dto.CreditServiceId);
            ValidateCreditServiceLimits(creditService, dto.Amount, dto.TermMonths);
        }

        cc.Purpose = dto.Purpose;
        cc.Amount = dto.Amount;
        cc.TermMonths = dto.TermMonths;
        cc.CreditServiceId = dto.CreditServiceId;

        await DeleteAndRegenerateRepaymentPlan(cc, creditService);
        await _creditRepository.SaveChangesAsync();

        await _activityLogService.LogAsync(requestingUserId, "UpdateConsumerCredit", "Credit", id, $"Updated consumer credit {id}");

        return CreditMapper.ToDto(cc);
    }

    public async Task<CreditResponseDto> UpdateMortgageCreditAsync(Guid id, UpdateMortgageCreditDto dto, Guid requestingUserId, bool isAdmin)
    {
        var mc = await ValidateAndPrepareUpdateAsync<MortgageCredit>(id, requestingUserId, isAdmin);

        var creditService = mc.CreditService;

        if (dto.CreditServiceId != mc.CreditServiceId)
        {
            creditService = await _creditServiceRepository.GetByIdAsync(dto.CreditServiceId)
                ?? throw new NotFoundException("CreditService", dto.CreditServiceId);
            ValidateCreditServiceLimits(creditService, dto.Amount, dto.TermMonths);
        }

        mc.PropertyAddress = dto.PropertyAddress;
        mc.PropertyType = dto.PropertyType;
        mc.Amount = dto.Amount;
        mc.TermMonths = dto.TermMonths;
        mc.CreditServiceId = dto.CreditServiceId;

        await DeleteAndRegenerateRepaymentPlan(mc, creditService);
        await _creditRepository.SaveChangesAsync();

        await _activityLogService.LogAsync(requestingUserId, "UpdateMortgageCredit", "Credit", id, $"Updated mortgage credit {id}");

        return CreditMapper.ToDto(mc);
    }

    public async Task<RepaymentPlanResponseDto> GetRepaymentPlanAsync(Guid creditId, Guid requestingUserId, bool isAdmin)
    {
        var credit = await _creditRepository.GetByIdWithDetailsAsync(creditId)
            ?? throw new NotFoundException("Credit", creditId);

        if (!isAdmin && credit.Client.CreatedByUserId != requestingUserId)
            throw new UnauthorizedException("You do not have access to this credit.");

        if (credit.RepaymentPlan == null)
            throw new NotFoundException("RepaymentPlan", creditId);

        return CreditMapper.ToDto(credit.RepaymentPlan);
    }

    private async Task<T> ValidateAndPrepareUpdateAsync<T>(Guid id, Guid requestingUserId, bool isAdmin) where T : Credit
    {
        var credit = await _creditRepository.GetByIdWithDetailsAsync(id)
            ?? throw new NotFoundException("Credit", id);

        if (credit is not T typedCredit)
            throw new NotFoundException(typeof(T).Name, id);

        if (!isAdmin && typedCredit.Client.CreatedByUserId != requestingUserId)
            throw new UnauthorizedException("You do not have access to this credit.");

        if (typedCredit.Status != CreditStatus.Active)
            throw new ValidationException("Cannot update a credit that is not active.");

        if (typedCredit.RepaymentPlan?.Installments.Any(i => i.PaidAt != null) == true)
            throw new ValidationException("Cannot update a credit with paid installments.");

        return typedCredit;
    }

    private static void ValidateCreditServiceLimits(CreditServiceEntity creditService, decimal amount, int termMonths)
    {
        if (amount > creditService.MaxAmount)
            throw new ValidationException($"Amount exceeds the maximum allowed ({creditService.MaxAmount}).");

        if (termMonths > creditService.MaxTermMonths)
            throw new ValidationException($"Term exceeds the maximum allowed ({creditService.MaxTermMonths} months).");
    }

    private async Task DeleteAndRegenerateRepaymentPlan(Credit credit, CreditServiceEntity creditService)
    {
        var existingPlan = await _context.RepaymentPlans
            .Include(rp => rp.Installments)
            .FirstOrDefaultAsync(rp => rp.CreditId == credit.Id);
        if (existingPlan != null)
        {
            _context.RepaymentInstallments.RemoveRange(existingPlan.Installments);
            _context.RepaymentPlans.Remove(existingPlan);
        }

        var plan = GenerateRepaymentPlan(credit, creditService);
        await _context.RepaymentPlans.AddAsync(plan);
    }

    private static RepaymentPlan GenerateRepaymentPlan(Credit credit, CreditServiceEntity creditService)
    {
        decimal monthlyRate = creditService.InterestRate / 100 / 12;
        decimal monthlyInstallment = credit.Amount *
            (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), credit.TermMonths)) /
            ((decimal)Math.Pow((double)(1 + monthlyRate), credit.TermMonths) - 1);

        monthlyInstallment = Math.Round(monthlyInstallment, 2);

        var plan = new RepaymentPlan
        {
            CreditId = credit.Id,
            MonthlyInstallment = monthlyInstallment,
            GeneratedAt = DateTime.UtcNow
        };

        decimal remainingBalance = credit.Amount;
        var installments = new List<RepaymentInstallment>();

        for (int i = 1; i <= credit.TermMonths; i++)
        {
            decimal interest = Math.Round(remainingBalance * monthlyRate, 2);
            decimal principal = Math.Round(monthlyInstallment - interest, 2);

            if (i == credit.TermMonths)
                principal = remainingBalance;

            remainingBalance = Math.Round(remainingBalance - principal, 2);

            installments.Add(new RepaymentInstallment
            {
                RepaymentPlanId = plan.Id,
                InstallmentNumber = i,
                DueDate = DateTime.UtcNow.AddMonths(i),
                PrincipalPart = principal,
                InterestPart = interest,
                RemainingBalance = remainingBalance
            });
        }

        plan.Installments = installments;
        return plan;
    }
}
