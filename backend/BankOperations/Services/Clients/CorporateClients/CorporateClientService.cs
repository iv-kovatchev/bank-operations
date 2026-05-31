using BankOperations.DTOs.Clients.CorporateClients;
using BankOperations.Entities;
using BankOperations.Entities.Clients;
using BankOperations.Exceptions;
using BankOperations.Mappers.Clients;
using BankOperations.Repositories.Clients;
using BankOperations.Services.Email;
using BankOperations.Services.Password;
using Microsoft.AspNetCore.Identity;

namespace BankOperations.Services.Clients.CorporateClients;

public class CorporateClientService : ICorporateClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IEmailService _emailService;

    public CorporateClientService(
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

    public async Task<CorporateClientResponseDto> CreateAsync(CreateCorporateClientDto dto, Guid createdByUserId)
    {
        if (await _clientRepository.ExistsByEmailAsync(dto.Email))
            throw new ConflictException("A client with this email already exists.");

        if (await _clientRepository.ExistsByEIKAsync(dto.EIK))
            throw new ConflictException("A client with this EIK already exists.");

        var password = _passwordGenerator.GeneratePassword();

        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            FirstName = dto.RepresentativeFirstName,
            LastName = dto.RepresentativeLastName
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new ValidationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, "Client");

        var client = new CorporateClient
        {
            ClientId = user.Id,
            CreatedByUserId = createdByUserId,
            CompanyName = dto.CompanyName,
            EIK = dto.EIK,
            RepresentativeFirstName = dto.RepresentativeFirstName,
            RepresentativeLastName = dto.RepresentativeLastName,
            User = user
        };

        await _clientRepository.AddAsync(client);
        await _clientRepository.SaveChangesAsync();

        await _emailService.SendWelcomeEmailAsync(dto.Email, dto.RepresentativeFirstName, password);

        return ClientMapper.ToDto(client);
    }

    public async Task<CorporateClientResponseDto> UpdateAsync(Guid id, UpdateCorporateClientDto dto)
    {
        var client = await _clientRepository.GetByIdWithDetailsAsync(id);

        if (client is not CorporateClient cc)
            throw new NotFoundException("CorporateClient", id);

        cc.CompanyName = dto.CompanyName;
        cc.RepresentativeFirstName = dto.RepresentativeFirstName;
        cc.RepresentativeLastName = dto.RepresentativeLastName;
        await _userManager.SetEmailAsync(cc.User, dto.Email);
        await _userManager.SetUserNameAsync(cc.User, dto.Email);

        await _clientRepository.UpdateAsync(cc);
        await _clientRepository.SaveChangesAsync();

        return ClientMapper.ToDto(cc);
    }
}
