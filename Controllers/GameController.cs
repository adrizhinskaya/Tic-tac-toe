using Tic_tac_toe.Data;
using Tic_tac_toe.Entities;
using Microsoft.AspNetCore.Identity;
using System.Text;
using Newtonsoft.Json;
using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Tic_tac_toe.Controllers
{
    public class GameController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private UserManager<Player> _userManager;
        public GameController(AppDbContext appDbContext, UserManager<Player> userManager)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
        }

        [HttpPost]
        [Route("api/join")]
        [Authorize]
        [Produces("application/json")]
        public async Task<IActionResult> Join([FromBody] string playerId)
        {
            Player? currPlayer = await _userManager.FindByIdAsync(playerId);
            if (currPlayer == null)
                return BadRequest("Зарегистрируйтесь для прохождения игры");

            Game? gameWithCurPlayer = _appDbContext.Games.Where(g => g.Player1 != null).FirstOrDefault();
            if (gameWithCurPlayer != null)
            {
                if (gameWithCurPlayer.Player1 != currPlayer)
                {
                    await AddPlayerToGame(gameWithCurPlayer, currPlayer);
                    await CallApi(gameWithCurPlayer.Id, currPlayer.Id, "api/join/{gameId}/game");
                }
                return Ok("Ожидайте второго игрока, периодически прозванивая адрес.");
            }
            else
            {
                await AddPlayerToGame(gameWithCurPlayer, currPlayer);
                return Ok("Ожидайте второго игрока, периодически прозванивая адрес.");
            }
        }

        [HttpPost]
        [Route("api/join/{gameId}/game")]
        [Authorize]
        [Produces("application/json")]
        
        public IActionResult Game([FromBody] string gameId, string playerId)
        {
            Game? game = GetGameById(gameId);
            if (game is null || !game.Status)
                return BadRequest("Невозможно присоединиться к игре.");

            if (game.CurrentPlayer.Id != playerId)
                return Ok("Сейчас НЕ ваш ход! Ожидайте хода второго игрока, периодически прозванивая адрес.");
            else
            {
                char winnerSign = CheckWinner(game);
                if (!Char.IsWhiteSpace(winnerSign))
                {
                    Player winner = NominateWinner(game, winnerSign);
                    return Ok($"Игра окончена! Победил(-ла) {winner.UserName} - {winner.Sign}");
                }
                var moveOptions = GetMoveOptions(game);
                if (moveOptions.Count() == 0)
                    return Ok("Ничья");

                string movesStr = GetMoveOptionsStr(moveOptions);
                return Ok($"Ваш ход! Варианты ходов:\n{movesStr}Сделайте ваш ход по адресу api/games/{{gameid}}/move и отправьте, playerId, gameId, row, col");
            }
        }

        [HttpPost]
        [Route("api/games/{gameid}/move")]
        [Authorize]
        [Produces("application/json")]
        public async Task<IActionResult> Move([FromBody] string playerId, string gameId, int row, int col)
        {
            Game? game = GetGameById(gameId);

            if (game is null || !game.Status)
                return BadRequest("Невозможно присоединиться к игре.");

            if (game.CurrentPlayer.Id != playerId)
                return BadRequest("Сейчас ходит другой игрок");

            if (row < 0 || row > 2 || col < 0 || col > 2)
                return BadRequest("Неверные координаты ячейки.");

            if (game.Board[row, col] != null)
                return BadRequest("Указанная ячейка уже занята.");

            game.Board[row, col] = game.CurrentPlayer.Sign;
            await _appDbContext.SaveChangesAsync();
            await ChangeCurPl(game);
            await CallApi(game.Id, playerId, "api/join/{gameId}/game");
            return Ok();
        }

        private async Task AddPlayerToGame(Game? game, Player newPlayer)
        {
            if (game != null)
            {
                newPlayer.Sign = 'O';
                game.Player2 = newPlayer;
                game.Status = true;
            }
            else
            {
                newPlayer.Sign = 'X';
                Game newGame = new Game() { Id = Guid.NewGuid(), Player1 = newPlayer, CurrentPlayer = newPlayer };
                await _appDbContext.Games.AddAsync(newGame);
            }
            await _appDbContext.SaveChangesAsync();
        }

        private async Task CallApi(Guid gameId, string playerId, string url)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5000/");
            var data = new { gameId = gameId, playerId = playerId };
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            //if (response.IsSuccessStatusCode)
            //{
            //    var result = await response.Content.ReadAsStringAsync();
            //}
        }

        private Game? GetGameById(string gameId)
        {
            Game? game = _appDbContext.Games.Where(g => g.Id.ToString() == gameId).FirstOrDefault();
            return game;
        }

        private char CheckWinner(Game game)
        {
            var nullCells = GetMoveOptions(game);
            if(nullCells.Count() <= 4)
            {
                return FindWinCombination(game.Board);
            }

            return ' ';
        }

        private Player NominateWinner(Game game, char winnerSign)
        {
            Player winner;
            if (winnerSign == game.Player1.Sign)
            {
                winner = game.Player1;
            }
            else
            {
                winner = game.Player2;
            }
            game.Status = false;
            return winner;
        }

        private char FindWinCombination(char[,] board)
        {
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 0] == board[i, 1] && board[i, 1] == board[i, 2])
                { 
                    return board[i, 0];
                }
                if (board[0, i] == board[1, i] && board[1, i] == board[2, i])
                {
                    return board[0, i];
                }
            }

            if (board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2])
            {
                return board[0, 0];
            }
            if (board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0])
            {
                return board[0, 2];
            }

            return ' ';
        }

        private IEnumerable<Point> GetMoveOptions(Game game)
        {
            var moveOptions = game.Board.Cast<string>()
                    .Select((value, index) => new { Value = value, Index = index })
                    .Where(x => x.Value == null)
                    .Select(x => new Point { X = x.Index / game.Board.GetLength(1), Y = x.Index % game.Board.GetLength(1) });

            return moveOptions;
        }

        private string GetMoveOptionsStr(IEnumerable<Point> moveOptions)
        {
            StringBuilder movesStr = new StringBuilder();
            foreach (var move in moveOptions)
            {
                movesStr.Append($"move\n");
            }

            return movesStr.ToString();
        }

        private async Task ChangeCurPl(Game game)
        {
            if (game.CurrentPlayer == game.Player1)
                game.CurrentPlayer = game.Player2;
            else
                game.CurrentPlayer = game.Player1;

            await _appDbContext.SaveChangesAsync();
        }
    }
}
