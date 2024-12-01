using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using KursovaHomeGarden.Models.Category;
using KursovaHomeGarden.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;


[Authorize(Roles = SD.Role_Admin)]
public class CategoryController : Controller
{
    private readonly string _connectionString;

    public CategoryController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
    }

    [HttpGet]
    public IActionResult Index()
    {
        var categories = new List<Category>();
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string query = "SELECT * FROM Categories";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new Category
                            {
                                category_id = Convert.ToInt32(reader["category_id"]),
                                category_name = reader["category_name"].ToString()
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

        return View(categories);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }

        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string query = "INSERT INTO Categories (category_name) VALUES (@categoryName)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@categoryName", category.category_name);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"Error: {ex.Message}";
            return View(category);
        }
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        Category category = null;

        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string query = "SELECT * FROM Categories WHERE category_id = @categoryId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@categoryId", id);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            category = new Category
                            {
                                category_id = Convert.ToInt32(reader["category_id"]),
                                category_name = reader["category_name"].ToString()
                            };
                        }
                    }
                }
            }

            if (category == null)
            {
                return RedirectToAction("Index");
            }
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"Error: {ex.Message}";
            return RedirectToAction("Index");
        }

        return View(category);
    }

    [HttpPost]
    public IActionResult Edit(int id, Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }

        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string query = "UPDATE Categories SET category_name = @categoryName WHERE category_id = @categoryId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@categoryName", category.category_name);
                    command.Parameters.AddWithValue("@categoryId", id);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"Error: {ex.Message}";
            return View(category);
        }
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string checkQuery = "SELECT COUNT(*) FROM Plants WHERE category_id = @categoryId";
                using (SqlCommand command = new SqlCommand(checkQuery, connection))
                {
                    command.Parameters.AddWithValue("@categoryId", id);
                    connection.Open();
                    int count = (int)command.ExecuteScalar();

                    if (count > 0)
                    {
                        return Json(new { success = false, message = "This category cannot be deleted because it is linked to other records." });
                    }
                }
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                const string deleteQuery = "DELETE FROM Categories WHERE category_id = @categoryId";
                using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@categoryId", id);
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
