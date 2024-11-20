using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using KursovaHomeGarden.Models.CareLevel;

namespace KursovaHomeGarden.Controllers
{
    public class CareLevelController : Controller
    {
        private readonly string _connectionString;

        public CareLevelController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
        }

        // GET: CareLevel/Index
        public IActionResult Index()
        {
            var careLevels = new List<CareLevel>();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string query = "SELECT care_level_id, level_name FROM CareLevels";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                careLevels.Add(new CareLevel
                                {
                                    care_level_id = Convert.ToInt32(reader["care_level_id"]),
                                    level_name = reader["level_name"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error loading care levels: {ex.Message}";
            }

            return View(careLevels);
        }

        // GET: CareLevel/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CareLevel/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CareLevel careLevel)
        {
            if (!ModelState.IsValid)
            {
                return View(careLevel);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string query = "INSERT INTO CareLevels (level_name) VALUES (@levelName)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@levelName", careLevel.level_name);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error creating care level: {ex.Message}";
                return View(careLevel);
            }
        }

        // GET: CareLevel/Edit/{id}
        public IActionResult Edit(int id)
        {
            CareLevel careLevel = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string query = "SELECT care_level_id, level_name FROM CareLevels WHERE care_level_id = @careLevelId";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@careLevelId", id);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                careLevel = new CareLevel
                                {
                                    care_level_id = Convert.ToInt32(reader["care_level_id"]),
                                    level_name = reader["level_name"].ToString()
                                };
                            }
                        }
                    }
                }

                if (careLevel == null)
                {
                    ViewBag.Message = "Care level not found.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error loading care level: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }

            return View(careLevel);
        }

        // POST: CareLevel/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CareLevel careLevel)
        {
            if (!ModelState.IsValid)
            {
                return View(careLevel);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string query = "UPDATE CareLevels SET level_name = @levelName WHERE care_level_id = @careLevelId";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@levelName", careLevel.level_name);
                        command.Parameters.AddWithValue("@careLevelId", id);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error updating care level: {ex.Message}";
                return View(careLevel);
            }
        }

        // POST: CareLevel/Delete/{id}
        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string query = "DELETE FROM CareLevels WHERE care_level_id = @careLevelId";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@careLevelId", id);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true });
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                return Json(new { success = false, message = "This care level is linked to other records and cannot be deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting care level: {ex.Message}" });
            }
        }
    }
}
