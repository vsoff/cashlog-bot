using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cashlog.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChatName = table.Column<string>(type: "text", nullable: false),
                    ChatToken = table.Column<string>(type: "text", nullable: false),
                    AdminToken = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillingPeriods",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    PeriodBegin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillingPeriods_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    Caption = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MoneyOperations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BillingPeriodId = table.Column<long>(type: "bigint", nullable: false),
                    CustomerFromId = table.Column<long>(type: "bigint", nullable: false),
                    CustomerToId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    OperationType = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MoneyOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MoneyOperations_BillingPeriods_BillingPeriodId",
                        column: x => x.BillingPeriodId,
                        principalTable: "BillingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Receipts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    PurchaseTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAmount = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<byte>(type: "smallint", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: true),
                    BillingPeriodId = table.Column<long>(type: "bigint", nullable: false),
                    FiscalDocument = table.Column<string>(type: "text", nullable: true),
                    FiscalNumber = table.Column<string>(type: "text", nullable: true),
                    FiscalSign = table.Column<string>(type: "text", nullable: true),
                    RetailAddress = table.Column<string>(type: "text", nullable: true),
                    RetailInn = table.Column<string>(type: "text", nullable: true),
                    CompanyName = table.Column<string>(type: "text", nullable: true),
                    CashierName = table.Column<string>(type: "text", nullable: true),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Receipts_BillingPeriods_BillingPeriodId",
                        column: x => x.BillingPeriodId,
                        principalTable: "BillingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Receipts_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Receipts_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptConsumerMaps",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReceiptId = table.Column<long>(type: "bigint", nullable: false),
                    ConsumerId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptConsumerMaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptConsumerMaps_Customers_ConsumerId",
                        column: x => x.ConsumerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptConsumerMaps_Receipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "Receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillingPeriods_GroupId",
                table: "BillingPeriods",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_GroupId",
                table: "Customers",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_MoneyOperations_BillingPeriodId",
                table: "MoneyOperations",
                column: "BillingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptConsumerMaps_ConsumerId",
                table: "ReceiptConsumerMaps",
                column: "ConsumerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptConsumerMaps_ReceiptId",
                table: "ReceiptConsumerMaps",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_BillingPeriodId",
                table: "Receipts",
                column: "BillingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_CustomerId",
                table: "Receipts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_GroupId",
                table: "Receipts",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MoneyOperations");

            migrationBuilder.DropTable(
                name: "ReceiptConsumerMaps");

            migrationBuilder.DropTable(
                name: "Receipts");

            migrationBuilder.DropTable(
                name: "BillingPeriods");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Groups");
        }
    }
}
