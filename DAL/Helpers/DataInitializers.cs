using System.Collections.Generic;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace DAL.Helpers
{
    public class DataInitializers
    {
        public static void MigrateDatabase(AppDbContext context)
        {
            context.Database.Migrate();
        }
        public static void DeleteDatabase(AppDbContext context)
        {
            context.Database.EnsureDeleted();
        }

        public static void SeedDefaultData(AppDbContext context)
        {
            // Adding default GameOptions
            GameOption defaultGameOptions = new GameOption();
                    
            context.GameOptions.Add(defaultGameOptions);

            context.SaveChanges();
                    
            // Adding default Ships
            context.Ships.Add(new Ship() {Size = 5, Name = "Carrier"});
            context.Ships.Add(new Ship() {Size = 4, Name = "Battleship"});
            context.Ships.Add(new Ship() {Size = 3, Name = "Cruiser"});
            context.Ships.Add(new Ship() {Size = 2, Name = "Submarine"});
            context.Ships.Add(new Ship() {Size = 1, Name = "Destroyer"});
            context.SaveChanges();
                    
            // Adding default GameOptionShips
            foreach (var ship in context.Ships)
            {
                var gameOptionShip = new GameOptionShip()
                {
                    Amount = 1,
                    ShipId = ship.ShipId,
                    GameOptionId = defaultGameOptions.GameOptionId
                };
                context.GameOptionShips.Add(gameOptionShip);
            }
            context.SaveChanges();
        }
    }
}