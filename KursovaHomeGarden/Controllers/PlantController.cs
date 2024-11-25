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
            _connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
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
                               p.care_level_id, cl.level_name,
                               p.img 
                        FROM Plants p
                        JOIN Categories c ON p.category_id = c.category_id
                        JOIN CareLevels cl ON p.care_level_id = cl.care_level_id";

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
                                    level_name = reader["level_name"].ToString()
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
            System.Diagnostics.Debug.WriteLine($"Create method started");
            System.Diagnostics.Debug.WriteLine($"Received plant: Name={plant.name}, Price={plant.price}, CategoryId={plant.category_id}, CareLevelId={plant.care_level_id}");
            System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"Validation error: {error.ErrorMessage}");
                }
            }

            if (string.IsNullOrWhiteSpace(plant.name))
            {
                ModelState.AddModelError("name", "Name is required.");
            }

            if (plant.price <= 0)
            {
                ModelState.AddModelError("price", "Price must be greater than 0.");
            }

            string fileName = string.Empty;
            if (HttpContext.Request.Form.Files["img"] is IFormFile imageFile)
            {
                System.Diagnostics.Debug.WriteLine($"Processing image file: {imageFile.FileName}");
                var filePath = Path.Combine(_environment.WebRootPath, "images/plants");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var fullPath = Path.Combine(filePath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                plant.img = fileName;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No image file received");
                plant.img = null;
            }

            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("ModelState is invalid, returning to view");
                ViewBag.Categories = GetCategories();
                ViewBag.CareLevels = GetCareLevels();
                return View(plant);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    System.Diagnostics.Debug.WriteLine($"Connection string: {_connectionString}");
                    string query = "INSERT INTO Plants (name, description, price, category_id, care_level_id, img) " +
                                   "VALUES (@name, @description, @price, @category_id, @care_level_id, @img)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        System.Diagnostics.Debug.WriteLine("Setting up SQL parameters");
                        command.Parameters.AddWithValue("@name", plant.name);
                        command.Parameters.AddWithValue("@description", plant.description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@price", plant.price);
                        command.Parameters.AddWithValue("@category_id", plant.category_id);
                        command.Parameters.AddWithValue("@care_level_id", plant.care_level_id);
                        command.Parameters.AddWithValue("@img", plant.img ?? (object)DBNull.Value);

                        connection.Open();
                        System.Diagnostics.Debug.WriteLine("Executing SQL command");
                        var rowsAffected = command.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Rows affected: {rowsAffected}");
                    }
                }

                System.Diagnostics.Debug.WriteLine("Successfully inserted plant, redirecting to Index");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inserting plant: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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
                                description = reader["description"] == DBNull.Value ? null : reader["description"].ToString(),
                                price = (decimal)reader["price"],
                                category_id = (int)reader["category_id"],
                                care_level_id = (int)reader["care_level_id"],
                                img = reader["img"] == DBNull.Value ? null : reader["img"].ToString()
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
            // Add logging
            System.Diagnostics.Debug.WriteLine($"Edit method started");
            System.Diagnostics.Debug.WriteLine($"Received plant: ID={plant.plant_id}, Name={plant.name}, Price={plant.price}");

            if (string.IsNullOrWhiteSpace(plant.name))
            {
                ModelState.AddModelError("name", "Name is required.");
            }

            if (plant.price <= 0)
            {
                ModelState.AddModelError("price", "Price must be greater than 0.");
            }

         

            string currentImg = null;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT img FROM Plants WHERE plant_id = @plant_id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@plant_id", plant.plant_id);
                    connection.Open();
                    var result = command.ExecuteScalar();
                    currentImg = result == DBNull.Value ? null : (string)result;
                }
            }


            // Handle image upload
            if (HttpContext.Request.Form.Files["img"] is IFormFile imageFile)
            {
                var filePath = Path.Combine(_environment.WebRootPath, "images/plants");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var fullPath = Path.Combine(filePath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                // Delete old image if exists
                if (!string.IsNullOrEmpty(currentImg))
                {
                    var oldImagePath = Path.Combine(_environment.WebRootPath, "images/plants", currentImg);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                plant.img = fileName;
            }
            else
            {
                // Если новая картинка не загружена, сохраняем текущую
                plant.img = currentImg;
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
                        command.Parameters.AddWithValue("@plant_id", plant.plant_id);
                        command.Parameters.AddWithValue("@name", plant.name);
                        command.Parameters.AddWithValue("@description", plant.description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@price", plant.price);
                        command.Parameters.AddWithValue("@category_id", plant.category_id);
                        command.Parameters.AddWithValue("@care_level_id", plant.care_level_id);
                        command.Parameters.AddWithValue("@img", plant.img ?? (object)DBNull.Value);

                        connection.Open();
                        var rowsAffected = command.ExecuteNonQuery();
                        System.Diagnostics.Debug.WriteLine($"Rows affected: {rowsAffected}");
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating plant: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
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
                string query = "SELECT care_level_id, level_name FROM CareLevels";  // Ensure correct column names
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        careLevels.Add(new SelectListItem
                        {
                            Value = reader["care_level_id"].ToString(),
                            Text = reader["level_name"].ToString()  // Make sure 'level_name' is correct
                        });
                    }
                }
            }
            return careLevels;
        }

        [HttpPost]
        public JsonResult CheckAndDelete(int id)
        {
            try
            {
                bool isReferenced = false;

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string checkQuery = "SELECT COUNT(*) FROM ActionFrequencies WHERE plant_id = @plant_id";
                    using (SqlCommand command = new SqlCommand(checkQuery, connection))
                    {
                        command.Parameters.AddWithValue("@plant_id", id);
                        connection.Open();
                        isReferenced = (int)command.ExecuteScalar() > 0;
                    }
                }

                if (isReferenced)
                {
                    return Json(new { referenced = true, message = "The plant is referenced in ActionFrequencies. Are you sure you want to delete it?" });
                }

                DeletePlantAndReferences(id);
                return Json(new { success = true, message = "Plant deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }


        [HttpPost]
        public JsonResult DeletePlantAndReferences(int id)
        {
            try
            {
                string imageFileName = null;

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string getImageQuery = "SELECT img FROM Plants WHERE plant_id = @plant_id";
                    using (SqlCommand command = new SqlCommand(getImageQuery, connection))
                    {
                        command.Parameters.AddWithValue("@plant_id", id);
                        connection.Open();
                        var result = command.ExecuteScalar();
                        imageFileName = result == DBNull.Value ? null : (string)result;
                    }
                }

                if (!string.IsNullOrEmpty(imageFileName))
                {
                    var imageFilePath = Path.Combine(_environment.WebRootPath, "images/plants", imageFileName);
                    if (System.IO.File.Exists(imageFilePath))
                    {
                        System.IO.File.Delete(imageFilePath);
                    }
                }

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string deleteActionFrequencies = "DELETE FROM ActionFrequencies WHERE plant_id = @plant_id";
                    string deletePlant = "DELETE FROM Plants WHERE plant_id = @plant_id";

                    connection.Open();

                    using (SqlCommand command = new SqlCommand(deleteActionFrequencies, connection))
                    {
                        command.Parameters.AddWithValue("@plant_id", id);
                        command.ExecuteNonQuery();
                    }

                    using (SqlCommand command = new SqlCommand(deletePlant, connection))
                    {
                        command.Parameters.AddWithValue("@plant_id", id);
                        command.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true, message = "Plant, related references, and image deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }


    }
}
