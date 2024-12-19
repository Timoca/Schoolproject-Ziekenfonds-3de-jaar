using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Groepsreizen_team_tet.Migrations
{
    /// <inheritdoc />
    public partial class MIG0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Activiteiten",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Beschrijving = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activiteiten", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Voornaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Straat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Huisnummer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gemeente = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Postcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Geboortedatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Huisdokter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContractNummer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RekeningNummer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActief = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bestemmingen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Naam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Beschrijving = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinLeeftijd = table.Column<int>(type: "int", nullable: false),
                    MaxLeeftijd = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bestemmingen", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Kinderen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersoonId = table.Column<int>(type: "int", nullable: false),
                    Naam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Voornaam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Geboortedatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Allergieën = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Medicatie = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kinderen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kinderen_AspNetUsers_PersoonId",
                        column: x => x.PersoonId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Afbeelding = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    BestemmingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fotos_Bestemmingen_BestemmingId",
                        column: x => x.BestemmingId,
                        principalTable: "Bestemmingen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Groepsreizen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Begindatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Einddatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Prijs = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OnkostenIngegeven = table.Column<bool>(type: "bit", nullable: false),
                    Deelnemerslimiet = table.Column<int>(type: "int", nullable: false),
                    IsGeannuleerd = table.Column<bool>(type: "bit", nullable: false),
                    RedenAnnulatie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BestemmingId = table.Column<int>(type: "int", nullable: false),
                    IsKopie = table.Column<bool>(type: "bit", nullable: false),
                    OrigineleGroepsreisId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groepsreizen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groepsreizen_Bestemmingen_BestemmingId",
                        column: x => x.BestemmingId,
                        principalTable: "Bestemmingen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Groepsreizen_Groepsreizen_OrigineleGroepsreisId",
                        column: x => x.OrigineleGroepsreisId,
                        principalTable: "Groepsreizen",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Deelnemers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KindId = table.Column<int>(type: "int", nullable: false),
                    GroepsreisDetailsId = table.Column<int>(type: "int", nullable: false),
                    Opmerkingen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewScore = table.Column<int>(type: "int", nullable: true),
                    Review = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InschrijvingDatum = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deelnemers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deelnemers_Groepsreizen_GroepsreisDetailsId",
                        column: x => x.GroepsreisDetailsId,
                        principalTable: "Groepsreizen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Deelnemers_Kinderen_KindId",
                        column: x => x.KindId,
                        principalTable: "Kinderen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Monitoren",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersoonId = table.Column<int>(type: "int", nullable: false),
                    GroepsreisDetailsId = table.Column<int>(type: "int", nullable: true),
                    IsHoofdMonitor = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Monitoren", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Monitoren_AspNetUsers_PersoonId",
                        column: x => x.PersoonId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Monitoren_Groepsreizen_GroepsreisDetailsId",
                        column: x => x.GroepsreisDetailsId,
                        principalTable: "Groepsreizen",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OnkostenLijst",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroepsreisId = table.Column<int>(type: "int", nullable: false),
                    Titel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Omschrijving = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Locatie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bedrag = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Datum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Betaalbewijs = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    IngegevenDoorHoofdmonitor = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnkostenLijst", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnkostenLijst_Groepsreizen_GroepsreisId",
                        column: x => x.GroepsreisId,
                        principalTable: "Groepsreizen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Programmas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActiviteitId = table.Column<int>(type: "int", nullable: false),
                    GroepsreisId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Programmas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Programmas_Activiteiten_ActiviteitId",
                        column: x => x.ActiviteitId,
                        principalTable: "Activiteiten",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Programmas_Groepsreizen_GroepsreisId",
                        column: x => x.GroepsreisId,
                        principalTable: "Groepsreizen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Wachtlijst",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroepsreisId = table.Column<int>(type: "int", nullable: false),
                    KindId = table.Column<int>(type: "int", nullable: false),
                    InschrijvingDatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Opmerkingen = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wachtlijst", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wachtlijst_Groepsreizen_GroepsreisId",
                        column: x => x.GroepsreisId,
                        principalTable: "Groepsreizen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wachtlijst_Kinderen_KindId",
                        column: x => x.KindId,
                        principalTable: "Kinderen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Opleidingen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Beschrijving = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Begindatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Einddatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AantalPlaatsen = table.Column<int>(type: "int", nullable: false),
                    Locatie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpleidingVereistId = table.Column<int>(type: "int", nullable: true),
                    FotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Afbeelding = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    MonitorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opleidingen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Opleidingen_Monitoren_MonitorId",
                        column: x => x.MonitorId,
                        principalTable: "Monitoren",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Opleidingen_Opleidingen_OpleidingVereistId",
                        column: x => x.OpleidingVereistId,
                        principalTable: "Opleidingen",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OpleidingPersoon",
                columns: table => new
                {
                    OpleidingenId = table.Column<int>(type: "int", nullable: false),
                    PersonenId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpleidingPersoon", x => new { x.OpleidingenId, x.PersonenId });
                    table.ForeignKey(
                        name: "FK_OpleidingPersoon_AspNetUsers_PersonenId",
                        column: x => x.PersonenId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OpleidingPersoon_Opleidingen_OpleidingenId",
                        column: x => x.OpleidingenId,
                        principalTable: "Opleidingen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Deelnemers_GroepsreisDetailsId",
                table: "Deelnemers",
                column: "GroepsreisDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Deelnemers_KindId",
                table: "Deelnemers",
                column: "KindId");

            migrationBuilder.CreateIndex(
                name: "IX_Fotos_BestemmingId",
                table: "Fotos",
                column: "BestemmingId");

            migrationBuilder.CreateIndex(
                name: "IX_Groepsreizen_BestemmingId",
                table: "Groepsreizen",
                column: "BestemmingId");

            migrationBuilder.CreateIndex(
                name: "IX_Groepsreizen_OrigineleGroepsreisId",
                table: "Groepsreizen",
                column: "OrigineleGroepsreisId");

            migrationBuilder.CreateIndex(
                name: "IX_Kinderen_PersoonId",
                table: "Kinderen",
                column: "PersoonId");

            migrationBuilder.CreateIndex(
                name: "IX_Monitoren_GroepsreisDetailsId",
                table: "Monitoren",
                column: "GroepsreisDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Monitoren_PersoonId",
                table: "Monitoren",
                column: "PersoonId");

            migrationBuilder.CreateIndex(
                name: "IX_Monitoren_PersoonId_GroepsreisDetailsId",
                table: "Monitoren",
                columns: new[] { "PersoonId", "GroepsreisDetailsId" },
                unique: true,
                filter: "[GroepsreisDetailsId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OnkostenLijst_GroepsreisId",
                table: "OnkostenLijst",
                column: "GroepsreisId");

            migrationBuilder.CreateIndex(
                name: "IX_Opleidingen_MonitorId",
                table: "Opleidingen",
                column: "MonitorId");

            migrationBuilder.CreateIndex(
                name: "IX_Opleidingen_OpleidingVereistId",
                table: "Opleidingen",
                column: "OpleidingVereistId");

            migrationBuilder.CreateIndex(
                name: "IX_OpleidingPersoon_PersonenId",
                table: "OpleidingPersoon",
                column: "PersonenId");

            migrationBuilder.CreateIndex(
                name: "IX_Programmas_ActiviteitId",
                table: "Programmas",
                column: "ActiviteitId");

            migrationBuilder.CreateIndex(
                name: "IX_Programmas_GroepsreisId",
                table: "Programmas",
                column: "GroepsreisId");

            migrationBuilder.CreateIndex(
                name: "IX_Wachtlijst_GroepsreisId",
                table: "Wachtlijst",
                column: "GroepsreisId");

            migrationBuilder.CreateIndex(
                name: "IX_Wachtlijst_KindId",
                table: "Wachtlijst",
                column: "KindId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Deelnemers");

            migrationBuilder.DropTable(
                name: "Fotos");

            migrationBuilder.DropTable(
                name: "OnkostenLijst");

            migrationBuilder.DropTable(
                name: "OpleidingPersoon");

            migrationBuilder.DropTable(
                name: "Programmas");

            migrationBuilder.DropTable(
                name: "Wachtlijst");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Opleidingen");

            migrationBuilder.DropTable(
                name: "Activiteiten");

            migrationBuilder.DropTable(
                name: "Kinderen");

            migrationBuilder.DropTable(
                name: "Monitoren");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Groepsreizen");

            migrationBuilder.DropTable(
                name: "Bestemmingen");
        }
    }
}
