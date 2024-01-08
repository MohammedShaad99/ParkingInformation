using System.ComponentModel.DataAnnotations;

namespace CycleParkingViewer.Models;
public class ParkingType{
    
    public int Id{get; set;}

    [Required(ErrorMessage = "Parking Type is required is required.")]
    [Display(Name = "Parking Type")]
    public string ParkingTypeName{get; set;}
}