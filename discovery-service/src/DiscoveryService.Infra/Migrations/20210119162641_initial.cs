using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscoveryService.Infra.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "GrpcMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    Name = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrpcMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    Name = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrpcMethodId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastHealthCheck = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsAlive = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceMethods_GrpcMethods_GrpcMethodId",
                        column: x => x.GrpcMethodId,
                        principalTable: "GrpcMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceMethods_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceMethods_GrpcMethodId",
                table: "ServiceMethods",
                column: "GrpcMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceMethods_ServiceId",
                table: "ServiceMethods",
                column: "ServiceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceMethods");

            migrationBuilder.DropTable(
                name: "GrpcMethods");

            migrationBuilder.DropTable(
                name: "Services");
        }
    }
}
