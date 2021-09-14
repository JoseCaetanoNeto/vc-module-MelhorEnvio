using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace vc_module_MelhorEnvio.Data.Migrations
{
    public partial class InitialOrders2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "PackageState", table: "OrderShipmentPackage", maxLength: 64, nullable: true);
            migrationBuilder.AddColumn<string>(name: "TrackingCode", table: "OrderShipmentPackage", maxLength: 128, nullable: true);
            migrationBuilder.AddColumn<string>(name: "OuterId", table: "OrderShipmentPackage", maxLength: 128, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Protocol", table: "OrderShipmentPackage", maxLength: 128, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Discriminator", table: "OrderShipmentPackage", nullable: false, maxLength: 128, defaultValue: "ShipmentPackage2Entity");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("PackageState", "OrderShipmentPackage");
            migrationBuilder.DropColumn("TrackingCode", "OrderShipmentPackage");
            migrationBuilder.DropColumn("OuterId", "OrderShipmentPackage");
            migrationBuilder.DropColumn("Protocol", "OrderShipmentPackage");
            migrationBuilder.DropColumn("Discriminator", "OrderShipmentPackage");
        }
    }
}
