using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CycleParkingViewer.Models;
using CycleParkingViewer.DataM;
using CsvHelper;
using System.Globalization;
using System.IO;
using OfficeOpenXml;

namespace CycleParkingViewer.Controllers;

public class ParkingTypeController: Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _db;
    public ParkingTypeController(ILogger<HomeController> _logger,ApplicationDbContext _db)
    {   
        this._logger=_logger;
        this._db=_db;
    }
    public IActionResult Index()
    {   
        var recordsToImport = new List<ParkingType>();
        IEnumerable<ParkingType> databaseEntires = _db.ParkingTypes; 
           using (var package = new ExcelPackage(new FileInfo("/home/mohammedshaad/Downloads/CodingChallengeData.xlsx")))
            {
                var worksheetCount = package.Workbook.Worksheets.Count;
                var worksheet = package.Workbook.Worksheets[0]; 
                
                if (databaseEntires == null)
                {
                    for (int row = worksheet.Dimension.Start.Row + 1; row <= worksheet.Dimension.End.Row; row++)
                    {
                        var rowData = new ParkingType
                        {
                            ParkingTypeName=worksheet.Cells[row,3].Text
                        };
                        recordsToImport.Add(rowData);
                    }
                    _db.ParkingTypes.AddRange(recordsToImport);
                    _db.SaveChanges();
                }
            }
        return View(databaseEntires);
    }
    //Loading the Create page
    public IActionResult CreateViewType()
    {
        return View("CreateView");
    }
    //Storing the info in the create page to the DB
    [HttpPost]
    public IActionResult Create(ParkingType pt)  
    {
        try{
            if (!ModelState.IsValid)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                _db.ParkingTypes.Add(pt);
                _db.SaveChanges();
                return RedirectToAction("Index","Home");  
            }
            return View("CreateView",pt);  
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal Server Error");
        }
    }
}