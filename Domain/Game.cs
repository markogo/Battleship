using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;

namespace Domain
{
    public class Game
    {
        public int GameId { get; set; }

        public int GameOptionId { get; set; }
        public GameOption? GameOption { get; set; }

        [MaxLength(512)]
        public string Description { get; set; } = DateTime.Now.ToLongDateString();

        public GamePhaseEnum GamePhase { get; set; }
        
        public int Player1Id { get; set; }
        public Player? Player1 { get; set; }
        
        public int Player2Id { get; set; }
        public Player? Player2 { get; set; }

        public ICollection<GameSave>? GameSaves { get; set; }
    }
}