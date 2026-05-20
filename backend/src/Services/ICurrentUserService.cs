namespace backend.src.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
}
