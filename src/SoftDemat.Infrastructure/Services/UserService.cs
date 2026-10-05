using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using SoftDemat.Application.Common;
using SoftDemat.Application.DTOs;
using SoftDemat.Application.Services.Interfaces;
using SoftDemat.Domain.Constants;
using SoftDemat.Domain.Entities;
using SoftDemat.Domain.Enums;
using SoftDemat.Domain.Exceptions;
using SoftDemat.Domain.Interfaces;
using SoftDemat.Domain.Rules;

namespace SoftDemat.Infrastructure.Services;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IUserSecurityRepository _securities;
    private readonly IAuthSessionRepository _sessions;
    private readonly IPasswordHasher _hasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(
        AuthStores stores,
        IPasswordHasher hasher,
        IUnitOfWork unitOfWork,
        ILogger<UserService> logger)
    {
        _users = stores.Users;
        _securities = stores.Securities;
        _sessions = stores.Sessions;
        _hasher = hasher;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PaginatedResult<UserResponse>> SearchAsync(PaginationQuery query, CancellationToken cancellationToken = default)
    {
        var page = PageRequest.Normalize(query.Page, query.Size);
        var result = await _users.SearchAsync(
            query.Search,
            query.SortBy,
            PageRequest.IsDescending(query.SortDirection),
            page.Skip,
            page.Size,
            cancellationToken);
        var items = result.Items.Select(ToResponse).ToList();
        return new PaginatedResult<UserResponse>(items, result.TotalCount, page.Page, page.Size, PageRequest.PageCount(result.TotalCount, page.Size));
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var username = request.Username.Trim();
        if (await _users.UsernameExistsAsync(username, null, cancellationToken))
            throw new ConflictException("Cet identifiant existe déjà.");

        var user = new UserAccount
        {
            Name = request.Name.Trim(),
            Username = username,
            Pc = request.Pc.Trim(),
            Role = (UserRole)request.RoleId,
            Password = _hasher.Hash(request.Password)
        };
        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Utilisateur {Username} créé", user.Username);
        return ToResponse(user);
    }

    public async Task<UserResponse> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await GetEditableAsync(id, cancellationToken);
        var username = request.Username.Trim();
        if (await _users.UsernameExistsAsync(username, id, cancellationToken))
            throw new ConflictException("Cet identifiant existe déjà.");

        user.Name = request.Name.Trim();
        user.Username = username;
        user.Pc = request.Pc.Trim();
        user.Role = (UserRole)request.RoleId;
        if (!string.IsNullOrEmpty(request.Password))
            user.Password = _hasher.Hash(request.Password);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(user);
    }

    public async Task DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default)
    {
        if (id == currentUserId)
            throw new DomainException("Vous ne pouvez pas supprimer votre propre compte.");

        var user = await GetEditableAsync(id, cancellationToken);
        _users.Remove(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Utilisateur {Username} supprimé", user.Username);
    }

    public async Task<ResetPasswordResponse> ResetPasswordAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await GetEditableAsync(id, cancellationToken);
        var temporary = CreateTemporaryPassword();
        user.Password = _hasher.Hash(temporary);
        var security = await _securities.GetAsync(user.Id, cancellationToken);
        if (security is null)
            await _securities.AddAsync(new UserSecurity { UserId = user.Id, MustChangePassword = true }, cancellationToken);
        else
            security.MustChangePassword = true;

        await _sessions.RevokeActiveAsync(user.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Mot de passe réinitialisé pour {Username}", user.Username);
        return new ResetPasswordResponse(temporary);
    }

    private async Task<UserAccount> GetEditableAsync(int id, CancellationToken cancellationToken)
    {
        if (id == AuthConstants.SystemUserId)
            throw new DomainException("Ce compte ne peut pas être modifié.");

        return await _users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Utilisateur introuvable.");
    }

    private static UserResponse ToResponse(UserAccount user)
        => new(user.Id, user.Name, user.Username, user.Role.ToString(), user.Pc);

    private static string CreateTemporaryPassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@#$%*";
        var alphabet = upper + lower + digits + special;
        var bytes = RandomNumberGenerator.GetBytes(16);
        var chars = new char[16];
        chars[0] = upper[bytes[0] % upper.Length];
        chars[1] = lower[bytes[1] % lower.Length];
        chars[2] = digits[bytes[2] % digits.Length];
        chars[3] = special[bytes[3] % special.Length];
        for (var index = 4; index < chars.Length; index++)
            chars[index] = alphabet[bytes[index] % alphabet.Length];

        return new string(chars);
    }
}
