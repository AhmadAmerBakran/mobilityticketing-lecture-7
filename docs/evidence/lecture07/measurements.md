# Measured query results

These counts were captured from actual PostgreSQL reads in the Docker workflow.

| Data size | Strategy | Reads | Route entities | Trip entities | SQL |
| --- | --- | ---: | ---: | ---: | --- |
| 4 routes / 9 trips | Baseline | 5 | 4 | 9 | [SQL](logs/20261025T1132128647441Z-baseline-897593ca.sql) |
| 4 routes / 9 trips | Live board | 1 | 0 | 0 | [SQL](logs/20261025T1132145439133Z-live-66bca0a7.sql) |
| 200 routes / 100,000 trips | Baseline | 201 | 200 | 100000 | [SQL](logs/20261025T1132230562509Z-baseline-002c6ca3.sql) |
| 200 routes / 100,000 trips | Live board | 1 | 0 | 0 | [SQL](logs/20261025T1132247024093Z-live-6eb9e9cb.sql) |

Both projected live-board reads use one database command without materializing Route or Trip entities.
The baseline loads source entities and triggers an additional read for each route.

The matching JSON files in logs contain the returned rows and recorded SQL parameters.
