using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using KursovaHomeGarden.Models;

public class FertilizeController : Controller
{
    private readonly string _connectionString;

    public FertilizeController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
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
            ViewBag.Message = $"Error: {ex.Message}";
        }

        return View(fertilizers);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Fertilize fertilize)
    {
        if (!ModelState.IsValid)
        {
            return View(fertilize);
        }

        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string query = "INSERT INTO Fertilizes (type_name, units, note) VALUES (@typeName, @units, @note)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@typeName", fertilize.type_name);
                    command.Parameters.AddWithValue("@units", fertilize.units);
                    command.Parameters.AddWithValue("@note", (object)fertilize.note ?? DBNull.Value);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
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
                return RedirectToAction("Index");
            }
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"Error: {ex.Message}";
            return RedirectToAction("Index");
        }

        return View(fertilize);
    }

    [HttpPost]
    public IActionResult Edit(int id, Fertilize fertilize)
    {
        if (!ModelState.IsValid)
        {
            return View(fertilize);
        }

        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string query = "UPDATE Fertilizes SET type_name = @typeName, units = @units, note = @note WHERE Fert_type_id = @fertTypeId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@typeName", fertilize.type_name);
                    command.Parameters.AddWithValue("@units", fertilize.units);
                    command.Parameters.AddWithValue("@note", (object)fertilize.note ?? DBNull.Value);
                    command.Parameters.AddWithValue("@fertTypeId", id);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"Error: {ex.Message}";
            return View(fertilize);
        }
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string checkQuery = "SELECT COUNT(*) FROM ActionFrequencies WHERE Fert_type_id = @fertTypeId";
                using (SqlCommand command = new SqlCommand(checkQuery, connection))
                {
                    command.Parameters.AddWithValue("@fertTypeId", id);
                    connection.Open();
                    int count = (int)command.ExecuteScalar();

                    if (count > 0)
                    {
                        return Json(new { success = false, message = "This fertilizer cannot be deleted because it is linked to action frequencies." });
                    }
                }
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string deleteQuery = "DELETE FROM Fertilizes WHERE Fert_type_id = @fertTypeId";
                using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@fertTypeId", id);
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
}