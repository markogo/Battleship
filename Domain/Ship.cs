using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Ship
    {
        public int ShipId { get; set; }
        
        [Range(1,int.MaxValue)]
        public int Size { get; set; }
        
        [MaxLength(32)]
        public string Name { get; set; } = null!;

        public ICollection<GameOptionShip>? GameOptionShips { get; set; }
    }
}