using System.ComponentModel.DataAnnotations;

namespace CycleParkingViewer.Models;
public class Location{
    
    public int Id{get;set;}

    [Required(ErrorMessage = "Location Name is required.")]
    [Display(Name = "Location Name")]
    public string LocationName{get;set;}
}