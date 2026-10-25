from pathlib import Path
import json
import re

directory = Path("docs/evidence/lecture07")
samples = (
    ("4 routes / 9 trips", "Baseline", "small-baseline"),
    ("4 routes / 9 trips", "Live board", "small-live"),
    ("200 routes / 100,000 trips", "Baseline", "large-baseline"),
    ("200 routes / 100,000 trips", "Live board", "large-live"),
)

lines = [
    "# Measured query results",
    "",
    "These counts were captured from actual PostgreSQL reads in the Docker workflow.",
    "",
    "| Data size | Strategy | Reads | Route entities | Trip entities | SQL |",
    "| --- | --- | ---: | ---: | ---: | --- |",
]

for size, strategy, name in samples:
    output = (directory / "logs" / (name + ".txt")).read_text()
    start = output.index("{")
    result, _ = json.JSONDecoder().raw_decode(output[start:])
    match = re.search(r"SQL: docs/evidence/lecture07/logs/([^\s]+\.sql)", output)
    if not match:
        raise ValueError("No SQL log was printed for " + name)
    sql_path = "logs/" + match.group(1)
    lines.append(
        f"| {size} | {strategy} | {result['ExecutedReadCommands']} | "
        f"{result['MaterializedRoutes']} | {result['MaterializedTrips']} | "
        f"[SQL]({sql_path}) |"
    )

lines += [
    "",
    "Both projected live-board reads use one database command without materializing Route or Trip entities.",
    "The baseline loads source entities and triggers an additional read for each route.",
    "",
    "The matching JSON files in logs contain the returned rows and recorded SQL parameters.",
]
(directory / "measurements.md").write_text("\n".join(lines) + "\n")
print("\n".join(lines))
