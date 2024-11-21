using Microsoft.AspNetCore.Mvc;
using KursovaHomeGarden.Models.Plant;
using KursovaHomeGarden.Models.Category;
using KursovaHomeGarden.Models.CareLevel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;

namespace KursovaHomeGarden.Controllers
{
    public class PlantsController : Controller
    {
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _environment;

        public PlantsController(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<Plant> plants = new List<Plant>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = @"
                        SELECT p.plant_id, p.name, p.description, p.price, 
                               p.category_id, c.category_name, 
                               p.care_level_id, cl.care_name, 
                               p.img 
                        FROM Plants p
                        JOIN Categories c ON p.category_id = c.category_id
                        JOIN Care_level cl ON p.care_level_id = cl.care_level_id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            plants.Add(new Plant
                            {
                                plant_id = (int)reader["plant_id"],
                                name = reader["name"].ToString(),
                                description = reader["description"].ToString(),
                                price = (decimal)reader["price"],
                                category_id = (int)reader["category_id"],
                                Category = new Category
                                {
                                    category_id = (int)reader["category_id"],
                                    category_name = reader["category_name"].ToString()
                                },
                                care_level_id = (int)reader["care_level_id"],
                                CareLevel = new CareLevel
                                {
                                    care_level_id = (int)reader["care_level_id"],
                                    level_name = reader["care_name"].ToString()
                                },
                                img = reader["img"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
            }

            return View(plants);
        }

        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                ViewBag.Categories = GetCategories();
                ViewBag.CareLevels = GetCareLevels();
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        public IActionResult Create(Plant plant)
        {
            if (string.IsNullOrWhiteSpace(plant.name))
            {
                ModelState.AddModelError("name", "Name is required.");
            }

            if (plant.price <= 0)
            {
                ModelState.AddModelError("price", "Price must be greater than 0.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = GetCategories();
                ViewBag.CareLevels = GetCareLevels();
                return View(plant);
            }

            try
            {
                string fileName = string.Empty;

                if (HttpContext.Request.Form.Files["Imagefile"] is IFormFile imageFile)
                {
                    var filePath = Path.Combine(_environment.WebRootPath, "images/plants");
                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var fullPath = Path.Combine(filePath, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    plant.img = fileName;
                }

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "INSERT INTO Plants (name, description, price, category_id, care_level_id, img) " +
                                   "VALUES (@name, @description, @price, @category_id, @care_level_id, @img)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", plant.name);
                        command.Parameters.AddWithValue("@description", plant.description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@price", plant.price);
                        command.Parameters.AddWithValue("@category_id", plant.category_id);
                        command.Parameters.AddWithValue("@care_level_id", plant.care_level_id);
                        command.Parameters.AddWithValue("@img", plant.img ?? (object)DBNull.Value);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
                ViewBag.Categories = GetCategories();
                ViewBag.CareLevels = GetCareLevels();
                return View(plant);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                Plant plant = null;
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM Plants WHERE plant_id = @plant_id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@plant_id", id);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            plant = new Plant
                            {
                                plant_id = (int)reader["plant_id"],
                                name = reader["name"].ToString(),
                                description = reader["description"].ToString(),
                                price = (decimal)reader["price"],
                                category_id = (int)reader["category_id"],
                                care_level_id = (int)reader["care_level_id"],
                                img = reader["img"].ToString()
                            };
                        }
                    }
                }

                if (plant == null)
                {
                    return RedirectToAction("Index");
                }

                ViewBag.Categories = GetCategories();
                ViewBag.CareLevels = GetCareLevels();
                return View(plant);
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult Edit(Plant plant)
        {
            if (string.IsNullOrWhiteSpace(plant.name))
            {
                ModelState.AddModelError("name", "Name is required.");
            }

            if (plant.price <= 0)
            {
                ModelState.AddModelError("price", "Price must be greater than 0.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = GetCategories();
                ViewBag.CareLevels = GetCareLevels();
                return View(plant);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "UPDATE Plants SET name = @name, description = @description, price = @price, " +
                                   "category_id = @category_id, care_level_id = @care_level_id, img = @img " +
                                   "WHERE plant_id = @plant_id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", plant.name);
                        command.Parameters.AddWithValue("@description", plant.description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@price", plant.price);
                        command.Parameters.AddWithValue("@category_id", plant.category_id);
                        command.Parameters.AddWithValue("@care_level_id", plant.care_level_id);
                        command.Parameters.AddWithValue("@img", plant.img ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@plant_id", plant.plant_id);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
                ViewBag.Categories = GetCategories();
                ViewBag.CareLevels = GetCareLevels();
                return View(plant);
            }
        }

        private List<SelectListItem> GetCategories()
        {
            List<SelectListItem> categories = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT category_id, category_name FROM Categories";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        categories.Add(new SelectListItem
                        {
                            Value = reader["category_id"].ToString(),
                            Text = reader["category_name"].ToString()
                        });
                    }
                }
            }
            return categories;
        }

        private List<SelectListItem> GetCareLevels()
        {
            List<SelectListItem> careLevels = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT care_level_id, care_name FROM Care_level";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        careLevels.Add(new SelectListItem
                        {
                            Value = reader["care_level_id"].ToString(),
                            Text = reader["care_name"].ToString()
                        });
                    }
                }
            }
            return careLevels;
        }
    }
}
