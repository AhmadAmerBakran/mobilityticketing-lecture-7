# MobilityTicketing lecture 7

This lab implements a live route board, booking commands and published dispatch plans in .NET 8 with EF Core and PostgreSQL 17. The board reads current route and trip data. Published plans keep a copy of every value shown at publication time, so later cancellations and renames do not alter them.

The application contract is in [docs/contract.md](docs/contract.md). The data access decision and transaction boundaries are described in [docs/design.md](docs/design.md).

## Running the lab

Docker Desktop with Compose is required. From the repository directory, start PostgreSQL and build the application.

```sh
docker compose up -d --wait postgres
docker compose build app tests
docker compose run --rm app reset
docker compose run --rm app overview
```

The fixture uses 23 October 2026 and the default window 06:00 to 07:00 UTC. It contains four routes and nine trips. The following commands demonstrate booking retries and the difference between a live board and a saved plan.

```sh
docker compose run --rm app publish morning
docker compose run --rm app book T201 campus-1
docker compose run --rm app book T201 campus-1
docker compose run --rm app cancel T102
docker compose run --rm app rename R1 "City Ring"
docker compose run --rm app overview
docker compose run --rm app snapshot morning
docker compose run --rm app publish revised
docker compose run --rm app snapshot revised
```

The repeated booking returns its original receipt and does not create another row. The current board shows City Ring with one trip departing at 06:20. The morning plan still shows City Loop with two trips and a 06:00 departure. A new booking for cancelled T102 is rejected.

A plan can be published for another window by supplying both timestamps:

```sh
docker compose run --rm app publish later 2026-10-23T07:00:00Z 2026-10-23T08:00:00Z
```

Retrying a publication with the same ID and window keeps the first copy. Reusing that ID with different window boundaries produces an error.

## Tests and SQL

```sh
docker compose run --rm tests --filter Category=Starter
docker compose run --rm tests --filter Category=Acceptance
docker compose run --rm tests --filter Category=Student
```

Each test resets the separate mobility_l07_test database. After code or schema changes, rebuild the Docker images. The measured baseline and live query can be compared on the large fixture:

```sh
docker compose run --rm app reset
docker compose run --rm app scale
docker compose run --rm app baseline
docker compose run --rm app overview
```

The application writes SQL and result logs to docs/evidence/lecture07/logs. The [measurement results](docs/evidence/lecture07/measurements.md) record the actual read counts, loaded entities and SQL for both fixture sizes. The GitHub workflow also runs the test suites and saves those logs in the repository.

Stop the environment with docker compose stop.
