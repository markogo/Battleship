using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class GameSave
    {
        public int GameSaveId { get; set; }

        [Display(Name = "Save name")]
        [MaxLength(256)]
        public string? Name { get; set; }
        
        public bool CreatedByUser { get; set; }
        
        public int GameId { get; set; }
        
        public Game? Game { get; set; }

        public string? SerializedGame { get; set; }
    }
}