using Microsoft.EntityFrameworkCore;
using MobilityTicketing.Lecture07.Contracts;
using MobilityTicketing.Lecture07.Data;

namespace MobilityTicketing.Lecture07.Queries;

public static class PublishedSnapshotQuery
{
    public static async Task<PublishedSnapshot> ExecuteAsync(
        LectureDbContext db, string id, CancellationToken ct = default)
    {
        DatabaseSession.RequireText(id, nameof(id));
        var plan = await db.PublishedPlans
            .AsNoTracking()
            .Include(p => p.Routes)
            .AsSingleQuery()
            .SingleOrDefaultAsync(p => p.Id == id, ct);

        if (plan is null) throw new SnapshotNotFoundException(id);

        var rows = plan.Routes
            .OrderBy(row => row.RouteId)
            .Select(row => new RouteOverviewDto(
                row.RouteId, row.RouteName, row.ScheduledTripCount, row.NextDepartureUtc))
            .ToList();

        return new PublishedSnapshot(plan.Id,
            new TimeWindow(plan.FromUtc, plan.ToUtc), plan.PublishedAtUtc, rows);
    }
}
