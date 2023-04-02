using Microsoft.AspNetCore.Identity;
using Tic_tac_toe.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Tic_tac_toe.Controllers
{
    public class AccountController : ControllerBase
    {
        private readonly UserManager<Player> _userManager;

        public AccountController(UserManager<Player> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        [Produces("application/json")]// СДЕЛАТЬ ТО ЧТО В ПОСЛЕДНЕМ CHAT GPT
        public async Task<IActionResult> Register([FromBody] Player player)
        {
            var user = new Player(player.UserName);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

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
