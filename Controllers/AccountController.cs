using Microsoft.AspNetCore.Identity;
using Tic_tac_toe.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Tic_tac_toe.Data;

namespace Tic_tac_toe.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _appDbContext;
        public AccountController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        [Produces("application/json")]
        public async Task<IActionResult> Register([FromBody] Player player)
        {
            var user = new Player()
            {
                UserName = player.UserName,
            };

            await _appDbContext.Players.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            //if (!result.Succeeded)
            //{
            //    return GetErrorResult(result);
            //}

            return Ok($"Вы успешно зарегистрировались. Ваш ID - {user.Id} ./nДля начала игры и поиска второго игрока перейдите по адресу\n\\api/join\\ и отправьте ваш ID");
        }

        private IActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                 return BadRequest();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Code);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}
