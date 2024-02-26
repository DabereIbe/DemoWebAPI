using DemoWebAPI.Models;
using DemoWebAPI.Services;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace DemoWebAPI.Controllers
{
    [Route("api/v1/[controller]")]//Routing includes URI Versioning as a measure against Insecure Design (A04)
    [ApiController]
/*    [ValidateAntiForgeryToken]
*/    public class AccountsController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IConfiguration configuration;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly SerilogLogger logger;

        public AccountsController(UserManager<AppUser> userManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager, SerilogLogger logger) 
        {
            this.userManager = userManager;
            this.configuration = configuration;
            this.roleManager = roleManager;
            this.logger = logger;
        }

        [HttpPost]
        [Route(nameof(Register))]
        public async Task<ActionResult> Register([FromBody] RegisterModel model)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest);
                }
                AppUser appUser = new AppUser() 
                { 
                    Email = model.Email, 
                    UserName = model.Email, 
                    DebitCardNumber = Argon2Hasher(model.DebitCardNumber) // Value for DebitCardNumber hashed to prevent Cryptographic Failures (A02)
                };
                var result = await userManager.CreateAsync(appUser, model.Password);
                if (result.Succeeded)
                {
                    if (model.Email == "admin@usermail.com")
                        await userManager.AddToRoleAsync(appUser, "Admin");

                    var userRoles = await userManager.GetRolesAsync(appUser);

                    var authClaims = new List<Claim>()
                        {
                        new Claim(ClaimTypes.Name, appUser.Email),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };

                    foreach (var claim in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, claim));
                    }

                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));

                    var token = new JwtSecurityToken(
                        issuer: configuration["JWT:ValidIssuer"],
                        audience: configuration["JWT:ValidAudience"],
                        expires: DateTime.Now.AddHours(3),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                        );
                    return StatusCode(StatusCodes.Status201Created, new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo,
                        User = appUser.UserName,
                        DebitCardNumber = appUser.DebitCardNumber
                    });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            
            else
            {
                return BadRequest(ModelState);
            }
        }



        [HttpPost]
        [Route(nameof(Login))]
        /*[ValidateAntiForgeryToken]*/
        public async Task<ActionResult> Login([FromBody] LoginModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach (var claim in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, claim));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: configuration["JWT:ValidIssuer"],
                    audience: configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );
                return Ok(new 
                { 
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    User = user.UserName
                });
            }
            logger.LogMessage($"user with IP {GetUserIP()} tried but failed to login");
            return BadRequest();
        }

        /*[HttpPost]
        public async Task<ActionResult> Logout()
        {

        }*/

        private string GetUserIP()
        {
            // Getting host name 
            string host = Dns.GetHostName();

            // Getting ip address using host name 
            IPHostEntry ip = Dns.GetHostEntry(host);
            return ip.AddressList[0].ToString();
        }

        private string Argon2Hasher(string key)
        {
            string password = key;
            byte[] salt = Encoding.UTF8.GetBytes("mysalt123");
            int iterations = 2;
            int memorySize = 19923; // in kilobytes
            int parallelism = 1;
            int hashLength = 32; // in bytes

            // Create an instance of Argon2id
            using (var hasher = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                hasher.Salt = salt;
                hasher.DegreeOfParallelism = parallelism;
                hasher.MemorySize = memorySize;
                hasher.Iterations = iterations;

                // Perform the hash
                byte[] hash = hasher.GetBytes(hashLength);

                // Convert the hash to a hex string for storage or comparison
                string hashedKey = BitConverter.ToString(hash).Replace("-", "").ToLower();

                return hashedKey;
            }
        }
    }
}
