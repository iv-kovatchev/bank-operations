using BankOperations.DTOs.Clients.IndividualClients;
using BankOperations.Entities;
using BankOperations.Entities.Clients;
using BankOperations.Exceptions;
using BankOperations.Mappers.Clients;
using BankOperations.Repositories.Clients;
using BankOperations.Services.Email;
using BankOperations.Services.Password;
using Microsoft.AspNetCore.Identity;

namespace BankOperations.Services.Clients.IndividualClients;

public class IndividualClientService : IIndividualClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IEmailService _emailService;

    public IndividualClientService(
        IClientRepository clientRepository,
        UserManager<ApplicationUser> userManager,
        IPasswordGenerator passwordGenerator,
        IEmailService emailService)
    {
        _clientRepository = clientRepository;
        _userManager = userManager;
        _passwordGenerator = passwordGenerator;
        _emailService = emailService;
    }

    public async Task<IndividualClientResponseDto> CreateAsync(CreateIndividualClientDto dto, Guid createdByUserId)
    {
        if (await _clientRepository.ExistsByEmailAsync(dto.Email))
            throw new ConflictException("A client with this email already exists.");

        if (await _clientRepository.ExistsByEGNAsync(dto.EGN))
            throw new ConflictException("A client with this EGN already exists.");

        var password = _passwordGenerator.GeneratePassword();

        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new ValidationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, "Client");

        var client = new IndividualClient
        {
            ClientId = user.Id,
            CreatedByUserId = createdByUserId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            EGN = dto.EGN,
            User = user
        };

        await _clientRepository.AddAsync(client);
        await _clientRepository.SaveChangesAsync();

        await _emailService.SendWelcomeEmailAsync(dto.Email, dto.FirstName, password);

        return ClientMapper.ToDto(client);
    }

    public async Task<IndividualClientResponseDto> UpdateAsync(Guid id, UpdateIndividualClientDto dto)
    {
        var client = await _clientRepository.GetByIdWithDetailsAsync(id);

        if (client is not IndividualClient ic)
            throw new NotFoundException("IndividualClient", id);

        ic.FirstName = dto.FirstName;
        ic.LastName = dto.LastName;
        ic.User.FirstName = dto.FirstName;
        ic.User.LastName = dto.LastName;
        await _userManager.SetEmailAsync(ic.User, dto.Email);
        await _userManager.SetUserNameAsync(ic.User, dto.Email);

        await _clientRepository.UpdateAsync(ic);
        await _clientRepository.SaveChangesAsync();

        return ClientMapper.ToDto(ic);
    }
}
