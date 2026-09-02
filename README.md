# Kanban

Kanban Board System

A full-stack Kanban board application designed to streamline task management and workflow visualization. Built as a comprehensive academic project (Group 44), this system demonstrates strong software engineering principles, including an N-Tier architecture, comprehensive testing, and robust logging.

Key Highlights for Recruiters

Clean Architecture: Separation of concerns using distinct layers for Data, Business logic, and Services.

Full-Stack Development: Complete implementation from the backend server to the frontend user interface.

Test-Driven Approach: Extensive test coverage across different layers (BusinessLayerTests, ServiceLayerTests, and FrontendTests1).

Production-Ready Practices: Integrated logging using log4net and a structured Visual Studio solution environment.

Architecture & Technologies

Backend: C# / .NET (Structured via Kanban.sln)

Database: Local database integration (kanban.db)

Logging: log4net for application monitoring and debugging

Testing: Dedicated test suites for Business, Service, and Frontend layers.

Version Control: Git with configured .gitignore and CODEOWNERS for team collaboration.

Project Structure

Backend/ - Contains the core server logic, API endpoints, and request handling.

Frontend/ - Client-side application for the user interface.

Data/ - Data access layer handling interactions with kanban.db.

Documents/ - Project documentation, requirements, and academic deliverables.

BusinessLayerTests/ - Unit and integration tests for core business logic.

ServiceLayerTests/ - Tests validating the service API layer.

FrontendTests1/ - Automated tests for the user interface and client logic.

Getting Started

Clone the repository to your local machine.

Open Kanban.sln in Visual Studio.

Restore NuGet packages and build the solution.

Run the Backend and Frontend projects simultaneously to launch the application.
