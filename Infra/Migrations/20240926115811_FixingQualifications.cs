using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Migrations
{
    /// <inheritdoc />
    public partial class FixingQualifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Qualifications_Persons_StudentId",
                table: "Qualifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Qualifications_Persons_TutorId",
                table: "Qualifications");

            migrationBuilder.DropIndex(
                name: "IX_Qualifications_StudentId",
                table: "Qualifications");

            migrationBuilder.DropIndex(
                name: "IX_Qualifications_TutorId",
                table: "Qualifications");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Qualifications");

            migrationBuilder.DropColumn(
                name: "TutorId",
                table: "Qualifications");

            migrationBuilder.DropColumn(
                name: "AvailableQualificationsIds",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "InterestedQualificationsIds",
                table: "Persons");

            migrationBuilder.CreateTable(
                name: "StudentQualifications",
                columns: table => new
                {
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentQualifications", x => new { x.StudentId, x.QualificationId });
                    table.ForeignKey(
                        name: "FK_StudentQualifications_Persons_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentQualifications_Qualifications_QualificationId",
                        column: x => x.QualificationId,
                        principalTable: "Qualifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TutorQualifications",
                columns: table => new
                {
                    TutorId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualificationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorQualifications", x => new { x.TutorId, x.QualificationId });
                    table.ForeignKey(
                        name: "FK_TutorQualifications_Persons_TutorId",
                        column: x => x.TutorId,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TutorQualifications_Qualifications_QualificationId",
                        column: x => x.QualificationId,
                        principalTable: "Qualifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentQualifications_QualificationId",
                table: "StudentQualifications",
                column: "QualificationId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorQualifications_QualificationId",
                table: "TutorQualifications",
                column: "QualificationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentQualifications");

            migrationBuilder.DropTable(
                name: "TutorQualifications");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "Qualifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TutorId",
                table: "Qualifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "AvailableQualificationsIds",
                table: "Persons",
                type: "uuid[]",
                nullable: true);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "InterestedQualificationsIds",
                table: "Persons",
                type: "uuid[]",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Qualifications_StudentId",
                table: "Qualifications",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Qualifications_TutorId",
                table: "Qualifications",
                column: "TutorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Qualifications_Persons_StudentId",
                table: "Qualifications",
                column: "StudentId",
                principalTable: "Persons",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Qualifications_Persons_TutorId",
                table: "Qualifications",
                column: "TutorId",
                principalTable: "Persons",
                principalColumn: "Id");
        }
    }
}
