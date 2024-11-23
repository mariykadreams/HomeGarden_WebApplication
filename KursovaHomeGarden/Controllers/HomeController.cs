using KursovaHomeGarden.Models.CareLevel;
using KursovaHomeGarden.Models.Category;
using KursovaHomeGarden.Models;
using KursovaHomeGarden.Models.Plant;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using KursovaHomeGarden.Data;


namespace KursovaHomeGarden.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _connectionString;
        private readonly HomeGardenDbContext _context;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, HomeGardenDbContext context)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
            _context = context;
        }

        public IActionResult Index()
        {
            List<Plant> plants = new List<Plant>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT 
                p.plant_id, p.name, p.description, p.price, p.img, 
                p.category_id, c.category_name 
            FROM Plants p
            LEFT JOIN Categories c ON p.category_id = c.category_id";

                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var plant = new Plant
                        {
                            plant_id = reader.GetInt32(reader.GetOrdinal("plant_id")),
                            name = reader.GetString(reader.GetOrdinal("name")),
                            description = reader["description"] as string,
                            price = reader.GetDecimal(reader.GetOrdinal("price")),
                            img = reader["img"] as string,
                            Category = new Category
                            {
                                category_name = reader["category_name"] as string
                            }
                        };

                        plants.Add(plant);
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error fetching plants: {0}", ex.Message);
                }
            }

            return View(plants);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            Plant plant = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
            SELECT 
                p.plant_id, p.name, p.description, p.price, p.img, 
                c.category_name, 
                cl.level_name AS care_level_name,
                af.Interval, af.volume, af.notes,
                s.season_name, at.type_name AS action_type_name
            FROM Plants p
            LEFT JOIN Categories c ON p.category_id = c.category_id
            LEFT JOIN CareLevels cl ON p.care_level_id = cl.care_level_id
            LEFT JOIN ActionFrequencies af ON p.plant_id = af.plant_id
            LEFT JOIN Seasons s ON af.season_id = s.season_id
            LEFT JOIN ActionTypes at ON af.action_type_id = at.action_type_id
            WHERE p.plant_id = @id";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        if (plant == null)
                        {
                            plant = new Plant
                            {
                                plant_id = reader.GetInt32(reader.GetOrdinal("plant_id")),
                                name = reader.GetString(reader.GetOrdinal("name")),
                                description = reader["description"] as string,
                                price = reader.GetDecimal(reader.GetOrdinal("price")),
                                img = reader["img"] as string,
                                Category = new Category
                                {
                                    category_name = reader["category_name"] as string
                                },
                                CareLevel = new CareLevel
                                {
                                    level_name = reader["care_level_name"] as string
                                },
                                ActionFrequencies = new List<ActionFrequency>()
                            };
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("Interval")))
                        {
                            plant.ActionFrequencies.Add(new ActionFrequency
                            {
                                Interval = reader["Interval"].ToString(),
                                volume = reader.GetDecimal(reader.GetOrdinal("volume")),
                                notes = reader["notes"] as string,
                                Season = new Season
                                {
                                    season_name = reader["season_name"].ToString()
                                },
                                ActionType = new ActionType
                                {
                                    type_name = reader["action_type_name"].ToString()
                                }
                            });
                        }
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error fetching plant details: {0}", ex.Message);
                }
            }

            if (plant == null)
            {
                return NotFound();
            }

            return View(plant);
        }






        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
