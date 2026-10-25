# Read and write design

The source data lives in lab07.routes, lab07.trips and lab07.bookings. Published plans have a separate header table and one detail row per displayed route. A published detail stores the route name, trip count and next departure rather than referring back to values that can change.

\`\`\`text
routes + trips -> LiveRouteOverviewQuery -> departure board
                              |
                              v
                       PublishSnapshotCommand -> published_plans
                                              -> published_plan_routes

bookings <- BookTripCommand -> locked trip
trips    <- CancelTripCommand -> locked trip
routes   <- RenameRouteCommand
published_plans + published_plan_routes -> PublishedSnapshotQuery
\`\`\`

## Decision

I considered maintaining a current route summary table and calculating the board from the source tables on each request. A summary table could make frequent reads inexpensive, but cancellations, route renames and changes to the requested time window would require extra update logic. I chose direct projection instead. EF Core calculates the scheduled trip count and earliest departure in the database, including routes without departures, without loading full Route and Trip objects. The query uses one database read and always reflects committed changes.

The published board has a different purpose. It needs to retain what dispatch actually published, so its rows are copied into separate tables. Publishing the header and details happens in one transaction. A per-plan advisory lock serializes retries and protects the plan ID. Booking takes a per-request lock, checks for an existing receipt, then locks the current trip row. Cancellation locks the same trip row. These boundaries prevent a stale display or simultaneous requests from bypassing the current trip status.
