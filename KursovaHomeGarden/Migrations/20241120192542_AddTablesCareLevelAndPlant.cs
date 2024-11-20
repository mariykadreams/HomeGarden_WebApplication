using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KursovaHomeGarden.Migrations
{
    /// <inheritdoc />
    public partial class AddTablesCareLevelAndPlant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CareLevels",
                columns: table => new
                {
                    care_level_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    level_name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CareLevels", x => x.care_level_id);
                });

            migrationBuilder.CreateTable(
                name: "Plants",
                columns: table => new
                {
                    plant_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    img = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    care_level_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plants", x => x.plant_id);
                    table.ForeignKey(
                        name: "FK_Plants_CareLevels_care_level_id",
                        column: x => x.care_level_id,
                        principalTable: "CareLevels",
                        principalColumn: "care_level_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Plants_Categories_category_id",
                        column: x => x.category_id,
                        principalTable: "Categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Plants_care_level_id",
                table: "Plants",
                column: "care_level_id");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_category_id",
                table: "Plants",
                column: "category_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Plants");

            migrationBuilder.DropTable(
                name: "CareLevels");
        }
    }
}
