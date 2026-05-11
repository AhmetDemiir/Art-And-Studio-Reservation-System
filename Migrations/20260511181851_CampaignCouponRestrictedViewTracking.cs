using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Online_Art_Gallery_and_Studio_Reservation_System.Migrations
{
    /// <inheritdoc />
    public partial class CampaignCouponRestrictedViewTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CampaignId",
                table: "WorkshopEvents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RestrictedUserId",
                table: "Coupons",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CampaignId",
                table: "Artworks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopEvents_CampaignId",
                table: "WorkshopEvents",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_RestrictedUserId",
                table: "Coupons",
                column: "RestrictedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Artworks_CampaignId",
                table: "Artworks",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_Artworks_Campaigns_CampaignId",
                table: "Artworks",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "CampaignId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Coupons_AspNetUsers_RestrictedUserId",
                table: "Coupons",
                column: "RestrictedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkshopEvents_Campaigns_CampaignId",
                table: "WorkshopEvents",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "CampaignId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Artworks_Campaigns_CampaignId",
                table: "Artworks");

            migrationBuilder.DropForeignKey(
                name: "FK_Coupons_AspNetUsers_RestrictedUserId",
                table: "Coupons");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkshopEvents_Campaigns_CampaignId",
                table: "WorkshopEvents");

            migrationBuilder.DropIndex(
                name: "IX_WorkshopEvents_CampaignId",
                table: "WorkshopEvents");

            migrationBuilder.DropIndex(
                name: "IX_Coupons_RestrictedUserId",
                table: "Coupons");

            migrationBuilder.DropIndex(
                name: "IX_Artworks_CampaignId",
                table: "Artworks");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "WorkshopEvents");

            migrationBuilder.DropColumn(
                name: "RestrictedUserId",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "Artworks");
        }
    }
}
