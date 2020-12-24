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
    public class DeleteModel : PageModel
    {
        private readonly DAL.AppDbContext _context;

        public DeleteModel(DAL.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty] public GameOptionShip GameOptionShip { get; set; } = null!;

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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            GameOptionShip = await _context.GameOptionShips.FindAsync(id);

            if (GameOptionShip != null)
            {
                _context.GameOptionShips.Remove(GameOptionShip);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
