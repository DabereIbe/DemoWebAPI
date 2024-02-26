using DemoWebAPI.Data;
using DemoWebAPI.Models.DTOs;
using DemoWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DemoWebAPI.Controllers
{
    [Authorize] // Implemented Authorization Middleware to prevent Broken Access Control (A01)
    [Route("api/v1/[controller]")]//Routing includes URI Versioning as a measure against Insecure Design (A04)
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly AesEncryption aes;
        private readonly SerilogLogger logger;

        public UsersController(AppDbContext context, AesEncryption aes, SerilogLogger logger)
        {
            this.context = context;
            this.aes = aes;
            this.logger = logger;
        }

        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<IEnumerable<UsersDTO>>> Get()
        {
            if (!User.IsInRole("Admin"))
            {
                var userIP = GetUserIP();
                logger.LogMessage($"{userIP} tried to access forbidden resource");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            var users = await context.Users.Select(x => UsersToDTO(x)).ToListAsync();
            return users;
        }

        [HttpPost("GetUserById")]
        public async Task<ActionResult<UsersDTO>> Get(byte[] encryptedId)
        {
            if (!User.IsInRole("Admin"))
            {
                var userIP = GetUserIP();
                logger.LogMessage($"{userIP} tried to access forbidden resource");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }
            //var encryptedId = aes.Encrypt(Convert.ToString(id));
            var decryptedId = aes.Decrypt(encryptedId);
            var user = await context.Users.SingleOrDefaultAsync(x => x.Id == Convert.ToInt32(decryptedId));
            if (user == null)
            {
                return BadRequest();
            }
            else
            {
                var encryptedObj = aes.Encrypt(user);
                return Ok(encryptedObj);
            }

        }

        [HttpPost("AddUser")]
        public async Task<ActionResult> Post(byte[] encryptedObj)
        {
            if (!User.IsInRole("Admin"))
            {
                var userIP = GetUserIP();
                logger.LogMessage($"{userIP} tried to access forbidden resource");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }
            var decryptedObj = aes.Decrypt<UsersDTO>(encryptedObj);

            var user = new Models.Users()
            {
                Name = decryptedObj.Name,
                Code = RandomNumber()
            };

            await context.Users.AddAsync(user);
            context.SaveChanges();

            var newUser = await context.Users.FirstOrDefaultAsync(x => x.Name == user.Name);
            var newEncryptedObj = aes.Encrypt(newUser);

            return Ok(newEncryptedObj);
        }

        /*[HttpPut("EditUser/{encryptedId}/{encryptedObj}")]
        public async Task<IActionResult> Put([FromRoute]byte[] encryptedId, [FromRoute]byte[] encryptedObj)
        {
            if (!User.IsInRole("Admin"))
            {
                var userIP = GetUserIP();
                logger.LogMessage($"{userIP} tried to access forbidden resource");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }
            //var encryptedId = aes.Encrypt(Convert.ToString(id));
            var decryptedId = aes.Decrypt(encryptedId);

            var user = await context.Users.FindAsync(Convert.ToInt32(decryptedId));
            if (user == null)
            {
                return NotFound();
            }
            if (user.Id != Convert.ToInt32(decryptedId))
            {
                return BadRequest();
            }
            else
            {
                //
                var decryptedObj = aes.Decrypt<UsersDTO>(encryptedObj);

                user.Name = decryptedObj.Name;
                context.SaveChanges();
                var updatedUser = await context.Users.Where(x => x.Id == user.Id).SingleOrDefaultAsync();
                var encryptedUpdatedObj = aes.Encrypt(updatedUser);
                return Ok(encryptedUpdatedObj);
            }
        }*/

        [HttpPut("EditUser")]
        public async Task<IActionResult> Put([FromBody] Dictionary<string, byte[]> encryptedParams)
        {
            if (!User.IsInRole("Admin"))
            {
                var userIP = GetUserIP();
                logger.LogMessage($"{userIP} tried to access forbidden resource");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            if (!encryptedParams.ContainsKey("encryptedId") || !encryptedParams.ContainsKey("encryptedObj"))
            {
                return BadRequest("Both encryptedId and encryptedObj are required parameters.");
            }

            byte[] encryptedId = encryptedParams["encryptedId"];
            byte[] encryptedObj = encryptedParams["encryptedObj"];

            var decryptedId = aes.Decrypt(encryptedId);
            var decryptedObj = aes.Decrypt<UsersDTO>(encryptedObj);

            var user = await context.Users.FindAsync(Convert.ToInt32(decryptedId));
            if (user == null)
            {
                return NotFound();
            }

            if (user.Id != Convert.ToInt32(decryptedId))
            {
                return BadRequest();
            }
            else
            {
                user.Name = decryptedObj.Name;
                context.SaveChanges();
                var updatedUser = await context.Users.Where(x => x.Id == user.Id).SingleOrDefaultAsync();
                var encryptedUpdatedObj = aes.Encrypt(updatedUser);
                return Ok(encryptedUpdatedObj);
            }
        }


        [HttpDelete("DeleteUser")]
        public async Task<ActionResult> Delete(byte[] encryptedId)
        {
            if (!User.IsInRole("Admin"))
            {
                var userIP = GetUserIP();
                logger.LogMessage($"{userIP} tried to access forbidden resource");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }
            //var encryptedId = aes.Encrypt(Convert.ToString(id));
            var decryptedId = aes.Decrypt(encryptedId);
            var user = await context.Users.FindAsync(Convert.ToInt32(decryptedId));
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                context.Remove(user);
                context.SaveChanges();
                return Ok(await context.Users.Select(x => UsersToDTO(x)).ToListAsync());
            }
        }

        private static UsersDTO UsersToDTO(Models.Users users) =>
            new UsersDTO
            {
                Id = users.Id,
                Name = users.Name,
            };

        private static int RandomNumber()
        {
            var random = new Random((int)DateTime.Now.Ticks);
            return random.Next(1000, 9999);
        }
        private string GetUserIP()
        {
            // Getting host name 
            string host = Dns.GetHostName();


            // Getting ip address using host name 
            IPHostEntry ip = Dns.GetHostEntry(host);
            return ip.AddressList[0].ToString();
        }
    }
}
