# Measured query results

These counts were captured from actual PostgreSQL reads in the Docker workflow.

| Data size | Strategy | Reads | Route entities | Trip entities | SQL |
| --- | --- | ---: | ---: | ---: | --- |
| 4 routes / 9 trips | Baseline | 5 | 4 | 9 | [SQL](logs/20261025T1127162424373Z-baseline-4b9febf8.sql) |
| 4 routes / 9 trips | Live board | 1 | 0 | 0 | [SQL](logs/20261025T1127176990021Z-live-e366f257.sql) |
| 200 routes / 100,000 trips | Baseline | 201 | 200 | 100000 | [SQL](logs/20261025T1127249305144Z-baseline-b0215e5e.sql) |
| 200 routes / 100,000 trips | Live board | 1 | 0 | 0 | [SQL](logs/20261025T1127263956889Z-live-3c8f07f2.sql) |

Both projected live-board reads use one database command without materializing Route or Trip entities.
The baseline loads source entities and triggers an additional read for each route.

The matching JSON files in logs contain the returned rows and recorded SQL parameters.
