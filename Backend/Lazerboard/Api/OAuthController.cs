using System.Security.Claims;
using Lazerboard.Data.ApiFetchers;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Lazerboard.Data.OsuEntities.OsuEntityToDtoService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lazerboard.Api;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("[controller]")]
public class OAuthController(ILogger<OAuthController> logger, 
    IOsuApiFetcher osuApiFetcher,
    IUserRepository userRepository,
    IOsuEntityToDtoService osuEntityToDtoService): ControllerBase
{
    /// <summary>
    /// osu! API authentication
    /// </summary>
    [HttpGet("login")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult Authenticate()
    {
        var authenticationProperties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("CompleteAuthentication", "OAuth")
        };

        return Challenge(authenticationProperties, "osu-user");
    }
    
    /// <summary>
    /// Complete osu! API authentication
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/></param>
    [HttpGet("complete")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> CompleteAuthentication(CancellationToken ct = default)
    {
        var authResult = await HttpContext.AuthenticateAsync("ExternalCookies");
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var accessToken = await HttpContext.GetTokenAsync("ExternalCookies", "access_token");
        var refreshToken = await HttpContext.GetTokenAsync("ExternalCookies", "refresh_token");

        APIUser? user;
        try
        {
            user = await osuApiFetcher.GetUserInfoAsync(accessToken!, ct);
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, ex, "User info retrieval from osu! API has failed!");
            return Forbid();
        }

        if (accessToken is null || refreshToken is null)
        {
            return Forbid();
        }

        var existingUser = await userRepository.GetByIdAsync(user.Id, ct);
        if (existingUser is null)
        {
            var dbUser = osuEntityToDtoService.UserEntityToDto(user);
            dbUser.IsRegistered = true;
            logger.Log(LogLevel.Information, "osu! user {userID} has registered", dbUser.Id);
            userRepository.Create(dbUser);
        }
        else
        {
            existingUser.Username = user.Username;
            existingUser.CountryCode = user.CountryCode;
            if (!existingUser.IsRegistered)
            {
                existingUser.IsRegistered = true;
                logger.Log(LogLevel.Information, "User {UserId} has registered", existingUser.Id);
            }
            userRepository.Update(existingUser);
        }
        await userRepository.SaveChangesAsync(ct);

        var tokenExpiration = await HttpContext.GetTokenAsync("ExternalCookies", "expires_in");
        var tokenExpirationDate = tokenExpiration != null 
            ? DateTime.UtcNow.Add(TimeSpan.FromSeconds(int.Parse(tokenExpiration))) 
            : DateTime.UtcNow.AddDays(1);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Id.ToString()),
        };

        var id = new ClaimsIdentity(claims, "InternalCookies");
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = authResult.Properties?.ExpiresUtc
        };

        await HttpContext.SignInAsync("InternalCookies", new ClaimsPrincipal(id), authProperties);
        await HttpContext.SignOutAsync("ExternalCookies");
        
        logger.Log(LogLevel.Information, "osu user {userID} logged in, token expires on {tokenExpiration}", user.Id, tokenExpirationDate);

        return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/");
    }
    
    /// <summary>
    /// Sign out from current user
    /// </summary>
    [Authorize]
    [HttpGet("signout")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("InternalCookies");

        return Redirect($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/");
    }
}