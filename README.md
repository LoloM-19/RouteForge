# RouteForge 🚚

**An intelligent logistics optimization platform built with C# and ASP.NET Core.**

RouteForge is a logistics optimization platform designed to manage delivery locations, calculate road distances, and optimize delivery routes. It combines a .NET backend, PostgreSQL database, route optimization algorithms, and an interactive map to demonstrate how software can improve real-world delivery planning.

The project is being developed with an emphasis on clean architecture, input validation, automated testing, persistent data management, and practical optimization algorithms.

## Current Features

* **Delivery location management:** Retrieve, create, update, and delete delivery locations through a REST API.
* **PostgreSQL persistence:** Store delivery locations in a relational database using Entity Framework Core.
* **Input validation:** Validate location names and geographic coordinates, rejecting invalid requests.
* **Road-distance calculations:** Use OSRM to obtain road-distance data instead of relying solely on straight-line distances.
* **Route optimization:** Implement the Nearest Neighbour algorithm and 2-opt improvement to determine efficient delivery sequences.
* **Route visualization:** Display optimized routes on an interactive map using React, TypeScript, and Leaflet.
* **Road-following geometry:** Visualize routes using road geometry returned by OSRM.
* **Automated testing:** Use xUnit and ASP.NET Core integration testing to verify API behaviour and route optimization functionality.
* **Version control:** Maintain the project using Git and GitHub.

## Technology Stack

* **Language:** C#
* **Backend framework:** ASP.NET Core Web API
* **Platform:** .NET 10
* **Database:** PostgreSQL
* **ORM:** Entity Framework Core
* **Frontend:** React and TypeScript
* **Build tool:** Vite
* **Mapping:** Leaflet and React-Leaflet
* **Routing engine:** OSRM (Open Source Routing Machine)
* **Testing:** xUnit and ASP.NET Core integration testing
* **Version control:** Git and GitHub

## API Endpoints

| Method | Endpoint | Description |
| --- | --- | --- |
| GET | `/api/locations` | Retrieve all delivery locations |
| GET | `/api/locations/{id}` | Retrieve a location by ID |
| POST | `/api/locations` | Create a delivery location |
| PUT | `/api/locations/{id}` | Update an existing delivery location |
| DELETE | `/api/locations/{id}` | Delete a delivery location |
| POST | `/api/routes/optimize` | Calculate an optimized delivery route |

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

### Optimize a Delivery Route

Send a `POST` request to `/api/routes/optimize` with a warehouse ID and the IDs of the delivery locations to visit:

```json
{
  "warehouseId": 1,
  "deliveryLocationIds": [2, 3]
}
```

The API calculates an optimized delivery sequence using road-distance data and returns route information, including distance metrics and route geometry for visualization.

The warehouse and delivery location IDs must correspond to locations stored in the database.

## Getting Started

### Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* [Node.js](https://nodejs.org/)
* [Git](https://git-scm.com/)
* [PostgreSQL](https://www.postgresql.org/)

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

Apply pending Entity Framework Core migrations:

```powershell
dotnet ef database update --project backend/RouteForge.Api
```

This command applies pending migrations to the configured database. Database migrations are managed through EF Core tooling rather than automatically at application startup.

### Run the Backend

From the project root, run:

```bash
dotnet run --project backend/RouteForge.Api
```

The API runs at the local address reported in the terminal. In the current development setup, this is:

`http://localhost:5124`

### Run the Frontend

Open a second terminal and run:

```bash
cd frontend
npm install
npm run dev
```

On Windows PowerShell, use `npm.cmd` instead of `npm` if PowerShell's execution policy prevents the npm script from running.

Open the frontend address reported by Vite. In the current development setup, this is:

`http://localhost:5173`

Keep the backend running while using the frontend.

### Run the Tests

From the project root, run:

```bash
dotnet test RouteForge.slnx
```

The automated test suite covers delivery-location operations, API behaviour, validation, and route optimization functionality.

The latest verified test run completed with **30 tests passed and 0 failed**.

### Build the Frontend

From the `frontend` directory, run:

```bash
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
│   │   ├── Models/
│   │   ├── Optimization/
│   │   ├── Services/
│   │   └── Program.cs
│   └── RouteForge.Api.Tests/
│       ├── IntegrationTests/
│       ├── Optimization/
│       └── UnitTest1.cs
├── frontend/
│   └── src/
│       ├── components/
│       ├── services/
│       └── ...
├── RouteForge.slnx
└── .gitignore