using Microsoft.EntityFrameworkCore;
using CycleParkingViewer.Models;


namespace CycleParkingViewer.DataM;

public class ApplicationDbContext :DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }
    public DbSet<ParkingInfo> ParkingInfos {get;set;}
    public DbSet<ParkingType> ParkingTypes{get; set;}
    public DbSet<Location> Locations{get; set;}
}