namespace MobilityTicketing.Lecture07.Data;

public class PublishedPlan
{
    public string Id { get; set; } = "";
    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }
    public DateTime PublishedAtUtc { get; set; }
    public virtual List<PublishedRoute> Routes { get; set; } = [];
}

public class PublishedRoute
{
    public string PlanId { get; set; } = "";
    public string RouteId { get; set; } = "";
    public string RouteName { get; set; } = "";
    public int ScheduledTripCount { get; set; }
    public DateTime? NextDepartureUtc { get; set; }
}
