using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketStat.Database.Context.Migrations
{
    /// <inheritdoc />
    public partial class RenameIsEtlUserToIsAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_etl_user",
                schema: "marketstat",
                table: "users",
                newName: "is_admin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_admin",
                schema: "marketstat",
                table: "users",
                newName: "is_etl_user");
        }
    }
}
