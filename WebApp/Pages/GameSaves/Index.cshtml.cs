using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL;
using Domain;

namespace WebApp.Pages.GameSaves
{
    public class IndexModel : PageModel
    {
        private readonly DAL.AppDbContext _context;

        public IndexModel(DAL.AppDbContext context)
        {
            _context = context;
        }

        public IList<GameSave> GameSave { get;set; } = null!;

        public async Task OnGetAsync()
        {
            GameSave = await _context.GameSaves
                .Include(g => g.Game).Where(gs => gs.CreatedByUser).ToListAsync();
        }
    }
}
