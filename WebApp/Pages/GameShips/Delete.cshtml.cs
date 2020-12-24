using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL;
using Domain;

namespace WebApp.Pages.GameShips
{
    public class DeleteModel : PageModel
    {
        private readonly DAL.AppDbContext _context;

        public DeleteModel(DAL.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty] public GameShip GameShip { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            GameShip = await _context.GameShips
                .Include(g => g.Player).FirstOrDefaultAsync(m => m.GameShipId == id);

            if (GameShip == null)
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

            GameShip = await _context.GameShips.FindAsync(id);

            if (GameShip != null)
            {
                _context.GameShips.Remove(GameShip);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
