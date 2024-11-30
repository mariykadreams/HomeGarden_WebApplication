using KursovaHomeGarden.Models;
using KursovaHomeGarden.Models.Plant;
using KursovaHomeGarden.Models.Category;
using KursovaHomeGarden.Models.CareLevel;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;
using KursovaHomeGarden.Services;

namespace KursovaHomeGarden.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _connectionString;
        private readonly IPlantService _plantService;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IPlantService plantService)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
            _plantService = plantService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm, decimal? minPrice, decimal? maxPrice,
            int? categoryId, int? careLevelId, string sortBy)
        {
            try
            {
                var (plants, categories, careLevels) = await _plantService.GetFilteredPlantsAsync(
                    searchTerm, minPrice, maxPrice, categoryId, careLevelId, sortBy);

                ViewBag.Categories = categories;
                ViewBag.CareLevels = careLevels;
                ViewBag.CurrentSearchTerm = searchTerm;
                ViewBag.CurrentMinPrice = minPrice;
                ViewBag.CurrentMaxPrice = maxPrice;
                ViewBag.CurrentCategoryId = categoryId;
                ViewBag.CurrentCareLevelId = careLevelId;
                ViewBag.CurrentSortBy = sortBy;

                return View(plants);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving plants: {ex.Message}");
                return View( new KursovaHomeGarden.Models.Plant.Plant());
            }
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
                }
            }

            return View(plants);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToMyPlants(int plantId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using var transaction = connection.BeginTransaction();

                    try
                    {
                        // Check if user has enough money
                        decimal plantPrice = 0;
                        decimal userBalance = 0;

                        // Get plant price
                        using (var command = new SqlCommand(
                            "SELECT price FROM Plants WHERE plant_id = @plantId",
                            connection,
                            transaction))
                        {
                            command.Parameters.AddWithValue("@plantId", plantId);
                            var result = command.ExecuteScalar();
                            if (result != null)
                            {
                                plantPrice = (decimal)result;
                            }
                        }

                        // Get user's balance
                        using (var command = new SqlCommand(
                            "SELECT AmountOfMoney FROM AspNetUsers WHERE Id = @userId",
                            connection,
                            transaction))
                        {
                            command.Parameters.AddWithValue("@userId", userId);
                            var result = command.ExecuteScalar();
                            if (result != DBNull.Value)
                            {
                                userBalance = (decimal)result;
                            }
                        }

                        if (userBalance < plantPrice)
                        {
                            transaction.Rollback();
                            return Json(new { success = false, message = "Insufficient funds to purchase this plant." });
                        }

                        // Update user's balance
                        using (var command = new SqlCommand(
                            "UPDATE AspNetUsers SET AmountOfMoney = AmountOfMoney - @price WHERE Id = @userId",
                            connection,
                            transaction))
                        {
                            command.Parameters.AddWithValue("@price", plantPrice);
                            command.Parameters.AddWithValue("@userId", userId);
                            command.ExecuteNonQuery();
                        }

                        // Add plant to user's collection
                        using (var command = new SqlCommand(
                            "INSERT INTO User_Plants (purchase_date, plant_id, user_id) VALUES (@purchaseDate, @plantId, @userId)",
                            connection,
                            transaction))
                        {
                            command.Parameters.AddWithValue("@purchaseDate", DateTime.Now);
                            command.Parameters.AddWithValue("@plantId", plantId);
                            command.Parameters.AddWithValue("@userId", userId);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return Json(new { success = true, message = "Plant added to your collection successfully!" });
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding plant to user's collection: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while adding the plant to your collection." });
            }
        }

        [Authorize]
        public IActionResult MyPlants()
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