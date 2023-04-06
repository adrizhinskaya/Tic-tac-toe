using Microsoft.AspNetCore.Identity;

namespace Tic_tac_toe.Entities
{
    public class Player
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public char Sign { get; set; }
        public Guid GameId { get; set; }
        public Game Game { get; set; }
    }
}
