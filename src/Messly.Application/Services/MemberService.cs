using FluentValidation;
using Messly.Application.Common;
using Messly.Application.DTOs;
using Messly.Application.Interfaces.Persistence;
using Messly.Application.Interfaces.Services;
using Messly.Domain.Entities;
using Messly.Domain.Enums;

namespace Messly.Application.Services;

public class MemberService(
    IFlatMemberRepository memberRepository,
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IUnitOfWork unitOfWork,
    IValidator<MemberUpsertDto> validator) : IMemberService
{
    public async Task<IReadOnlyList<MemberDto>> GetMembersAsync(Guid flatId, CancellationToken cancellationToken = default)
    {
        var members = await memberRepository.GetByFlatIdAsync(flatId, cancellationToken);
        return members.Select(MapToDto).ToList();
    }

    public async Task<MemberDto?> GetMemberAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var member = await memberRepository.GetByIdWithDetailsAsync(id, cancellationToken);
        return member is null ? null : MapToDto(member);
    }

    public async Task<Guid> SaveMemberAsync(MemberUpsertDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            throw new BusinessException(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));

        var role = await roleRepository.GetByRoleTypeAsync(dto.RoleType, cancellationToken)
            ?? throw new BusinessException("Invalid role.");

        if (dto.Id.HasValue)
        {
            var member = await memberRepository.GetByIdWithDetailsAsync(dto.Id.Value, cancellationToken)
                ?? throw new BusinessException("Member not found.");

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
            return member.Id;
        }

        var existingUser = await userRepository.GetByEmailAsync(dto.Email.Trim(), cancellationToken);
        if (existingUser is not null)
            throw new BusinessException("A user with this email already exists. Use a unique email for each member.");

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
            FlatId = dto.FlatId,
            UserId = user.Id,
            RoleId = role.Id,
            JoinedAt = DateTime.UtcNow,
            IsActive = dto.IsActive
        };
        await memberRepository.AddAsync(flatMember, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return flatMember.Id;
    }

    public async Task DeleteMemberAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var member = await memberRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new BusinessException("Member not found.");

        if (member.Role?.RoleType == RoleType.Manager)
        {
            var managerCount = await memberRepository.CountActiveManagersAsync(member.FlatId, cancellationToken);
            if (managerCount <= 1)
                throw new BusinessException("Cannot remove the last manager of this flat.");
        }

        memberRepository.Remove(member);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

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
