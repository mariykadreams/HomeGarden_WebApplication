using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Text;
using KursovaHomeGarden.Models;
using KursovaHomeGarden.Models.Plant;

namespace KursovaHomeGarden.Services
{
    public class ActionFrequencyService : IActionFrequencyService
    {
        private readonly string _connectionString;
        private readonly ILogger<ActionFrequencyService> _logger;

        public ActionFrequencyService(IConfiguration configuration, ILogger<ActionFrequencyService> logger)
        {
            _connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
            _logger = logger;
        }

        public async Task<List<ActionFrequency>> GetActionFrequenciesAsync(string searchTerm = null, int? plantId = null,
            int? seasonId = null, int? actionTypeId = null, int? fertTypeId = null)
        {
            var actionFrequencies = new List<ActionFrequency>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var queryBuilder = new StringBuilder(@"
                    SELECT af.*, p.name as plant_name, s.season_name, 
                           at.type_name as action_type_name, f.type_name as fertilizer_name
                    FROM ActionFrequencies af
                    LEFT JOIN Plants p ON af.plant_id = p.plant_id
                    LEFT JOIN Seasons s ON af.season_id = s.season_id
                    LEFT JOIN ActionTypes at ON af.action_type_id = at.action_type_id
                    LEFT JOIN Fertilizes f ON af.Fert_type_id = f.Fert_type_id
                    WHERE 1=1");

                var parameters = new List<SqlParameter>();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    queryBuilder.Append(@" AND (p.name LIKE @searchTerm 
                                          OR af.Interval LIKE @searchTerm 
                                          OR at.type_name LIKE @searchTerm 
                                          OR s.season_name LIKE @searchTerm)");
                    parameters.Add(new SqlParameter("@searchTerm", $"%{searchTerm}%"));
                }

                if (plantId.HasValue)
                {
                    queryBuilder.Append(" AND af.plant_id = @plantId");
                    parameters.Add(new SqlParameter("@plantId", plantId.Value));
                }

                if (seasonId.HasValue)
                {
                    queryBuilder.Append(" AND af.season_id = @seasonId");
                    parameters.Add(new SqlParameter("@seasonId", seasonId.Value));
                }

                if (actionTypeId.HasValue)
                {
                    queryBuilder.Append(" AND af.action_type_id = @actionTypeId");
                    parameters.Add(new SqlParameter("@actionTypeId", actionTypeId.Value));
                }

                if (fertTypeId.HasValue)
                {
                    queryBuilder.Append(" AND af.Fert_type_id = @fertTypeId");
                    parameters.Add(new SqlParameter("@fertTypeId", fertTypeId.Value));
                }

                using var command = new SqlCommand(queryBuilder.ToString(), connection);
                command.Parameters.AddRange(parameters.ToArray());
                await connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    actionFrequencies.Add(new ActionFrequency
                    {
                        Action_frequency_id = reader.GetInt32(reader.GetOrdinal("Action_frequency_id")),
                        Interval = reader.GetString(reader.GetOrdinal("Interval")),
                        volume = reader.IsDBNull(reader.GetOrdinal("volume")) ? null : reader.GetDecimal(reader.GetOrdinal("volume")),
                        notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes")),
                        plant_id = reader.GetInt32(reader.GetOrdinal("plant_id")),
                        season_id = reader.GetInt32(reader.GetOrdinal("season_id")),
                        action_type_id = reader.GetInt32(reader.GetOrdinal("action_type_id")),
                        Fert_type_id = reader.IsDBNull(reader.GetOrdinal("Fert_type_id")) ? null : reader.GetInt32(reader.GetOrdinal("Fert_type_id")),
                        Plant = new Plant { name = reader.GetString(reader.GetOrdinal("plant_name")) },
                        Season = new Season { season_name = reader.GetString(reader.GetOrdinal("season_name")) },
                        ActionType = new ActionType { type_name = reader.GetString(reader.GetOrdinal("action_type_name")) },
                        Fertilize = reader.IsDBNull(reader.GetOrdinal("fertilizer_name")) ? null :
                                  new Fertilize { type_name = reader.GetString(reader.GetOrdinal("fertilizer_name")) }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving action frequencies: {ex.Message}");
                throw;
            }

            return actionFrequencies;
        }

        public async Task<(List<SelectListItem> Plants, List<SelectListItem> Seasons,
            List<SelectListItem> ActionTypes, List<SelectListItem> Fertilizers)> LoadFilterOptionsAsync()
        {
            try
            {
                var plants = await LoadSelectListItemsAsync("SELECT plant_id, name FROM Plants", "plant_id", "name");
                var seasons = await LoadSelectListItemsAsync("SELECT season_id, season_name FROM Seasons", "season_id", "season_name");
                var actionTypes = await LoadSelectListItemsAsync("SELECT action_type_id, type_name FROM ActionTypes", "action_type_id", "type_name");
                var fertilizers = await LoadSelectListItemsAsync("SELECT Fert_type_id, type_name FROM Fertilizes", "Fert_type_id", "type_name");

                return (plants, seasons, actionTypes, fertilizers);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading filter options: {ex.Message}");
                throw;
            }
        }

        private async Task<List<SelectListItem>> LoadSelectListItemsAsync(string query, string valueField, string textField)
        {
            var items = new List<SelectListItem>();
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);
            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new SelectListItem
                {
                    Value = reader[valueField].ToString(),
                    Text = reader[textField].ToString()
                });
            }

            return items;
        }
    }
}