# Measured query results

These counts were captured from actual PostgreSQL reads in the Docker workflow.

| Data size | Strategy | Reads | Route entities | Trip entities | SQL |
| --- | --- | ---: | ---: | ---: | --- |
| 4 routes / 9 trips | Baseline | 5 | 4 | 9 | [SQL](logs/20261025T1129448000481Z-baseline-d1a9580a.sql) |
| 4 routes / 9 trips | Live board | 1 | 0 | 0 | [SQL](logs/20261025T1129467099942Z-live-1c85f69a.sql) |
| 200 routes / 100,000 trips | Baseline | 201 | 200 | 100000 | [SQL](logs/20261025T1129563607805Z-baseline-f2d82535.sql) |
| 200 routes / 100,000 trips | Live board | 1 | 0 | 0 | [SQL](logs/20261025T1129582966859Z-live-90c22b61.sql) |

Both projected live-board reads use one database command without materializing Route or Trip entities.
The baseline loads source entities and triggers an additional read for each route.

The matching JSON files in logs contain the returned rows and recorded SQL parameters.
