using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoErrorhandler.Migrations
{
    /// <inheritdoc />
    public partial class CreateAutoErrorHandlerstable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tbl_AutoErrorHandlers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fkProjectid = table.Column<int>(type: "int", nullable: false),
                    SourceFilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DestinationFilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Filename = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    language = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_AutoErrorHandlers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ErrorDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Wrong_Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Correct_Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Error_description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AutoErrorHandlerRequestId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ErrorDetail_tbl_AutoErrorHandlers_AutoErrorHandlerRequestId",
                        column: x => x.AutoErrorHandlerRequestId,
                        principalTable: "tbl_AutoErrorHandlers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ErrorDetail_AutoErrorHandlerRequestId",
                table: "ErrorDetail",
                column: "AutoErrorHandlerRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErrorDetail");

            migrationBuilder.DropTable(
                name: "tbl_AutoErrorHandlers");
        }
    }
}
