using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL;
using Domain;

namespace WebApp.Pages.GameOptionShips
{
    public class IndexModel : PageModel
    {
        private readonly DAL.AppDbContext _context;

        public IndexModel(DAL.AppDbContext context)
        {
            _context = context;
        }

        public IList<GameOptionShip> GameOptionShip { get;set; } = null!;

        public async Task OnGetAsync()
        {
            GameOptionShip = await _context.GameOptionShips
                .Include(g => g.GameOption)
                .Include(g => g.Ship).ToListAsync();
        }
    }
}
