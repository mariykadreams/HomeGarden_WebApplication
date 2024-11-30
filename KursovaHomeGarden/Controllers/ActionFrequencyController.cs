using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using KursovaHomeGarden.Models;
using KursovaHomeGarden.Models.Plant;

namespace KursovaHomeGarden.Controllers
{
    public class ActionFrequencyController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<ActionFrequencyController> _logger;

        public ActionFrequencyController(IConfiguration configuration, ILogger<ActionFrequencyController> logger)
        {
            _connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
            _logger = logger;
        }

        public IActionResult Index()
        {
            var actionFrequencies = new List<ActionFrequency>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string query = @"
                        SELECT af.*, p.name as plant_name, s.season_name, at.type_name as action_type_name, f.type_name as fertilizer_name
                        FROM ActionFrequencies af
                        LEFT JOIN Plants p ON af.plant_id = p.plant_id
                        LEFT JOIN Seasons s ON af.season_id = s.season_id
                        LEFT JOIN ActionTypes at ON af.action_type_id = at.action_type_id
                        LEFT JOIN Fertilizes f ON af.Fert_type_id = f.Fert_type_id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                actionFrequencies.Add(new ActionFrequency
                                {
                                    Action_frequency_id = reader.GetInt32(reader.GetOrdinal("Action_frequency_id")),
                                    Interval = reader.GetString(reader.GetOrdinal("Interval")),
                                    volume = reader.IsDBNull(reader.GetOrdinal("volume")) ? null : reader.GetDecimal(reader.GetOrdinal("volume")),
                                    notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
                                    plant_id = reader.GetInt32(reader.GetOrdinal("plant_id")),
                                    season_id = reader.GetInt32(reader.GetOrdinal("season_id")),
                                    action_type_id = reader.GetInt32(reader.GetOrdinal("action_type_id")),
                                    Fert_type_id = reader.IsDBNull(reader.GetOrdinal("Fert_type_id")) ? null : reader.GetInt32(reader.GetOrdinal("Fert_type_id")),
                                    Plant = new Plant
                                    {
                                        name = reader.GetString(reader.GetOrdinal("plant_name"))
                                    },
                                    Season = new Season { season_name = reader.GetString(reader.GetOrdinal("season_name")) },
                                    ActionType = new ActionType { type_name = reader.GetString(reader.GetOrdinal("action_type_name")) },
                                    Fertilize = reader.IsDBNull(reader.GetOrdinal("fertilizer_name")) ? null :
                                              new Fertilize { type_name = reader.GetString(reader.GetOrdinal("fertilizer_name")) }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving action frequencies: {ex.Message}");
                TempData["ErrorMessage"] = "Error retrieving action frequencies.";
            }

            return View(actionFrequencies);
        }

        private void LoadViewBagData()
        {
            try
            {
                // Load Plants
                var plants = new List<SelectListItem>();
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string query = "SELECT plant_id, name FROM Plants";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                plants.Add(new SelectListItem
                                {
                                    Value = reader["plant_id"].ToString(),
                                    Text = reader["name"].ToString()
                                });
                            }
                        }
                    }
                }
                ViewBag.Plants = new SelectList(plants, "Value", "Text");

                // Load Seasons
                var seasons = new List<SelectListItem>();
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string query = "SELECT season_id, season_name FROM Seasons";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                seasons.Add(new SelectListItem
                                {
                                    Value = reader["season_id"].ToString(),
                                    Text = reader["season_name"].ToString()
                                });
                            }
                        }
                    }
                }
                ViewBag.Seasons = new SelectList(seasons, "Value", "Text");

                // Load Action Types
                var actionTypes = new List<SelectListItem>();
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string query = "SELECT action_type_id, type_name FROM ActionTypes";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                actionTypes.Add(new SelectListItem
                                {
                                    Value = reader["action_type_id"].ToString(),
                                    Text = reader["type_name"].ToString()
                                });
                            }
                        }
                    }
                }
                ViewBag.ActionTypes = new SelectList(actionTypes, "Value", "Text");

                // Load Fertilizers
                var fertilizers = new List<SelectListItem>();
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string query = "SELECT Fert_type_id, type_name FROM Fertilizes";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                fertilizers.Add(new SelectListItem
                                {
                                    Value = reader["Fert_type_id"].ToString(),
                                    Text = reader["type_name"].ToString()
                                });
                            }
                        }
                    }
                }
                fertilizers.Insert(0, new SelectListItem { Value = "", Text = "-- Select Fertilizer (Optional) --" });
                ViewBag.Fertilizes = new SelectList(fertilizers, "Value", "Text");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading ViewBag data: {ex.Message}");
                throw;
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                LoadViewBagData();
                return View(new ActionFrequency());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Create GET: {ex.Message}");
                TempData["ErrorMessage"] = "Error loading form data.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ActionFrequency actionFrequency)
        {
            if (!ModelState.IsValid)
            {
                LoadViewBagData();
                return View(actionFrequency);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using var transaction = connection.BeginTransaction();

                    try
                    {
                        const string query = @"
                            INSERT INTO ActionFrequencies 
                            (Interval, volume, notes, plant_id, season_id, action_type_id, Fert_type_id) 
                            VALUES 
                            (@interval, @volume, @notes, @plantId, @seasonId, @actionTypeId, @fertTypeId);
                            SELECT SCOPE_IDENTITY();";

                        using (SqlCommand command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@interval", actionFrequency.Interval);
                            command.Parameters.AddWithValue("@volume", actionFrequency.volume ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@notes", actionFrequency.notes ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@plantId", actionFrequency.plant_id);
                            command.Parameters.AddWithValue("@seasonId", actionFrequency.season_id);
                            command.Parameters.AddWithValue("@actionTypeId", actionFrequency.action_type_id);
                            command.Parameters.AddWithValue("@fertTypeId", actionFrequency.Fert_type_id ?? (object)DBNull.Value);

                            var newId = Convert.ToInt32(command.ExecuteScalar());
                            actionFrequency.Action_frequency_id = newId;
                        }

                        transaction.Commit();
                        TempData["SuccessMessage"] = "Action frequency created successfully!";
                        return RedirectToAction(nameof(Index));
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
                _logger.LogError($"Error creating action frequency: {ex.Message}");
                ModelState.AddModelError("", "Error creating action frequency. Please try again.");
                LoadViewBagData();
                return View(actionFrequency);
            }
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            try
            {
                ActionFrequency actionFrequency = null;
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string query = "SELECT * FROM ActionFrequencies WHERE Action_frequency_id = @id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                actionFrequency = new ActionFrequency
                                {
                                    Action_frequency_id = reader.GetInt32(reader.GetOrdinal("Action_frequency_id")),
                                    Interval = reader.GetString(reader.GetOrdinal("Interval")),
                                    volume = reader.IsDBNull(reader.GetOrdinal("volume")) ? null : reader.GetDecimal(reader.GetOrdinal("volume")),
                                    notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
                                    plant_id = reader.GetInt32(reader.GetOrdinal("plant_id")),
                                    season_id = reader.GetInt32(reader.GetOrdinal("season_id")),
                                    action_type_id = reader.GetInt32(reader.GetOrdinal("action_type_id")),
                                    Fert_type_id = reader.IsDBNull(reader.GetOrdinal("Fert_type_id")) ? null : reader.GetInt32(reader.GetOrdinal("Fert_type_id"))
                                };
                            }
                        }
                    }
                }

                if (actionFrequency == null)
                {
                    return NotFound();
                }

                LoadViewBagData();
                return View(actionFrequency);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving action frequency: {ex.Message}");
                TempData["ErrorMessage"] = "Error retrieving action frequency.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ActionFrequency actionFrequency)
        {
            if (id != actionFrequency.Action_frequency_id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                LoadViewBagData();
                return View(actionFrequency);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using var transaction = connection.BeginTransaction();

                    try
                    {
                        const string query = @"
                            UPDATE ActionFrequencies 
                            SET Interval = @interval,
                                volume = @volume,
                                notes = @notes,
                                plant_id = @plantId,
                                season_id = @seasonId,
                                action_type_id = @actionTypeId,
                                Fert_type_id = @fertTypeId
                            WHERE Action_frequency_id = @id";

                        using (SqlCommand command = new SqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@interval", actionFrequency.Interval);
                            command.Parameters.AddWithValue("@volume", actionFrequency.volume ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@notes", actionFrequency.notes ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@plantId", actionFrequency.plant_id);
                            command.Parameters.AddWithValue("@seasonId", actionFrequency.season_id);
                            command.Parameters.AddWithValue("@actionTypeId", actionFrequency.action_type_id);
                            command.Parameters.AddWithValue("@fertTypeId", actionFrequency.Fert_type_id ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@id", id);

                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                throw new Exception("No records were updated. The action frequency may have been deleted.");
                            }
                        }

                        transaction.Commit();
                        TempData["SuccessMessage"] = "Action frequency updated successfully!";
                        return RedirectToAction(nameof(Index));
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
                _logger.LogError($"Error updating action frequency: {ex.Message}");
                ModelState.AddModelError("", "Error updating action frequency. Please try again.");
                LoadViewBagData();
                return View(actionFrequency);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using var transaction = connection.BeginTransaction();

                    try
                    {
                        const string deleteQuery = "DELETE FROM ActionFrequencies WHERE Action_frequency_id = @id";
                        using (SqlCommand command = new SqlCommand(deleteQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id", id);
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                throw new Exception("No records were deleted. The action frequency may have been deleted already.");
                            }
                        }

                        transaction.Commit();
                        _logger.LogInformation($"Deleted action frequency with ID: {id}");
                        return Json(new { success = true });
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
                _logger.LogError($"Error deleting action frequency: {ex.Message}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}