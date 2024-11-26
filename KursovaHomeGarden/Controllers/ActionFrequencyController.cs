using Microsoft.AspNetCore.Mvc;
using KursovaHomeGarden.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
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
                    string query = @"SELECT af.Action_frequency_id, 
                                   af.Interval, 
                                   af.volume, 
                                   af.notes,
                                   af.plant_id, 
                                   af.season_id, 
                                   af.action_type_id, 
                                   af.Fert_type_id,
                                   p.name as plant_name,
                                   s.season_name,
                                   at.type_name
                            FROM ActionFrequencies af 
                            LEFT JOIN Plants p ON af.plant_id = p.plant_id
                            LEFT JOIN Seasons s ON af.season_id = s.season_id
                            LEFT JOIN ActionTypes at ON af.action_type_id = at.action_type_id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.CommandTimeout = 30;
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var actionFrequency = new ActionFrequency
                                {
                                    Action_frequency_id = reader.GetInt32(reader.GetOrdinal("Action_frequency_id")),
                                    Interval = reader.GetString(reader.GetOrdinal("Interval")),
                                    volume = reader.GetDecimal(reader.GetOrdinal("volume")),
                                    notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
                                    plant_id = reader.GetInt32(reader.GetOrdinal("plant_id")),
                                    Plant = new Plant
                                    {
                                        name = reader.GetString(reader.GetOrdinal("plant_name"))
                                    },
                                    season_id = reader.GetInt32(reader.GetOrdinal("season_id")),
                                    Season = new Season
                                    {
                                        season_name = reader.GetString(reader.GetOrdinal("season_name"))
                                    },
                                    action_type_id = reader.GetInt32(reader.GetOrdinal("action_type_id")),
                                    ActionType = new ActionType
                                    {
                                        type_name = reader.GetString(reader.GetOrdinal("type_name"))
                                    },
                                    Fert_type_id = reader.IsDBNull(reader.GetOrdinal("Fert_type_id")) ?
                                        null : reader.GetInt32(reader.GetOrdinal("Fert_type_id"))
                                };
                                actionFrequencies.Add(actionFrequency);
                            }
                        }
                    }
                }
                return View(actionFrequencies);
            }
            catch (SqlException ex)
            {
                _logger.LogError($"Database error: {ex.Message}");
                TempData["Error"] = "Unable to retrieve action frequencies. Please try again later.";
                return View(new List<ActionFrequency>());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                TempData["Error"] = "An unexpected error occurred. Please try again later.";
                return View(new List<ActionFrequency>());
            }
        }

        public IActionResult Create()
        {
            ViewBag.Plants = GetPlants();
            ViewBag.Seasons = GetSeasons();
            ViewBag.ActionTypes = GetActionTypes();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ActionFrequency actionFrequency)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Plants = GetPlants();
                ViewBag.Seasons = GetSeasons();
                ViewBag.ActionTypes = GetActionTypes();
                return View(actionFrequency);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = @"INSERT INTO ActionFrequencies 
                           (Interval, volume, notes, plant_id, season_id, action_type_id, Fert_type_id) 
                           VALUES 
                           (@Interval, @volume, @notes, @plant_id, @season_id, @action_type_id, @Fert_type_id)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Interval", actionFrequency.Interval);
                        command.Parameters.AddWithValue("@volume", actionFrequency.volume);
                        command.Parameters.AddWithValue("@notes", (object)actionFrequency.notes ?? DBNull.Value);
                        command.Parameters.AddWithValue("@plant_id", actionFrequency.plant_id);
                        command.Parameters.AddWithValue("@season_id", actionFrequency.season_id);
                        command.Parameters.AddWithValue("@action_type_id", actionFrequency.action_type_id);
                        command.Parameters.AddWithValue("@Fert_type_id",
                            (object)actionFrequency.Fert_type_id ?? DBNull.Value);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating action frequency: {ex.Message}");
                ModelState.AddModelError("", "Error creating action frequency. Please try again.");
                ViewBag.Plants = GetPlants();
                ViewBag.Seasons = GetSeasons();
                ViewBag.ActionTypes = GetActionTypes();
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
                    string query = @"
                    SELECT af.*, p.name as plant_name, s.season_name, at.type_name 
                    FROM ActionFrequencies af
                    JOIN Plants p ON af.plant_id = p.plant_id
                    JOIN Seasons s ON af.season_id = s.season_id
                    JOIN ActionTypes at ON af.action_type_id = at.action_type_id
                    WHERE Action_frequency_id = @id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            actionFrequency = new ActionFrequency
                            {
                                Action_frequency_id = reader.GetInt32(reader.GetOrdinal("Action_frequency_id")),
                                Interval = reader.GetString(reader.GetOrdinal("Interval")),
                                volume = reader.GetDecimal(reader.GetOrdinal("volume")),
                                notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
                                plant_id = reader.GetInt32(reader.GetOrdinal("plant_id")),
                                season_id = reader.GetInt32(reader.GetOrdinal("season_id")),
                                action_type_id = reader.GetInt32(reader.GetOrdinal("action_type_id")),
                                Fert_type_id = reader.IsDBNull(reader.GetOrdinal("Fert_type_id")) ? null : reader.GetInt32(reader.GetOrdinal("Fert_type_id"))
                            };
                        }
                    }
                }

                if (actionFrequency == null)
                {
                    return NotFound();
                }

                ViewBag.Plants = GetPlants();
                ViewBag.Seasons = GetSeasons();
                ViewBag.ActionTypes = GetActionTypes();
                return View(actionFrequency);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving action frequency: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult Edit(ActionFrequency actionFrequency)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Plants = GetPlants();
                ViewBag.Seasons = GetSeasons();
                ViewBag.ActionTypes = GetActionTypes();
                return View(actionFrequency);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = @"UPDATE ActionFrequencies 
                            SET Interval = @Interval, 
                                volume = @volume, 
                                notes = @notes, 
                                plant_id = @plant_id, 
                                season_id = @season_id, 
                                action_type_id = @action_type_id, 
                                Fert_type_id = @Fert_type_id 
                            WHERE Action_frequency_id = @Action_frequency_id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Action_frequency_id", actionFrequency.Action_frequency_id);
                        command.Parameters.AddWithValue("@Interval", actionFrequency.Interval);
                        command.Parameters.AddWithValue("@volume", actionFrequency.volume);
                        command.Parameters.AddWithValue("@notes", (object)actionFrequency.notes ?? DBNull.Value);
                        command.Parameters.AddWithValue("@plant_id", actionFrequency.plant_id);
                        command.Parameters.AddWithValue("@season_id", actionFrequency.season_id);
                        command.Parameters.AddWithValue("@action_type_id", actionFrequency.action_type_id);
                        command.Parameters.AddWithValue("@Fert_type_id",
                            actionFrequency.Fert_type_id.HasValue ? (object)actionFrequency.Fert_type_id : DBNull.Value);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating action frequency: {ex.Message}");
                ViewBag.Message = $"Error: {ex.Message}";
                ViewBag.Plants = GetPlants();
                ViewBag.Seasons = GetSeasons();
                ViewBag.ActionTypes = GetActionTypes();
                return View(actionFrequency);
            }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string deleteQuery = "DELETE FROM ActionFrequencies WHERE Action_frequency_id = @actionFrequencyId";
                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@actionFrequencyId", id);
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Json(new { success = true });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Record not found or could not be deleted." });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError($"SQL Error while deleting action frequency: {ex.Message}");
                return Json(new { success = false, message = "Database error occurred while deleting the record." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while deleting action frequency: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while deleting the record." });
            }
        }



        private List<SelectListItem> GetPlants()
        {
            var plants = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT plant_id, name FROM Plants";
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
            return plants;
        }

        private List<SelectListItem> GetSeasons()
        {
            List<SelectListItem> seasons = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT season_id, season_name FROM Seasons";
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
            return seasons;
        }

        private List<SelectListItem> GetActionTypes()
        {
            List<SelectListItem> actionTypes = new List<SelectListItem>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT action_type_id, type_name FROM ActionTypes";
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
            return actionTypes;
        }
    }
}
