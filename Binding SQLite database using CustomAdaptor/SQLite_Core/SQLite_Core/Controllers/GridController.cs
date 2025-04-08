using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Syncfusion.EJ2.Base;
using Microsoft.Data.Sqlite;

namespace SQLite_Core.Controllers
{

    [ApiController]
    public class GridController : ControllerBase
    {

        string ConnectionString = @"Data Source=C:\sqlite\gui\OrderManagementDB.db;";


        /// <summary>
        /// Processes the DataManager request to perform searching, filtering, sorting, and paging operations.
        /// </summary>
        /// <param name="DataManagerRequest">Contains the details of the data operation requested.</param>
        /// <returns>Returns a JSON object with the filtered, sorted, and paginated data along with the total record count.</returns>
        [HttpPost]
        [Route("api/[controller]")]
        public object Post([FromBody] DataManagerRequest DataManagerRequest)
        {
            // Retrieve data from the data source (e.g., database).
            IQueryable<Orders> DataSource = GetOrderData().AsQueryable();

            // Initialize QueryableOperation instance.
            QueryableOperation queryableOperation = new QueryableOperation();

            // Handling searching operation.
            if (DataManagerRequest.Search != null && DataManagerRequest.Search.Count > 0)
            {
                DataSource = queryableOperation.PerformSearching(DataSource, DataManagerRequest.Search);
                //Add custom logic here if needed and remove above method.
            }

            // Handling filtering operation.
            if (DataManagerRequest.Where != null && DataManagerRequest.Where.Count > 0)
            {
                foreach (WhereFilter condition in DataManagerRequest.Where)
                {
                    foreach (WhereFilter predicate in condition.predicates)
                    {
                        DataSource = queryableOperation.PerformFiltering(DataSource, DataManagerRequest.Where, predicate.Operator);
                        //Add custom logic here if needed and remove above method.
                    }
                }
            }

            // Handling sorting operation.
            if (DataManagerRequest.Sorted != null && DataManagerRequest.Sorted.Count > 0)
            {
                DataSource = queryableOperation.PerformSorting(DataSource, DataManagerRequest.Sorted);
                //Add custom logic here if needed and remove above method.
            }

            // Get the total count of records.
            int totalRecordsCount = DataSource.Count();

            // Handling paging operation.
            if (DataManagerRequest.Skip != 0)
            {
                DataSource = queryableOperation.PerformSkip(DataSource, DataManagerRequest.Skip);
                //Add custom logic here if needed and remove above method.
            }
            if (DataManagerRequest.Take != 0)
            {
                DataSource = queryableOperation.PerformTake(DataSource, DataManagerRequest.Take);
                //Add custom logic here if needed and remove above method.
            }

            // Return data based on the request.
            return new { result = DataSource, count = totalRecordsCount };
        }

        /// <summary>
        /// Inserts a new data item into the data collection.
        /// </summary>
        /// <param name="value">It contains the new record detail which is need to be inserted.</param>
        /// <returns>Returns void.</returns>
        [HttpPost]
        [Route("api/[controller]/Insert")]
        public void Insert([FromBody] CRUDModel<Orders> value)
        {
            //Create query to insert the specific into the database by accessing its properties.
            string queryStr = $"Insert into Orders(CustomerID,Freight,ShipCity,EmployeeID) values('{value.value.CustomerID}','{value.value.Freight}','{value.value.ShipCity}','{value.value.EmployeeID}')";

            // Establishes a connection to the SQLite database using the provided connection string.
            SqliteConnection Connection = new SqliteConnection(ConnectionString);

            // Opens the database connection to execute queries.
            Connection.Open();

            //Execute the SQLite command
            SqliteCommand Command = new SqliteCommand(queryStr, Connection);

            //Execute this code to reflect the changes into the database.
            Command.ExecuteNonQuery();
            Connection.Close();

            //Add custom logic here if needed and remove above method.
        }

        /// <summary>
        /// Update a existing data item from the data collection.
        /// </summary>
        /// <param name="value">It contains the updated record detail which is need to be updated.</param>
        /// <returns>Returns void.</returns>
        [HttpPost]
        [Route("api/[controller]/Update")]
        public void Update([FromBody] CRUDModel<Orders> value)
        {
            //Create query to update the changes into the database by accessing its properties.
            string queryStr = $"Update Orders set CustomerID='{value.value.CustomerID}', Freight='{value.value.Freight}',EmployeeID='{value.value.EmployeeID}',ShipCity='{value.value.ShipCity}' where OrderID='{value.value.OrderID}'";

            // Establishes a connection to the SQLite database using the provided connection string.
            SqliteConnection Connection = new SqliteConnection(ConnectionString);

            // Opens the database connection to execute queries.
            Connection.Open();

            //Execute the SQLite command.
            SqliteCommand Command = new SqliteCommand(queryStr, Connection);

            //Execute this code to reflect the changes into the database.
            Command.ExecuteNonQuery();
            Connection.Close();

            //Add custom logic here if needed and remove above method.
        }
        [HttpPost]
        [Route("api/[controller]/Remove")]
        /// <summary>
        /// Remove a specific data item from the data collection.
        /// </summary>
        /// <param name="CRUDModel<T>">The set of information along with specific record detail which is need to be removed.</param>
        /// <returns>Returns void</returns>
        public void Remove([FromBody] CRUDModel<Orders> value)
        {
            //Create query to remove the specific from database by passing the primary key column value.
            string Query = $"DELETE FROM Orders WHERE OrderID={value.key}";
            SqliteConnection Connection = new SqliteConnection(ConnectionString);
            Connection.Open();
            //Execute the SQLite Command
            SqliteCommand Command = new SqliteCommand(Query, Connection);
            //Execute this code to reflect the changes into the database
            Command.ExecuteNonQuery();
            Connection.Close();
            //Add custom logic here if needed and remove above method

            //Add custom logic here if needed and remove above method.
        }

        /// <summary>
        /// Batch update (Insert, Update, and Delete) a collection of data items from the data collection.
        /// </summary>
        /// <param name="value">The set of information along with details about the CRUD actions to be executed from the database.</param>
        /// <returns>Returns void.</returns>
        [HttpPost]
        [Route("api/[controller]/BatchUpdate")]
        public IActionResult BatchUpdate([FromBody] CRUDModel<Orders> value)
        {
            if (value.changed != null && value.changed.Count > 0)
            {
                foreach (Orders Record in (IEnumerable<Orders>)value.changed)
                {
                    //Create query to update the changes into the database by accessing its properties
                    string queryStr = $"Update Orders set CustomerID='{Record.CustomerID}', Freight='{Record.Freight}',EmployeeID='{Record.EmployeeID}',ShipCity='{Record.ShipCity}' where OrderID='{Record.OrderID}'";

                    // Establishes a connection to the SQLite database using the provided connection string.
                    SqliteConnection Connection = new SqliteConnection(ConnectionString);

                    // Opens the database connection to execute queries.
                    Connection.Open();

                    //Execute the SQLite command.
                    SqliteCommand Command = new SqliteCommand(queryStr, Connection);

                    //Execute this code to reflect the changes into the database.
                    Command.ExecuteNonQuery();
                    Connection.Close();

                    //Add custom logic here if needed and remove above method.
                }
            }
            if (value.added != null && value.added.Count > 0)
            {
                foreach (Orders Record in (IEnumerable<Orders>)value.added)
                {
                    //Create query to insert the specific into the database by accessing its properties 
                    string queryStr = $"Insert into Orders(CustomerID,Freight,ShipCity,EmployeeID) values('{Record.CustomerID}','{Record.Freight}','{Record.ShipCity}','{Record.EmployeeID}')";

                    // Establishes a connection to the SQLite database using the provided connection string.
                    SqliteConnection Connection = new SqliteConnection(ConnectionString);

                    // Opens the database connection to execute queries.
                    Connection.Open();

                    //Execute the SQLite command.
                    SqliteCommand Command = new SqliteCommand(queryStr, Connection);

                    //Execute this code to reflect the changes into the database.
                    Command.ExecuteNonQuery();
                    Connection.Close();

                    //Add custom logic here if needed and remove above method.
                }
            }
            if (value.deleted != null && value.deleted.Count > 0)
            {
                foreach (Orders Record in (IEnumerable<Orders>)value.deleted)
                {
                    //Create query to remove the specific from database by passing the primary key column value.
                    string queryStr = $"Delete from Orders where OrderID={Record.OrderID}";

                    // Establishes a connection to the SQLite database using the provided connection string.
                    SqliteConnection Connection = new SqliteConnection(ConnectionString);

                    // Opens the database connection to execute queries.
                    Connection.Open();

                    //Execute the SQLite command.
                    SqliteCommand Command = new SqliteCommand(queryStr, Connection);

                    //Execute this code to reflect the changes into the database.
                    Command.ExecuteNonQuery();
                    Connection.Close();

                    //Add custom logic here if needed and remove above method.
                }
            }
            return new JsonResult(value);
        }


        public class CRUDModel<T> where T : class
        {
            public string? action { get; set; }
            public string? keyColumn { get; set; }
            public object? key { get; set; }
            public T? value { get; set; }
            public List<T>? added { get; set; }
            public List<T>? changed { get; set; }
            public List<T>? deleted { get; set; }
            public IDictionary<string, object>? @params { get; set; }
        }

        /// <summary>
        /// Retrieves the order data from the database.
        /// </summary>
        /// <returns>Returns a list of orders fetched from the database.</returns>
        [HttpGet]
        [Route("api/[controller]")]
        public List<Orders> GetOrderData()
        {
            // SQL query to fetch all records from the orders table and sort them by OrderID.
            string queryStr = "SELECT * FROM Orders ORDER BY OrderID;";

            // Creates an empty list to store the retrieved orders from the database.
            List<Orders> DataSource = new List<Orders>();

            // Establishes a connection to the SQLite database using the provided connection string.
            SqliteConnection Connection = new SqliteConnection(ConnectionString);

            // Opens the database connection to enable SQL queries.
            Connection.Open();

            //Using SqliteCommand and Query create connection with database.
            SqliteCommand Command = new SqliteCommand(queryStr, Connection);

            // Execute the SQLite command and retrieve data using SqliteDataReader.
            using (SqliteDataReader reader = Command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Orders order = new Orders
                    {
                        OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                        CustomerID = reader.GetString(reader.GetOrdinal("CustomerID")),
                        EmployeeID = reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                        ShipCity = reader.GetString(reader.GetOrdinal("ShipCity")),
                        Freight = reader.GetDecimal(reader.GetOrdinal("Freight"))
                    };
                    DataSource.Add(order);
                }
            }
            return DataSource;
        }

        public class Orders
        {
            [Key]
            public int? OrderID { get; set; }
            public string? CustomerID { get; set; }
            public int? EmployeeID { get; set; }
            public decimal? Freight { get; set; }
            public string? ShipCity { get; set; }
        }
    }
}
