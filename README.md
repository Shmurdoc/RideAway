🚗 RideAway - Project Overview
RideAway is a modern, scalable ride-hailing platform backend—designed and built by a passionate graduate developer who thrives on clean architecture and maintainable code. This ongoing project harnesses the power of ASP.NET Core (.NET 6) to deliver a robust API layer that supports key ride-hailing functionalities such as fare estimation, ride management, and real-time tracking.

🧠 Key Features & Architecture
💡 Clean & Scalable Design
This solution is grounded in Clean Architecture (Onion Architecture), emphasizing modularity, separation of concerns, and testability. Core components include:

Controllers: Handle HTTP requests/responses, kept thin by delegating logic to service layers.

Services & Interfaces: Encapsulate business logic with clear service contracts, adhering to SOLID principles.

Domain Models & DTOs: Ensure clean separation between internal business logic and API-facing data contracts.

Dependency Injection: Used throughout the project for flexibility and easy testing.

📊 FareController Explained
The FareController is a RESTful API controller marked with [ApiController] and [Route("api/[controller]")], ensuring adherence to ASP.NET Core's routing and validation conventions.

Constructor Injection: It receives IFareCalculationService and IRideMatchingService through DI for better modularity and test coverage.

Estimate Fare Endpoint: Although currently commented out, this endpoint is designed to accept a FareEstimationDTO via POST. It calculates the fare using the injected service, based on pickup location, destination, estimated distance, and time.

Service Abstraction: The use of interfaces enables loose coupling, making the application easier to maintain, test, and extend.

🧱 Technical Highlights
✅ .NET 6 with ASP.NET Core Web API

🔌 Layered architecture for separation of concerns

🧪 Test-friendly with full interface-driven design

🧠 Domain-driven development using value objects and rich domain models

🔄 Ongoing enhancements with room for future features and scale

🛠️ For Contributors & Testers
If you're testing or contributing:

⚠️ Remember to add your own API keys or configuration for testing. Make sure to remove or secure any sensitive data before committing your changes to version control.

🙌 Personal Note
As a developer early in my career, this project reflects both my curiosity and commitment to writing clean, professional-grade code. It's an evolving platform that grows with every feature, refactor, and lesson learned. I hope you find the architecture inspiring and the codebase helpful—whether you're here to contribute, learn, or build something amazing on top of it.
