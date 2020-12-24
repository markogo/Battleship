using System.ComponentModel.DataAnnotations;

namespace Domain.Enums
{
    public enum NextMoveAfterHitEnum
    {
        [Display(Name = "Same player's turn")]
        SamePlayerTurn = 1,
        
        [Display(Name = "Other player's turn")]
        OtherPlayerTurn = 2
    }
}