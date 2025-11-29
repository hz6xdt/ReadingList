using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReadingList.Models.Migrations
{
    /// <inheritdoc />
    public partial class LongestUnread : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HideFromLongestUnread",
                table: "Books",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HideFromLongestUnread",
                table: "Books");
        }
    }
}
