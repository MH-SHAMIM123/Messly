using FluentValidation;
using Messly.Application.Common;
using Messly.Application.DTOs;
using Messly.Application.Interfaces.Persistence;
using Messly.Application.Interfaces.Security;
using Messly.Application.Interfaces.Services;
using Messly.Domain.Entities;
using Messly.Domain.Enums;

namespace Messly.Application.Services;

public class MemberService(
    IFlatMemberRepository memberRepository,
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    IValidator<MemberUpsertDto> validator,
    IFlatAuthorizationService authorization) : IMemberService
{
    public async Task<IReadOnlyList<MemberDto>> GetMembersAsync(CancellationToken cancellationToken = default)
    {
        authorization.EnsureCanRead();
        var flatId = authorization.GetCurrentFlatId();
        var members = await memberRepository.GetByFlatIdAsync(flatId, cancellationToken);
        return members.Select(MapToDto).ToList();
    }

    public async Task<MemberDto?> GetMemberAsync(Guid id, CancellationToken cancellationToken = default)
    {
        authorization.EnsureCanRead();
        var flatId = authorization.GetCurrentFlatId();
        var member = await memberRepository.GetByIdWithDetailsAsync(id, flatId, cancellationToken);
        return member is null ? null : MapToDto(member);
    }

    public async Task<Guid> SaveMemberAsync(MemberUpsertDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Id.HasValue)
        {
            await UpdateMemberAsync(dto, cancellationToken);
            return dto.Id.Value;
        }

        return await CreateMemberAsync(dto, cancellationToken);
    }

    public async Task<Guid> CreateMemberAsync(MemberUpsertDto dto, CancellationToken cancellationToken = default)
    {
        authorization.EnsureManager();
        var flatId = authorization.GetCurrentFlatId();

        if (dto.Id.HasValue)
            throw new BusinessException("Cannot create a member with an existing id. Use update instead.");

        await ValidateAsync(dto, cancellationToken);

        var role = await GetRoleAsync(dto.RoleType, cancellationToken);

        var existingUser = await userRepository.GetByEmailAsync(dto.Email.Trim(), cancellationToken);
        if (existingUser is not null)
            throw new BusinessException("A user with this email already exists. Each member must have a unique email.");

        var user = new User
        {
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim(),
            Phone = dto.Phone?.Trim(),
            IsActive = dto.IsActive
        };
        await userRepository.AddAsync(user, cancellationToken);

        var flatMember = new FlatMember
        {
            FlatId = flatId,
            UserId = user.Id,
            RoleId = role.Id,
            JoinedAt = DateTime.UtcNow,
            IsActive = dto.IsActive
        };
        await memberRepository.AddAsync(flatMember, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return flatMember.Id;
    }

    public async Task UpdateMemberAsync(MemberUpsertDto dto, CancellationToken cancellationToken = default)
    {
        authorization.EnsureManager();
        var flatId = authorization.GetCurrentFlatId();

        if (!dto.Id.HasValue)
            throw new BusinessException("Member id is required for update.");

        await ValidateAsync(dto, cancellationToken);

        var role = await GetRoleAsync(dto.RoleType, cancellationToken);

        var member = await memberRepository.GetByIdWithDetailsForUpdateAsync(dto.Id.Value, flatId, cancellationToken)
            ?? throw new NotFoundException();

        var emailOwner = await userRepository.GetByEmailAsync(dto.Email.Trim(), cancellationToken);
        if (emailOwner is not null && emailOwner.Id != member.UserId)
            throw new BusinessException("Email is already used by another user.");

        member.User!.FullName = dto.FullName.Trim();
        member.User.Email = dto.Email.Trim();
        member.User.Phone = dto.Phone?.Trim();
        member.User.IsActive = dto.IsActive;
        member.User.UpdatedAt = DateTime.UtcNow;
        member.RoleId = role.Id;
        member.IsActive = dto.IsActive;
        member.UpdatedAt = DateTime.UtcNow;

        userRepository.Update(member.User);
        memberRepository.Update(member);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteMemberAsync(Guid id, CancellationToken cancellationToken = default)
    {
        authorization.EnsureManager();
        var flatId = authorization.GetCurrentFlatId();

        var member = await memberRepository.GetByIdWithDetailsForUpdateAsync(id, flatId, cancellationToken)
            ?? throw new NotFoundException();

        if (member.Role?.RoleType == RoleType.Manager)
        {
            var managerCount = await memberRepository.CountActiveManagersAsync(flatId, cancellationToken);
            if (managerCount <= 1)
                throw new BusinessException("Cannot remove the last manager of this flat.");
        }

        memberRepository.Remove(member);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateAsync(MemberUpsertDto dto, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            throw new BusinessException(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
    }

    private async Task<Role> GetRoleAsync(RoleType roleType, CancellationToken cancellationToken)
        => await roleRepository.GetByRoleTypeAsync(roleType, cancellationToken)
           ?? throw new BusinessException("Invalid role.");

    private static MemberDto MapToDto(FlatMember member) => new()
    {
        Id = member.Id,
        FlatId = member.FlatId,
        UserId = member.UserId,
        FullName = member.User?.FullName ?? string.Empty,
        Email = member.User?.Email ?? string.Empty,
        Phone = member.User?.Phone,
        RoleType = member.Role?.RoleType ?? RoleType.Member,
        RoleName = member.Role?.Name ?? string.Empty,
        JoinedAt = member.JoinedAt,
        IsActive = member.IsActive
    };
}
