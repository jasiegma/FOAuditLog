# FOAuditLog

FOAuditLog is a proofe of concept project designed to handle and process data events from D365 F&O and store them in a Azure Cosmos DB. This project utilizes Azure Functions and custom serialization to manage event data.

## Disclaimer

Please note that FOAuditLog is a proof of concept project and is not intended for use in production environments. It is designed for educational and development purposes only. Users should exercise caution and not rely on this project for any production-level applications or in any scenario where reliability, security, and performance are critical. The developers of FOAuditLog make no guarantees regarding its stability or security for production use.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

- .NET Core 3.1 SDK or later
- Visual Studio 2019 or later (with Azure development workload installed)
- An Azure account with an active subscription

### Installation

1. Clone the repository to your local machine.
2. Open the solution file `PoCAuditLog.sln` in Visual Studio.
3. Restore the NuGet packages by right-clicking on the solution and selecting "Restore NuGet Packages".
4. Build the solution by pressing `Ctrl+Shift+B` or by selecting "Build Solution" from the "Build" menu.

### Configuration

Before running the project, ensure that the `local.settings.json` file contains the necessary configuration for Azure Functions and any other services the application interacts with.

### Running Locally

To run the project locally, press `F5` in Visual Studio. This will start the Azure Functions runtime and the function can be triggered based on its configuration (e.g., HTTP trigger, timer trigger).

## Project Structure

- `CustomSystemTextJsonCosmosSerializer.cs`: Custom JSON serializer for Cosmos DB.
- `eventdata.cs`: Defines the event data model and custom JSON converters.
- `LogData.cs`: Model for log data entries.
- `Program.cs`: Entry point for the Azure Function.


## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.


