# Query measurements

The CLI writes its measurements and generated SQL into the logs directory. Run both strategies against the original fixture, then reset and load the larger fixture before repeating the comparison.

\`\`\`sh
docker compose run --rm app reset
docker compose run --rm app baseline
docker compose run --rm app overview

docker compose run --rm app reset
docker compose run --rm app scale
docker compose run --rm app baseline
docker compose run --rm app overview
\`\`\`

The baseline loads routes and uses lazy loading for trips. With four routes it executes five reads; with 200 routes it executes 201. The projected board executes one read, with no Route or Trip objects materialized. These are the expected properties of the queries, not independently captured measurements. The workflow runs the comparison and uploads the SQL files as an artifact. Actual counts should be copied from those runs rather than estimated.

The small fixture should produce City Loop with two trips at 06:00 and Campus Line with two at 06:10. After publishing morning, cancelling T102 and renaming R1 to City Ring, the live board should show City Ring with one trip at 06:20. The original publication should still show City Loop with two at 06:00. A fresh booking of T102 must be rejected.

The additional tests in StudentTests.cs cover name normalization, conflicting booking requests, publication rollback, simultaneous independent plans, reads from saved tables, booking after cancellation and empty departure windows. The acceptance suite also checks the precision of window bounds and the effect of retries.
