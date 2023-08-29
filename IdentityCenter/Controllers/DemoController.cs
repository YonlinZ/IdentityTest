using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Intrinsics.X86;

namespace IdentityCenter.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DemoController : ControllerBase
    {
        readonly UserManager<MyUser> userManager;
        readonly RoleManager<MyRole> roleManager;

        public DemoController(UserManager<MyUser> userManager, RoleManager<MyRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        [HttpPost]
        public async Task<ActionResult<string>> Test1()
        {
            if (await roleManager.RoleExistsAsync("admin") == false)
            {
                var role = new MyRole { Name = "admin" };
                var result = await roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    return BadRequest();
                }
            }
            var user1 = await userManager.FindByNameAsync("yzk");
            if (user1 == null)
            {

                user1 = new MyUser { UserName = "yzk" };
                var result = await userManager.CreateAsync(user1, "123456");
                if (!result.Succeeded) return BadRequest("userManager.CreateAsyncfailed");
            }

            if (!await userManager.IsInRoleAsync(user1, "admin"))
            {
                var result = await userManager.AddToRoleAsync(user1, "admin");
                if (!result.Succeeded) return BadRequest();
            }

            return "ok";
        }

        [HttpPost]
        public async Task<ActionResult> CheckPwd(CheckPwdRequest req)
        {
            var user = await userManager.FindByNameAsync(req.userName);
            if (user == null)
            {
                return NotFound("用户名不存在");
            }
            if (await userManager.IsLockedOutAsync(user))
            {
                return BadRequest("用户被锁定");
            }
            if (await userManager.CheckPasswordAsync(user, req.pwd))
            {
                await userManager.ResetAccessFailedCountAsync(user);
                return Ok("登录成功");
            }
            else
            {
                await userManager.AccessFailedAsync(user);
                return BadRequest("用户名或密码错误");
            }
        }

        [HttpPost]
        public async Task<ActionResult> SendResetPasswordToken(string userName)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound("用户名不存在");
            }
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            //await Console.Out.WriteLineAsync($"验证码: {token}");
            return Ok($"验证码: {token}");
        }

        [HttpPut]
        public async Task<ActionResult> ResetPassword(string userName, string token, string newPwd)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound("用户名不存在");
            }

            var result = await userManager.ResetPasswordAsync(user, token, newPwd);
            if (result.Succeeded)
            {
                await userManager.ResetAccessFailedCountAsync(user);
                return Ok("密码重置成功");
            }
            else
            {
                await userManager.AccessFailedAsync(user);
                return BadRequest("密码重置失败！");
            }
        }
    }
}
