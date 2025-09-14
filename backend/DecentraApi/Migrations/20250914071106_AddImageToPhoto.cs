using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecentraApi.Migrations
{
    /// <inheritdoc />
    public partial class AddImageToPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "photos",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "photos");
        }
    }
}
