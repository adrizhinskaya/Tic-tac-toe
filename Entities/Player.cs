using Microsoft.AspNetCore.Identity;

namespace Tic_tac_toe.Entities
{
    public class Player : IdentityUser
    {
        public Guid GameId { get; set; }
        public Game Game { get; set; }
        public char Sign { get; set; }
        public Player(string userName)
        {
            UserName = userName;
        }
    }
}
