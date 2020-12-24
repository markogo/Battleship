using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DAL;
using Domain;

namespace WebApp.Pages.GameOptionShips
{
    public class CreateModel : PageModel
    {
        private readonly DAL.AppDbContext _context;

        public CreateModel(DAL.AppDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["GameOptionId"] = new SelectList(_context.GameOptions, "GameOptionId", "Name");
        ViewData["ShipId"] = new SelectList(_context.Ships, "ShipId", "Name");
            return Page();
        }

        [BindProperty] public GameOptionShip GameOptionShip { get; set; } = null!;

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.GameOptionShips.Add(GameOptionShip);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
