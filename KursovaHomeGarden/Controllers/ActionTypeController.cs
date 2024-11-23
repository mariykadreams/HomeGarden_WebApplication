// Path: Controllers/ActionTypeController.cs
using Microsoft.AspNetCore.Mvc;
using KursovaHomeGarden.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace KursovaHomeGarden.Controllers
{
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
    }
}
