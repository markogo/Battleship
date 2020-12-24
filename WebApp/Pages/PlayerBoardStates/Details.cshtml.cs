using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL;
using Domain;

namespace WebApp.Pages.PlayerBoardStates
{
    public class DetailsModel : PageModel
    {
        private readonly DAL.AppDbContext _context;

        public DetailsModel(DAL.AppDbContext context)
        {
            _context = context;
        }

        public PlayerBoardState PlayerBoardState { get; set; } = null!;

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
            return Page();
        }
    }
}
