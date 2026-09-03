# Full Stack Kanban Board System

A full stack task management application implemented in C# as part of a Software Engineering degree at Ben Gurion University of the Negev. The system demonstrates practical application of modern software architecture patterns and comprehensive testing across the stack.

## Tech stack

* C# and .NET
* MVVM for the frontend
* N Tier architecture for the backend
* SQLite for persistence
* Log4Net for centralized logging and error tracking

## Architecture

* Frontend: MVVM pattern with Login, Dashboard and Board screens implemented as a desktop client following clear separation of concerns.
* Backend: N Tier architecture with Data Layer Business Layer and Service Layer that expose a service API for the client and encapsulate business rules.
* Database: SQLite provides a lightweight durable store for boards columns tasks and user data.
* Logging: Log4Net is used for system wide logging and error tracking across layers.

## Features

* User authentication and session handling
* Dashboard with an overview of boards and activity
* Board view with multiple columns and task management
* Create read update and delete operations for tasks and columns
* Board membership and simple user management
* Persistent data storage with SQLite
* Centralized logging for diagnostics and traceability

## Testing

Comprehensive unit and integration tests cover business logic service layer API and client side ViewModels. Relevant test projects include BusinessLayerTests ServiceLayerTests and FrontendTests1.

## How to run

Provide local setup and run instructions here. Example commands you can adapt for this solution:

```bash
cd Backend
dotnet build
dotnet run
```

Replace the example commands with the exact project and configuration details for your environment.

---

For more details see the source folders Backend Frontend Data and ServiceLayer in the repository.

