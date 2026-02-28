using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrenerPro.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNotesToClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Clients");
        }
    }
}
