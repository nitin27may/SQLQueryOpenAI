# SQLQueryAI

SQLQueryAI is a RESTful web API that uses OpenAI's GPT-4o model to generate SQL queries based on natural language inputs and database schema context. It also supports executing these queries against a SQL database and returning results in JSON format. The application is designed to make database interactions more accessible to non-technical users.

## Features

- **Natural Language SQL Query Generation:** Converts user prompts into SQL queries using OpenAI GPT-4o.
- **SQL Query Execution:** Executes generated SQL queries against a SQL database.
- **Schema and Relationship Retrieval:** Extracts and uses database schema details and relationships for accurate query generation.
- **Docker Support:** Simplified setup using Docker and Docker Compose.

## Prerequisites

- **Without Docker:**

  - .NET 9.0 SDK or later
  - Microsoft SQL Server
  - OpenAI API key
  - SQL script to seed the database

- **With Docker:**

  - Docker Desktop

## Setup

### 1. Clone the Repository

```sh
git clone https://github.com/yourusername/SQLQueryAI.git
cd SQLQueryAI
```

### 2. Running the Application

#### Option A: Without Docker

1. **Set Up the Database**

   Before running the application, ensure that the required database schema and seed data are available. Use the provided `seed.sql` file:

   ```sql
   -- Execute this script in your SQL Server instance to create the database and populate initial data.
   ```

2. **Configure Application Settings**

   Update `appsettings.Development.json` with your OpenAI API key and database connection string:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "your-database-connection-string"
     },
     "OpenAI": {
       "ApiKey": "your-api-key"
     }
   }
   ```

3. **Build and Run the Application**

   ```sh
   dotnet build
   dotnet run
   ```

4. **Verify the Application**

   The API will be available at `http://localhost:5275`.

#### Option B: With Docker

1. **Prepare Docker Environment**

   Ensure Docker and Docker Compose are installed on your machine.

2. Rename .env.example to .env

Update the .env file with your OpenAI API key and database configuration as needed.

3. **Build and Run Using Docker Compose**

   ```sh
   docker-compose up --build
   ```

4. **Verify the Application**

   The API will be available at `http://localhost:8000`.

5. **Optional:** By default, docker compose will seed the data but if changes are made to the database, run the `seed.sql` script manually inside the database container to reset or seed the data:

   ```sh
   docker exec -it <container_name> /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P <your_password> -d DemoDB -i seed.sql
   ```

## Usage

### Generate SQL Query

Send a POST request to `/generate` with a JSON body containing the user prompt and schema context.

#### Example Request

```http
POST {{SQLQueryAI_HostAddress}}/api/sql/generate
Content-Type: application/json

"List all orders with order details, customer name placed in December 2023, including order IDs and total amounts."

```

### Execute SQL Query

Send a POST request to `/execute` with a prompt.

#### Example Request

```http
POST {{SQLQueryAI_HostAddress}}/api/sql/execute
Content-Type: application/json

"SELECT o.OrderID, c.CustomerName, o.OrderDate, o.TotalAmount, od.ProductName, od.Quantity, od.Price \nFROM [Order] o\nJOIN OrderDetails od ON o.OrderID = od.OrderID\nJOIN Customer c ON o.CustomerID = c.CustomerID\nWHERE o.OrderDate >= '2023-12-01' AND o.OrderDate < '2024-01-01';"

```

### Generate and Execute

Send a POST request to `/generate-and-execute` with a prompt.

#### Example Request

```http
POST {{SQLQueryAI_HostAddress}}/api/sql/generate-and-execute
Content-Type: application/json

"List all orders with order details, customer name placed in December 2023, including order IDs and total amounts."
```

## License

This project is licensed under the MIT License.

