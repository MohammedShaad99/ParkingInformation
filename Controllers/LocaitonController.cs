using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CycleParkingViewer.Models;
using CycleParkingViewer.DataM;
using CsvHelper;
using System.Globalization;
using System.IO;
using OfficeOpenXml;

namespace CycleParkingViewer.Controllers;

public class LocationController: Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _db;
    public LocationController(ILogger<HomeController> _logger,ApplicationDbContext _db)
    {   
        this._logger=_logger;
        this._db=_db;
    }
    public IActionResult Index()
    {   
        try
        {
            var recordsToImport = new List<Location>();
            IEnumerable<Location> databaseEntires = _db.Locations; 
            using (var package = new ExcelPackage(new FileInfo("/home/mohammedshaad/Downloads/CodingChallengeData.xlsx")))
                {
                    var worksheetCount = package.Workbook.Worksheets.Count;
                    var worksheet = package.Workbook.Worksheets[0]; 
                    //Console.WriteLine("------------------>",databaseEntires);
                    if (databaseEntires == null)
                    {
                        for (int row = worksheet.Dimension.Start.Row + 1; row <= worksheet.Dimension.End.Row; row++)
                        {
                            var rowData = new Location
                            {
                                LocationName=worksheet.Cells[row,2].Text
                            };
                            recordsToImport.Add(rowData);
                        }
                        _db.Locations.AddRange(recordsToImport);
                        _db.SaveChanges();
                    }
                }
                
            return View(databaseEntires);
        }   
        catch (Exception ex)
        {
            return StatusCode(500, "Internal Server Error");
        }
    }
    //Loading the Create View Page
    public IActionResult CreateViewLocation()
    {
        return View("CreateView");
    }
    //Storing the info in the Create page.
    [HttpPost]
    public IActionResult Create(Location location)  
    {   
        try
        {
            if (!ModelState.IsValid)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                _db.Locations.Add(location);
                _db.SaveChanges();
                return RedirectToAction("Index","Home");  
            }

            return View("CreateView",location); 
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal Server Error");
        } 
    }

}