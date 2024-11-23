// Path: Controllers/SeasonController.cs
using Microsoft.AspNetCore.Mvc;
using KursovaHomeGarden.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace KursovaHomeGarden.Controllers
{
    public class SeasonController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<SeasonController> _logger;

        public SeasonController(IConfiguration configuration, ILogger<SeasonController> logger)
        {
            _connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
            _logger = logger;
        }

        public IActionResult Index()
        {
            var seasons = new List<Season>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM Seasons";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                seasons.Add(new Season
                                {
                                    season_id = reader.GetInt32(reader.GetOrdinal("season_id")),
                                    season_name = reader.GetString(reader.GetOrdinal("season_name")),
                                    season_start = reader.GetDateTime(reader.GetOrdinal("season_start")),
                                    season_end = reader.GetDateTime(reader.GetOrdinal("season_end")),
                                    temperature_range_min = reader.GetDecimal(reader.GetOrdinal("temperature_range_min")),
                                    temperature_range_max = reader.GetDecimal(reader.GetOrdinal("temperature_range_max"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving seasons: {ex.Message}");
                ViewBag.Message = $"Error: {ex.Message}";
            }
            return View(seasons);
        }
    }
}
