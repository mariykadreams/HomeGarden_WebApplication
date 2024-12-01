using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using KursovaHomeGarden.Models;
using KursovaHomeGarden.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = SD.Role_Admin)]
public class FertilizeController : Controller
{
    private readonly string _connectionString;
    private readonly ILogger<FertilizeController> _logger;

    public FertilizeController(IConfiguration configuration, ILogger<FertilizeController> logger)
    {
        _connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var fertilizers = new List<Fertilize>();
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string query = "SELECT * FROM Fertilizes";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fertilizers.Add(new Fertilize
                            {
                                Fert_type_id = Convert.ToInt32(reader["Fert_type_id"]),
                                type_name = reader["type_name"].ToString(),
                                units = reader["units"].ToString(),
                                note = reader["note"]?.ToString()
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fertilizers");
            ViewBag.Message = $"Error: {ex.Message}";
        }

        return View(fertilizers);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Fertilize());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("type_name,units,note")] Fertilize fertilize)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    const string insertQuery = @"
                        INSERT INTO Fertilizes (type_name, units, note) 
                        VALUES (@typeName, @units, @note);
                        SELECT SCOPE_IDENTITY();";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@typeName", fertilize.type_name);
                        command.Parameters.AddWithValue("@units", fertilize.units);
                        command.Parameters.AddWithValue("@note", (object)fertilize.note ?? DBNull.Value);

                        var newId = Convert.ToInt32(command.ExecuteScalar());
                        fertilize.Fert_type_id = newId;
                    }

                    transaction.Commit();
                    _logger.LogInformation($"Created new fertilizer with ID: {fertilize.Fert_type_id}");

                    TempData["SuccessMessage"] = "Fertilizer created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fertilizer");
            ViewBag.Message = $"Error: {ex.Message}";
            return View(fertilize);
        }
    }


    [HttpGet]
    public IActionResult Edit(int id)
    {
        Fertilize fertilize = null;

        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string query = "SELECT * FROM Fertilizes WHERE Fert_type_id = @fertTypeId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@fertTypeId", id);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            fertilize = new Fertilize
                            {
                                Fert_type_id = Convert.ToInt32(reader["Fert_type_id"]),
                                type_name = reader["type_name"].ToString(),
                                units = reader["units"].ToString(),
                                note = reader["note"]?.ToString()
                            };
                        }
                    }
                }
            }

            if (fertilize == null)
            {
                _logger.LogWarning($"Fertilizer with ID {id} not found");
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving fertilizer with ID {id}");
            ViewBag.Message = $"Error: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }

        return View(fertilize);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("Fert_type_id,type_name,units,note")] Fertilize fertilize)
    {
        if (id != fertilize.Fert_type_id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(fertilize);
        }

        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    const string query = "UPDATE Fertilizes SET type_name = @typeName, units = @units, note = @note WHERE Fert_type_id = @fertTypeId";
                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@typeName", fertilize.type_name);
                        command.Parameters.AddWithValue("@units", fertilize.units);
                        command.Parameters.AddWithValue("@note", (object)fertilize.note ?? DBNull.Value);
                        command.Parameters.AddWithValue("@fertTypeId", id);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            throw new Exception("No records were updated. The fertilizer may have been deleted by another user.");
                        }
                    }

                    transaction.Commit();
                    _logger.LogInformation($"Updated fertilizer with ID: {id}");

                    TempData["SuccessMessage"] = "Fertilizer updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Error while updating fertilizer", ex);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating fertilizer with ID {id}");
            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, contact your system administrator.");
            ViewBag.Message = $"Error: {ex.Message}";
            return View(fertilize);
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
                    const string checkQuery = "SELECT COUNT(*) FROM ActionFrequencies WHERE Fert_type_id = @fertTypeId";
                    using (SqlCommand command = new SqlCommand(checkQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@fertTypeId", id);
                        int count = (int)command.ExecuteScalar();

                        if (count > 0)
                        {
                            return Json(new { success = false, message = "This fertilizer cannot be deleted because it is linked to action frequencies." });
                        }
                    }

                    const string deleteQuery = "DELETE FROM Fertilizes WHERE Fert_type_id = @fertTypeId";
                    using (SqlCommand command = new SqlCommand(deleteQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@fertTypeId", id);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            throw new Exception("No records were deleted. The fertilizer may have been deleted by another user.");
                        }
                    }

                    transaction.Commit();
                    _logger.LogInformation($"Deleted fertilizer with ID: {id}");

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Error while deleting fertilizer", ex);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting fertilizer with ID {id}");
            return Json(new { success = false, message = $"Error: {ex.Message}" });
        }
    }
}