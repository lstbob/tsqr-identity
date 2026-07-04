using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TSQR.Identity.Application.Commands.Profile;
using TSQR.Identity.Application.Queries.Profile;

namespace TSQR.Identity.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public sealed class ProfileController : ControllerBase
{
    private readonly IInteractor<GetProfileQuery, Result<ProfileResult>> _getProfile;
    private readonly IInteractor<UpdateProfileCommand, Result> _updateProfile;
    private readonly IInteractor<RequestEmailChangeCommand, Result<string>> _requestEmailChange;
    private readonly IInteractor<ChangeEmailCommand, Result> _changeEmail;

    public ProfileController(
        IInteractor<GetProfileQuery, Result<ProfileResult>> getProfile,
        IInteractor<UpdateProfileCommand, Result> updateProfile,
        IInteractor<RequestEmailChangeCommand, Result<string>> requestEmailChange,
        IInteractor<ChangeEmailCommand, Result> changeEmail)
    {
        _getProfile = getProfile;
        _updateProfile = updateProfile;
        _requestEmailChange = requestEmailChange;
        _changeEmail = changeEmail;
    }

    [HttpGet]
    public async Task<ActionResult> Get(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var result = await _getProfile.ExecuteAsync(new GetProfileQuery(userId), ct);
        if (!result.IsSuccess)
            return ToErrorResult(result.Error);

        return Ok(result.Value);
    }

    [HttpPut]
    public async Task<ActionResult> Update(UpdateProfileRequest request, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var result = await _updateProfile.ExecuteAsync(
            new UpdateProfileCommand(userId, request.FirstName, request.LastName, request.Bio), ct);
        if (!result.IsSuccess)
            return ToErrorResult(result.Error);

        return Ok();
    }

    [HttpPost("email")]
    public async Task<ActionResult> RequestEmailChange(EmailChangeRequest request, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var result = await _requestEmailChange.ExecuteAsync(
            new RequestEmailChangeCommand(userId, request.NewEmail), ct);
        if (!result.IsSuccess)
            return ToErrorResult(result.Error);

        return Ok(new { token = result.Value });
    }

    [HttpPost("email/confirm")]
    public async Task<ActionResult> ConfirmEmailChange(ConfirmEmailChangeRequest request, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        var result = await _changeEmail.ExecuteAsync(
            new ChangeEmailCommand(userId, request.NewEmail, request.Token), ct);
        if (!result.IsSuccess)
            return ToErrorResult(result.Error);

        return Ok();
    }

    [HttpPost("avatar")]
    [RequestSizeLimit(5_242_880)] // 5 MB
    public async Task<ActionResult> UploadAvatar(IFormFile file, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId is null) return Unauthorized();

        if (file is null || file.Length == 0)
            return BadRequest(new ErrorResponse("file", "No file provided."));

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".gif" && ext != ".webp")
            return BadRequest(new ErrorResponse("file", "Invalid file type. Accepted: jpg, png, gif, webp."));

        var avatarsDir = Path.Combine(Directory.GetCurrentDirectory(), "avatars");
        Directory.CreateDirectory(avatarsDir);

        var fileName = $"{userId}{ext}";
        var filePath = Path.Combine(avatarsDir, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        return Ok(new { avatarUrl = $"/api/profile/avatar/{userId}" });
    }

    [HttpGet("avatar/{userId}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetAvatar(string userId)
    {
        var avatarsDir = Path.Combine(Directory.GetCurrentDirectory(), "avatars");
        var filePath = Directory.GetFiles(avatarsDir, $"{userId}.*").FirstOrDefault();

        if (filePath is null)
            return NotFound();

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        var contentType = ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream",
        };

        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(bytes, contentType);
    }

    private ActionResult ToErrorResult(Error error)
    {
        return error switch
        {
            NotFoundError err => NotFound(new ErrorResponse(err.Code, err.Message)),
            _ => BadRequest(new ErrorResponse(error.Code, error.Message)),
        };
    }
}
