using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    public UserController(UserDbContext context)
    {
        _context = context;
    }

    // GET: api/User
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        var userDtos = users.Select(UserDTO.FromUser).ToList();
        return userDtos;
    }

    // GET: api/User/5
    [HttpGet("{id}")]
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
    public async Task<IActionResult> PutUser(long id, UpdateUserForm updateUserForm)
    {
        if (id != updateUserForm.Id)
        {
            return BadRequest();
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
    [HttpPost]
    public async Task<ActionResult<User>> Register(RegisterUserForm userForm)
    {
        var (hash, salt) = PasswordHelper.EncryptPassword(userForm.Password);
        var user = new User()
        {
            Email = userForm.Email,
            FirstName = userForm.FirstName,
            LastName = userForm.LastName,
            PasswordHash = hash,
            PasswordSalt = salt
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetUser", new {id = user.Id}, user);
    }

    // DELETE: api/User/5
    [HttpDelete("{id}")]
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