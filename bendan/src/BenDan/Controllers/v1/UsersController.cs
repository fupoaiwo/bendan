using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BenDan.Controllers.v1
{
    [Route("v1/user")]
    [ApiExplorerSettings(GroupName = "v1")]
    [ApiController]
    public class UsersController : ControllerBase
    {

        [HttpPost]
        [Route("register")]
        [ApiExplorerSettings(GroupName = "v1")]
        public IActionResult register(string userName, string pwd)
        {
            return Ok("你好");
        }
    

        [HttpPost]
        [Route("signin")]
        [ApiExplorerSettings(GroupName = "v1")]
        public IActionResult signin(string userName, string pwd)
        {
            return Ok("你好");
        }
    }
}