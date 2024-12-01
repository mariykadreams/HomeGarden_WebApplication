using KursovaHomeGarden.Models.Plant;
using KursovaHomeGarden.Models.Category;
using KursovaHomeGarden.Models.CareLevel;
using Microsoft.Data.SqlClient;
using System.Text;

namespace KursovaHomeGarden.Services
{
    public class PlantService : IPlantService
    {
        private readonly string _connectionString;
        private readonly ILogger<PlantService> _logger;

        private static class PlantQueries
        {
            public const string GetCategories = "SELECT category_id, category_name FROM Categories";
            public const string GetCareLevels = "SELECT care_level_id, level_name FROM CareLevels";
            public const string GetPlantPrice = "SELECT price FROM Plants WHERE plant_id = @plantId";
            public const string GetUserBalance = "SELECT AmountOfMoney FROM AspNetUsers WHERE Id = @userId";
            public const string UpdateUserBalance = "UPDATE AspNetUsers SET AmountOfMoney = AmountOfMoney - @price WHERE Id = @userId";
            public const string AddPlantToUser = "INSERT INTO User_Plants (purchase_date, plant_id, user_id) VALUES (@purchaseDate, @plantId, @userId)";
            public const string GetFilteredPlants = @"
                SELECT p.plant_id, p.name, p.description, p.price, p.img,
                       c.category_id, c.category_name,
                       cl.care_level_id, cl.level_name
                FROM Plants p
                LEFT JOIN Categories c ON p.category_id = c.category_id
                LEFT JOIN CareLevels cl ON p.care_level_id = cl.care_level_id
                WHERE 1=1";
        }

        public PlantService(IConfiguration configuration, ILogger<PlantService> logger)
        {
            _connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
            _logger = logger;
        }

        public async Task<(List<Plant> Plants, List<Category> Categories, List<CareLevel> CareLevels)> GetFilteredPlantsAsync(
            string searchTerm, decimal? minPrice, decimal? maxPrice, int? categoryId, int? careLevelId, string sortBy)
        {
            var plants = new List<Plant>();
            var categories = new List<Category>();
            var careLevels = new List<CareLevel>();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Get categories
            using (var command = new SqlCommand(PlantQueries.GetCategories, connection))
            {
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    categories.Add(new Category
                    {
                        category_id = reader.GetInt32(0),
                        category_name = reader.GetString(1)
                    });
                }
            }

            // Get care levels
            using (var command = new SqlCommand(PlantQueries.GetCareLevels, connection))
            {
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    careLevels.Add(new CareLevel
                    {
                        care_level_id = reader.GetInt32(0),
                        level_name = reader.GetString(1)
                    });
                }
            }

            // Build main query
            var queryBuilder = new StringBuilder(PlantQueries.GetFilteredPlants);
            var parameters = new List<SqlParameter>();

            // Modified to search only by plant name
            if (!string.IsNullOrEmpty(searchTerm))
            {
                queryBuilder.Append(" AND p.name LIKE @searchTerm");
                parameters.Add(new SqlParameter("@searchTerm", $"%{searchTerm}%"));
            }

            if (minPrice.HasValue)
            {
                queryBuilder.Append(" AND p.price >= @minPrice");
                parameters.Add(new SqlParameter("@minPrice", minPrice.Value));
            }

            if (maxPrice.HasValue)
            {
                queryBuilder.Append(" AND p.price <= @maxPrice");
                parameters.Add(new SqlParameter("@maxPrice", maxPrice.Value));
            }

            if (categoryId.HasValue)
            {
                queryBuilder.Append(" AND p.category_id = @categoryId");
                parameters.Add(new SqlParameter("@categoryId", categoryId.Value));
            }

            if (careLevelId.HasValue)
            {
                queryBuilder.Append(" AND p.care_level_id = @careLevelId");
                parameters.Add(new SqlParameter("@careLevelId", careLevelId.Value));
            }

            queryBuilder.Append(GetSortClause(sortBy));

            using (var command = new SqlCommand(queryBuilder.ToString(), connection))
            {
                command.Parameters.AddRange(parameters.ToArray());
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    plants.Add(new Plant
                    {
                        plant_id = reader.GetInt32(reader.GetOrdinal("plant_id")),
                        name = reader.GetString(reader.GetOrdinal("name")),
                        description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
                        price = reader.GetDecimal(reader.GetOrdinal("price")),
                        img = reader.IsDBNull(reader.GetOrdinal("img")) ? null : reader.GetString(reader.GetOrdinal("img")),
                        Category = new Category
                        {
                            category_id = reader.GetInt32(reader.GetOrdinal("category_id")),
                            category_name = reader.GetString(reader.GetOrdinal("category_name"))
                        },
                        CareLevel = new CareLevel
                        {
                            care_level_id = reader.GetInt32(reader.GetOrdinal("care_level_id")),
                            level_name = reader.GetString(reader.GetOrdinal("level_name"))
                        }
                    });
                }
            }

            return (plants, categories, careLevels);
        }

        public async Task<bool> AddPlantToUserCollectionAsync(int plantId, string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                decimal plantPrice = 0;
                decimal userBalance = 0;

                // Get plant price
                using (var command = new SqlCommand(PlantQueries.GetPlantPrice, connection, transaction))
                {
                    command.Parameters.AddWithValue("@plantId", plantId);
                    var result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        plantPrice = (decimal)result;
                    }
                }

                // Get user balance
                using (var command = new SqlCommand(PlantQueries.GetUserBalance, connection, transaction))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    var result = await command.ExecuteScalarAsync();
                    if (result != DBNull.Value)
                    {
                        userBalance = (decimal)result;
                    }
                }

                if (userBalance < plantPrice)
                {
                    return false;
                }

                // Update user balance
                using (var command = new SqlCommand(PlantQueries.UpdateUserBalance, connection, transaction))
                {
                    command.Parameters.AddWithValue("@price", plantPrice);
                    command.Parameters.AddWithValue("@userId", userId);
                    await command.ExecuteNonQueryAsync();
                }

                // Add plant to user's collection
                using (var command = new SqlCommand(PlantQueries.AddPlantToUser, connection, transaction))
                {
                    command.Parameters.AddWithValue("@purchaseDate", DateTime.Now);
                    command.Parameters.AddWithValue("@plantId", plantId);
                    command.Parameters.AddWithValue("@userId", userId);
                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Error adding plant to user's collection: {ex.Message}");
                return false;
            }
        }

        private string GetSortClause(string sortBy) => sortBy switch
        {
            "price_asc" => " ORDER BY p.price ASC",
            "price_desc" => " ORDER BY p.price DESC",
            "name_desc" => " ORDER BY p.name DESC",
            _ => " ORDER BY p.name ASC"
        };
    }
}