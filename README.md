# RouteForge 🚚

**An intelligent logistics optimization platform built with C# and ASP.NET Core.**

RouteForge is a logistics optimization platform designed to manage warehouses and delivery locations, calculate real-world road distances, and optimize delivery routes.

The platform combines an ASP.NET Core Web API, PostgreSQL database, route optimization algorithms, and an interactive React map to demonstrate how software engineering and algorithmic optimization can improve real-world logistics operations.

The project is being developed with an emphasis on clean architecture, input validation, automated testing, persistent data management, and practical optimization algorithms.

## Current Features

- **Delivery location management:** Create, retrieve, update, and delete delivery locations through a REST API.
- **Warehouse management:** Register warehouses, retrieve warehouse records, and manage them through dedicated API endpoints.
- **PostgreSQL persistence:** Store warehouse and delivery location data using Entity Framework Core.
- **Location classification:** Distinguish between warehouses and delivery locations using a dedicated `LocationType` enumeration.
- **Input validation:** Validate location names and geographic coordinates, rejecting invalid requests.
- **Address autocomplete:** Search for real-world addresses using the Photon geocoding service.
- **Road-distance calculations:** Use OSRM to obtain road-distance data instead of relying solely on straight-line distances.
- **Route optimization:** Implement the Nearest Neighbour algorithm and 2-opt improvement to determine efficient delivery sequences.
- **Custom route planning:** Specify independent starting and ending locations, with delivery stops between them.
- **Route visualization:** Display optimized routes on an interactive map using React, TypeScript, and Leaflet.
- **Road-following geometry:** Visualize routes using road geometry returned by OSRM.
- **Automated testing:** Use xUnit and ASP.NET Core integration testing to verify API behaviour and route optimization functionality.
- **Version control:** Maintain the project using Git and GitHub.

## Technology Stack

| Category | Technology |
|---|---|
| Language | C# |
| Backend framework | ASP.NET Core Web API |
| Platform | .NET 10 |
| Database | PostgreSQL |
| ORM | Entity Framework Core |
| Frontend | React and TypeScript |
| Build tool | Vite |
| Mapping | Leaflet and React-Leaflet |
| Geocoding | Photon |
| Routing engine | OSRM |
| Optimization algorithms | Nearest Neighbour and 2-opt |
| Testing | xUnit and ASP.NET Core integration testing |
| Version control | Git and GitHub |

## API Endpoints

### Delivery Locations

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/locations` | Retrieve all locations |
| GET | `/api/locations/{id}` | Retrieve a location by ID |
| POST | `/api/locations` | Create a delivery location |
| PUT | `/api/locations/{id}` | Update an existing location |
| DELETE | `/api/locations/{id}` | Delete a location |

### Warehouses

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/warehouses` | Retrieve all warehouses |
| POST | `/api/warehouses` | Register a new warehouse |
| PUT | `/api/warehouses/{id}` | Update an existing warehouse |
| DELETE | `/api/warehouses/{id}` | Delete a warehouse |

### Route Optimization and Address Search

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/routes/optimize` | Calculate an optimized delivery route |
| GET | `/api/addresses/search?q={query}` | Search for address suggestions |

## Example Requests

### Create a Delivery Location

Send a `POST` request to `/api/locations` with the following JSON body:

```json
{
  "name": "Pretoria Distribution Centre",
  "latitude": -25.7479,
  "longitude": 28.2293
}
```

A successful request returns `201 Created` and the newly created location, including its assigned ID.

Invalid names or out-of-range coordinates are rejected with `400 Bad Request`.

### Register a Warehouse

Send a `POST` request to `/api/warehouses`:

```json
{
  "name": "Cape Town Distribution Centre",
  "latitude": -33.9249,
  "longitude": 18.4241
}
```

The API creates the warehouse and classifies it as a `Warehouse` using the location type enumeration.

### Optimize a Delivery Route

Send a `POST` request to `/api/routes/optimize` with the following JSON body:

```json
{
  "startName": "Johannesburg Warehouse",
  "startLatitude": -26.2041,
  "startLongitude": 28.0473,
  "endName": "Pretoria Distribution Centre",
  "endLatitude": -25.7479,
  "endLongitude": 28.2293,
  "stops": [
    {
      "name": "Sandton Delivery Hub",
      "latitude": -26.1076,
      "longitude": 28.0567
    },
    {
      "name": "Midrand Delivery Point",
      "latitude": -25.9895,
      "longitude": 28.1284
    }
  ]
}
```

The API:

1. Retrieves road-distance information from OSRM.
2. Calculates an initial delivery sequence using Nearest Neighbour.
3. Improves the sequence using the 2-opt algorithm.
4. Retrieves road-following route geometry.
5. Returns the optimized route, distance metrics, improvement percentages, and map geometry.

The starting point and ending point are specified independently. Delivery stops are reordered without changing the selected start and end locations.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js](https://nodejs.org/)
- [Git](https://git-scm.com/)
- [PostgreSQL](https://www.postgresql.org/)

### Clone the Repository

```bash
git clone https://github.com/LoloM-19/RouteForge.git
cd RouteForge
```

### Configure the Database

Install PostgreSQL and ensure the PostgreSQL service is running.

Create a PostgreSQL database named `routeforge`.

Configure the database connection using the `ConnectionStrings:RouteForge` configuration key. For local development, store the connection string securely using .NET User Secrets rather than committing database credentials to GitHub.

From the project root, configure the connection string using your own PostgreSQL credentials:

```powershell
dotnet user-secrets set "ConnectionStrings:RouteForge" "Host=localhost;Port=5432;Database=routeforge;Username=postgres;Password=YOUR_PASSWORD" --project backend/RouteForge.Api
```

Replace `YOUR_PASSWORD` with your local PostgreSQL password. Never commit real credentials to the repository.

If the Entity Framework Core CLI is not installed, install it:

```powershell
dotnet tool install --global dotnet-ef
```

Apply pending database migrations:

```powershell
dotnet ef database update --project backend/RouteForge.Api
```

This command applies pending migrations to the configured database. Database migrations are managed through EF Core tooling rather than automatically at application startup.

### Run the Backend

From the project root:

```powershell
dotnet run --project backend/RouteForge.Api
```

The API runs at the local address reported in the terminal. In the current development setup, this is:

`http://localhost:5124`

### Run the Frontend

Open a second terminal:

```powershell
cd frontend
npm install
npm run dev
```

On Windows PowerShell, use `npm.cmd` instead of `npm` if PowerShell's execution policy prevents the npm script from running.

Open the frontend address reported by Vite. In the current development setup, this is:

`http://localhost:5173`

Keep the backend running while using the frontend.

### Run the Tests

From the project root:

```powershell
dotnet test RouteForge.slnx
```

The automated test suite covers API behaviour, delivery-location operations, validation, and route optimization functionality.

**Latest verified test results: 30 passed, 0 failed.**

### Build the Frontend

From the `frontend` directory:

```powershell
npm run build
```

This runs the TypeScript build checks and creates an optimized production build.

## Project Structure

```text
RouteForge/
├── backend/
│   ├── RouteForge.Api/
│   │   ├── DTOs/
│   │   ├── Data/
│   │   │   └── Migrations/
│   │   ├── Enums/
│   │   ├── Models/
│   │   ├── Optimization/
│   │   ├── Services/
│   │   └── Program.cs
│   │
│   └── RouteForge.Api.Tests/
│       ├── IntegrationTests/
│       ├── Optimization/
│       └── UnitTest1.cs
│
├── frontend/
│   └── src/
│       ├── components/
│       ├── services/
│       ├── App.tsx
│       └── App.css
│
├── RouteForge.slnx
└── .gitignore
```

## Development Roadmap

- [x] ASP.NET Core Web API foundation
- [x] PostgreSQL database integration
- [x] Entity Framework Core migrations
- [x] Delivery location CRUD endpoints
- [x] Nearest Neighbour route optimization
- [x] 2-opt route improvement
- [x] OSRM road-distance integration
- [x] Interactive route visualization
- [x] Custom start and end locations
- [x] Address autocomplete
- [x] Warehouse registration and management API
- [x] Warehouse registration frontend
- [x] Frontend and backend integration
- [x] Automated API and optimization tests
- [ ] Warehouse editing and deletion interface
- [ ] Delivery location management interface
- [ ] Saved routes and route history
- [ ] Dashboard analytics and performance metrics
- [ ] User authentication and role-based access control
- [ ] Production deployment

## External Services

RouteForge currently integrates with:

- **[OSRM](https://project-osrm.org/):** Provides road-distance calculations and route geometry.
- **[Photon](https://photon.komoot.io/):** Provides address search and geocoding.

These services require an internet connection. Their public instances are used during development.

## Project Status

RouteForge is an active development project. Its core backend, database integration, route optimization engine, interactive map, and initial warehouse management workflow are implemented.

The next development phase focuses on completing warehouse management through the frontend, expanding delivery location management, and introducing saved route history and dashboard analytics.

---

**Developed by [LoloM-19](https://github.com/LoloM-19)**