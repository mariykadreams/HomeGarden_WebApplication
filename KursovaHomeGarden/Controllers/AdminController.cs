﻿using KursovaHomeGarden.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace KursovaHomeGarden.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class AdminController : Controller
    {
        private readonly string _connectionString;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _connectionString = configuration.GetConnectionString("HomeGardenDbContextConnection");
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var plants = new List<dynamic>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT plant_id, name FROM Plants";
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dynamic plant = new ExpandoObject();
                            plant.plant_id = reader.GetInt32(reader.GetOrdinal("plant_id"));
                            plant.name = reader.GetString(reader.GetOrdinal("name"));
                            plants.Add(plant);
                        }
                    }
                }
            }

            ViewBag.Plants = plants;
            return View();
        }

        [HttpPost]
        public IActionResult GetActionStatistics()
        {
            var actionStatistics = new List<dynamic>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
            SELECT 
                at.type_name AS action_type,
                p.name AS plant_name,
                af.[Interval] AS action_interval,
                COUNT(pch.care_id) AS action_count
            FROM 
                Plant_Care_History pch
            JOIN 
                ActionTypes at ON pch.action_type_id = at.action_type_id
            JOIN 
                User_Plants up ON pch.user_plant_id = up.user_plant_id
            JOIN 
                Plants p ON up.plant_id = p.plant_id
            JOIN 
                ActionFrequencies af ON p.plant_id = af.plant_id AND at.action_type_id = af.action_type_id
            GROUP BY 
                at.type_name, p.name, af.[Interval]
            ORDER BY 
                action_count DESC;";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dynamic stat = new ExpandoObject();
                            stat.ActionType = reader.GetString(reader.GetOrdinal("action_type"));
                            stat.PlantName = reader.GetString(reader.GetOrdinal("plant_name"));
                            stat.ActionInterval = int.Parse(reader.GetString(reader.GetOrdinal("action_interval")));
                            stat.ActionCount = reader.GetInt32(reader.GetOrdinal("action_count"));
                            actionStatistics.Add(stat);
                        }
                    }
                }
            }

            return PartialView("_ActionStatisticsTable", actionStatistics);
        }

        [HttpPost]
        public IActionResult GetActionFrequenciesSummary()
        {
            var actionFrequencies = new List<dynamic>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT 
                        at.action_type_id,
                        at.type_name AS action_type,
                        COUNT(af.action_frequency_id) AS action_frequency_count
                    FROM 
                        ActionTypes at
                    LEFT JOIN
                        ActionFrequencies af ON at.action_type_id = af.action_type_id
                    GROUP BY 
                        at.action_type_id, at.type_name
                    ORDER BY 
                        action_frequency_count DESC;";

                using (var command = new SqlCommand(query, connection))
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

            return PartialView("_ActionFrequenciesSummaryTable", actionFrequencies);
        }

        [HttpPost]
        public IActionResult GetActionFrequencies(DateTime startDate, DateTime endDate, int plantId)
        {
            var actionFrequencies = new List<dynamic>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    WITH DateRange AS (
                        SELECT 
                            DATEDIFF(day, @StartDate, @EndDate) + 1 AS total_days
                    )
                    SELECT 
                        at.type_name as action_type,
                        CAST(af.Interval AS VARCHAR(50)) as interval_str,
                        af.notes,
                        CAST(CEILING(
                            CAST(DateRange.total_days AS FLOAT) / 
                            CAST(af.Interval AS FLOAT)
                        ) AS INT) as required_actions
                    FROM 
                        ActionFrequencies af
                    JOIN 
                        ActionTypes at ON af.action_type_id = at.action_type_id
                    CROSS JOIN 
                        DateRange
                    WHERE 
                        af.plant_id = @PlantId
                    ORDER BY 
                        at.type_name;";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);
                    command.Parameters.AddWithValue("@PlantId", plantId);

                    try
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dynamic item = new ExpandoObject();
                                item.ActionType = reader.GetString(reader.GetOrdinal("action_type"));
                                item.Interval = reader.GetString(reader.GetOrdinal("interval_str"));
                                item.Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString(reader.GetOrdinal("notes"));
                                item.RequiredActions = reader.GetInt32(reader.GetOrdinal("required_actions"));
                                actionFrequencies.Add(item);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error executing query: {ex.Message}");
                        throw;
                    }
                }
            }

            return PartialView("_PlantActionFrequenciesTable", actionFrequencies);
        }

        public IActionResult ViewTable()
        {
            var users = new List<dynamic>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT 
                        u.Id AS UserId,
                        u.UserName AS UserName,
                        u.AmountOfMoney,
                        r.Name AS RoleName
                    FROM AspNetUsers u
                    LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
                    LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
                    ORDER BY u.UserName";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dynamic user = new ExpandoObject();
                            user.UserId = reader.GetString(reader.GetOrdinal("UserId"));
                            user.UserName = reader.GetString(reader.GetOrdinal("UserName"));
                            user.AmountOfMoney = reader.IsDBNull(reader.GetOrdinal("AmountOfMoney"))
                                                 ? (decimal?)null
                                                 : reader.GetDecimal(reader.GetOrdinal("AmountOfMoney"));
                            user.RoleName = reader.IsDBNull(reader.GetOrdinal("RoleName"))
                                            ? "No Associated Role"
                                            : reader.GetString(reader.GetOrdinal("RoleName"));
                            users.Add(user);
                        }
                    }
                }
            }

            return View("_ViewTable", users);
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (currentUserId == id)
            {
                return Json(new { success = false, message = "You cannot delete your own account!" });
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string deleteUserPlantsQuery = "DELETE FROM User_Plants WHERE user_id = @UserId";
                            using (SqlCommand cmd = new SqlCommand(deleteUserPlantsQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserId", id);
                                cmd.ExecuteNonQuery();
                            }

                            string deleteUserRolesQuery = "DELETE FROM AspNetUserRoles WHERE UserId = @UserId";
                            using (SqlCommand cmd = new SqlCommand(deleteUserRolesQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserId", id);
                                cmd.ExecuteNonQuery();
                            }

                            string deleteUserQuery = "DELETE FROM AspNetUsers WHERE Id = @UserId";
                            using (SqlCommand cmd = new SqlCommand(deleteUserQuery, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserId", id);
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            return Json(new { success = true, message = "User and all related data deleted successfully." });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return Json(new { success = false, message = $"Error during deletion: {ex.Message}" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Database connection error: {ex.Message}" });
            }
        }

        public IActionResult CategoryStatistics()
        {
            var stats = new List<dynamic>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT 
                        c.category_name AS CategoryName, 
                        AVG(p.price) AS average_price, 
                        COUNT(DISTINCT up.user_id) AS user_count 
                    FROM 
                        User_Plants up
                    JOIN 
                        Plants p ON up.plant_id = p.plant_id 
                    JOIN 
                        Categories c ON p.category_id = c.category_id 
                    GROUP BY 
                        c.category_name
                    ORDER BY 
                        user_count DESC, 
                        average_price DESC;";

                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dynamic stat = new ExpandoObject();
                            stat.CategoryName = reader.GetString(reader.GetOrdinal("CategoryName"));
                            stat.AveragePrice = reader.GetDecimal(reader.GetOrdinal("average_price"));
                            stat.UserCount = reader.GetInt32(reader.GetOrdinal("user_count"));
                            stats.Add(stat);
                        }
                    }
                }
            }

            return PartialView("_CategoryStatistics", stats);
        }
    }
}