using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DocSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Iktatokonyvek",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nev = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Kod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Evszam = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Iktatokonyvek", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FoszamCounters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Next = table.Column<int>(type: "integer", nullable: false),
                    IktatokonyvId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoszamCounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FoszamCounters_Iktatokonyvek_IktatokonyvId",
                        column: x => x.IktatokonyvId,
                        principalTable: "Iktatokonyvek",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Iktatoszamok",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Foszam = table.Column<int>(type: "integer", nullable: false),
                    Alszam = table.Column<int>(type: "integer", nullable: false),
                    Valid = table.Column<bool>(type: "boolean", nullable: false),
                    IktatokonyvId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Iktatoszamok", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Iktatoszamok_Iktatokonyvek_IktatokonyvId",
                        column: x => x.IktatokonyvId,
                        principalTable: "Iktatokonyvek",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlszamCounters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Next = table.Column<int>(type: "integer", nullable: false),
                    IktatoszamId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlszamCounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlszamCounters_Iktatoszamok_IktatoszamId",
                        column: x => x.IktatoszamId,
                        principalTable: "Iktatoszamok",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ugyiratok",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IktatoszamId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ugyiratok", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ugyiratok_Iktatoszamok_IktatoszamId",
                        column: x => x.IktatoszamId,
                        principalTable: "Iktatoszamok",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Iratok",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IktatoszamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Details = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    UgyiratId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Iratok", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Iratok_Iktatoszamok_IktatoszamId",
                        column: x => x.IktatoszamId,
                        principalTable: "Iktatoszamok",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Iratok_Ugyiratok_UgyiratId",
                        column: x => x.UgyiratId,
                        principalTable: "Ugyiratok",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlszamCounters_IktatoszamId",
                table: "AlszamCounters",
                column: "IktatoszamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FoszamCounters_IktatokonyvId",
                table: "FoszamCounters",
                column: "IktatokonyvId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Iktatokonyvek_Kod_Evszam",
                table: "Iktatokonyvek",
                columns: new[] { "Kod", "Evszam" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Iktatoszamok_IktatokonyvId_Foszam_Alszam",
                table: "Iktatoszamok",
                columns: new[] { "IktatokonyvId", "Foszam", "Alszam" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Iratok_IktatoszamId",
                table: "Iratok",
                column: "IktatoszamId");

            migrationBuilder.CreateIndex(
                name: "IX_Iratok_UgyiratId",
                table: "Iratok",
                column: "UgyiratId");

            migrationBuilder.CreateIndex(
                name: "IX_Ugyiratok_IktatoszamId",
                table: "Ugyiratok",
                column: "IktatoszamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlszamCounters");

            migrationBuilder.DropTable(
                name: "FoszamCounters");

            migrationBuilder.DropTable(
                name: "Iratok");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Ugyiratok");

            migrationBuilder.DropTable(
                name: "Iktatoszamok");

            migrationBuilder.DropTable(
                name: "Iktatokonyvek");
        }
    }
}
