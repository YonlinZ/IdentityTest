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
    }
}
