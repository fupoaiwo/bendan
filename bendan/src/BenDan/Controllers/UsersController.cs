using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BenDan.Controllers
{
    /// <summary>
    ///    用户管理
    /// </summary>
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUsersRepository _user;

        public UsersController(IUsersRepository user)
        {
            _user = user;
        }
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("get")]
        [ApiExplorerSettings(GroupName = "v2")]
        public IActionResult Get(string userName, string pwd) {
            _user.deleteuseer();
            return Ok("你好");
        }
    }
}