using Microsoft.EntityFrameworkCore;
using MobilityTicketing.Lecture07.Contracts;
using MobilityTicketing.Lecture07.Data;
using MobilityTicketing.Lecture07.Queries;

namespace MobilityTicketing.Lecture07.Commands;

public static class PublishSnapshotCommand
{
    public static Task<PublishResult> ExecuteAsync(
        string connection, TimeProvider clock, ApplicationHooks hooks,
        string id, TimeWindow window, CancellationToken ct = default)
    {
        DatabaseSession.RequireText(id, nameof(id));
        window.Validate();

        return DatabaseSession.WriteAsync(connection, "publication:" + id, async db =>
        {
            var existing = await db.PublishedPlans.AsNoTracking()
                .SingleOrDefaultAsync(p => p.Id == id, ct);

            if (existing is not null)
            {
                if (existing.FromUtc != window.FromUtc || existing.ToUtc != window.ToUtc)
                    throw new SnapshotConflictException(id);

                return PublishResult.AlreadyPublished;
            }

            var rows = await LiveRouteOverviewQuery.ExecuteAsync(db, window, ct);
            var timestamp = clock.GetUtcNow().UtcDateTime;
            timestamp = timestamp.AddTicks(-(timestamp.Ticks % TimeSpan.TicksPerMicrosecond));

            db.PublishedPlans.Add(new PublishedPlan
            {
                Id = id,
                FromUtc = window.FromUtc,
                ToUtc = window.ToUtc,
                PublishedAtUtc = timestamp,
                Routes = rows.Select(row => new PublishedRoute
                {
                    PlanId = id,
                    RouteId = row.RouteId,
                    RouteName = row.RouteName,
                    ScheduledTripCount = row.ScheduledTripCount,
                    NextDepartureUtc = row.NextDepartureUtc
                }).ToList()
            });

            return PublishResult.Published;
        }, ct, hooks.BeforePublishCommit);
    }
}
