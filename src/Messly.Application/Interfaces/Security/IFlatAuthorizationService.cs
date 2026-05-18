namespace Messly.Application.Interfaces.Security;

public interface IFlatAuthorizationService
{
    Guid GetCurrentFlatId();
    Task EnsureUserIsActiveMemberAsync(Guid userId, CancellationToken cancellationToken = default);
    void EnsureManager();
    void EnsureCanRead();
    void EnsureCanWrite();
}
