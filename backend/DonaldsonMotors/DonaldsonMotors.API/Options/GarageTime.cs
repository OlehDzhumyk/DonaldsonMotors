namespace DonaldsonMotors.API.Options
{
    /// <summary>
    /// Working hours and lunch breaks are wall-clock times at the garage (UK time),
    /// while bookings are stored in UTC. This converts between the two, so slots stay
    /// at 09:00 local time through BST and GMT.
    /// </summary>
    public static class GarageTime
    {
        public static readonly TimeZoneInfo Zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");

        /// <summary>The UTC instant of a local wall-clock time on a given garage date.</summary>
        public static DateTime ToUtc(DateTime localDate, TimeOnly localTime) =>
            TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localDate.Date.Add(localTime.ToTimeSpan()), DateTimeKind.Unspecified), Zone);

        public static DateTime ToLocal(DateTime utc) =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zone);
    }
}
