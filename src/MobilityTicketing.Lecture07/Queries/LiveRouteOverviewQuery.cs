using Microsoft.EntityFrameworkCore;
using MobilityTicketing.Lecture07.Contracts;
using MobilityTicketing.Lecture07.Data;

namespace MobilityTicketing.Lecture07.Queries;

public static class LiveRouteOverviewQuery
{
    public static Task<List<RouteOverviewDto>> ExecuteAsync(
        LectureDbContext db, TimeWindow window, CancellationToken ct = default)
    {
        window.Validate();
        var from = window.FromUtc;
        var to = window.ToUtc;

        return db.Routes
            .AsNoTracking()
            .OrderBy(route => route.Id)
            .Select(route => new RouteOverviewDto(
                route.Id,
                route.Name,
                route.Trips.Count(trip =>
                    trip.Status == "Scheduled" &&
                    trip.DepartureUtc >= from && trip.DepartureUtc < to),
                route.Trips
                    .Where(trip => trip.Status == "Scheduled" &&
                        trip.DepartureUtc >= from && trip.DepartureUtc < to)
                    .Min(trip => (DateTime?)trip.DepartureUtc)))
            .ToListAsync(ct);
    }
}
