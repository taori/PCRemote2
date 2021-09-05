using Microsoft.EntityFrameworkCore.Migrations;

namespace Amusoft.PCR.Model.Migrations
{
    public partial class VoiceRecognition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AudioFeeds",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioFeeds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KeyValueSettings",
                columns: table => new
                {
                    Key = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyValueSettings", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "AudioFeedAliases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    FeedId = table.Column<string>(type: "nvarchar(45)", nullable: true),
                    Alias = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioFeedAliases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AudioFeedAliases_AudioFeeds_FeedId",
                        column: x => x.FeedId,
                        principalTable: "AudioFeeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AudioFeedAliases_FeedId",
                table: "AudioFeedAliases",
                column: "FeedId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AudioFeedAliases");

            migrationBuilder.DropTable(
                name: "KeyValueSettings");

            migrationBuilder.DropTable(
                name: "AudioFeeds");
        }
    }
}
