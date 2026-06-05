using backend.src.Common.Exceptions;
using backend.src.Data;
using backend.src.Entities;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.src.Handlers.Users;

public class UpdateAvatarRequest
{
    public string ImagePath { get; set; } = string.Empty;
}

public class UpdateAvatarResponse
{
    public Guid UserId { get; set; }
    public string? AvatarUrl { get; set; }
}

public interface IUpdateAvatarHandler
{
    Task<UpdateAvatarResponse> HandleAsync(UpdateAvatarRequest request, CancellationToken ct);
}

public class UpdateAvatarHandler : IUpdateAvatarHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISupabaseStorageService _storageService;
    private readonly AppDbContext _context;

    public UpdateAvatarHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        ISupabaseStorageService storageService,
        AppDbContext context)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _storageService = storageService;
        _context = context;
    }

    public async Task<UpdateAvatarResponse> HandleAsync(UpdateAvatarRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ImagePath))
        {
            throw new BusinessRuleException("image_path_required", "O caminho da imagem é obrigatório.");
        }

        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new BusinessRuleException("user_not_found", "Usuário não encontrado.");
        }

        // Delete old avatar if exists
        if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
        {
            var oldPath = user.AvatarUrl;
            // Path is stored as "avatars/userId/filename.ext" or just "avatars/..."
            if (oldPath.StartsWith("avatars/"))
            {
                var bucketPath = oldPath["avatars/".Length..];
                await _storageService.DeleteObjectAsync("avatars", bucketPath);
            }
        }

        user.AvatarUrl = request.ImagePath;
        _userRepository.Update(user);
        await _context.SaveChangesAsync(ct);

        var publicUrl = _storageService.GetPublicUrl("avatars", request.ImagePath["avatars/".Length..]);

        return new UpdateAvatarResponse
        {
            UserId = user.Id,
            AvatarUrl = publicUrl
        };
    }
}
