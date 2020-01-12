using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "varchar(32)", nullable: true),
                    password = table.Column<string>(type: "varchar(64)", nullable: true),
                    mail = table.Column<string>(type: "varchar(150)", nullable: true),
                    url = table.Column<string>(type: "varchar(150)", nullable: true),
                    nickName = table.Column<string>(type: "varchar(150)", nullable: true),
                    created = table.Column<int>(type: "int(10)", nullable: false),
                    activated = table.Column<int>(type: "int(10)", nullable: false),
                    group = table.Column<string>(type: "varchar(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
