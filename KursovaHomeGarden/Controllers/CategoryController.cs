using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using KursovaHomeGarden.Models;
using KursovaHomeGarden.Models.Category;

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
        DataTable categoryTable = new DataTable();
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Categories";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(categoryTable);
                }
            }
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"Error: {ex.Message}";
        }

        return View(categoryTable);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(CategoryDto categoryDto)
    {
        if (string.IsNullOrWhiteSpace(categoryDto.category_name))
        {
            ModelState.AddModelError("category_name", "Category name is required.");
        }

        if (!ModelState.IsValid)
        {
            return View(categoryDto);
        }

        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO Categories (category_name) VALUES (@categoryName)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@categoryName", categoryDto.category_name);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"Error: {ex.Message}";
            return View(categoryDto);
        }
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        DataTable categoryTable = new DataTable();
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Categories WHERE category_id = @categoryId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@categoryId", id);
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(categoryTable);
                }
            }

            if (categoryTable.Rows.Count == 0)
            {
                return RedirectToAction("Index");
            }

            var categoryDto = new CategoryDto
            {
                category_name = categoryTable.Rows[0]["category_name"].ToString()
            };

            ViewData["category_id"] = id;
            return View(categoryDto);
        }
        catch (Exception ex)
        {
            ViewBag.Message = $"Error: {ex.Message}";
            return View();
        }
    }

    [HttpPost]
    public IActionResult Edit(int id, CategoryDto categoryDto)
    {
        if (string.IsNullOrWhiteSpace(categoryDto.category_name))
        {
            ModelState.AddModelError("category_name", "Category name is required.");
        }

        if (!ModelState.IsValid)
        {
            return View(categoryDto);
        }

        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Categories SET category_name = @categoryName WHERE category_id = @categoryId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@categoryName", categoryDto.category_name);
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
            return View(categoryDto);
        }
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Categories WHERE category_id = @categoryId";
                using (SqlCommand command = new SqlCommand(query, connection))
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
            return Json(new { success = false, message = "This category cannot be deleted because it is linked to other records." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error: {ex.Message}" });
        }
    }
}