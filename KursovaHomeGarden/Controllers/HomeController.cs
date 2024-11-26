using KursovaHomeGarden.Models;
using KursovaHomeGarden.Models.Plant;
using KursovaHomeGarden.Models.Category;
using KursovaHomeGarden.Models.CareLevel;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace KursovaHomeGarden.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _connectionString;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
        }

        public IActionResult Index()
        {
            var plants = new List<Plant>();

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    using var command = new SqlCommand(@"
                        SELECT p.plant_id, p.name, p.description, p.price, p.img,
                               c.category_id, c.category_name
                        FROM Plants p
                        LEFT JOIN Categories c ON p.category_id = c.category_id", connection);

                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        plants.Add(new Plant
                        {
                            plant_id = reader.GetInt32(reader.GetOrdinal("plant_id")),
                            name = reader.GetString(reader.GetOrdinal("name")),
                            description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                            price = reader.GetDecimal(reader.GetOrdinal("price")),
                            img = reader.IsDBNull(reader.GetOrdinal("img")) ? null : reader.GetString(reader.GetOrdinal("img")),
                            Category = new Category
                            {
                                category_id = reader.GetInt32(reader.GetOrdinal("category_id")),
                                category_name = reader.GetString(reader.GetOrdinal("category_name"))
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Database error: {ex.Message}");
                    // Return empty list on error instead of throwing
                }
            }

            return View(plants);
        }

        public IActionResult Details(int id)
        {
            Plant plant = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    using var command = new SqlCommand(@"
                        SELECT p.plant_id, p.name, p.description, p.price, p.img,
                               c.category_id, c.category_name,
                               cl.care_level_id, cl.level_name,
                               af.action_frequency_id, af.Interval, af.volume, af.notes,
                               s.season_id, s.season_name,
                               at.action_type_id, at.type_name
                        FROM Plants p
                        LEFT JOIN Categories c ON p.category_id = c.category_id
                        LEFT JOIN CareLevels cl ON p.care_level_id = cl.care_level_id
                        LEFT JOIN ActionFrequencies af ON p.plant_id = af.plant_id
                        LEFT JOIN Seasons s ON af.season_id = s.season_id
                        LEFT JOIN ActionTypes at ON af.action_type_id = at.action_type_id
                        WHERE p.plant_id = @PlantId", connection);

                    command.Parameters.AddWithValue("@PlantId", id);

                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (plant == null)
                        {
                            plant = new Plant
                            {
                                plant_id = reader.GetInt32(reader.GetOrdinal("plant_id")),
                                name = reader.GetString(reader.GetOrdinal("name")),
                                description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                                price = reader.GetDecimal(reader.GetOrdinal("price")),
                                img = reader.IsDBNull(reader.GetOrdinal("img")) ? null : reader.GetString(reader.GetOrdinal("img")),
                                Category = new Category
                                {
                                    category_id = reader.GetInt32(reader.GetOrdinal("category_id")),
                                    category_name = reader.GetString(reader.GetOrdinal("category_name"))
                                },
                                CareLevel = new CareLevel
                                {
                                    care_level_id = reader.GetInt32(reader.GetOrdinal("care_level_id")),
                                    level_name = reader.GetString(reader.GetOrdinal("level_name"))
                                },
                                ActionFrequencies = new List<ActionFrequency>()
                            };
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("Action_frequency_id")))
                        {
                            plant.ActionFrequencies.Add(new ActionFrequency
                            {
                                Action_frequency_id = reader.GetInt32(reader.GetOrdinal("Action_frequency_id")),
                                Interval = reader.GetString(reader.GetOrdinal("Interval")),
                                volume = reader.IsDBNull(reader.GetOrdinal("volume")) ? null : reader.GetDecimal(reader.GetOrdinal("volume")),
                                notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
                                Season = new Season
                                {
                                    season_id = reader.GetInt32(reader.GetOrdinal("season_id")),
                                    season_name = reader.GetString(reader.GetOrdinal("season_name"))
                                },
                                ActionType = new ActionType
                                {
                                    action_type_id = reader.GetInt32(reader.GetOrdinal("action_type_id")),
                                    type_name = reader.GetString(reader.GetOrdinal("type_name"))
                                }
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Database error: {ex.Message}");
                    return RedirectToAction(nameof(Index));
                }
            }

            if (plant == null)
            {
                return NotFound();
            }

            return View(plant);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}