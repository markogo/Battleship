using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Domain
{
    public class GameOption
    {
        public int GameOptionId { get; set; }

        [MaxLength(128)]
        public string Name { get; set; } = "Classical options";

        [Range(10, 26)]
        public int BoardWidth { get; set; } = 10;
        
        [Range(10, 26)]
        public int BoardHeight { get; set; } = 10;

        public CanShipsTouchEnum CanShipsTouchEnum { get; set; } = CanShipsTouchEnum.No;
        public NextMoveAfterHitEnum NextMoveAfterHitEnum { get; set; } = NextMoveAfterHitEnum.OtherPlayerTurn;

        public ICollection<GameOptionShip>? GameOptionShips { get; set; }

        public ICollection<Game>? Games { get; set; }

    }
}