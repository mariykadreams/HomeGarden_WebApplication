using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using KursovaHomeGarden.Models;
using KursovaHomeGarden.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;

namespace KursovaHomeGarden.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class SunlightRequirementController : Controller
    {

        private readonly string _connectionString;

        public SunlightRequirementController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
        }

        [HttpGet]
        public IActionResult Index()
        {
            var requirements = new List<SunlightRequirement>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string query = "SELECT * FROM SunlightRequirements";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                requirements.Add(new SunlightRequirement
                                {
                                    sunlight_requirements_id = Convert.ToInt32(reader["sunlight_requirements_id"]),
                                    light_intensity = reader["light_intensity"].ToString(),
                                    hours_per_day = Convert.ToInt32(reader["hours_per_day"]),
                                    notes = reader["notes"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
            }

            return View(requirements);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(SunlightRequirement requirement)
        {
            if (!ModelState.IsValid)
            {
                return View(requirement);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string query = @"INSERT INTO SunlightRequirements 
                        (light_intensity, hours_per_day, notes) 
                        VALUES (@lightIntensity, @hoursPerDay, @notes)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@lightIntensity", requirement.light_intensity);
                        command.Parameters.AddWithValue("@hoursPerDay", requirement.hours_per_day);
                        command.Parameters.AddWithValue("@notes", (object)requirement.notes ?? DBNull.Value);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
                return View(requirement);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            SunlightRequirement requirement = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string query = "SELECT * FROM SunlightRequirements WHERE sunlight_requirements_id = @id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                requirement = new SunlightRequirement
                                {
                                    sunlight_requirements_id = Convert.ToInt32(reader["sunlight_requirements_id"]),
                                    light_intensity = reader["light_intensity"].ToString(),
                                    hours_per_day = Convert.ToInt32(reader["hours_per_day"]),
                                    notes = reader["notes"].ToString()
                                };
                            }
                        }
                    }
                }

                if (requirement == null)
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
                return RedirectToAction("Index");
            }

            return View(requirement);
        }

        [HttpPost]
        public IActionResult Edit(int id, SunlightRequirement requirement)
        {
            if (!ModelState.IsValid)
            {
                return View(requirement);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string query = @"UPDATE SunlightRequirements 
                        SET light_intensity = @lightIntensity, 
                            hours_per_day = @hoursPerDay, 
                            notes = @notes 
                        WHERE sunlight_requirements_id = @id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@lightIntensity", requirement.light_intensity);
                        command.Parameters.AddWithValue("@hoursPerDay", requirement.hours_per_day);
                        command.Parameters.AddWithValue("@notes", (object)requirement.notes ?? DBNull.Value);
                        command.Parameters.AddWithValue("@id", id);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
                return View(requirement);
            }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string checkQuery = "SELECT COUNT(*) FROM Plants WHERE sunlight_requirements_id = @id";
                    using (SqlCommand command = new SqlCommand(checkQuery, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        connection.Open();
                        int count = (int)command.ExecuteScalar();

                        if (count > 0)
                        {
                            return Json(new { success = false, message = "This sunlight requirement cannot be deleted because it is linked to plants." });
                        }
                    }
                }

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string deleteQuery = "DELETE FROM SunlightRequirements WHERE sunlight_requirements_id = @id";
                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult CreateAjax([FromForm] SunlightRequirement requirement)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            try
            {
                int newId;
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string query = @"
                INSERT INTO SunlightRequirements (light_intensity, hours_per_day, notes)
                OUTPUT INSERTED.sunlight_requirements_id
                VALUES (@lightIntensity, @hoursPerDay, @notes)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@lightIntensity", requirement.light_intensity);
                        command.Parameters.AddWithValue("@hoursPerDay", requirement.hours_per_day);
                        command.Parameters.AddWithValue("@notes", (object)requirement.notes ?? DBNull.Value);

                        connection.Open();
                        newId = (int)command.ExecuteScalar();
                    }
                }

                return Json(new
                {
                    success = true,
                    value = newId.ToString(),
                    text = $"{requirement.light_intensity} ({requirement.hours_per_day} hours/day)"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}