# RemitAnnex API Project

## 📖 What is This Project?

**RemitAnnex** is a **Web API** (Application Programming Interface) built with **.NET 8.0**. Think of it as a service that other applications can call over the internet to update transaction/remittance data in a database.

### Simple Analogy
Imagine a restaurant:
- **Customers** (other applications) place orders (send requests)
- **Waiters** (Controllers) take the orders
- **Chefs** (Business Layer) prepare the food (process the data)
- **Kitchen Storage** (Database) stores ingredients (data)

This project follows the same pattern - it receives requests, validates them, processes the data, and saves it to a database.

---

## 🏗️ Project Architecture (Layered Architecture)

This project uses a **3-Layer Architecture** (also called **Layered Architecture** or **N-Tier Architecture**). This is like organizing a company into departments:

### Layer 1: **Controllers** (Presentation Layer)
- **What it does**: Receives HTTP requests from clients (like web browsers or mobile apps)
- **Think of it as**: The reception desk - first point of contact
- **Location**: `Controllers/` folder
- **Example**: `RemitAnexController.cs` - handles incoming API requests

### Layer 2: **BusinessLayer** (Business Logic Layer)
- **What it does**: Contains the business rules and logic (how data should be processed)
- **Think of it as**: The manager who decides what to do with the request
- **Location**: `BusinessLayer/` folder
- **Example**: `RemitAnexBL.cs` - processes transaction data and converts it to XML

### Layer 3: **DBManager** (Data Access Layer)
- **What it does**: Handles all database operations (saving, reading, updating data)
- **Think of it as**: The warehouse worker who stores and retrieves items
- **Location**: `DBManager/` folder
- **Example**: `RemitAnexDBMgt.cs` - connects to Oracle database and executes stored procedures

### Supporting Components:

#### **Models** (Data Structures)
- **What it does**: Defines the shape/structure of data (like a form template)
- **Location**: `Models/` folder
- **Example**: `RemitAnexUpdateRequest.cs` - defines what data the API expects

#### **Manager** (Cross-Cutting Concerns)
- **What it does**: Handles tasks that apply across all layers (like validation)
- **Location**: `Manager/` folder
- **Example**: `ValidatorManager.cs` - validates API keys and request data

#### **Utility** (Helper Classes)
- **What it does**: Stores constants and helper functions used throughout the project
- **Location**: `Utility/` folder
- **Example**: `Definition.cs` - stores API key constants

---

## 🔧 Technologies Used

### 1. **ASP.NET Core Web API**
- **What it is**: Microsoft's framework for building web APIs
- **Why use it**: Makes it easy to create REST APIs that can be called over HTTP
- **Version**: .NET 8.0

### 2. **Oracle Database**
- **What it is**: A relational database management system
- **Why use it**: Stores transaction data persistently
- **Package**: `Oracle.ManagedDataAccess.Core` (version 3.21.110)

### 3. **Swagger/OpenAPI**
- **What it is**: A tool that automatically generates API documentation
- **Why use it**: Allows developers to test and understand the API easily
- **Package**: `Swashbuckle.AspNetCore` (version 6.6.2)

---

## 📁 Project Structure Explained

```
RemitAnnex/
│
├── Controllers/              # Layer 1: Receives HTTP requests
│   └── RemitAnexController.cs
│
├── BusinessLayer/            # Layer 2: Business logic
│   └── RemitAnexBL.cs
│
├── DBManager/                # Layer 3: Database operations
│   └── RemitAnexDBMgt.cs
│
├── Models/                   # Data structures
│   ├── Request/              # Request models (data coming in)
│   │   └── RemitAnexUpdateRequest.cs
│   ├── DB/                   # Database models
│   │   └── SaveErrorLog.cs
│   └── Validation/           # Validation response models
│       └── CommonValidationResponse.cs
│
├── Manager/                  # Cross-cutting concerns
│   └── ValidatorManager.cs
│
├── Utility/                  # Helper classes and constants
│   └── Definition.cs
│
├── Program.cs              # Application entry point (starts the API)
├── appsettings.json        # Configuration file
└── RemitAnnex.csproj      # Project file (lists dependencies)
```

---

## 🔄 How the API Works (Request Flow)

When someone calls the API, here's what happens:

```
1. Client sends HTTP POST request
   ↓
2. Controller receives request
   ├── Extracts API key from headers
   ├── Gets request data (JSON)
   └── Calls ValidatorManager
   ↓
3. ValidatorManager validates:
   ├── Is API key present?
   ├── Is API key correct?
   ├── Is request data valid?
   └── Returns validation result
   ↓
4. If valid → BusinessLayer processes data
   ├── Converts transaction list to XML
   └── Calls DBManager
   ↓
5. DBManager executes database operation
   ├── Connects to Oracle database
   ├── Calls stored procedure
   └── Returns success/failure
   ↓
6. Controller returns response to client
   └── Success (200) or Error (400)
```

---

## 🚀 How to Build a Similar Project

Follow these steps to create your own layered architecture API:

### Step 1: Install Prerequisites

1. **Install .NET 8.0 SDK**
   - Download from: https://dotnet.microsoft.com/download
   - Verify installation: Open terminal and run `dotnet --version`

2. **Install Visual Studio or Visual Studio Code**
   - Visual Studio: https://visualstudio.microsoft.com/
   - VS Code: https://code.visualstudio.com/ (with C# extension)

### Step 2: Create a New Web API Project

Open terminal/command prompt and run:

```bash
# Create a new Web API project
dotnet new webapi -n YourProjectName

# Navigate to project folder
cd YourProjectName
```

### Step 3: Set Up Project Structure

Create the following folders in your project:

```
YourProjectName/
├── Controllers/
├── BusinessLayer/
├── DBManager/
├── Models/
│   ├── Request/
│   ├── DB/
│   └── Validation/
├── Manager/
└── Utility/
```

**How to create folders:**
- In Visual Studio: Right-click project → Add → New Folder
- In VS Code: Right-click in Explorer → New Folder
- In terminal: `mkdir Controllers BusinessLayer DBManager Models Manager Utility`

### Step 4: Create Your Models (Data Structures)

Start with defining what data your API will work with.

**Example: `Models/Request/YourRequest.cs`**
```csharp
namespace YourProjectName.Models.Request
{
    public class YourRequest
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
```

### Step 5: Create Your Database Manager (Data Access Layer)

This handles database operations.

**Example: `DBManager/YourDBMgt.cs`**
```csharp
using System.Data;
using Oracle.ManagedDataAccess.Client; // or SqlConnection for SQL Server

namespace YourProjectName.DBManager
{
    public class YourDBMgt
    {
        private string ConnectionString = "your-connection-string-here";
        
        public int SaveData(string data)
        {
            // Database logic here
            return 1; // success
        }
    }
}
```

### Step 6: Create Your Business Layer

This contains your business logic.

**Example: `BusinessLayer/YourBL.cs`**
```csharp
using YourProjectName.DBManager;

namespace YourProjectName.BusinessLayer
{
    public class YourBL
    {
        public int ProcessData(string data)
        {
            // Business logic here
            YourDBMgt dbMgt = new YourDBMgt();
            return dbMgt.SaveData(data);
        }
    }
}
```

### Step 7: Create Your Controller (API Endpoint)

This receives HTTP requests.

**Example: `Controllers/YourController.cs`**
```csharp
using Microsoft.AspNetCore.Mvc;
using YourProjectName.BusinessLayer;
using YourProjectName.Models.Request;

namespace YourProjectName.Controllers
{
    [ApiController]
    [Route("api/your-endpoint")]
    public class YourController : ControllerBase
    {
        [HttpPost("process")]
        public IActionResult Process([FromBody] YourRequest request)
        {
            try
            {
                YourBL businessLayer = new YourBL();
                int status = businessLayer.ProcessData(request.Name);
                
                if (status == 1)
                {
                    return Ok(new { Message = "Success", Code = "000" });
                }
                else
                {
                    return BadRequest(new { Message = "Failed", Code = "999" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message, Code = "999" });
            }
        }
    }
}
```

### Step 8: Add Database Package

If using Oracle:
```bash
dotnet add package Oracle.ManagedDataAccess.Core
```

If using SQL Server:
```bash
dotnet add package System.Data.SqlClient
```

### Step 9: Configure Your Application

**Update `Program.cs`** (already created by template):
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
```

### Step 10: Run Your API

```bash
dotnet run
```

Your API will be available at: `http://localhost:5073` (or port shown in terminal)

Access Swagger UI at: `http://localhost:5073/swagger`

---

## 📝 Key Concepts Explained

### What is a Web API?
A Web API is like a waiter in a restaurant. Clients (other applications) send requests, and the API responds with data or performs actions.

### What is Layered Architecture?
It's like organizing a company:
- **Controllers** = Reception (receives visitors)
- **BusinessLayer** = Management (makes decisions)
- **DBManager** = Warehouse (stores data)

**Benefits:**
- ✅ Easy to understand
- ✅ Easy to maintain
- ✅ Easy to test
- ✅ Easy to modify one layer without affecting others

### What is a Controller?
A controller is a class that handles HTTP requests. It's decorated with `[ApiController]` and contains methods (endpoints) that respond to HTTP verbs like GET, POST, PUT, DELETE.

### What is Dependency Injection?
Instead of creating objects directly (`new YourClass()`), you register them in `Program.cs` and the framework provides them automatically. This makes code more testable and flexible.

### What is Swagger?
Swagger is an interactive documentation tool. When you run your API, visit `/swagger` to see all endpoints and test them directly in the browser.

---

## 🔐 Security Considerations

**Current Implementation:**
- API key validation via header (`RA-Api-Key`)
- API key stored in code (not recommended for production)

**For Production, Consider:**
- Store API keys in `appsettings.json` or environment variables
- Use proper authentication (JWT tokens, OAuth)
- Implement HTTPS
- Add rate limiting
- Validate and sanitize all inputs

---

## 🧪 Testing the API

### Using Swagger UI
1. Run the project: `dotnet run`
2. Open browser: `http://localhost:5073/swagger`
3. Click on an endpoint
4. Click "Try it out"
5. Enter request data
6. Click "Execute"

### Using Postman or cURL
```bash
curl -X POST "http://localhost:5073/api/remitannex/update-data" \
  -H "RA-Api-Key: 7hfjdhgf&4#2.ljfgjdgutdf" \
  -H "Content-Type: application/json" \
  -d '{
    "TranDetails": [
      {
        "PartyId": 1,
        "ReferenceNumber": "REF123",
        "EntityId": 1,
        "TranChannel": "WEB",
        "IsIncentive": false,
        "IsReverse": false,
        "StatusCode": "SUCCESS"
      }
    ],
    "MakerDetails": {
      "PurposeOfUpdate": "Correction",
      "EmployeeId": "EMP001"
    }
  }'
```

---

## 📚 Learning Resources

### For Beginners:
1. **Microsoft Learn - ASP.NET Core**
   - https://learn.microsoft.com/en-us/aspnet/core/

2. **C# Basics**
   - https://learn.microsoft.com/en-us/dotnet/csharp/

3. **REST API Concepts**
   - Search: "REST API tutorial for beginners"

### Key Topics to Learn:
- C# programming language
- Object-Oriented Programming (OOP)
- HTTP methods (GET, POST, PUT, DELETE)
- JSON (JavaScript Object Notation)
- Database concepts (SQL, stored procedures)
- API design principles

---

## 🐛 Common Issues and Solutions

### Issue: "Cannot connect to database"
**Solution**: Check your connection string in `DBManager` class

### Issue: "Package not found"
**Solution**: Run `dotnet restore` in project directory

### Issue: "Port already in use"
**Solution**: Change port in `Properties/launchSettings.json`

### Issue: "API key validation fails"
**Solution**: Check that header name matches exactly: `RA-Api-Key`

---

## 📋 Project Checklist (Building Similar Project)

- [ ] Install .NET 8.0 SDK
- [ ] Create new Web API project
- [ ] Set up folder structure (Controllers, BusinessLayer, DBManager, Models, Manager, Utility)
- [ ] Create request/response models
- [ ] Create database manager class
- [ ] Create business layer class
- [ ] Create controller with endpoints
- [ ] Add database package (Oracle/SQL Server)
- [ ] Configure connection string
- [ ] Test API using Swagger
- [ ] Add error handling
- [ ] Add validation
- [ ] Add logging

---

## 🎯 Summary

This project demonstrates a **clean, layered architecture** for building Web APIs:

1. **Controllers** receive requests
2. **BusinessLayer** processes business logic
3. **DBManager** handles database operations
4. **Models** define data structures
5. **Manager** handles cross-cutting concerns (validation, etc.)

This architecture makes your code:
- ✅ **Organized**: Each layer has a clear responsibility
- ✅ **Maintainable**: Easy to find and fix issues
- ✅ **Scalable**: Easy to add new features
- ✅ **Testable**: Each layer can be tested independently

---

## 📞 Next Steps

1. **Understand the code**: Read through each file and understand what it does
2. **Modify it**: Try changing the API endpoint or adding new validation
3. **Build your own**: Follow the step-by-step guide above to create your own API
4. **Learn more**: Explore Microsoft documentation and tutorials

---

**Happy Coding! 🚀**

