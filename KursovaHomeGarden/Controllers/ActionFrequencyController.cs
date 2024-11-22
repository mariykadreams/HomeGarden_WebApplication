﻿using Microsoft.AspNetCore.Mvc;
// Controllers/ActionFrequencyController.cs
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

        public ActionFrequencyController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
        }


        public IActionResult Index()
        {
            var actionFrequencies = new List<ActionFrequency>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"SELECT af.*, p.name as plant_name 
                        FROM ActionFrequencies af 
                        LEFT JOIN Plants p ON af.plant_id = p.plant_id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            actionFrequencies.Add(new ActionFrequency
                            {
                                Action_frequency_id = reader.GetInt32("Action_frequency_id"),
                                Interval = reader.GetString("Interval"),
                                volume = reader.GetDecimal("volume"),
                                plant_id = reader.GetInt32("plant_id"),
                                Plant = new Plant { name = reader.GetString("plant_name") },
                                season_id = reader.GetInt32("season_id"),
                                action_type_id = reader.GetInt32("action_type_id"),
                                Fert_type_id = reader.IsDBNull("Fert_type_id") ? null : reader.GetInt32("Fert_type_id")
                            });
                        }
                    }
                }
            }
            return View(actionFrequencies);
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

            System.Diagnostics.Debug.WriteLine($"Create method started");
            System.Diagnostics.Debug.WriteLine($"Received action frequency: PlantId={actionFrequency.plant_id}, SeasonId={actionFrequency.season_id}, ActionTypeId={actionFrequency.action_type_id}, Volume={actionFrequency.volume}");
            System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                foreach (var modelStateEntry in ModelState.Values)
                {
                    foreach (var error in modelStateEntry.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Validation error: {error.ErrorMessage}");
                    }
                }
            }
            // Add validation checks similar to PlantController
            if (string.IsNullOrWhiteSpace(actionFrequency.Interval))
            {
                ModelState.AddModelError("Interval", "Interval is required.");
            }

            if (actionFrequency.volume <= 0)
            {
                ModelState.AddModelError("volume", "Volume must be greater than 0.");
            }

            if (!ModelState.IsValid)
            {
                // Repopulate ViewBag data before returning to view
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
                           (Interval, volume, plant_id, season_id, action_type_id, Fert_type_id) 
                           VALUES 
                           (@Interval, @volume, @plant_id, @season_id, @action_type_id, @Fert_type_id)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Interval", actionFrequency.Interval);
                        command.Parameters.AddWithValue("@volume", actionFrequency.volume);
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
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error creating action frequency: {ex.Message}");
                ViewBag.Message = $"Error: {ex.Message}";
                // Repopulate ViewBag data
                ViewBag.Plants = GetPlants();
                ViewBag.Seasons = GetSeasons();
                ViewBag.ActionTypes = GetActionTypes();
                return View(actionFrequency);
            }
        }



        public IActionResult Edit(int id)
        {
            ViewBag.Plants = GetPlants();

            ActionFrequency actionFrequency = null;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SELECT * FROM ActionFrequencies WHERE Action_frequency_id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            actionFrequency = new ActionFrequency
                            {
                                Action_frequency_id = reader.GetInt32("Action_frequency_id"),
                                volume = reader.GetDecimal("volume"),
                                plant_id = reader.GetInt32("plant_id"),
                                season_id = reader.GetInt32("season_id"),
                                action_type_id = reader.GetInt32("action_type_id"),
                                Fert_type_id = reader.IsDBNull("Fert_type_id") ? null : reader.GetInt32("Fert_type_id")
                            };
                        }
                    }
                }
            }
            if (actionFrequency == null)
            {
                return NotFound();
            }
            return View(actionFrequency);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ActionFrequency actionFrequency)
        {
            if (id != actionFrequency.Action_frequency_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = @"UPDATE ActionFrequencies 
                               SET volume = @volume, 
                                   plant_id = @plant_id, 
                                   season_id = @season_id, 
                                   action_type_id = @action_type_id, 
                                   Fert_type_id = @Fert_type_id 
                               WHERE Action_frequency_id = @id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@volume", actionFrequency.volume);
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

            ViewBag.Plants = GetPlants();
            ViewBag.Seasons = GetSeasons();
            ViewBag.ActionTypes = GetActionTypes();
            return View(actionFrequency);
        }

        
        public IActionResult Delete(int id)
        {
            ActionFrequency actionFrequency = null;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SELECT * FROM ActionFrequencies WHERE Action_frequency_id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            actionFrequency = new ActionFrequency
                            {
                                Action_frequency_id = reader.GetInt32("Action_frequency_id"),
                                volume = reader.GetDecimal("volume"),
                                plant_id = reader.GetInt32("plant_id"),
                                season_id = reader.GetInt32("season_id"),
                                action_type_id = reader.GetInt32("action_type_id"),
                                Fert_type_id = reader.IsDBNull("Fert_type_id") ? null : reader.GetInt32("Fert_type_id")
                            };
                        }
                    }
                }
            }

            if (actionFrequency == null)
            {
                return NotFound();
            }
            return View(actionFrequency);
        }

      
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("DELETE FROM ActionFrequencies WHERE Action_frequency_id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            return RedirectToAction(nameof(Index));
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
                                Text = $"{reader["name"]} (ID: {reader["plant_id"]})"
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
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        seasons.Add(new SelectListItem
                        {
                            Value = reader["season_id"].ToString(),
                            Text = $"{reader["season_name"]}"
                        });
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
                    SqlDataReader reader = command.ExecuteReader();
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
            return actionTypes;
        }

    }

}
