using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Splitza.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BillSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "ZAR"),
                    PaymentMode = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Vat = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Tip = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ServiceFee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TipAllocation = table.Column<int>(type: "integer", nullable: false),
                    ServiceFeeAllocation = table.Column<int>(type: "integer", nullable: false),
                    DiscountAllocation = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BillSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsHost = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                    table.ForeignKey(
                        name: "FK_People_BillSessions_BillSessionId",
                        column: x => x.BillSessionId,
                        principalTable: "BillSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BillSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsManuallyAdded = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptItems_BillSessions_BillSessionId",
                        column: x => x.BillSessionId,
                        principalTable: "BillSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemAllocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiptItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocationType = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    QuantityPortion = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemAllocations_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemAllocations_ReceiptItems_ReceiptItemId",
                        column: x => x.ReceiptItemId,
                        principalTable: "ReceiptItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "BillSessions",
                columns: new[] { "Id", "CreatedAt", "Currency", "Discount", "DiscountAllocation", "Name", "PaymentMode", "ServiceFee", "ServiceFeeAllocation", "Status", "Subtotal", "Tip", "TipAllocation", "Total", "UpdatedAt", "Vat" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "ZAR", 0m, 1, "Demo — Harbour House", 0, 0m, 0, 4, 520.00m, 78.00m, 1, 598.00m, new DateTime(2025, 1, 1, 12, 30, 0, 0, DateTimeKind.Utc), 0m });

            migrationBuilder.InsertData(
                table: "People",
                columns: new[] { "Id", "BillSessionId", "IsHost", "Name" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("11111111-1111-1111-1111-111111111111"), true, "Alice" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new Guid("11111111-1111-1111-1111-111111111111"), false, "Bob" }
                });

            migrationBuilder.InsertData(
                table: "ReceiptItems",
                columns: new[] { "Id", "BillSessionId", "IsManuallyAdded", "LineTotal", "Name", "Quantity", "SortOrder", "UnitPrice" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444444"), new Guid("11111111-1111-1111-1111-111111111111"), false, 260.00m, "Grilled Kingklip", 1m, 1, 260.00m },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new Guid("11111111-1111-1111-1111-111111111111"), false, 120.00m, "Calamari Starter", 1m, 2, 120.00m },
                    { new Guid("66666666-6666-6666-6666-666666666666"), new Guid("11111111-1111-1111-1111-111111111111"), false, 140.00m, "House Wines x2", 2m, 3, 70.00m }
                });

            migrationBuilder.InsertData(
                table: "ItemAllocations",
                columns: new[] { "Id", "AllocationType", "Amount", "PersonId", "QuantityPortion", "ReceiptItemId" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-777777777777"), 0, 260.00m, new Guid("22222222-2222-2222-2222-222222222222"), 0m, new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("88888888-8888-8888-8888-888888888888"), 0, 120.00m, new Guid("33333333-3333-3333-3333-333333333333"), 0m, new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("99999999-9999-9999-9999-999999999999"), 1, 70.00m, new Guid("22222222-2222-2222-2222-222222222222"), 1m, new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), 1, 70.00m, new Guid("33333333-3333-3333-3333-333333333333"), 1m, new Guid("66666666-6666-6666-6666-666666666666") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillSessions_CreatedAt",
                table: "BillSessions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAllocations_PersonId",
                table: "ItemAllocations",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAllocations_ReceiptItemId",
                table: "ItemAllocations",
                column: "ReceiptItemId");

            migrationBuilder.CreateIndex(
                name: "IX_People_BillSessionId",
                table: "People",
                column: "BillSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptItems_BillSessionId",
                table: "ReceiptItems",
                column: "BillSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemAllocations");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropTable(
                name: "ReceiptItems");

            migrationBuilder.DropTable(
                name: "BillSessions");
        }
    }
}
