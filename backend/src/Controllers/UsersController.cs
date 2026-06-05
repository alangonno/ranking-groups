using backend.src.Handlers.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUpdateAvatarHandler _updateAvatarHandler;

    public UsersController(IUpdateAvatarHandler updateAvatarHandler)
    {
        _updateAvatarHandler = updateAvatarHandler;
    }

    [HttpPatch("me/avatar")]
    public async Task<IActionResult> UpdateAvatar([FromBody] UpdateAvatarRequest request, CancellationToken ct)
    {
        var response = await _updateAvatarHandler.HandleAsync(request, ct);
        return Ok(response);
    }
}
