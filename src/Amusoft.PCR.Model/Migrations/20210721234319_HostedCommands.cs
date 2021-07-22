using Microsoft.EntityFrameworkCore.Migrations;

namespace Amusoft.PCR.Model.Migrations
{
    public partial class HostedCommands : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HostCommands",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    CommandName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ProgramPath = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Arguments = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostCommands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    PermissionType = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => new { x.UserId, x.SubjectId, x.PermissionType });
                    table.ForeignKey(
                        name: "FK_Permissions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HostCommands");

            migrationBuilder.DropTable(
                name: "Permissions");
        }
    }
}
