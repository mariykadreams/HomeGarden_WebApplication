using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Dynamic;

namespace KursovaHomeGarden.Controllers.Users
{
	public class AdminController : Controller
	{
		private readonly string _connectionString;

		public AdminController(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
		}

		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public IActionResult GetActionFrequencies()
		{
			var actionFrequencies = new List<dynamic>();

			using (var connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				using (var command = new SqlCommand(GetSqlQuery(), connection))
				{
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							dynamic item = new ExpandoObject();
							item.ActionTypeId = reader.GetInt32(0);
							item.ActionType = reader.GetString(1);
							item.ActionFrequencyCount = reader.GetInt32(2);
							actionFrequencies.Add(item);
						}
					}
				}
			}

			return PartialView("_ActionFrequenciesTable", actionFrequencies);
		}

		private string GetSqlQuery()
		{
			return @"
                SELECT 
                    af.action_type_id,
                    at.type_name AS action_type,
                    COUNT(af.action_type_id) AS action_frequency_count
                FROM 
                    [KursovaHomeGardenDB].[dbo].[ActionFrequencies] af
                JOIN
                    [KursovaHomeGardenDB].[dbo].[ActionTypes] at
                    ON af.action_type_id = at.action_type_id
                GROUP BY 
                    af.action_type_id, at.type_name
                ORDER BY 
                    action_frequency_count DESC;
            ";
		}
	}
}