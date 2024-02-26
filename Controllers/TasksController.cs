using DemoWebAPI.Data;
using DemoWebAPI.Models;
using DemoWebAPI.Models.DTOs;
using DemoWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace DemoWebAPI.Controllers
{
    /*[Authorize]*/
    [Route("api/v1/[controller]")]//Routing includes URI Versioning as a measure against Insecure Design (A04)
    [ApiController]
    /*[ValidateAntiForgeryToken]*/
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly SerilogLogger logger;
        private readonly AesEncryption aes;

        public TasksController(AppDbContext context, SerilogLogger logger, AesEncryption aes) 
        {
            this.context = context;
            this.logger = logger;
            this.aes = aes;
        }

        [HttpGet("GetAllTasks")]
        public async Task<ActionResult<IEnumerable<TasksDTO>>> Get()
        {
            if (!User.IsInRole("Admin"))
            {
                var userIP = GetUserIP();
                logger.LogMessage($"{userIP} tried to access forbidden resource");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }

            var tasks = await context.Tasks.Select(x => TasksToDTO(x)).ToListAsync();
            return tasks;
        }

        [HttpPost("GetTaskById")]
        public async Task<ActionResult<TasksDTO>> Get(byte[] encryptedId)
        {
            if (!User.IsInRole("Admin"))
            {
                var userIP = GetUserIP();
                logger.LogMessage($"{userIP} tried to access forbidden resource");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }
            //var encryptedId = aes.Encrypt(Convert.ToString(id));
            var decryptedId = aes.Decrypt(encryptedId);
            var task = await context.Tasks.FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(decryptedId));
            if (task == null)
            {
                return BadRequest();
            }
            else
            {
                var encryptedObj = aes.Encrypt(task);
                return Ok(encryptedObj);
            }
            
        }

        [HttpPost("AddTask")]
        public async Task<ActionResult> Post(byte[] encryptedObj)
        {
            if (!User.IsInRole("Admin"))
            {
                var userIP = GetUserIP();
                logger.LogMessage($"{userIP} tried to access forbidden resource");
                return StatusCode(StatusCodes.Status401Unauthorized);
            }
            
            var decryptedObj = aes.Decrypt<TasksDTO>(encryptedObj);
            
            var task = new Models.Tasks()
            {
                Name = decryptedObj.Name,
                IsComplete = decryptedObj.IsComplete,
                DateAdded = DateTime.Now,
                DateModified = null
            };
            if (task.IsComplete is false)
            {
                task.DateCompleted = null;
            }
            else
            {
                task.DateCompleted = DateTime.Now;
            }

            await context.Tasks.AddAsync(task);
            context.SaveChanges();
            var newTask = await context.Tasks.SingleOrDefaultAsync(x => x.Name == task.Name);
            var encryptedTask = aes.Encrypt(newTask);
            return Ok(encryptedTask);
        }

        [HttpPut("EditTask")]
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

            // encryptedId = aes.Encrypt(Convert.ToString(id));
            var decryptedId = aes.Decrypt(encryptedId);
            var task = await context.Tasks.FindAsync(Convert.ToInt32(decryptedId));
            if (task == null)
            {
                return NotFound();
            }
            else
            {
                //var encryptedObj = aes.Encrypt(taskDTO);
                var decryptedObj = aes.Decrypt<TasksDTO>(encryptedObj);
                task.Name = decryptedObj.Name;
                task.IsComplete = decryptedObj.IsComplete;
                if (task.IsComplete is false)
                {
                    task.DateCompleted = null;
                }
                else
                {
                    task.DateCompleted = DateTime.Now;
                }
                task.DateModified = DateTime.Now;
                context.SaveChanges();

                var updatedTask = await context.Tasks.FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(decryptedId));
                var encryptedUpdatedObj = aes.Encrypt(updatedTask);
                return Ok(encryptedUpdatedObj);
            }
        }

        [HttpDelete("DeleteTask")]
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
            var task = await context.Tasks.FindAsync(Convert.ToInt32(decryptedId));
            if (task == null)
            {
                return NotFound();
            }
            else
            {
                context.Remove(task);
                context.SaveChanges();
                return Ok(await context.Tasks.Select(x => TasksToDTO(x)).ToListAsync());
            }
        }

        private static TasksDTO TasksToDTO(Models.Tasks tasks) =>
            new TasksDTO
            {
                Id = tasks.Id,
                Name = tasks.Name,
                IsComplete = tasks.IsComplete
            };
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
