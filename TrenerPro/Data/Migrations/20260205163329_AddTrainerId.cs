using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrenerPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TrainerId",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrainerId",
                table: "Clients");
        }
    }
}
