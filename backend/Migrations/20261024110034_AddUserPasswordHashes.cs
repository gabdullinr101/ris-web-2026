using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPasswordHashes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 1,
                column: "password_hash",
                value: "AQAAAAIAAYagAAAAEAw/EGtrNeEt1C9Uo7lWtpUMvIsHVMeTfWUsHnApVx7Mu1EXYmWv0scWDnyEmL4hvA==");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 2,
                column: "password_hash",
                value: "AQAAAAIAAYagAAAAEDcgI/aryuvEd4XMzMXz53H0v9Duh1xsLmHnVYGb9eYwM7lXFkh3gzPnt3L7vaLy7Q==");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 3,
                column: "password_hash",
                value: "AQAAAAIAAYagAAAAEO39nFcWqFHojE5/9bd3ciqh887qf639LgbPU2WF+YEnHDl+0XcIg4oCKgDRy6FRQw==");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 4,
                column: "password_hash",
                value: "AQAAAAIAAYagAAAAEDiJbFxktscOqHrVrk/CWkxC0w2J/l0JxsNBRKdueSBOAMlU6Cjs7mItGbmS61stkQ==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 1,
                column: "password_hash",
                value: null);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 2,
                column: "password_hash",
                value: null);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 3,
                column: "password_hash",
                value: null);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: 4,
                column: "password_hash",
                value: null);
        }
    }
}
