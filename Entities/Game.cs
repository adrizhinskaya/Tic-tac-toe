using System.ComponentModel.DataAnnotations.Schema;

namespace Tic_tac_toe.Entities
{
    public class Game
    {
        public Guid Id { get; set; }
        public bool Status { get; set; } = false;
        [NotMapped]
        public char[,] Board { get; set; } = new char[3, 3];

        public Guid Player1Id { get; set; }
        public Player Player1 { get; set; }

        public Guid Player2Id { get; set; }
        public Player Player2 { get; set; }

        public Guid CurrentPlayerId { get; set; }
        public Player CurrentPlayer { get; set; }

        public ICollection<Player> Players { get; set; }

        public Game()
        {
            Players.Add(Player1);
            Players.Add(Player2);
        }
    }
}
