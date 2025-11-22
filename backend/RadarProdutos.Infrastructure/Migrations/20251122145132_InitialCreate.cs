using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RadarProdutos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalysisConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MinMarginPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxMarginPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    WeightSales = table.Column<decimal>(type: "numeric", nullable: false),
                    WeightCompetition = table.Column<decimal>(type: "numeric", nullable: false),
                    WeightSentiment = table.Column<decimal>(type: "numeric", nullable: false),
                    WeightMargin = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MinMarginPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    TargetMarginPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    MercadoLivreFixedFee = table.Column<decimal>(type: "numeric", nullable: false),
                    MercadoLivrePercentFee = table.Column<decimal>(type: "numeric", nullable: false),
                    MercadoLivreBoostFee = table.Column<decimal>(type: "numeric", nullable: false),
                    ImportTaxPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    CompanyTaxPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    UsdToBrlRate = table.Column<decimal>(type: "numeric", nullable: false),
                    AutoUpdateExchangeRate = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultShippingCostUsd = table.Column<decimal>(type: "numeric", nullable: false),
                    UseEstimatedShipping = table.Column<bool>(type: "boolean", nullable: false),
                    MinSalesVolume = table.Column<int>(type: "integer", nullable: false),
                    MinSupplierRating = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxDeliveryDays = table.Column<int>(type: "integer", nullable: false),
                    WeightMargin = table.Column<decimal>(type: "numeric", nullable: false),
                    WeightSales = table.Column<decimal>(type: "numeric", nullable: false),
                    WeightRating = table.Column<decimal>(type: "numeric", nullable: false),
                    WeightDelivery = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PriceMonthly = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxSearchesPerMonth = table.Column<int>(type: "integer", nullable: false),
                    MaxSearchesPerDay = table.Column<int>(type: "integer", nullable: false),
                    HasPrioritySupport = table.Column<bool>(type: "boolean", nullable: false),
                    HasAdvancedFilters = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShippingEstimates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<string>(type: "text", nullable: false),
                    CategoryName = table.Column<string>(type: "text", nullable: false),
                    MinWeight = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxWeight = table.Column<decimal>(type: "numeric", nullable: false),
                    AverageWeight = table.Column<decimal>(type: "numeric", nullable: false),
                    ShippingCostUsd = table.Column<decimal>(type: "numeric", nullable: false),
                    MinDeliveryDays = table.Column<int>(type: "integer", nullable: false),
                    MaxDeliveryDays = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingEstimates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Analyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Keyword = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Analyses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SearchesUsedThisMonth = table.Column<int>(type: "integer", nullable: false),
                    SearchesUsedToday = table.Column<int>(type: "integer", nullable: false),
                    LastResetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSearchDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Supplier = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    SupplierPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    EstimatedSalePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    MarginPercent = table.Column<decimal>(type: "numeric", nullable: false),
                    Rating = table.Column<decimal>(type: "numeric", nullable: false),
                    Orders = table.Column<int>(type: "integer", nullable: false),
                    CompetitionLevel = table.Column<string>(type: "text", nullable: false),
                    Sentiment = table.Column<string>(type: "text", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProductAnalysisId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Analyses_ProductAnalysisId",
                        column: x => x.ProductAnalysisId,
                        principalTable: "Analyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AnalysisConfigs",
                columns: new[] { "Id", "MaxMarginPercent", "MinMarginPercent", "WeightCompetition", "WeightMargin", "WeightSales", "WeightSentiment" },
                values: new object[] { 1, 80m, 20m, 2.0m, 3.0m, 2.5m, 1.5m });

            migrationBuilder.InsertData(
                table: "MarketplaceConfigs",
                columns: new[] { "Id", "AutoUpdateExchangeRate", "CompanyTaxPercent", "CreatedAt", "DefaultShippingCostUsd", "ImportTaxPercent", "MaxDeliveryDays", "MercadoLivreBoostFee", "MercadoLivreFixedFee", "MercadoLivrePercentFee", "MinMarginPercent", "MinSalesVolume", "MinSupplierRating", "TargetMarginPercent", "UpdatedAt", "UsdToBrlRate", "UseEstimatedShipping", "WeightDelivery", "WeightMargin", "WeightRating", "WeightSales" },
                values: new object[] { 1, false, 8.93m, new DateTime(2025, 11, 22, 14, 51, 31, 153, DateTimeKind.Utc).AddTicks(2363), 15m, 60m, 45, 5m, 20m, 15m, 30m, 100, 4.0m, 50m, new DateTime(2025, 11, 22, 14, 51, 31, 153, DateTimeKind.Utc).AddTicks(2364), 5.70m, true, 0.5m, 1.5m, 1.0m, 1.0m });

            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "Id", "Description", "HasAdvancedFilters", "HasPrioritySupport", "IsActive", "MaxSearchesPerDay", "MaxSearchesPerMonth", "Name", "PriceMonthly" },
                values: new object[,]
                {
                    { 1, "Plano gratuito com 10 buscas por mês", false, false, true, 3, 10, "Free", 0m },
                    { 2, "Teste grátis de 7 dias com 30 buscas", true, false, true, 10, 30, "Trial", 0m },
                    { 3, "Plano profissional com buscas ilimitadas", true, true, true, -1, -1, "Pro", 47.90m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Analyses_CreatedAt",
                table: "Analyses",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Analyses_UserId",
                table: "Analyses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ExternalId",
                table: "Products",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductAnalysisId",
                table: "Products",
                column: "ProductAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingEstimates_CategoryId",
                table: "ShippingEstimates",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanId",
                table: "Subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId_IsActive",
                table: "Subscriptions",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisConfigs");

            migrationBuilder.DropTable(
                name: "MarketplaceConfigs");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "ShippingEstimates");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Analyses");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
