using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CycleParkingViewer.Models;
using CycleParkingViewer.DataM;
using CsvHelper;
using System.Globalization;
using System.IO;
using OfficeOpenXml;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace CycleParkingViewer.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _db;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext _db)
    {
        _logger = logger;
        this._db = _db;
    }

    public IActionResult Index()
    {
        try
        {
            var recordsToImport = new List<ParkingInfo>();
            var recordsToImportLoc = new List<Location>();
            var recordsToImportPT = new List<ParkingType>();
            IEnumerable<ParkingType> dbEntriesPT = _db.ParkingTypes; 
            IEnumerable<Location> dbEntriesLoc = _db.Locations; 
            IEnumerable<ParkingInfo> databaseEntires = _db.ParkingInfos; 
            //extracting the data from Excel sheet into the DB.
            using (var package = new ExcelPackage(new FileInfo("../CodingChallengeData.xlsx")))
            {
                var worksheetCount = package.Workbook.Worksheets.Count;
                var worksheet = package.Workbook.Worksheets[0]; // Assuming the CSV is in the first worksheet

                if (!_db.ParkingInfos.Any())
                {
                    for (int row = worksheet.Dimension.Start.Row + 1; row <= worksheet.Dimension.End.Row; row++)
                    {
                        var xlLocName = worksheet.Cells[row, 2].Text;
                        var xlType = worksheet.Cells[row, 3].Text;

                        var locName = _db.Locations.FirstOrDefault(loc => loc.LocationName == xlLocName);
                        var type = _db.ParkingTypes.FirstOrDefault(type => type.ParkingTypeName == xlType);

                        var rowData = new ParkingInfo
                        {
                            Title = worksheet.Cells[row, 1].Text,
                            Secure = TryParseYesNo(worksheet.Cells[row, 4].Text),
                            CapacityNo = int.Parse(worksheet.Cells[row,5].Text),
                            Availability = worksheet.Cells[row, 6].Text,
                            Longitude = float.Parse(worksheet.Cells[row, 7].Text),
                            Latitude = float.Parse(worksheet.Cells[row, 8].Text),
                        };
                        //Updating the Foreign Key ID
                        if (locName != null)
                        {
                            rowData.LocationId = locName.Id;
                        }
                        else
                        {
                            var rowData1 = new Location
                            {
                                LocationName = worksheet.Cells[row, 2].Text
                            };
                            _db.Locations.Add(rowData1);
                            _db.SaveChanges();
                            rowData.LocationId = rowData1.Id;
                        }

                        if (type != null)
                        {
                            rowData.ParkingTypeId = type.Id;
                        }
                        else
                        {
                            var rowData1 = new ParkingType
                            {
                                ParkingTypeName = worksheet.Cells[row, 3].Text
                            };
                            _db.ParkingTypes.Add(rowData1);
                            _db.SaveChanges();
                            rowData.ParkingTypeId = rowData1.Id;
                        }

                        recordsToImport.Add(rowData);
                    }
                    //Storing the records in the database
                    _db.ParkingInfos.AddRange(recordsToImport);
                    _db.SaveChanges();
                }
            }
    
            // Retrieve ParkingInfos including related data (Location and Types)
            var parkingInfos = _db.ParkingInfos.Include(pi => pi.Location).Include(pi=>pi.Type).ToList();

            return View(parkingInfos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal Server Error");
        }
    }
    //Search box 
    public IActionResult Search(string search)
    {
        IEnumerable<ParkingInfo> elements = Enumerable.Empty<ParkingInfo>();

        if (!string.IsNullOrEmpty(search))
        {  
            elements = _db.ParkingInfos
                          .Include(p=>p.Type).Include(p=>p.Location).Where(p => p.Title.Contains(search) || p.Type.ParkingTypeName.Contains(search))
                          .ToList();
        }
        return View("Index", elements);
    }
    //Loading the create page
    public IActionResult CreateViewParkingInfo()
    {    
        try{
            var model = new ParkingInfo
            {
                locationList = _db.Locations
                            .Select(loc => new SelectListItem { Value = loc.LocationName, Text = loc.LocationName })
                            .ToList(),
                typeList = _db.ParkingTypes
                            .Select(type => new SelectListItem { Value = type.ParkingTypeName, Text = type.ParkingTypeName})
                            .ToList()
            };
            return View("CreateViewParkingInfo",model);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal Server Error");
        }
    }
    //Delete action
    public IActionResult Delete(int id)
    {
        try
        {
            var pi=_db.ParkingInfos.FirstOrDefault(model=>model.Id==id);

            if (pi==null)
            {
                return NotFound();
            }
            _db.ParkingInfos.Remove(pi);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            // Log or handle the exception as needed
            return StatusCode(500, "Internal Server Error");
        }
    }
    //Loading the Edit View Page
    public IActionResult EditView(int id)
    {
        try
        {
            ViewBag.locationList = _db.Locations
                           .Select(loc => new SelectListItem { Value = loc.LocationName, Text = loc.LocationName })
                           .ToList();
            ViewBag.typeList = _db.ParkingTypes
                           .Select(type => new SelectListItem { Value = type.ParkingTypeName, Text = type.ParkingTypeName})
                           .ToList();
            var pi=_db.ParkingInfos.Include(m=>m.Location).Include(m=>m.Type).FirstOrDefault(m=>m.Id==id);
            
            if (pi==null)
            {
                return NotFound();
            }
            return View("CreateViewParkingInfo", pi);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal Server Error");
        }
    }
    //Storing the edit page with info into the DB
    [HttpPost]
    public IActionResult Edit(ParkingInfo pi)  
    {  
        try
        {
            //Console.WriteLine("---------->1");
            var piVar=_db.ParkingInfos.FirstOrDefault(m=>m.Id==pi.Id);
            if (ModelState.IsValid)
            {
                if(piVar==null)
                {
                    return NotFound();
                }
                piVar.UpdateFromModel(pi);
                _db.SaveChanges();
                return RedirectToAction("Index","Home");  
            }
            return View("CreateViewParkingInfo",pi); 
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal Server Error");
        }
            //Console.WriteLine("---------->4");
        
    }
    //Storing the create page into DB 
    [HttpPost]
    public IActionResult Create(ParkingInfo pi)  
    {   
        try
        {
            Console.WriteLine("---------->1");

            if (ModelState.IsValid)
            {
                Console.WriteLine("---------->2");
                _db.ParkingInfos.Add(pi);
                _db.SaveChanges();
                return RedirectToAction("Index","Home");  
            }
                Console.WriteLine("---------->4");
            return View("CreateViewParkingInfo",pi); 
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal Server Error");
        } 
    }
    //Displaying the Deail page.
    public IActionResult Detail(int id)
    {
        try{//Console.WriteLine("------------------------->");
            var parkingInfo = _db.ParkingInfos.Include(p => p.Location).Include(p => p.Type).FirstOrDefault(p => p.Id == id);

            if (parkingInfo == null)
            {
                return NotFound(); 
            }
            return View(parkingInfo);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal Server Error");
        }
    }
    //It is used in the Index Action for the Secure variable
    bool TryParseYesNo(string input)
    {
        if (string.Equals(input, "Yes", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        else if (string.Equals(input, "No", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        else
        {
            // Handle other cases or return a default value
            return false; // Or throw an exception, depending on your requirements
        }
    }       

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
