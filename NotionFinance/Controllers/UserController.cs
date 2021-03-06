using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotionFinance.Data;
using NotionFinance.Helpers;
using NotionFinance.Models;

namespace NotionFinance.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserDbContext _context;
    private readonly IConfiguration _configuration;

    public UserController(UserDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // GET: api/User
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        var userDtos = users.Select(UserDTO.FromUser).ToList();
        return userDtos;
    }

    // GET: api/User/5
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<UserDTO>> GetUser(long id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return UserDTO.FromUser(user);
    }

    // PUT: api/User/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> PutUser(long id, UpdateUserForm updateUserForm)
    {
        if (id != updateUserForm.Id)
        {
            return BadRequest(Messages.InvalidUser);
        }

        var user = await _context.Users.FirstAsync(x => x.Id == updateUserForm.Id);
        user.FirstName = updateUserForm.FirstName;
        user.LastName = updateUserForm.LastName;
        user.Email = updateUserForm.Email;

        _context.Entry(user).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/User
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterUserForm userForm)
    {
        if (userForm == null) return BadRequest();
        if (await _context.Users.AnyAsync(x => x.Email == userForm.Email))
            return BadRequest(Messages.UserWithEmailExists);
        var (hash, salt) = PasswordHelper.EncryptPassword(userForm.Password);
        var isAdmin = userForm.Email == "yunus.emre.orcun@outlook.com";
        var user = new User()
        {
            Email = userForm.Email,
            FirstName = userForm.FirstName,
            LastName = userForm.LastName,
            Membership = userForm.Membership,
            IsAdmin = isAdmin,
            PasswordHash = hash,
            PasswordSalt = salt,
            NotionUserSettings = new NotionUserSettings()
            {
                MasterDatabaseExists = false
            }
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new {id = user.Id}, UserDTO.FromUser(user));
    }

    [AllowAnonymous]
    [HttpPost("authenticate")]
    public async Task<ActionResult<object>> Authenticate(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

        if (user == null) return BadRequest(Messages.InvalidUser);
        var checkPassword = PasswordHelper.CheckPassword(user, password);

        if (checkPassword)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var role = user.IsAdmin ? "Administrator" : "User";

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.Role, user.Membership.ToString())
            };

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], null, claims,
                expires: DateTime.Now.AddDays(7), signingCredentials: credentials);

            return Ok(new {token = new JwtSecurityTokenHandler().WriteToken(token), expires = token.ValidTo});
        }
        return BadRequest(Messages.PasswordIncorrect);
    }

    // DELETE: api/User/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> DeleteUser(long id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool UserExists(long id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}