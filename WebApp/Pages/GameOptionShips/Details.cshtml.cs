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
    public class DetailsModel : PageModel
    {
        private readonly DAL.AppDbContext _context;

        public DetailsModel(DAL.AppDbContext context)
        {
            _context = context;
        }

        public GameOptionShip GameOptionShip { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            GameOptionShip = await _context.GameOptionShips
                .Include(g => g.GameOption)
                .Include(g => g.Ship).FirstOrDefaultAsync(m => m.GameOptionShipId == id);

            if (GameOptionShip == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
