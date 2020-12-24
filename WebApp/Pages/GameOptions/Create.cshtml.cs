using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Domain;

namespace WebApp.Pages.GameOptions
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
            return Page();
        }

        [BindProperty] public GameOption GameOption { get; set; } = null!;
        [BindProperty] public GameOptionShip Carrier { get; set; } = null!;
        [BindProperty] public GameOptionShip Battleship { get; set; } = null!;
        [BindProperty] public GameOptionShip Cruiser { get; set; } = null!;
        [BindProperty] public GameOptionShip Submarine { get; set; } = null!;
        [BindProperty] public GameOptionShip Destroyer { get; set; } = null!;

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.GameOptions.Add(GameOption);
            await _context.SaveChangesAsync();

            foreach (var ship in _context.Ships)
            {
                switch (ship.Name)
                {
                    case "Carrier":
                        Carrier.GameOptionId = GameOption.GameOptionId;
                        Carrier.ShipId = ship.ShipId;
                        _context.GameOptionShips.Add(Carrier);
                        break;
                    case "Battleship":
                        Battleship.GameOptionId = GameOption.GameOptionId;
                        Battleship.ShipId = ship.ShipId;
                        _context.GameOptionShips.Add(Battleship);
                        break;
                    case "Cruiser":
                        Cruiser.GameOptionId = GameOption.GameOptionId;
                        Cruiser.ShipId = ship.ShipId;
                        _context.GameOptionShips.Add(Cruiser);
                        break;
                    case "Submarine":
                        Submarine.GameOptionId = GameOption.GameOptionId;
                        Submarine.ShipId = ship.ShipId;
                        _context.GameOptionShips.Add(Submarine);
                        break;
                    default:
                        Destroyer.GameOptionId = GameOption.GameOptionId;
                        Destroyer.ShipId = ship.ShipId;
                        _context.GameOptionShips.Add(Destroyer);
                        break;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("../Index");
        }
    }
}
