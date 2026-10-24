using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "halls",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("halls_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    login = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                    table.CheckConstraint("users_role_check", "role IN ('Admin', 'Trainer', 'Manager')");
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    number = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    hall_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "New"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    created_by_user_id = table.Column<int>(type: "integer", nullable: false),
                    assignee_user_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("bookings_pkey", x => x.id);
                    table.CheckConstraint("bookings_description_check", "char_length(description) BETWEEN 10 AND 500");
                    table.CheckConstraint("bookings_status_check", "status IN ('New', 'InProgress', 'Closed', 'Cancelled')");
                    table.CheckConstraint("bookings_title_check", "char_length(title) BETWEEN 5 AND 80");
                    table.ForeignKey(
                        name: "bookings_assignee_user_id_fkey",
                        column: x => x.assignee_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "bookings_created_by_user_id_fkey",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "bookings_hall_id_fkey",
                        column: x => x.hall_id,
                        principalTable: "halls",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "halls",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Тренажёрный зал" },
                    { 2, "Зал единоборств" },
                    { 3, "Зал групповых программ" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "full_name", "login", "password_hash", "role" },
                values: new object[,]
                {
                    { 1, "Анна Ковалёва", "admin", null, "Admin" },
                    { 2, "Илья Соколов", "trainer", null, "Trainer" },
                    { 3, "Марат Хайруллин", "trainer2", null, "Trainer" },
                    { 4, "Ольга Петрова", "manager", null, "Manager" }
                });

            migrationBuilder.CreateIndex(
                name: "bookings_assignee_user_id_idx",
                table: "bookings",
                column: "assignee_user_id");

            migrationBuilder.CreateIndex(
                name: "bookings_created_by_user_id_idx",
                table: "bookings",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "bookings_hall_id_idx",
                table: "bookings",
                column: "hall_id");

            migrationBuilder.CreateIndex(
                name: "bookings_number_key",
                table: "bookings",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "bookings_status_idx",
                table: "bookings",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "halls_name_key",
                table: "halls",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "users_login_key",
                table: "users",
                column: "login",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "halls");
        }
    }
}
