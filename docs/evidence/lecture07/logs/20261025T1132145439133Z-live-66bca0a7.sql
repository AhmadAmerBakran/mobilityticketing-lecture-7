-- Parameters: {"@__from_0":"2026-10-23T06:00:00.0000000Z","@__to_1":"2026-10-23T07:00:00.0000000Z"}
SELECT r.id, r.short_name, (
    SELECT count(*)::int
    FROM lab07.trips AS t
    WHERE r.id = t.route_id AND t.status = 'Scheduled' AND t.scheduled_departure_utc >= @__from_0 AND t.scheduled_departure_utc < @__to_1), (
    SELECT min(t0.scheduled_departure_utc)
    FROM lab07.trips AS t0
    WHERE r.id = t0.route_id AND t0.status = 'Scheduled' AND t0.scheduled_departure_utc >= @__from_0 AND t0.scheduled_departure_utc < @__to_1)
FROM lab07.routes AS r
ORDER BY r.id