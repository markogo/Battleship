using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL;
using Domain;

namespace WebApp.Pages.Games
{
    public class IndexModel : PageModel
    {
        private readonly DAL.AppDbContext _context;

        public IndexModel(DAL.AppDbContext context)
        {
            _context = context;
        }

        public IList<Game> Game { get;set; } = null!;

        public async Task OnGetAsync()
        {
            Game = await _context.Games
                .Include(g => g.GameOption)
                .Include(g => g.Player1)
                .Include(g => g.Player2).ToListAsync();
        }
    }
}
