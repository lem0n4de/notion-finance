using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notion.Client;
using NotionFinance.Data;

namespace NotionFinance.Controllers;

[ApiController]
[Route("api/notion")]
public class NotionAuthorizationController : Controller
{
    private const string NotionOAuthUrl = "https://api.notion.com/v1/oauth/authorize";
    private UserDbContext _context;
    private readonly IConfiguration _configuration;
    private HttpClient _client;

    public NotionAuthorizationController(UserDbContext userDbContext, IConfiguration configuration)
    {
        _context = userDbContext;
        _configuration = configuration;
        _client = new HttpClient();
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("get-authorization-url")]
    public async Task<ActionResult<string>> GetNotionAuthorizeUrl()
    {
        var emailClaim = User.FindFirst(ClaimTypes.Email);
        if (emailClaim == null) return BadRequest(Messages.InvalidUser);
        var user = await _context.Users.FirstAsync(x => x.Email == emailClaim.Value);
        // state is inside double brackets because notion thinks its a number and not a string.
        var url =
            $"{NotionOAuthUrl}?owner=user&response_type=code&client_id={_configuration["Notion:ClientId"]}" +
            $"&redirect_uri={_configuration["Notion:CallbackBaseUrl"]}/api/notion/callback&state=\"{user.Id}\"";
        return url;
    }

    [HttpGet("callback")]
    public async Task<ActionResult<object>> NotionCallback(string? code, string? state, string? error)
    {
        if (error != null) BadRequest(Messages.NotionError);
        const string url = "https://api.notion.com/v1/oauth/token";
        if (state == null) return BadRequest(Messages.NotionError);
        state = state.Replace("\"", "");

        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        var base64Secret =
            Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_configuration["Notion:ClientId"]}:{_configuration["Notion:ClientSecret"]}"));
        var serializedString = JsonSerializer.Serialize(new
        {
            grant_type = "authorization_code", code = code, redirect_uri = $"{_configuration["Notion:CallbackBaseUrl"]}/api/notion/callback"
        });
        req.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64Secret}");
        req.Headers.TryAddWithoutValidation("Notion-Version", "2021-08-16");
        req.Content = new StringContent(serializedString);
        req.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
        var res = await _client.SendAsync(req);

        if (res.IsSuccessStatusCode)
        {
            var user = await _context.Users.FirstAsync(x => x.Id == long.Parse(state));
            var json = JsonNode.Parse(await res.Content.ReadAsStringAsync());
            if (json == null) return BadRequest(Messages.NotionError);
            var notionUser = json["owner"]?.AsObject();
            if (notionUser != null && notionUser.ContainsKey("type"))
            {
                var owner_user = notionUser["user"]?.AsObject();
                var owner_user_id = owner_user?["id"]?.ToString();

                if (owner_user == null || owner_user_id == null)
                    return BadRequest(Messages.NotionError);

                user.NotionId = owner_user_id;
                user.NotionAccessToken = json["access_token"]!.ToString();
                user.AuthorizedWorkspaceId = json["workspace_id"]!.ToString();
                await _context.SaveChangesAsync();
            }

            return Ok(Messages.OperationSuccessful);
        }
        else return BadRequest(Messages.NotionError);
    }
}