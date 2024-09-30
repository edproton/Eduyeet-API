namespace Application.Errors;

public static class General
{
    public static Error UnexpectedError => Error.Unexpected(
        code: "General.UnexpectedError",
        description: "An unexpected error occurred.");
}

public static class Student
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Student.NotFound",
        description: $"A student with the ID '{id}' was not found.");

    public static Error DuplicateEmail => Error.Conflict(
        code: "Student.DuplicateEmail",
        description: "A student with this email already exists.");
}

public static class Tutor
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Tutor.NotFound",
        description: $"A tutor with the ID '{id}' was not found.");

    public static Error DuplicateEmail => Error.Conflict(
        code: "Tutor.DuplicateEmail",
        description: "A tutor with this email already exists.");
    
    public static Error NoQualifications => Error.Validation(
        "NoQualifications",
        "The tutor must have at least one qualification before setting availability.");
}

public static class Qualification
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Qualification.NotFound",
        description: $"A qualification with the ID '{id}' was not found.");

    public static Error DuplicateName => Error.Conflict(
        code: "Qualification.DuplicateName",
        description: "A qualification with this name already exists.");
}

public static class Booking
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Booking.NotFound",
        description: $"A booking with the ID '{id}' was not found.");

    public static Error QualificationNotAvailable => Error.Validation(
        code: "Booking.QualificationNotAvailable",
        description: "The tutor does not offer this qualification.");

    public static Error StudentNotInterestedInQualification => Error.Validation(
        code: "Booking.StudentNotInterestedInQualification",
        description: "The student is not interested in this qualification.");

    public static Error TutorNotAvailable => Error.Validation(
        code: "Booking.TutorNotAvailable",
        description: "The tutor is not available at the requested time.");

    public static Error OverlappingBooking => Error.Conflict(
        code: "Booking.OverlappingBooking",
        description: "The tutor already has a booking at this time.");

    public static Error InvalidStartTime => Error.Validation(
        code: "Booking.InvalidStartTime",
        description: "Booking must start on the hour or half-hour.");

    public static Error PastStartTime => Error.Validation(
        code: "Booking.PastStartTime",
        description: "Booking start time must be in the future.");
}

public static class Availability
{
    public static Error NotFound(Guid id) => Error.NotFound(
        code: "Availability.NotFound",
        description: $"An availability with the ID '{id}' was not found.");

    public static Error InvalidTimeSlot => Error.Validation(
        code: "Availability.InvalidTimeSlot",
        description: "The provided time slot is invalid.");

    public static Error OverlappingTimeSlot => Error.Conflict(
        code: "Availability.OverlappingTimeSlot",
        description: "The provided time slot overlaps with an existing one.");
}

public static class TimeSlot
{
    public static Error InvalidStartTime => Error.Validation(
        code: "TimeSlot.InvalidStartTime",
        description: "Start time must be earlier than end time.");

    public static Error InvalidDuration => Error.Validation(
        code: "TimeSlot.InvalidDuration",
        description: "Time slot duration must be at least 30 minutes.");
}