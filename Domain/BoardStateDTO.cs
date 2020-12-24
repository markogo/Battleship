using System.Collections.Generic;

namespace Domain
{
    public class BoardStateDTO
    {
        public BoardSquareState[][] PlayerBoard { get; set; } = null!;

        public int BoardHeight { get; set; }
        
        public int BoardWidth { get; set; }
        
        public Dictionary<string, bool> GameShipsStates { get; set; } = null!;
    }
}