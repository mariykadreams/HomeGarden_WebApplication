using KursovaHomeGarden.Areas.Identity.Data;
using KursovaHomeGarden.Models;
using KursovaHomeGarden.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

[Authorize(Roles = SD.Role_Admin)]
public class ReportController : Controller
{
    private readonly string _connectionString;
    private readonly IPdfService _pdfService;

    public ReportController(IConfiguration configuration, IPdfService pdfService)
    {
        _connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
        _pdfService = pdfService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GenerateReport()
    {
        var reportData = new ReportData();

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            // Get Plant Popularity Data
            var plantQuery = @"
                SELECT 
                    p.name AS PlantName,
                    COUNT(up.plant_id) AS Popularity
                FROM 
                    [KursovaHomeGardenDB].[dbo].[User_Plants] up
                INNER JOIN 
                    [KursovaHomeGardenDB].[dbo].[Plants] p ON up.plant_id = p.plant_id
                GROUP BY 
                    p.name
                ORDER BY 
                    Popularity DESC;";

            using (var command = new SqlCommand(plantQuery, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reportData.PlantPopularity.Add(new PlantPopularityReport
                        {
                            PlantName = reader.GetString(reader.GetOrdinal("PlantName")),
                            Popularity = reader.GetInt32(reader.GetOrdinal("Popularity"))
                        });
                    }
                }
            }

            // Get Category Statistics
            var categoryQuery = @"
                SELECT 
                    c.category_name AS CategoryName, 
                    AVG(p.price) AS average_price, 
                    COUNT(DISTINCT up.user_id) AS user_count 
                FROM 
                    User_Plants up
                JOIN 
                    Plants p ON up.plant_id = p.plant_id 
                JOIN 
                    Categories c ON p.category_id = c.category_id 
                GROUP BY 
                    c.category_name
                ORDER BY 
                    user_count DESC, 
                    average_price DESC;";

            using (var command = new SqlCommand(categoryQuery, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reportData.CategoryStatistics.Add(new CategoryStatisticsReport
                        {
                            CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                            AveragePrice = reader.GetDecimal(reader.GetOrdinal("average_price")),
                            UserCount = reader.GetInt32(reader.GetOrdinal("user_count"))
                        });
                    }
                }
            }

            // Get User Data
            var userQuery = @"
                SELECT 
                    u.UserName,
                    u.AmountOfMoney,
                    r.Name AS RoleName
                FROM AspNetUsers u
                LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
                LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
                ORDER BY u.UserName";

            using (var command = new SqlCommand(userQuery, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reportData.Users.Add(new UserReport
                        {
                            UserName = reader.GetString(reader.GetOrdinal("UserName")),
                            AmountOfMoney = reader.IsDBNull(reader.GetOrdinal("AmountOfMoney"))
                                ? null
                                : reader.GetDecimal(reader.GetOrdinal("AmountOfMoney")),
                            Role = reader.IsDBNull(reader.GetOrdinal("RoleName"))
                                ? "No Role"
                                : reader.GetString(reader.GetOrdinal("RoleName"))
                        });
                    }
                }
            }
        }

        return View("Report", reportData);
    }

    [HttpGet]
    public IActionResult ExportToPdf()
    {
        var reportData = GenerateReportData();
        var pdfBytes = _pdfService.GenerateReportPdf(reportData);

        return File(
            pdfBytes,
            "application/pdf",
            $"GardenManagementReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
        );
    }

    private ReportData GenerateReportData()
    {
        var reportData = new ReportData();

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            // Get Plant Popularity Data
            var plantQuery = @"
                SELECT 
                    p.name AS PlantName,
                    COUNT(up.plant_id) AS Popularity
                FROM 
                    [KursovaHomeGardenDB].[dbo].[User_Plants] up
                INNER JOIN 
                    [KursovaHomeGardenDB].[dbo].[Plants] p ON up.plant_id = p.plant_id
                GROUP BY 
                    p.name
                ORDER BY 
                    Popularity DESC;";

            using (var command = new SqlCommand(plantQuery, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reportData.PlantPopularity.Add(new PlantPopularityReport
                        {
                            PlantName = reader.GetString(reader.GetOrdinal("PlantName")),
                            Popularity = reader.GetInt32(reader.GetOrdinal("Popularity"))
                        });
                    }
                }
            }

            // Get Category Statistics
            var categoryQuery = @"
                SELECT 
                    c.category_name AS CategoryName, 
                    AVG(p.price) AS average_price, 
                    COUNT(DISTINCT up.user_id) AS user_count 
                FROM 
                    User_Plants up
                JOIN 
                    Plants p ON up.plant_id = p.plant_id 
                JOIN 
                    Categories c ON p.category_id = c.category_id 
                GROUP BY 
                    c.category_name
                ORDER BY 
                    user_count DESC, 
                    average_price DESC;";

            using (var command = new SqlCommand(categoryQuery, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reportData.CategoryStatistics.Add(new CategoryStatisticsReport
                        {
                            CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                            AveragePrice = reader.GetDecimal(reader.GetOrdinal("average_price")),
                            UserCount = reader.GetInt32(reader.GetOrdinal("user_count"))
                        });
                    }
                }
            }

            // Get User Data
            var userQuery = @"
                SELECT 
                    u.UserName,
                    u.AmountOfMoney,
                    r.Name AS RoleName
                FROM AspNetUsers u
                LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
                LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
                ORDER BY u.UserName";

            using (var command = new SqlCommand(userQuery, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reportData.Users.Add(new UserReport
                        {
                            UserName = reader.GetString(reader.GetOrdinal("UserName")),
                            AmountOfMoney = reader.IsDBNull(reader.GetOrdinal("AmountOfMoney"))
                                ? null
                                : reader.GetDecimal(reader.GetOrdinal("AmountOfMoney")),
                            Role = reader.IsDBNull(reader.GetOrdinal("RoleName"))
                                ? "No Role"
                                : reader.GetString(reader.GetOrdinal("RoleName"))
                        });
                    }
                }
            }
        }

        return reportData;
    }
}