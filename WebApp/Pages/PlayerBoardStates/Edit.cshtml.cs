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

namespace WebApp.Pages.PlayerBoardStates
{
    public class EditModel : PageModel
    {
        private readonly DAL.AppDbContext _context;

        public EditModel(DAL.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty] public PlayerBoardState PlayerBoardState { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PlayerBoardState = await _context.PlayerBoardStates
                .Include(p => p.Player).FirstOrDefaultAsync(m => m.PlayerBoardStateId == id);

            if (PlayerBoardState == null)
            {
                return NotFound();
            }
           ViewData["PlayerId"] = new SelectList(_context.Players, "PlayerId", "Name");
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

            _context.Attach(PlayerBoardState).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerBoardStateExists(PlayerBoardState.PlayerBoardStateId))
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

        private bool PlayerBoardStateExists(int id)
        {
            return _context.PlayerBoardStates.Any(e => e.PlayerBoardStateId == id);
        }
    }
}
