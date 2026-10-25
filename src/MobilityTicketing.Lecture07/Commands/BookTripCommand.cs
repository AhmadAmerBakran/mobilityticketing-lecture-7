using Microsoft.EntityFrameworkCore;
using MobilityTicketing.Lecture07.Contracts;
using MobilityTicketing.Lecture07.Data;

namespace MobilityTicketing.Lecture07.Commands;

public static class BookTripCommand
{
    public static Task<BookingResult> ExecuteAsync(
        string connection, TimeProvider clock, string tripId, string requestId,
        CancellationToken ct = default)
    {
        DatabaseSession.RequireText(tripId, nameof(tripId));
        DatabaseSession.RequireText(requestId, nameof(requestId));

        return DatabaseSession.WriteAsync(connection, "booking:" + requestId, async db =>
        {
            var saved = await db.Bookings.AsNoTracking()
                .SingleOrDefaultAsync(booking => booking.RequestId == requestId, ct);

            if (saved is not null)
            {
                if (saved.TripId != tripId)
                    return new BookingResult(BookingStatus.RequestConflict);

                return new BookingResult(BookingStatus.AlreadyBooked,
                    new BookingReceipt(saved.RequestId, saved.TripId, saved.BookedAtUtc));
            }

            var trip = await DatabaseSession.LockTripAsync(db, tripId, ct);
            if (trip is null) return new BookingResult(BookingStatus.NotFound);
            if (trip.Status != "Scheduled") return new BookingResult(BookingStatus.Cancelled);

            var timestamp = clock.GetUtcNow().UtcDateTime;
            timestamp = timestamp.AddTicks(-(timestamp.Ticks % TimeSpan.TicksPerMicrosecond));
            db.Bookings.Add(new Booking
            {
                RequestId = requestId,
                TripId = tripId,
                BookedAtUtc = timestamp
            });

            return new BookingResult(BookingStatus.Booked,
                new BookingReceipt(requestId, tripId, timestamp));
        }, ct);
    }
}
