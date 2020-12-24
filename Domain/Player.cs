using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain
{
    public class Player
    {
        public int PlayerId { get; set; }

        [MaxLength(128)]
        public string Name { get; set; } = null!;

        public bool PlayerTurn { get; set; }

        public PlayerTypeEnum PlayerTypeEnum { get; set; }

        public int? GameId { get; set; }
        public Game? Game { get; set; }

        public ICollection<GameShip>? GameShips { get; set; }

        public ICollection<PlayerBoardState>? PlayerBoardStates { get; set; }
    }
}