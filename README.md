# Oracle Database Client for Windows

A modern Oracle Database Client application built with .NET 8.0 and Windows Forms, similar to Oracle SQL Developer but with enhanced TNSNAMES.ORA support.

## Features

- **TNSNAMES.ORA Integration**: Automatically detects and parses TNSNAMES.ORA files for easy connection management
- **Dual Connection Methods**: Support for both TNS Names and direct connection strings
- **Modern UI**: Clean, responsive interface with professional styling
- **SQL Query Interface**: Execute SQL queries with syntax highlighting and result display
- **Database Object Browser**: Navigate through database tables and objects
- **Connection Management**: Save and manage multiple database connections
- **Result Grid**: View query results in a data grid with export capabilities

## Requirements

- Windows 10/11
- .NET 8.0 Runtime
- Oracle Database (any version supported by Oracle.ManagedDataAccess.Core)

## Installation

1. Clone or download this repository
2. Ensure you have .NET 8.0 SDK installed
3. Build the solution:
   ```bash
   dotnet build
   ```
4. Run the application:
   ```bash
   dotnet run --project OracleClient
   ```

## Usage

### Connecting to Database

#### Using TNS Names
1. The application will automatically detect your TNSNAMES.ORA file
2. Select a TNS name from the dropdown
3. Enter your username and password
4. Click "Connect"

#### Using Direct Connection
1. Select "Direct Connection" from the connection type dropdown
2. Enter host, port, service name (or SID)
3. Enter username and password
4. Click "Connect"

### Executing Queries
1. Type your SQL query in the query text area
2. Click "Execute" to run the query
3. View results in the results grid below
4. Use "Clear" to clear the query and results

### Browsing Database Objects
1. After connecting, the left panel will show available tables
2. Double-click on a table to generate a SELECT query
3. The query will appear in the query text area

## TNSNAMES.ORA Support

The application automatically:
- Detects TNSNAMES.ORA files in common Oracle installation locations
- Parses TNS entries and makes them available in the connection dropdown
- Supports manual TNS file selection via the "Browse" button
- Handles both SERVICE_NAME and SID-based connections

## Project Structure

```
OracleClient/
├── Models/
│   ├── TnsNamesParser.cs      # TNSNAMES.ORA file parser
│   └── DatabaseManager.cs     # Database connection and query management
├── MainForm.cs                # Main application form
├── MainForm.Designer.cs       # Form designer file
├── Program.cs                 # Application entry point
└── OracleClient.csproj        # Project file with dependencies
```

## Dependencies

- **Oracle.ManagedDataAccess.Core**: Oracle's managed data access provider
- **System.Data.OracleClient**: Additional Oracle client support

## Building from Source

1. Install .NET 8.0 SDK
2. Clone the repository
3. Navigate to the project directory
4. Restore packages:
   ```bash
   dotnet restore
   ```
5. Build the solution:
   ```bash
   dotnet build --configuration Release
   ```

## Troubleshooting

### Connection Issues
- Ensure Oracle client libraries are properly installed
- Verify TNSNAMES.ORA file format and location
- Check network connectivity to the Oracle server
- Verify username/password credentials

### TNS Names Not Loading
- Check if TNSNAMES.ORA file exists and is readable
- Verify file format (should contain proper TNS entries)
- Use the "Browse" button to manually select the TNS file

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.