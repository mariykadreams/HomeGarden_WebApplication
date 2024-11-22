using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KursovaHomeGarden.Migrations
{
    /// <inheritdoc />
    public partial class AddTablesNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "img",
                table: "Plants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "sunlight_requirements_id",
                table: "Plants",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActionTypes",
                columns: table => new
                {
                    action_type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type_name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionTypes", x => x.action_type_id);
                });

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    season_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    season_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    season_start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    season_end = table.Column<DateTime>(type: "datetime2", nullable: false),
                    temperature_range_min = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    temperature_range_max = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.season_id);
                });

            migrationBuilder.CreateTable(
                name: "SunlightRequirements",
                columns: table => new
                {
                    sunlight_requirements_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    light_intensity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    hours_per_day = table.Column<int>(type: "int", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    plant_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SunlightRequirements", x => x.sunlight_requirements_id);
                    table.ForeignKey(
                        name: "FK_SunlightRequirements_Plants_plant_id",
                        column: x => x.plant_id,
                        principalTable: "Plants",
                        principalColumn: "plant_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActionFrequencies",
                columns: table => new
                {
                    Action_frequency_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Interval = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    volume = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    plant_id = table.Column<int>(type: "int", nullable: false),
                    season_id = table.Column<int>(type: "int", nullable: false),
                    action_type_id = table.Column<int>(type: "int", nullable: false),
                    Fert_type_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionFrequencies", x => x.Action_frequency_id);
                    table.ForeignKey(
                        name: "FK_ActionFrequencies_ActionTypes_action_type_id",
                        column: x => x.action_type_id,
                        principalTable: "ActionTypes",
                        principalColumn: "action_type_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActionFrequencies_Plants_plant_id",
                        column: x => x.plant_id,
                        principalTable: "Plants",
                        principalColumn: "plant_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActionFrequencies_Seasons_season_id",
                        column: x => x.season_id,
                        principalTable: "Seasons",
                        principalColumn: "season_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fertilizes",
                columns: table => new
                {
                    Fert_type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    units = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action_frequency_id = table.Column<int>(type: "int", nullable: false),
                    ActionFrequencyAction_frequency_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fertilizes", x => x.Fert_type_id);
                    table.ForeignKey(
                        name: "FK_Fertilizes_ActionFrequencies_ActionFrequencyAction_frequency_id",
                        column: x => x.ActionFrequencyAction_frequency_id,
                        principalTable: "ActionFrequencies",
                        principalColumn: "Action_frequency_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionFrequencies_action_type_id",
                table: "ActionFrequencies",
                column: "action_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_ActionFrequencies_Fert_type_id",
                table: "ActionFrequencies",
                column: "Fert_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_ActionFrequencies_plant_id",
                table: "ActionFrequencies",
                column: "plant_id");

            migrationBuilder.CreateIndex(
                name: "IX_ActionFrequencies_season_id",
                table: "ActionFrequencies",
                column: "season_id");

            migrationBuilder.CreateIndex(
                name: "IX_Fertilizes_ActionFrequencyAction_frequency_id",
                table: "Fertilizes",
                column: "ActionFrequencyAction_frequency_id");

            migrationBuilder.CreateIndex(
                name: "IX_SunlightRequirements_plant_id",
                table: "SunlightRequirements",
                column: "plant_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ActionFrequencies_Fertilizes_Fert_type_id",
                table: "ActionFrequencies",
                column: "Fert_type_id",
                principalTable: "Fertilizes",
                principalColumn: "Fert_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActionFrequencies_ActionTypes_action_type_id",
                table: "ActionFrequencies");

            migrationBuilder.DropForeignKey(
                name: "FK_ActionFrequencies_Fertilizes_Fert_type_id",
                table: "ActionFrequencies");

            migrationBuilder.DropTable(
                name: "SunlightRequirements");

            migrationBuilder.DropTable(
                name: "ActionTypes");

            migrationBuilder.DropTable(
                name: "Fertilizes");

            migrationBuilder.DropTable(
                name: "ActionFrequencies");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropColumn(
                name: "sunlight_requirements_id",
                table: "Plants");

            migrationBuilder.AlterColumn<string>(
                name: "img",
                table: "Plants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
