using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddingGetME : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Qualifications_Persons_StudentId",
                table: "Qualifications",
                column: "StudentId",
                principalTable: "Persons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Qualifications_Persons_StudentId",
                table: "Qualifications");

            migrationBuilder.DropIndex(
                name: "IX_Qualifications_StudentId",
                table: "Qualifications");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Qualifications");

            migrationBuilder.DropColumn(
                name: "AvailableQualificationsIds",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "InterestedQualificationsIds",
                table: "Persons");
        }
    }
}
