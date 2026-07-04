using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinkUpPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ships_GameId",
                table: "Ships");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_UserOneId",
                table: "Friendships");

            migrationBuilder.DropIndex(
                name: "IX_Attacks_GameId",
                table: "Attacks");

            migrationBuilder.AddColumn<string>(
                name: "PairKey",
                table: "FriendRequests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PairKey",
                table: "BattleshipGames",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Ships_GameId_OwnerId_StartRow_StartCol",
                table: "Ships",
                columns: new[] { "GameId", "OwnerId", "StartRow", "StartCol" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_UserOneId_UserTwoId",
                table: "Friendships",
                columns: new[] { "UserOneId", "UserTwoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_PairKey",
                table: "FriendRequests",
                column: "PairKey",
                unique: true,
                filter: "[Status] = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_BattleshipGames_PairKey",
                table: "BattleshipGames",
                column: "PairKey",
                unique: true,
                filter: "[Status] <> 'Finished'");

            migrationBuilder.CreateIndex(
                name: "IX_Attacks_GameId_AttackerId_Row_Col",
                table: "Attacks",
                columns: new[] { "GameId", "AttackerId", "Row", "Col" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ships_GameId_OwnerId_StartRow_StartCol",
                table: "Ships");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_UserOneId_UserTwoId",
                table: "Friendships");

            migrationBuilder.DropIndex(
                name: "IX_FriendRequests_PairKey",
                table: "FriendRequests");

            migrationBuilder.DropIndex(
                name: "IX_BattleshipGames_PairKey",
                table: "BattleshipGames");

            migrationBuilder.DropIndex(
                name: "IX_Attacks_GameId_AttackerId_Row_Col",
                table: "Attacks");

            migrationBuilder.DropColumn(
                name: "PairKey",
                table: "FriendRequests");

            migrationBuilder.DropColumn(
                name: "PairKey",
                table: "BattleshipGames");

            migrationBuilder.CreateIndex(
                name: "IX_Ships_GameId",
                table: "Ships",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_UserOneId",
                table: "Friendships",
                column: "UserOneId");

            migrationBuilder.CreateIndex(
                name: "IX_Attacks_GameId",
                table: "Attacks",
                column: "GameId");
        }
    }
}
