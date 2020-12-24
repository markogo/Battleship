using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DAL;
using Domain;

namespace WebApp.Pages.GameOptionShips
{
    public class EditModel : PageModel
    {
        private readonly DAL.AppDbContext _context;

        public EditModel(DAL.AppDbContext context)
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
           ViewData["GameOptionId"] = new SelectList(_context.GameOptions, "GameOptionId", "Name");
           ViewData["ShipId"] = new SelectList(_context.Ships, "ShipId", "Name");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(GameOptionShip).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GameOptionShipExists(GameOptionShip.GameOptionShipId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool GameOptionShipExists(int id)
        {
            return _context.GameOptionShips.Any(e => e.GameOptionShipId == id);
        }
    }
}
