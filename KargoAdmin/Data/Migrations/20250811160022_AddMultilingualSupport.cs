using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KargoAdmin.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMultilingualSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Blogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ContentEn",
                table: "Blogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescriptionEn",
                table: "Blogs",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SlugEn",
                table: "Blogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TagsEn",
                table: "Blogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TitleEn",
                table: "Blogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentEn",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "MetaDescriptionEn",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "SlugEn",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "TagsEn",
                table: "Blogs");

            migrationBuilder.DropColumn(
                name: "TitleEn",
                table: "Blogs");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Blogs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }
    }
}
