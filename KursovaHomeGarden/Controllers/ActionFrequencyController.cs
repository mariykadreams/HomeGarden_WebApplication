using Microsoft.AspNetCore.Mvc;
// Controllers/ActionFrequencyController.cs
using Microsoft.AspNetCore.Mvc;
using KursovaHomeGarden.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

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
                using (SqlCommand command = new SqlCommand("SELECT * FROM ActionFrequencies", connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            actionFrequencies.Add(new ActionFrequency
                            {
                                Action_frequency_id = reader.GetInt32("Action_frequency_id"),
                                volume = reader.GetDecimal("volume"),
                                plant_id = reader.GetInt32("plant_id"),
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
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ActionFrequency actionFrequency)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = @"INSERT INTO ActionFrequencies (volume, plant_id, season_id, action_type_id, Fert_type_id) 
                               VALUES (@volume, @plant_id, @season_id, @action_type_id, @Fert_type_id)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
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
            return View(actionFrequency);
        }

       
        public IActionResult Edit(int id)
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
    }

}
