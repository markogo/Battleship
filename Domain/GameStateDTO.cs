using System.Collections.Generic;
using Domain.Enums;

namespace Domain
{
    public class GameState
    {
        public bool PlayerOneMove { get; set; }

        public GamePhaseEnum GamePhase { get; set; }
        
        public BoardSquareState[][] Player1Board { get; set; } = null!;
        
        public BoardSquareState[][] Player2Board { get; set; } = null!;
        public int BoardHeight { get; set; } = 10;
        public int BoardWidth { get; set; } = 10;

        public Dictionary<string, bool> GameShipsStates { get; set; } = null!;
    }
}