using KursovaHomeGarden.Models.CareLevel;
using KursovaHomeGarden.Models.Category;
using KursovaHomeGarden.Models;
using KursovaHomeGarden.Models.Plant;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
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
            List<Plant> plants = new List<Plant>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT p.plant_id, 
                               p.name, 
                               p.description, 
                               p.price, 
                               p.img,
                               c.category_id,
                               c.category_name,
                               cl.care_level_id,
                               cl.level_name
                        FROM Plants p
                        JOIN Categories c ON p.category_id = c.category_id
                        JOIN CareLevels cl ON p.care_level_id = cl.care_level_id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                plants.Add(new Plant
                                {
                                    plant_id = reader.GetInt32(reader.GetOrdinal("plant_id")),
                                    name = reader.GetString(reader.GetOrdinal("name")),
                                    description = reader.IsDBNull(reader.GetOrdinal("description"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("description")),
                                    price = reader.GetDecimal(reader.GetOrdinal("price")),
                                    img = reader.IsDBNull(reader.GetOrdinal("img"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("img")),


                                    Category = new KursovaHomeGarden.Models.Category.Category
                                    {
                                        category_id = reader.GetInt32(reader.GetOrdinal("category_id")),
                                        category_name = reader.GetString(reader.GetOrdinal("category_name"))
                                    },



                                    CareLevel = new KursovaHomeGarden.Models.CareLevel.CareLevel
                                    {
                                        care_level_id = reader.GetInt32(reader.GetOrdinal("care_level_id")),
                                        level_name = reader.GetString(reader.GetOrdinal("level_name"))
                                    }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving plants: {ex.Message}");
                // You might want to handle the error appropriately
            }

            return View(plants);
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
