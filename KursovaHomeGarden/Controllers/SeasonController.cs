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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Season season)
        {
            if (!ModelState.IsValid)
            {
                return View(season);
            }

            if (season.temperature_range_min > season.temperature_range_max)
            {
                ModelState.AddModelError("", "Minimum temperature cannot be greater than maximum temperature.");
                return View(season);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = @"INSERT INTO Seasons (season_name, season_start, season_end, 
                                temperature_range_min, temperature_range_max) 
                                VALUES (@season_name, @season_start, @season_end, 
                                @temperature_range_min, @temperature_range_max)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@season_name", season.season_name);
                        command.Parameters.AddWithValue("@season_start", season.season_start);
                        command.Parameters.AddWithValue("@season_end", season.season_end);
                        command.Parameters.AddWithValue("@temperature_range_min", season.temperature_range_min);
                        command.Parameters.AddWithValue("@temperature_range_max", season.temperature_range_max);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating season: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while saving the season. Please try again.");
                return View(season);
            }
        }

        public IActionResult Edit(int id)
        {
            Season season = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM Seasons WHERE season_id = @id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                season = new Season
                                {
                                    season_id = reader.GetInt32(reader.GetOrdinal("season_id")),
                                    season_name = reader.GetString(reader.GetOrdinal("season_name")),
                                    season_start = reader.GetDateTime(reader.GetOrdinal("season_start")),
                                    season_end = reader.GetDateTime(reader.GetOrdinal("season_end")),
                                    temperature_range_min = reader.GetDecimal(reader.GetOrdinal("temperature_range_min")),
                                    temperature_range_max = reader.GetDecimal(reader.GetOrdinal("temperature_range_max"))
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving season: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }

            if (season == null)
            {
                return NotFound();
            }

            return View(season);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Season season)
        {
            if (id != season.season_id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(season);
            }

            if (season.temperature_range_min > season.temperature_range_max)
            {
                ModelState.AddModelError("", "Minimum temperature cannot be greater than maximum temperature.");
                return View(season);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = @"UPDATE Seasons 
                             SET season_name = @season_name, 
                                 season_start = @season_start, 
                                 season_end = @season_end, 
                                 temperature_range_min = @temperature_range_min, 
                                 temperature_range_max = @temperature_range_max 
                             WHERE season_id = @id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@season_name", season.season_name);
                        command.Parameters.AddWithValue("@season_start", season.season_start);
                        command.Parameters.AddWithValue("@season_end", season.season_end);
                        command.Parameters.AddWithValue("@temperature_range_min", season.temperature_range_min);
                        command.Parameters.AddWithValue("@temperature_range_max", season.temperature_range_max);
                        command.Parameters.AddWithValue("@id", id);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating season: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while updating the season. Please try again.");
                return View(season);
            }
        }
    }
}