using Microsoft.EntityFrameworkCore.Migrations;

namespace Genisis.Infrastructure.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Universes",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Universes", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Stories",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Logline = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UniverseId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Stories", x => x.Id);
                table.ForeignKey(
                    name: "FK_Stories_Universes_UniverseId",
                    column: x => x.UniverseId,
                    principalTable: "Universes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Characters",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Tier = table.Column<string>(type: "TEXT", nullable: false),
                Bio = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: true),
                Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UniverseId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Characters", x => x.Id);
                table.ForeignKey(
                    name: "FK_Characters_Universes_UniverseId",
                    column: x => x.UniverseId,
                    principalTable: "Universes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Chapters",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                ChapterOrder = table.Column<int>(type: "INTEGER", nullable: false),
                Content = table.Column<string>(type: "TEXT", maxLength: 10000, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                StoryId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Chapters", x => x.Id);
                table.ForeignKey(
                    name: "FK_Chapters_Stories_StoryId",
                    column: x => x.StoryId,
                    principalTable: "Stories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ChapterCharacter",
            columns: table => new
            {
                ChaptersId = table.Column<int>(type: "INTEGER", nullable: false),
                CharactersId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChapterCharacter", x => new { x.ChaptersId, x.CharactersId });
                table.ForeignKey(
                    name: "FK_ChapterCharacter_Chapters_CharactersId",
                    column: x => x.CharactersId,
                    principalTable: "Chapters",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ChapterCharacter_Characters_CharactersId",
                    column: x => x.CharactersId,
                    principalTable: "Characters",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Characters_UniverseId",
            table: "Characters",
            column: "UniverseId");

        migrationBuilder.CreateIndex(
            name: "IX_Chapters_StoryId",
            table: "Chapters",
            column: "StoryId");

        migrationBuilder.CreateIndex(
            name: "IX_ChapterCharacter_CharactersId",
            table: "ChapterCharacter",
            column: "CharactersId");

        migrationBuilder.CreateIndex(
            name: "IX_Stories_UniverseId",
            table: "Stories",
            column: "UniverseId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ChapterCharacter");

        migrationBuilder.DropTable(
            name: "Chapters");

        migrationBuilder.DropTable(
            name: "Characters");

        migrationBuilder.DropTable(
            name: "Stories");

        migrationBuilder.DropTable(
            name: "Universes");
    }
}
