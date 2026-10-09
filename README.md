# RouteForge 🚚

**An intelligent logistics optimization platform built with C# and ASP.NET Core.**

RouteForge is a logistics software project focused on managing delivery locations and building toward smarter delivery planning and route optimization.

The project is being developed incrementally, with an emphasis on clean backend architecture, input validation, automated testing, and reliable data management.

## Current Features

* **Delivery location management:** Retrieve all delivery locations and look up individual locations by ID.
* **Create delivery locations:** Add locations with a name, latitude, and longitude.
* **Input validation:** Reject blank names and coordinates outside valid geographic ranges.
* **Automated testing:** Unit tests for the service layer and integration tests for the HTTP API.
* **Version control:** Git-based development with a public GitHub repository.

## Technology Stack

* **Language:** C#
* **Framework:** ASP.NET Core Web API
* **Platform:** .NET 10
* **Testing:** xUnit and ASP.NET Core integration testing
* **Version control:** Git and GitHub

## API Endpoints

| Method | Endpoint              | Description                     |
| ------ | --------------------- | ------------------------------- |
| GET    | `/api/locations`      | Retrieve all delivery locations |
| GET    | `/api/locations/{id}` | Retrieve a location by ID       |
| POST   | `/api/locations`      | Create a delivery location      |

### Example Request

Create a delivery location by sending a `POST` request to `/api/locations` with this JSON body:

```json
{
  "name": "Pretoria Distribution Centre",
  "latitude": -25.7479,
  "longitude": 28.2293
}
```

A successful request returns `201 Created` and the newly created location, including its assigned ID.

Invalid names or out-of-range coordinates are rejected with `400 Bad Request`.

## Getting Started

### Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* [Git](https://git-scm.com/)

### Run the API

Clone the repository:

```bash
git clone https://github.com/YOUR-USERNAME/RouteForge.git
cd RouteForge
```

Replace `YOUR-USERNAME` with your GitHub username.

Run the API:

```bash
dotnet run --project backend/RouteForge.Api
```

Use the local URL reported in the terminal to access the API. The available endpoints depend on the configured launch settings.

### Run the Tests

From the project root, run:

```bash
dotnet test RouteForge.slnx
```

The test suite covers delivery-location service behaviour and retrieval through the HTTP API.

## Project Structure

```text
RouteForge/
├── backend/
│   ├── RouteForge.Api/
│   │   ├── DTOs/
│   │   ├── Models/
│   │   ├── Services/
│   │   └── Program.cs
│   └── RouteForge.Api.Tests/
│       ├── IntegrationTests/
│       └── UnitTest1.cs
├── RouteForge.slnx
└── .gitignore
```

## Development Roadmap

* [x] Create the ASP.NET Core Web API
* [x] Implement delivery-location retrieval and creation
* [x] Add request validation
* [x] Introduce a request DTO
* [x] Add unit and integration tests
* [x] Publish the project to GitHub
* [ ] Add persistent database storage
* [ ] Expand API and validation test coverage
* [ ] Implement distance calculations between delivery locations
* [ ] Develop route optimization functionality
* [ ] Build a frontend for interacting with the platform

## Current Limitations

Delivery locations are currently stored in memory. Data created through the API is not retained when the application restarts.

Database persistence and route optimization are planned future improvements.

## Project Status

**In development.** RouteForge is an evolving portfolio project demonstrating backend development, API design, automated testing, and logistics-focused problem solving.
