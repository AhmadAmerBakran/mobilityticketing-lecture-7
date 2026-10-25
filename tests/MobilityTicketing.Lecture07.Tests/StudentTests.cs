using Microsoft.EntityFrameworkCore;
using MobilityTicketing.Lecture07.Contracts;
using MobilityTicketing.Lecture07.Diagnostics;
using Xunit;

namespace MobilityTicketing.Lecture07.Tests;

[Trait("Category", "Student")]
public class StudentTests : Fixture
{
    [Fact]
    public async Task Route_names_are_trimmed_without_changing_an_earlier_publication()
    {
        await App.PublishSnapshotAsync("original", TimeWindow.Morning);

        Assert.Equal(RenameResult.Renamed, await App.RenameRouteAsync("R1", "  City Ring  "));

        var live = await App.ReadOverviewAsync(TimeWindow.Morning);
        var published = await App.ReadSnapshotAsync("original");
        Assert.Equal("City Ring", live[0].RouteName);
        Assert.Equal("City Loop", published.Routes[0].RouteName);
    }

    [Fact]
    public async Task Competing_requests_with_the_same_id_cannot_book_two_trips()
    {
        var results = await Task.WhenAll(
            App.BookTripAsync("T102", "shared-id"),
            App.BookTripAsync("T201", "shared-id"));

        Assert.Equal(1, results.Count(result => result.Status == BookingStatus.Booked));
        Assert.Equal(1, results.Count(result => result.Status == BookingStatus.RequestConflict));

        await using var db = Db();
        Assert.Equal(1, await db.Bookings.CountAsync());
    }

    [Fact]
    public async Task Failed_publication_rolls_back_the_header_and_all_routes()
    {
        var before = await State();
        var failing = new StudentApplication(Connection, Clock, new ApplicationHooks
        {
            BeforePublishCommit = () => throw new InvalidOperationException("commit interrupted")
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            failing.PublishSnapshotAsync("incomplete", TimeWindow.Morning));

        Assert.Equal(before, await State());
        await Assert.ThrowsAsync<SnapshotNotFoundException>(() =>
            App.ReadSnapshotAsync("incomplete"));
    }

    [Fact]
    public async Task Two_plans_published_concurrently_keep_their_own_windows()
    {
        var laterWindow = new TimeWindow(Utc(7, 0), Utc(8, 0));

        var results = await Task.WhenAll(
            App.PublishSnapshotAsync("morning", TimeWindow.Morning),
            App.PublishSnapshotAsync("later", laterWindow));

        Assert.All(results, result => Assert.Equal(PublishResult.Published, result));

        var morning = await App.ReadSnapshotAsync("morning");
        var later = await App.ReadSnapshotAsync("later");
        Assert.Equal(TimeWindow.Morning, morning.Window);
        Assert.Equal(laterWindow, later.Window);
        AssertRows(Baseline, morning.Routes);
        Assert.Equal(1, later.Routes[0].ScheduledTripCount);
        Assert.Equal(Utc(7, 0), later.Routes[0].NextDepartureUtc);
        Assert.Equal(0, later.Routes[1].ScheduledTripCount);
        Assert.Equal(4, later.Routes.Count);
    }

    [Fact]
    public async Task Published_read_does_not_query_live_routes_or_trips()
    {
        await App.PublishSnapshotAsync("saved", TimeWindow.Morning);

        var measurement = await RunLog.ExecuteAsync(Connection, "snapshot", snapshotId: "saved");

        Assert.Equal(1, measurement.ExecutedReadCommands);
        Assert.Equal(0, measurement.MaterializedRoutes);
        Assert.Equal(0, measurement.MaterializedTrips);
        Assert.Single(measurement.Commands);
        Assert.Contains("published_plans", measurement.Commands[0].Sql);
        Assert.DoesNotContain("lab07.trips", measurement.Commands[0].Sql);
    }

    [Fact]
    public async Task A_fresh_request_after_cancellation_cannot_reuse_an_old_receipt()
    {
        var original = await App.BookTripAsync("T102", "original");
        Assert.Equal(BookingStatus.Booked, original.Status);

        await App.CancelTripAsync("T102");

        var rejected = await App.BookTripAsync("T102", "new-request");
        var repeated = await App.BookTripAsync("T102", "original");
        Assert.Equal(BookingStatus.Cancelled, rejected.Status);
        Assert.Null(rejected.Receipt);
        Assert.Equal(BookingStatus.AlreadyBooked, repeated.Status);
        Assert.Equal(original.Receipt, repeated.Receipt);
    }

    [Fact]
    public async Task A_route_with_no_matching_departures_stays_on_the_board()
    {
        var window = new TimeWindow(Utc(6, 0).AddDays(2), Utc(7, 0).AddDays(2));

        var rows = await App.ReadOverviewAsync(window);

        Assert.Equal(4, rows.Count);
        Assert.All(rows, row =>
        {
            Assert.Equal(0, row.ScheduledTripCount);
            Assert.Null(row.NextDepartureUtc);
        });
    }
}
