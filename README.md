# P6-Project Backend
This repository contains the backend for the P6-Project. It uses:

-   **.NET 8** for building web endpoints.
    
-   **Entity Framework Core** for database interactions.
    
-   **PostgreSQL** as the database (configured via Docker).
    
-   **Python** for any machine learning modules (installed in the Docker image).
    
-   **Swashbuckle** for Swagger, which provides an interactive API documentation UI.

## Table of Contents
1.  [Prerequisites](#prerequisites)
    
2.  [Getting Started](#getting-started)
    
3.  [Running the Backend Using Docker](#running-the-backend-using-docker)
    
4.  [Running the Backend Locally (Without Docker)](#running-the-backend-locally-without-docker)
    
5.  [Database Migrations](#database-migrations)
    
6.  [Adding New Features](#adding-new-features)

## Prerequisites
-   **Docker** and **Docker Compose** installed (if you want to run via Docker).
    
-   **.NET 8 SDK** installed (if you want to run or develop locally without Docker).
    
-   **PostgreSQL** (if you want to run the database locally, though Docker is recommended).

## Getting Started
1.  **Clone or Download** this repository so that you have the code available on your machine.
    
2.  **Review the Directory Structure** to understand how the project is organized:
├── docker
│   ├── dev
│   │   └── Dockerfile
│   │   └── docker-compose.yml
│   └── prod
├── src
│   ├── Data
│   ├── Endpoints
│   ├── Entities
│   ├── Extensions
│   ├── Mappings
│   ├── Migrations
│   ├── Modules
│   └── Security
└── testing
3. **Environment Variables**:
-   For development, `.env` files can be used (though, in this setup, their contents are hardcoded into the compose, will change at a later time).
    
-   If you’re running the Docker container, the environment variables will be set in `docker-compose.yml`.

## Running the Backend Using Docker
The simplest way to get up and running (including the database) is via Docker and Docker Compose.

1.  **Navigate to the `docker/dev` folder**: 
    `cd docker/dev` 
    
2.  **Run Docker Compose**:
    `docker-compose up --build` 
    -   This command builds the `backend` image using the provided Dockerfile and spins up two containers:
        1.  **Prototype-P6.dev** (the .NET backend container).
            
        2.  **prototype-p6-db** (the PostgreSQL container).
            
3.  **Check the Logs**: You’ll see Docker logs showing the containers starting up and the database migrations running. Wait until you see something like:
    `Now listening on: http://0.0.0.0:5171
    Hosting environment: Development
    ...` 
    
4.  **Access Swagger**: Open your browser at  
    **[http://localhost:80/swagger](http://localhost:80/swagger)**  
    This is because we map internal port `5171` to host port `80` in `docker-compose.yml`.
    
5.  **Verify**: You should see the **Swagger UI** with the available endpoints.

#### Stopping the Containers
When you need to stop:
`docker-compose down`

## Running the Backend Locally (Without Docker)
If you prefer to run everything on your host machine:
1.  **Ensure You Have .NET 8 Installed**:
    -   You can verify via:
 `dotnet --version`

2.  **Set Up the Database**:  
    -   Option A: Use a local PostgreSQL instance.
        -   Make sure your connection string in `appsettings.json` points to your local DB.
            
    -   Option B: Use the Docker-based PostgreSQL, but run your local .NET code against it. Make sure you adjust your connection strings accordingly.
        
3. **Navigate to the `src` Folder**:
`cd src`

4. **Restore NuGet Packages**:
    `dotnet restore`  

5. **Run the Application**:
    `dotnet run` 
    By default, it will listen on `http://localhost:5171`.
    
6. **View Swagger**:
    -   Open your browser to **http://localhost:5171/swagger**.

## Database Migrations
We use Entity Framework Core for database migrations. Below are common commands (run from the `src` folder where the `.csproj` is located):
1. **Add a New Migration:**
`dotnet ef migrations add <MigrationName>`

2. **Update the Database:**
`dotnet ef database update`

3. **Remove the last Migration(if you made a mistake and haven’t applied it to the database yet):**
`dotnet ef migrations remove`

If you want to run these migrations against your **Docker** PostgreSQL database, ensure that:
-   The connection string in your `appsettings.json` (or environment variable) is set to the Docker host and port (e.g., `Host=localhost;Port=5432;Database=your_database_name;User Id=postgres;Password=YourStrong!Passw0rd;`).
    
-   **Then** run the commands above.

## Adding New Features
Below are the broad steps you might follow to add new elements to the codebase:

### 1. Adding a New Entity
1.  **Create an Entity class** in the `src/Entities/<FolderName>` directory, e.g. `Product.cs`.
    
2.  **Add a DbSet** to `PrototypeDbContext.cs`:
`public DbSet<Product> Products { get; set; }` 
    
3.  **(Optional) Create a DTO** if you need a data transfer object for requests/responses in `Entities/DTOs`.
    
4.  **(Optional) Create or Update Mappings** in `Mappings/` (e.g., to map between your `Product` entity and `ProductDTO`).

### 2. Exposing a New Endpoint
1.  **Create a new endpoint class** in `src/Endpoints/`, e.g. `ProductEndpoints.cs`.
    
2.  **Add an extension method** to map these endpoints:  
    `public  static  class  ProductEndpoints { public  static RouteGroupBuilder MapProductEndpoints(this RouteGroupBuilder group)
        { group.MapGet("/", GetAllProducts); group.MapPost("/", CreateProduct); // etc.  return  group;
        } // Example handlers  private  static IResult GetAllProducts(/* ... */) { /* ... */ } private  static IResult CreateProduct(/* ... */) { /* ... */ }
    }` 
    
3.  **Add the endpoint to `Program.cs`**:
    `app.MapGroup("/products").MapProductEndpoints();`

### 3. Updating the Database Schema
If your new entity or changes require a schema update, create a new migration and apply it:
`dotnet ef migrations add AddProductEntity
dotnet ef database update`