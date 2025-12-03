using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSBackupSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixNamingConventions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_moveset_game_gameid",
                table: "moveset");

            migrationBuilder.DropForeignKey(
                name: "fk_moveset_moveset_previoussetid",
                table: "moveset");

            migrationBuilder.DropForeignKey(
                name: "fk_systemroleclaims_systemroles_roleid",
                table: "SystemRoleClaims");

            migrationBuilder.DropForeignKey(
                name: "fk_systemuserclaims_systemusers_userid",
                table: "SystemUserClaims");

            migrationBuilder.DropForeignKey(
                name: "fk_systemuserlogins_systemusers_userid",
                table: "SystemUserLogins");

            migrationBuilder.DropForeignKey(
                name: "fk_systemuserroles_systemroles_roleid",
                table: "SystemUserRoles");

            migrationBuilder.DropForeignKey(
                name: "fk_systemuserroles_systemusers_userid",
                table: "SystemUserRoles");

            migrationBuilder.DropForeignKey(
                name: "fk_systemusertokens_systemusers_userid",
                table: "SystemUserTokens");

            migrationBuilder.DropForeignKey(
                name: "fk_unitorders_moveset_movesetid",
                table: "unitorders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_unitorders",
                table: "unitorders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_systemusertokens",
                table: "SystemUserTokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_systemusers",
                table: "SystemUsers");

            migrationBuilder.DropPrimaryKey(
                name: "pk_systemuserroles",
                table: "SystemUserRoles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_systemuserlogins",
                table: "SystemUserLogins");

            migrationBuilder.DropPrimaryKey(
                name: "pk_systemuserclaims",
                table: "SystemUserClaims");

            migrationBuilder.DropPrimaryKey(
                name: "pk_systemroles",
                table: "SystemRoles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_systemroleclaims",
                table: "SystemRoleClaims");

            migrationBuilder.DropPrimaryKey(
                name: "pk_moveset",
                table: "moveset");

            migrationBuilder.DropPrimaryKey(
                name: "pk_game",
                table: "game");

            migrationBuilder.RenameTable(
                name: "unitorders",
                newName: "unit_orders");

            migrationBuilder.RenameTable(
                name: "SystemUserTokens",
                newName: "system_user_tokens");

            migrationBuilder.RenameTable(
                name: "SystemUsers",
                newName: "system_users");

            migrationBuilder.RenameTable(
                name: "SystemUserRoles",
                newName: "system_user_roles");

            migrationBuilder.RenameTable(
                name: "SystemUserLogins",
                newName: "system_user_logins");

            migrationBuilder.RenameTable(
                name: "SystemUserClaims",
                newName: "system_user_claims");

            migrationBuilder.RenameTable(
                name: "SystemRoles",
                newName: "system_roles");

            migrationBuilder.RenameTable(
                name: "SystemRoleClaims",
                newName: "system_role_claims");

            migrationBuilder.RenameTable(
                name: "moveset",
                newName: "move_sets");

            migrationBuilder.RenameTable(
                name: "game",
                newName: "games");

            migrationBuilder.RenameColumn(
                name: "unittype",
                table: "unit_orders",
                newName: "unit_type");

            migrationBuilder.RenameColumn(
                name: "unitcoast",
                table: "unit_orders",
                newName: "unit_coast");

            migrationBuilder.RenameColumn(
                name: "resultreason",
                table: "unit_orders",
                newName: "result_reason");

            migrationBuilder.RenameColumn(
                name: "movesetid",
                table: "unit_orders",
                newName: "move_set_id");

            migrationBuilder.RenameIndex(
                name: "ix_unitorders_movesetid",
                table: "unit_orders",
                newName: "ix_unit_orders_move_set_id");

            migrationBuilder.RenameColumn(
                name: "loginprovider",
                table: "system_user_tokens",
                newName: "login_provider");

            migrationBuilder.RenameColumn(
                name: "userid",
                table: "system_user_tokens",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "username",
                table: "system_users",
                newName: "user_name");

            migrationBuilder.RenameColumn(
                name: "twofactorenabled",
                table: "system_users",
                newName: "two_factor_enabled");

            migrationBuilder.RenameColumn(
                name: "securitystamp",
                table: "system_users",
                newName: "security_stamp");

            migrationBuilder.RenameColumn(
                name: "passwordhash",
                table: "system_users",
                newName: "password_hash");

            migrationBuilder.RenameColumn(
                name: "normalizedusername",
                table: "system_users",
                newName: "normalized_user_name");

            migrationBuilder.RenameColumn(
                name: "normalizedemail",
                table: "system_users",
                newName: "normalized_email");

            migrationBuilder.RenameColumn(
                name: "lockoutend",
                table: "system_users",
                newName: "lockout_end");

            migrationBuilder.RenameColumn(
                name: "lockoutenabled",
                table: "system_users",
                newName: "lockout_enabled");

            migrationBuilder.RenameColumn(
                name: "emailconfirmed",
                table: "system_users",
                newName: "email_confirmed");

            migrationBuilder.RenameColumn(
                name: "concurrencystamp",
                table: "system_users",
                newName: "concurrency_stamp");

            migrationBuilder.RenameColumn(
                name: "accessfailedcount",
                table: "system_users",
                newName: "access_failed_count");

            migrationBuilder.RenameIndex(
                name: "UserNameIndex",
                table: "system_users",
                newName: "idx_users_normalized_user_name");

            migrationBuilder.RenameIndex(
                name: "EmailIndex",
                table: "system_users",
                newName: "idx_users_normalized_email");

            migrationBuilder.RenameColumn(
                name: "roleid",
                table: "system_user_roles",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "userid",
                table: "system_user_roles",
                newName: "user_id");

            migrationBuilder.RenameIndex(
                name: "ix_systemuserroles_roleid",
                table: "system_user_roles",
                newName: "ix_system_user_roles_role_id");

            migrationBuilder.RenameColumn(
                name: "userid",
                table: "system_user_logins",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "providerdisplayname",
                table: "system_user_logins",
                newName: "provider_display_name");

            migrationBuilder.RenameColumn(
                name: "providerkey",
                table: "system_user_logins",
                newName: "provider_key");

            migrationBuilder.RenameColumn(
                name: "loginprovider",
                table: "system_user_logins",
                newName: "login_provider");

            migrationBuilder.RenameIndex(
                name: "ix_systemuserlogins_userid",
                table: "system_user_logins",
                newName: "ix_system_user_logins_user_id");

            migrationBuilder.RenameColumn(
                name: "userid",
                table: "system_user_claims",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "claimvalue",
                table: "system_user_claims",
                newName: "claim_value");

            migrationBuilder.RenameColumn(
                name: "claimtype",
                table: "system_user_claims",
                newName: "claim_type");

            migrationBuilder.RenameIndex(
                name: "ix_systemuserclaims_userid",
                table: "system_user_claims",
                newName: "ix_system_user_claims_user_id");

            migrationBuilder.RenameColumn(
                name: "normalizedname",
                table: "system_roles",
                newName: "normalized_name");

            migrationBuilder.RenameColumn(
                name: "concurrencystamp",
                table: "system_roles",
                newName: "concurrency_stamp");

            migrationBuilder.RenameIndex(
                name: "RoleNameIndex",
                table: "system_roles",
                newName: "index_role_normalized_name");

            migrationBuilder.RenameColumn(
                name: "roleid",
                table: "system_role_claims",
                newName: "role_id");

            migrationBuilder.RenameColumn(
                name: "claimvalue",
                table: "system_role_claims",
                newName: "claim_value");

            migrationBuilder.RenameColumn(
                name: "claimtype",
                table: "system_role_claims",
                newName: "claim_type");

            migrationBuilder.RenameIndex(
                name: "ix_systemroleclaims_roleid",
                table: "system_role_claims",
                newName: "ix_system_role_claims_role_id");

            migrationBuilder.RenameColumn(
                name: "seasonindex",
                table: "move_sets",
                newName: "season_index");

            migrationBuilder.RenameColumn(
                name: "previoussetid",
                table: "move_sets",
                newName: "previous_set_id");

            migrationBuilder.RenameColumn(
                name: "preretreathash",
                table: "move_sets",
                newName: "pre_retreat_hash");

            migrationBuilder.RenameColumn(
                name: "lastseen",
                table: "move_sets",
                newName: "last_seen");

            migrationBuilder.RenameColumn(
                name: "gameid",
                table: "move_sets",
                newName: "game_id");

            migrationBuilder.RenameColumn(
                name: "fullhash",
                table: "move_sets",
                newName: "full_hash");

            migrationBuilder.RenameColumn(
                name: "firstseen",
                table: "move_sets",
                newName: "first_seen");

            migrationBuilder.RenameIndex(
                name: "ix_moveset_previoussetid",
                table: "move_sets",
                newName: "ix_move_sets_previous_set_id");

            migrationBuilder.RenameIndex(
                name: "ix_moveset_gameid",
                table: "move_sets",
                newName: "ix_move_sets_game_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_unit_orders",
                table: "unit_orders",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_system_user_tokens",
                table: "system_user_tokens",
                columns: new[] { "user_id", "login_provider", "name" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_system_users",
                table: "system_users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_system_user_roles",
                table: "system_user_roles",
                columns: new[] { "user_id", "role_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_system_user_logins",
                table: "system_user_logins",
                columns: new[] { "login_provider", "provider_key" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_system_user_claims",
                table: "system_user_claims",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_system_roles",
                table: "system_roles",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_system_role_claims",
                table: "system_role_claims",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_move_sets",
                table: "move_sets",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_games",
                table: "games",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_move_sets_games_game_id",
                table: "move_sets",
                column: "game_id",
                principalTable: "games",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_move_sets_move_sets_previous_set_id",
                table: "move_sets",
                column: "previous_set_id",
                principalTable: "move_sets",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_system_role_claims_system_roles_role_id",
                table: "system_role_claims",
                column: "role_id",
                principalTable: "system_roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_system_user_claims_system_users_user_id",
                table: "system_user_claims",
                column: "user_id",
                principalTable: "system_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_system_user_logins_system_users_user_id",
                table: "system_user_logins",
                column: "user_id",
                principalTable: "system_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_system_user_roles_system_roles_role_id",
                table: "system_user_roles",
                column: "role_id",
                principalTable: "system_roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_system_user_roles_system_users_user_id",
                table: "system_user_roles",
                column: "user_id",
                principalTable: "system_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_system_user_tokens_system_users_user_id",
                table: "system_user_tokens",
                column: "user_id",
                principalTable: "system_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_unit_orders_move_sets_move_set_id",
                table: "unit_orders",
                column: "move_set_id",
                principalTable: "move_sets",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_move_sets_games_game_id",
                table: "move_sets");

            migrationBuilder.DropForeignKey(
                name: "fk_move_sets_move_sets_previous_set_id",
                table: "move_sets");

            migrationBuilder.DropForeignKey(
                name: "fk_system_role_claims_system_roles_role_id",
                table: "system_role_claims");

            migrationBuilder.DropForeignKey(
                name: "fk_system_user_claims_system_users_user_id",
                table: "system_user_claims");

            migrationBuilder.DropForeignKey(
                name: "fk_system_user_logins_system_users_user_id",
                table: "system_user_logins");

            migrationBuilder.DropForeignKey(
                name: "fk_system_user_roles_system_roles_role_id",
                table: "system_user_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_system_user_roles_system_users_user_id",
                table: "system_user_roles");

            migrationBuilder.DropForeignKey(
                name: "fk_system_user_tokens_system_users_user_id",
                table: "system_user_tokens");

            migrationBuilder.DropForeignKey(
                name: "fk_unit_orders_move_sets_move_set_id",
                table: "unit_orders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_unit_orders",
                table: "unit_orders");

            migrationBuilder.DropPrimaryKey(
                name: "pk_system_users",
                table: "system_users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_system_user_tokens",
                table: "system_user_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_system_user_roles",
                table: "system_user_roles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_system_user_logins",
                table: "system_user_logins");

            migrationBuilder.DropPrimaryKey(
                name: "pk_system_user_claims",
                table: "system_user_claims");

            migrationBuilder.DropPrimaryKey(
                name: "pk_system_roles",
                table: "system_roles");

            migrationBuilder.DropPrimaryKey(
                name: "pk_system_role_claims",
                table: "system_role_claims");

            migrationBuilder.DropPrimaryKey(
                name: "pk_move_sets",
                table: "move_sets");

            migrationBuilder.DropPrimaryKey(
                name: "pk_games",
                table: "games");

            migrationBuilder.RenameTable(
                name: "unit_orders",
                newName: "unitorders");

            migrationBuilder.RenameTable(
                name: "system_users",
                newName: "SystemUsers");

            migrationBuilder.RenameTable(
                name: "system_user_tokens",
                newName: "SystemUserTokens");

            migrationBuilder.RenameTable(
                name: "system_user_roles",
                newName: "SystemUserRoles");

            migrationBuilder.RenameTable(
                name: "system_user_logins",
                newName: "SystemUserLogins");

            migrationBuilder.RenameTable(
                name: "system_user_claims",
                newName: "SystemUserClaims");

            migrationBuilder.RenameTable(
                name: "system_roles",
                newName: "SystemRoles");

            migrationBuilder.RenameTable(
                name: "system_role_claims",
                newName: "SystemRoleClaims");

            migrationBuilder.RenameTable(
                name: "move_sets",
                newName: "moveset");

            migrationBuilder.RenameTable(
                name: "games",
                newName: "game");

            migrationBuilder.RenameColumn(
                name: "unit_type",
                table: "unitorders",
                newName: "unittype");

            migrationBuilder.RenameColumn(
                name: "unit_coast",
                table: "unitorders",
                newName: "unitcoast");

            migrationBuilder.RenameColumn(
                name: "result_reason",
                table: "unitorders",
                newName: "resultreason");

            migrationBuilder.RenameColumn(
                name: "move_set_id",
                table: "unitorders",
                newName: "movesetid");

            migrationBuilder.RenameIndex(
                name: "ix_unit_orders_move_set_id",
                table: "unitorders",
                newName: "ix_unitorders_movesetid");

            migrationBuilder.RenameColumn(
                name: "user_name",
                table: "SystemUsers",
                newName: "username");

            migrationBuilder.RenameColumn(
                name: "two_factor_enabled",
                table: "SystemUsers",
                newName: "twofactorenabled");

            migrationBuilder.RenameColumn(
                name: "security_stamp",
                table: "SystemUsers",
                newName: "securitystamp");

            migrationBuilder.RenameColumn(
                name: "password_hash",
                table: "SystemUsers",
                newName: "passwordhash");

            migrationBuilder.RenameColumn(
                name: "normalized_user_name",
                table: "SystemUsers",
                newName: "normalizedusername");

            migrationBuilder.RenameColumn(
                name: "normalized_email",
                table: "SystemUsers",
                newName: "normalizedemail");

            migrationBuilder.RenameColumn(
                name: "lockout_end",
                table: "SystemUsers",
                newName: "lockoutend");

            migrationBuilder.RenameColumn(
                name: "lockout_enabled",
                table: "SystemUsers",
                newName: "lockoutenabled");

            migrationBuilder.RenameColumn(
                name: "email_confirmed",
                table: "SystemUsers",
                newName: "emailconfirmed");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "SystemUsers",
                newName: "concurrencystamp");

            migrationBuilder.RenameColumn(
                name: "access_failed_count",
                table: "SystemUsers",
                newName: "accessfailedcount");

            migrationBuilder.RenameIndex(
                name: "idx_users_normalized_user_name",
                table: "SystemUsers",
                newName: "UserNameIndex");

            migrationBuilder.RenameIndex(
                name: "idx_users_normalized_email",
                table: "SystemUsers",
                newName: "EmailIndex");

            migrationBuilder.RenameColumn(
                name: "login_provider",
                table: "SystemUserTokens",
                newName: "loginprovider");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "SystemUserTokens",
                newName: "userid");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "SystemUserRoles",
                newName: "roleid");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "SystemUserRoles",
                newName: "userid");

            migrationBuilder.RenameIndex(
                name: "ix_system_user_roles_role_id",
                table: "SystemUserRoles",
                newName: "ix_systemuserroles_roleid");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "SystemUserLogins",
                newName: "userid");

            migrationBuilder.RenameColumn(
                name: "provider_display_name",
                table: "SystemUserLogins",
                newName: "providerdisplayname");

            migrationBuilder.RenameColumn(
                name: "provider_key",
                table: "SystemUserLogins",
                newName: "providerkey");

            migrationBuilder.RenameColumn(
                name: "login_provider",
                table: "SystemUserLogins",
                newName: "loginprovider");

            migrationBuilder.RenameIndex(
                name: "ix_system_user_logins_user_id",
                table: "SystemUserLogins",
                newName: "ix_systemuserlogins_userid");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "SystemUserClaims",
                newName: "userid");

            migrationBuilder.RenameColumn(
                name: "claim_value",
                table: "SystemUserClaims",
                newName: "claimvalue");

            migrationBuilder.RenameColumn(
                name: "claim_type",
                table: "SystemUserClaims",
                newName: "claimtype");

            migrationBuilder.RenameIndex(
                name: "ix_system_user_claims_user_id",
                table: "SystemUserClaims",
                newName: "ix_systemuserclaims_userid");

            migrationBuilder.RenameColumn(
                name: "normalized_name",
                table: "SystemRoles",
                newName: "normalizedname");

            migrationBuilder.RenameColumn(
                name: "concurrency_stamp",
                table: "SystemRoles",
                newName: "concurrencystamp");

            migrationBuilder.RenameIndex(
                name: "index_role_normalized_name",
                table: "SystemRoles",
                newName: "RoleNameIndex");

            migrationBuilder.RenameColumn(
                name: "role_id",
                table: "SystemRoleClaims",
                newName: "roleid");

            migrationBuilder.RenameColumn(
                name: "claim_value",
                table: "SystemRoleClaims",
                newName: "claimvalue");

            migrationBuilder.RenameColumn(
                name: "claim_type",
                table: "SystemRoleClaims",
                newName: "claimtype");

            migrationBuilder.RenameIndex(
                name: "ix_system_role_claims_role_id",
                table: "SystemRoleClaims",
                newName: "ix_systemroleclaims_roleid");

            migrationBuilder.RenameColumn(
                name: "season_index",
                table: "moveset",
                newName: "seasonindex");

            migrationBuilder.RenameColumn(
                name: "previous_set_id",
                table: "moveset",
                newName: "previoussetid");

            migrationBuilder.RenameColumn(
                name: "pre_retreat_hash",
                table: "moveset",
                newName: "preretreathash");

            migrationBuilder.RenameColumn(
                name: "last_seen",
                table: "moveset",
                newName: "lastseen");

            migrationBuilder.RenameColumn(
                name: "game_id",
                table: "moveset",
                newName: "gameid");

            migrationBuilder.RenameColumn(
                name: "full_hash",
                table: "moveset",
                newName: "fullhash");

            migrationBuilder.RenameColumn(
                name: "first_seen",
                table: "moveset",
                newName: "firstseen");

            migrationBuilder.RenameIndex(
                name: "ix_move_sets_previous_set_id",
                table: "moveset",
                newName: "ix_moveset_previoussetid");

            migrationBuilder.RenameIndex(
                name: "ix_move_sets_game_id",
                table: "moveset",
                newName: "ix_moveset_gameid");

            migrationBuilder.AddPrimaryKey(
                name: "pk_unitorders",
                table: "unitorders",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_systemusers",
                table: "SystemUsers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_systemusertokens",
                table: "SystemUserTokens",
                columns: new[] { "userid", "loginprovider", "name" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_systemuserroles",
                table: "SystemUserRoles",
                columns: new[] { "userid", "roleid" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_systemuserlogins",
                table: "SystemUserLogins",
                columns: new[] { "loginprovider", "providerkey" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_systemuserclaims",
                table: "SystemUserClaims",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_systemroles",
                table: "SystemRoles",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_systemroleclaims",
                table: "SystemRoleClaims",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_moveset",
                table: "moveset",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_game",
                table: "game",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_moveset_game_gameid",
                table: "moveset",
                column: "gameid",
                principalTable: "game",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_moveset_moveset_previoussetid",
                table: "moveset",
                column: "previoussetid",
                principalTable: "moveset",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_systemroleclaims_systemroles_roleid",
                table: "SystemRoleClaims",
                column: "roleid",
                principalTable: "SystemRoles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_systemuserclaims_systemusers_userid",
                table: "SystemUserClaims",
                column: "userid",
                principalTable: "SystemUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_systemuserlogins_systemusers_userid",
                table: "SystemUserLogins",
                column: "userid",
                principalTable: "SystemUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_systemuserroles_systemroles_roleid",
                table: "SystemUserRoles",
                column: "roleid",
                principalTable: "SystemRoles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_systemuserroles_systemusers_userid",
                table: "SystemUserRoles",
                column: "userid",
                principalTable: "SystemUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_systemusertokens_systemusers_userid",
                table: "SystemUserTokens",
                column: "userid",
                principalTable: "SystemUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_unitorders_moveset_movesetid",
                table: "unitorders",
                column: "movesetid",
                principalTable: "moveset",
                principalColumn: "id");
        }
    }
}
