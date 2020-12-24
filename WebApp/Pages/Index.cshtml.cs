using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly DAL.AppDbContext _context;

        public IndexModel(ILogger<IndexModel> logger, DAL.AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        private IList<GameSave> GameSave { get; set; } = null!;

        [BindProperty] public GameOption GameOption { get; set; } = null!;

        public SelectList GameOptionsSelectList { get; set; } = null!;

        [BindProperty] public Player Player1 { get; set; } = null!;

        [BindProperty] public Player Player2 { get; set; } = null!;

        public async Task OnGetAsync()
        {
            GameSave = await _context.GameSaves.Where(x => x.CreatedByUser).OrderBy(x => x.Name).ToListAsync();
            
            GameOptionsSelectList = new SelectList(_context.GameOptions.ToList(), nameof(GameOption.GameOptionId), nameof(GameOption.Name));

            GameOption = _context.GameOptions.ToList().LastOrDefault()!;
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            var game = new Game()
            {
                GameOptionId = GameOption.GameOptionId,
                GamePhase = GamePhaseEnum.PlacingShips
            };
            
            if (string.IsNullOrWhiteSpace(Player1.Name))
            {
                Player1.Name = "Player 1";
            }

            if (string.IsNullOrWhiteSpace(Player2.Name))
            {
                Player2.Name = "Player 2";
            }
            
            Player1.PlayerTurn = true;
            Player1.PlayerTypeEnum = PlayerTypeEnum.Human;
            
            Player2.PlayerTurn = false;
            Player2.PlayerTypeEnum = PlayerTypeEnum.Human;

            game.Player1 = Player1;
            game.Player2 = Player2;

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            Player1.GameId = game.GameId;
            Player2.GameId = game.GameId;
            
            await _context.SaveChangesAsync();
            
            return RedirectToPage("./GamePlay/Index", new {gameId = game.GameId});
        }

    }
}