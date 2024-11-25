using KursovaHomeGarden.Models.Plant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;

namespace KursovaHomeGarden.Controllers.Users
{
	public class AdminController : Controller
	{
		private readonly string _connectionString;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public AdminController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
		{
			_connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
			_webHostEnvironment = webHostEnvironment;
		}


		public IActionResult Index()
		{
			var plants = new List<Plant>();
			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				var query = "SELECT plant_id, name FROM Plants";
				using (var command = new SqlCommand(query, connection))
				{
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							plants.Add(new Plant
							{
								plant_id = reader.GetInt32(reader.GetOrdinal("plant_id")),
								name = reader.GetString(reader.GetOrdinal("name"))
							});
						}
					}
				}
			}

			ViewBag.Plants = plants;
			return View();
		}


		[HttpPost]
		public IActionResult GetActionFrequencies()
		{
			var actionFrequencies = new List<dynamic>();

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				using (var command = new SqlCommand(GetSqlQuery(), connection))
				{
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							dynamic item = new ExpandoObject();
							item.ActionTypeId = reader.GetInt32(0);
							item.ActionType = reader.GetString(1);
							item.ActionFrequencyCount = reader.GetInt32(2);
							actionFrequencies.Add(item);
						}
					}
				}
			}

			return PartialView("_ActionFrequenciesTable", actionFrequencies);
		}



		[HttpPost]
		public IActionResult GetPlantActions(DateTime startDate, DateTime endDate, int plantId)
		{
			var plantActions = new List<dynamic>();

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();

				// Use the stored procedure to get plant actions
				using (var command = new SqlCommand(GetSqlQuery1(), connection))
				{
					command.CommandType = CommandType.StoredProcedure;

					// Add the parameters for the stored procedure
					command.Parameters.AddWithValue("@StartDate", startDate);
					command.Parameters.AddWithValue("@EndDate", endDate);
					command.Parameters.AddWithValue("@PlantId", plantId);

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							// Create dynamic objects for each row
							dynamic item = new ExpandoObject();
							item.PlantId = reader.GetInt32(reader.GetOrdinal("plant_id"));
							item.PlantName = reader.GetString(reader.GetOrdinal("plant_name"));
							item.SeasonName = reader.GetString(reader.GetOrdinal("season_name"));
							item.TypeName = reader.GetString(reader.GetOrdinal("type_name"));
							item.Interval = reader.GetString(reader.GetOrdinal("Interval"));
							item.Volume = reader.GetDecimal(reader.GetOrdinal("volume"));
							item.Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes"));
							item.FertName = reader.IsDBNull(reader.GetOrdinal("fert_name")) ? null : reader.GetString(reader.GetOrdinal("fert_name"));
							item.FertDescription = reader.IsDBNull(reader.GetOrdinal("fert_description")) ? null : reader.GetString(reader.GetOrdinal("fert_description"));

							// Add the dynamic item to the list
							plantActions.Add(item);
						}
					}
				}
			}

			// Return the partial view with the data
			return PartialView("_PlantActionsTable", plantActions);
		}






		private string LoadSqlFromFile(string fileName)
		{
			var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "sql", fileName);
			if (!System.IO.File.Exists(filePath))
			{
				throw new FileNotFoundException($"SQL file not found: {filePath}");
			}
			return System.IO.File.ReadAllText(filePath);
		}



		public string GetSqlQuery1()
		{
			return @"CREATE PROCEDURE GetPlantActionsForPeriod
    @StartDate DATE,
    @EndDate DATE,
    @PlantId INT
AS
BEGIN
    SELECT 
        p.plant_id,
        p.plant_name,
        s.season_name,
        at.type_name,
        af.Interval,
        af.volume,
        af.notes,
        f.fert_name,
        f.fert_description
    FROM 
        ActionFrequencies af -- Changed from ActionFrequency to ActionFrequencies
    INNER JOIN 
        Plants p ON af.plant_id = p.plant_id
    INNER JOIN 
        Seasons s ON af.season_id = s.season_id
    INNER JOIN 
        ActionTypes at ON af.action_type_id = at.action_type_id
    LEFT JOIN 
        Fertilizes f ON af.Fert_type_id = f.Fert_type_id
    WHERE 
        p.plant_id = @PlantId
        AND (
            (@StartDate BETWEEN s.season_start AND s.season_end)
            OR (@EndDate BETWEEN s.season_start AND s.season_end)
            OR (s.season_start BETWEEN @StartDate AND @EndDate)
        )
    ORDER BY 
        s.season_start, at.type_name, af.Interval;
END;
";
		}

		private string GetSqlQuery()
		{
			return @"
                SELECT 
                    af.action_type_id,
                    at.type_name AS action_type,
                    COUNT(af.action_type_id) AS action_frequency_count
                FROM 
                    [KursovaHomeGardenDB].[dbo].[ActionFrequencies] af
                JOIN
                    [KursovaHomeGardenDB].[dbo].[ActionTypes] at
                    ON af.action_type_id = at.action_type_id
                GROUP BY 
                    af.action_type_id, at.type_name
                ORDER BY 
                    action_frequency_count DESC;
            ";
		}
	}
}