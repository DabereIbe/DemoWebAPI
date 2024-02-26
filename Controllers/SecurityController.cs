using DemoWebAPI.Models.DTOs;
using DemoWebAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace DemoWebAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly AesEncryption aes;

        public SecurityController(AesEncryption aes)
        {
            this.aes = aes;
        }

        [HttpPost("EncryptId/{id}")]
        public ActionResult EncryptId(string id)
        {
            var encryptedId = aes.Encrypt(id);
            return Ok(encryptedId);
        }
        
        [HttpPost("DecryptId")]
        public ActionResult DecryptId(byte[] encryptedId)
        {
            var decryptedId = aes.Decrypt(encryptedId);
            return Ok(decryptedId);
        }
        
        [HttpPost("EncryptUserObj")]
        public ActionResult EncryptUserObj(UsersDTO user)
        {
            var encryptedObj = aes.Encrypt(user);
            return Ok(encryptedObj);
        }
        
        [HttpPost("DecryptUserObj")]
        public ActionResult DecryptUserObj(byte[] encryptedObj)
        {
            var decryptedObj = aes.Decrypt<UsersDTO>(encryptedObj);
            return Ok(decryptedObj);
        }
        
        [HttpPost("EncryptTaskObj")]
        public ActionResult EncryptTaskObj(TasksDTO task)
        {
            var encryptedObj = aes.Encrypt(task);
            return Ok(encryptedObj);
        }
        
        [HttpPost("DecryptTaskObj")]
        public ActionResult DecryptTaskObj(byte[] encryptedObj)
        {
            var decryptedObj = aes.Decrypt<TasksDTO>(encryptedObj);
            return Ok(decryptedObj);
        }
    }
}
