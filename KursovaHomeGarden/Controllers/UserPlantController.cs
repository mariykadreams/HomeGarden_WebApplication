using KursovaHomeGarden.Models.Category;
using KursovaHomeGarden.Models.Plant;
using KursovaHomeGarden.Models;
using KursovaHomeGarden.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using KursovaHomeGarden.Models.CareLevel;

namespace KursovaHomeGarden.Controllers
{
    [Authorize] // Add authorization at controller level
    public class UserPlantController : Controller
    {
        private readonly ILogger<UserPlantController> _logger;
        private readonly string _connectionString;

        public UserPlantController(ILogger<UserPlantController> logger, IConfiguration configuration, IPlantService plantService)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
        }

        public IActionResult Index()
        {
            return RedirectToAction("MyPlant");
        }

        public IActionResult MyPlant()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userPlants = new List<UserPlant>();

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    using var command = new SqlCommand(@"
                        SELECT up.user_plant_id, up.purchase_date, 
                               p.plant_id, p.name, p.description, p.price, p.img,
                               c.category_id, c.category_name
                        FROM User_Plants up
                        JOIN Plants p ON up.plant_id = p.plant_id
                        LEFT JOIN Categories c ON p.category_id = c.category_id
                        WHERE up.user_id = @userId
                        ORDER BY up.purchase_date DESC", connection);

                    command.Parameters.AddWithValue("@userId", userId);

                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        userPlants.Add(new UserPlant
                        {
                            user_plant_id = reader.GetInt32(reader.GetOrdinal("user_plant_id")),
                            purchase_date = reader.GetDateTime(reader.GetOrdinal("purchase_date")),
                            plant_id = reader.GetInt32(reader.GetOrdinal("plant_id")),
                            Plant = new Plant
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
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Database error: {ex.Message}");
                }
            }

            return View(userPlants);
        }

        public IActionResult Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            Plant plant = null;
            UserPlant userPlant = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();

                    // First check if user owns the plant
                    using (var checkOwnershipCommand = new SqlCommand(
                        "SELECT COUNT(1) FROM User_Plants WHERE plant_id = @PlantId AND user_id = @UserId",
                        connection))
                    {
                        checkOwnershipCommand.Parameters.AddWithValue("@PlantId", id);
                        checkOwnershipCommand.Parameters.AddWithValue("@UserId", userId);
                        int ownershipCount = (int)checkOwnershipCommand.ExecuteScalar();

                        if (ownershipCount == 0)
                        {
                            // User doesn't own this plant, redirect to their plant list
                            return RedirectToAction(nameof(MyPlant));
                        }
                    }

                    // Get user plant details
                    using (var userPlantCommand = new SqlCommand(@"
                        SELECT user_plant_id, purchase_date 
                        FROM User_Plants 
                        WHERE plant_id = @PlantId AND user_id = @UserId", connection))
                    {
                        userPlantCommand.Parameters.AddWithValue("@PlantId", id);
                        userPlantCommand.Parameters.AddWithValue("@UserId", userId);

                        using var userPlantReader = userPlantCommand.ExecuteReader();
                        if (userPlantReader.Read())
                        {
                            userPlant = new UserPlant
                            {
                                user_plant_id = userPlantReader.GetInt32(userPlantReader.GetOrdinal("user_plant_id")),
                                purchase_date = userPlantReader.GetDateTime(userPlantReader.GetOrdinal("purchase_date")),
                                plant_id = id,
                                user_id = userId
                            };
                        }
                    }

                    // Get plant details
                    using var plantCommand = new SqlCommand(@"
                        SELECT p.plant_id, p.name, p.description, p.price, p.img,
                               c.category_id, c.category_name,
                               cl.care_level_id, cl.level_name,
                               sr.sunlight_requirements_id, sr.light_intensity,
                               af.Action_frequency_id, af.Interval, af.volume, af.notes,
                               s.season_id, s.season_name,
                               at.action_type_id, at.type_name
                        FROM Plants p
                        LEFT JOIN Categories c ON p.category_id = c.category_id
                        LEFT JOIN CareLevels cl ON p.care_level_id = cl.care_level_id
                        LEFT JOIN SunlightRequirements sr ON p.sunlight_requirements_id = sr.sunlight_requirements_id
                        LEFT JOIN ActionFrequencies af ON p.plant_id = af.plant_id
                        LEFT JOIN Seasons s ON af.season_id = s.season_id
                        LEFT JOIN ActionTypes at ON af.action_type_id = at.action_type_id
                        WHERE p.plant_id = @PlantId", connection);

                    plantCommand.Parameters.AddWithValue("@PlantId", id);

                    using var reader = plantCommand.ExecuteReader();
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
                                SunlightRequirement = !reader.IsDBNull(reader.GetOrdinal("sunlight_requirements_id")) ? new SunlightRequirement
                                {
                                    sunlight_requirements_id = reader.GetInt32(reader.GetOrdinal("sunlight_requirements_id")),
                                    light_intensity = reader.GetString(reader.GetOrdinal("light_intensity"))
                                } : null,
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
                    return RedirectToAction(nameof(MyPlant));
                }
            }

            if (plant == null)
            {
                return NotFound();
            }

            ViewBag.UserPlant = userPlant;
            return View(plant);
        }

        [HttpPost]
        public IActionResult DeletePlant(int userPlantId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    using var command = new SqlCommand(
                        "DELETE FROM User_Plants WHERE user_plant_id = @userPlantId AND user_id = @userId",
                        connection);

                    command.Parameters.AddWithValue("@userPlantId", userPlantId);
                    command.Parameters.AddWithValue("@userId", userId);

                    var rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true });
                    }
                    return Json(new { success = false, message = "Plant not found or you don't have permission to delete it." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error deleting plant: {ex.Message}");
                    return Json(new { success = false, message = "An error occurred while deleting the plant." });
                }
            }
        }
    }
}
