using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddingGetME6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Qualifications_Subjects_QualificationId",
                table: "Qualifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_LearningSystems_LearningSystemId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_LearningSystemId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Qualifications_QualificationId",
                table: "Qualifications");

            migrationBuilder.DropColumn(
                name: "LearningSystemId",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "QualificationId",
                table: "Qualifications");

            migrationBuilder.DropColumn(
                name: "AvailableQualificationsIds",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "InterestedQualificationsIds",
                table: "Persons");

            migrationBuilder.AddForeignKey(
                name: "FK_Qualifications_Subjects_Id",
                table: "Qualifications",
                column: "Id",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_LearningSystems_Id",
                table: "Subjects",
                column: "Id",
                principalTable: "LearningSystems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Qualifications_Subjects_Id",
                table: "Qualifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_LearningSystems_Id",
                table: "Subjects");

            migrationBuilder.AddColumn<Guid>(
                name: "LearningSystemId",
                table: "Subjects",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "QualificationId",
                table: "Qualifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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
                name: "IX_Subjects_LearningSystemId",
                table: "Subjects",
                column: "LearningSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_Qualifications_QualificationId",
                table: "Qualifications",
                column: "QualificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Qualifications_Subjects_QualificationId",
                table: "Qualifications",
                column: "QualificationId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_LearningSystems_LearningSystemId",
                table: "Subjects",
                column: "LearningSystemId",
                principalTable: "LearningSystems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
