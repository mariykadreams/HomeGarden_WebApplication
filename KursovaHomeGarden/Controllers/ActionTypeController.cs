﻿
using Microsoft.AspNetCore.Mvc;
using KursovaHomeGarden.Models;
using Microsoft.Data.SqlClient;
using System.Data;
using KursovaHomeGarden.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;


namespace KursovaHomeGarden.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class ActionTypeController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<ActionTypeController> _logger;

        public ActionTypeController(IConfiguration configuration, ILogger<ActionTypeController> logger)
        {
            _connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
            _logger = logger;
        }

        public IActionResult Index()
        {
            var actionTypes = new List<ActionType>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM ActionTypes";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                actionTypes.Add(new ActionType
                                {
                                    action_type_id = reader.GetInt32(reader.GetOrdinal("action_type_id")),
                                    type_name = reader.GetString(reader.GetOrdinal("type_name")),
                                 
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving action types: {ex.Message}");
                ViewBag.Message = $"Error: {ex.Message}";
            }
            return View(actionTypes);
        }

        // Path: Controllers/ActionTypeController.cs
        // Add these methods to the existing ActionTypeController class

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ActionType actionType)
        {
            if (!ModelState.IsValid)
            {
                return View(actionType);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "INSERT INTO ActionTypes (type_name) VALUES (@type_name)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@type_name", actionType.type_name);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating action type: {ex.Message}");
                ViewBag.Message = $"Error: {ex.Message}";
                return View(actionType);
            }
        }

        public IActionResult Edit(int id)
        {
            ActionType actionType = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "SELECT * FROM ActionTypes WHERE action_type_id = @id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                actionType = new ActionType
                                {
                                    action_type_id = reader.GetInt32(reader.GetOrdinal("action_type_id")),
                                    type_name = reader.GetString(reader.GetOrdinal("type_name"))

                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving action type: {ex.Message}");
                ViewBag.Message = $"Error: {ex.Message}";
            }

            if (actionType == null)
            {
                return NotFound();
            }

            return View(actionType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ActionType actionType)
        {
            if (id != actionType.action_type_id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(actionType);
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    string query = "UPDATE ActionTypes SET type_name = @type_name WHERE action_type_id = @id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@type_name", actionType.type_name);
                        command.Parameters.AddWithValue("@id", id);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating action type: {ex.Message}");
                ViewBag.Message = $"Error: {ex.Message}";
                return View(actionType);
            }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string checkQuery = "SELECT COUNT(*) FROM ActionFrequencies WHERE action_type_id = @actionTypeId";
                    using (SqlCommand command = new SqlCommand(checkQuery, connection))
                    {
                        command.Parameters.AddWithValue("@actionTypeId", id);
                        connection.Open();
                        int count = (int)command.ExecuteScalar();

                        if (count > 0)
                        {
                            return Json(new { success = false, message = "This action type cannot be deleted because it is linked to other records." });
                        }
                    }
                }

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    const string deleteQuery = "DELETE FROM ActionTypes WHERE action_type_id = @actionTypeId";
                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@actionTypeId", id);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                return Json(new { success = true });
            }
            catch (SqlException ex)
            {
                return Json(new { success = false, message = $"SQL Error: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }


    }
}
