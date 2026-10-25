namespace MobilityTicketing.Lecture07.Data;

public sealed class PublishedPlan
{
    public string Id { get; set; } = "";
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }
    public DateTime PublishedAtUtc { get; set; }
    public List<PublishedRoute> Routes { get; set; } = [];
}

public sealed class PublishedRoute
{
    public string PlanId { get; set; } = "";
    public string RouteId { get; set; } = "";
    public string RouteName { get; set; } = "";
    public int ScheduledTripCount { get; set; }
    public DateTime? NextDepartureUtc { get; set; }
}
