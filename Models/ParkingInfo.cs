using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CycleParkingViewer.Models;


public class ParkingInfo
{
    [Key]
    public int Id {get;set;}
    [Required(ErrorMessage = "Title is a required field.")]
    [Display(Name = "Title")]
    public string Title {get;set;}

    //Foreign Key Location
    public int LocationId{get; set;}
    [ForeignKey("LocationId")]
    public virtual Location Location {get;set;}


    [Required(ErrorMessage = "Secure is required field.")]
    [Display(Name = "Secure")]
    public bool Secure {get;set;}
    //[Required]
    [Display(Name = "Capacity")]
    public int? CapacityNo {get;set;}
    [Display(Name = "Availability")]
    public string? Availability {get;set;}
    [Display(Name = "Longitude")]
    public float? Longitude {get;set;}
    [Display(Name = "Latitude")]
    public float? Latitude {get;set;}


    //Foreign Key Type
    public int ParkingTypeId {get; set;}
    [ForeignKey("ParkingTypeId")]
    public virtual ParkingType Type {get;set;}


    //For Drop Down list
    [NotMapped]
    public List<SelectListItem> locationList {get; set;}
    [NotMapped]
    public List<SelectListItem> typeList {get; set;}

    //Used in creating the INfo
    public ParkingInfo()
    {
        locationList = new List<SelectListItem>();
        typeList = new List<SelectListItem>();
    }
    //Used for editing the INfo.
    public void UpdateFromModel(ParkingInfo model)
    {
        Title = model.Title;
        LocationId= model.LocationId; 
        ParkingTypeId = model.ParkingTypeId;        
        Secure = model.Secure;
        CapacityNo = model.CapacityNo;
        Availability = model.Availability;
        Longitude = model.Longitude;
        Latitude = model.Latitude;
    }
}
