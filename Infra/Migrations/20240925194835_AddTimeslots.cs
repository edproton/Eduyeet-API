using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeslots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Subjects_SubjectId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Qualifications_Subjects_SubjectId",
                table: "Qualifications");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Availabilities");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Availabilities");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "Qualifications",
                newName: "QualificationId");

            migrationBuilder.RenameIndex(
                name: "IX_Qualifications_SubjectId",
                table: "Qualifications",
                newName: "IX_Qualifications_QualificationId");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "Bookings",
                newName: "QualificationId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_SubjectId",
                table: "Bookings",
                newName: "IX_Bookings_QualificationId");

            migrationBuilder.CreateTable(
                name: "TimeSlot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    AvailabilityId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSlot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeSlot_Availabilities_AvailabilityId",
                        column: x => x.AvailabilityId,
                        principalTable: "Availabilities",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlot_AvailabilityId",
                table: "TimeSlot",
                column: "AvailabilityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Qualifications_QualificationId",
                table: "Bookings",
                column: "QualificationId",
                principalTable: "Qualifications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Qualifications_Subjects_QualificationId",
                table: "Qualifications",
                column: "QualificationId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Qualifications_QualificationId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Qualifications_Subjects_QualificationId",
                table: "Qualifications");

            migrationBuilder.DropTable(
                name: "TimeSlot");

            migrationBuilder.RenameColumn(
                name: "QualificationId",
                table: "Qualifications",
                newName: "SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Qualifications_QualificationId",
                table: "Qualifications",
                newName: "IX_Qualifications_SubjectId");

            migrationBuilder.RenameColumn(
                name: "QualificationId",
                table: "Bookings",
                newName: "SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_QualificationId",
                table: "Bookings",
                newName: "IX_Bookings_SubjectId");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "Availabilities",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "Availabilities",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Subjects_SubjectId",
                table: "Bookings",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Qualifications_Subjects_SubjectId",
                table: "Qualifications",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
