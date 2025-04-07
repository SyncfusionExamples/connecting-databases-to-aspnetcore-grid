using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.Base;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Grid_MSSQL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GridController : ControllerBase
    {
        string ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\asp.core\Binding-data-from-remote-service-to-angular-data-grid\UrlAdaptor\UrlAdaptor.Server\App_Data\NORTHWND.MDF;Integrated Security=True;Connect Timeout=30;Encrypt=True";

        /// <summary>
        /// Processes the DataManager request to perform searching, filtering, sorting, and paging operations.
        /// </summary>
        /// <param name="DataManagerRequest">Contains the details of the data operation requested.</param>
        /// <returns>Returns a JSON object with the filtered, sorted, and paginated data along with the total record count.</returns>
        [HttpPost]
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
        /// Retrieves the order data from the database.
        /// </summary>
        /// <returns>Returns a list of orders fetched from the database.</returns>
        [HttpGet]
        public List<Orders> GetOrderData()
        {
            string queryStr = "SELECT * FROM dbo.Orders ORDER BY OrderID;";
            SqlConnection sqlConnection = new(ConnectionString);
            sqlConnection.Open();
            SqlCommand sqlCommand = new(queryStr, sqlConnection);
            SqlDataAdapter DataAdapter = new(sqlCommand);
            DataTable DataTable = new();
            DataAdapter.Fill(DataTable);
            sqlConnection.Close();

            // Map data to a list.
            List<Orders> dataSource = (from DataRow Data in DataTable.Rows
                                       select new Orders()
                                       {
                                           OrderID = Convert.ToInt32(Data["OrderID"]),
                                           CustomerID = Data["CustomerID"].ToString(),
                                           EmployeeID = Convert.IsDBNull(Data["EmployeeID"]) ? 0 : Convert.ToUInt16(Data["EmployeeID"]),
                                           ShipCity = Data["ShipCity"].ToString(),
                                           Freight = Convert.ToDecimal(Data["Freight"])
                                       }).ToList();
            return dataSource;
        }


        /// <summary>
        /// Inserts a new data item into the data collection.
        /// </summary>
        /// <param name="value">It contains the new record detail which is need to be inserted.</param>
        /// <returns>Returns void.</returns>
        [HttpPost("Insert")]
        public void Insert([FromBody] CRUDModel<Orders> value)
        {
            //Create query to insert the specific into the database by accessing its properties.
            string queryStr = $"Insert into Orders(CustomerID,Freight,ShipCity,EmployeeID) values('{value.value.CustomerID}','{value.value.Freight}','{value.value.ShipCity}','{value.value.EmployeeID}')";
            SqlConnection SqlConnection = new SqlConnection(ConnectionString);
            SqlConnection.Open();

            //Execute the SQL command.
            SqlCommand SqlCommand = new SqlCommand(queryStr, SqlConnection);

            //Execute this code to reflect the changes into the database.
            SqlCommand.ExecuteNonQuery();
            SqlConnection.Close();

            //Add custom logic here if needed and remove above method.
        }

        /// <summary>
        /// Update a existing data item from the data collection.
        /// </summary>
        /// <param name="value">It contains the updated record detail which is need to be updated.</param>
        /// <returns>Returns void.</returns>
        [HttpPost("Update")]
        public void Update([FromBody] CRUDModel<Orders> value)
        {
            //Create query to update the changes into the database by accessing its properties.
            string queryStr = $"Update Orders set CustomerID='{value.value.CustomerID}', Freight='{value.value.Freight}',EmployeeID='{value.value.EmployeeID}',ShipCity='{value.value.ShipCity}' where OrderID='{value.value.OrderID}'";
            SqlConnection SqlConnection = new SqlConnection(ConnectionString);
            SqlConnection.Open();

            //Execute the SQL command.
            SqlCommand SqlCommand = new SqlCommand(queryStr, SqlConnection);

            //Execute this code to reflect the changes into the database.
            SqlCommand.ExecuteNonQuery();
            SqlConnection.Close();

            //Add custom logic here if needed and remove above method.
        }

        /// <summary>
        /// Remove a specific data item from the data collection.
        /// </summary>
        /// <param name="value">It contains the specific record detail which is need to be removed.</param>
        /// <return>Returns void.</return>
        [HttpPost("Remove")]
        public void Remove([FromBody] CRUDModel<Orders> value)
        {
            //Create query to remove the specific from database by passing the primary key column value.
            string queryStr = $"Delete from Orders where OrderID={value.key}";
            SqlConnection SqlConnection = new SqlConnection(ConnectionString);
            SqlConnection.Open();

            //Execute the SQL command.
            SqlCommand SqlCommand = new SqlCommand(queryStr, SqlConnection);

            //Execute this code to reflect the changes into the database.
            SqlCommand.ExecuteNonQuery();
            SqlConnection.Close();

            //Add custom logic here if needed and remove above method.
        }

        /// <summary>
        /// Batch update (Insert, Update, and Delete) a collection of data items from the data collection.
        /// </summary>
        /// <param name="value">The set of information along with details about the CRUD actions to be executed from the database.</param>
        /// <returns>Returns void.</returns>
        [HttpPost("BatchUpdate")]
        public IActionResult BatchUpdate([FromBody] CRUDModel<Orders> value)
        {
            if (value.changed != null && value.changed.Count > 0)
            {
                foreach (Orders Record in (IEnumerable<Orders>)value.changed)
                {
                    //Create query to update the changes into the database by accessing its properties.
                    string queryStr = $"Update Orders set CustomerID='{Record.CustomerID}', Freight='{Record.Freight}',EmployeeID='{Record.EmployeeID}',ShipCity='{Record.ShipCity}' where OrderID='{Record.OrderID}'";
                    SqlConnection SqlConnection = new SqlConnection(ConnectionString);
                    SqlConnection.Open();

                    //Execute the SQL command.
                    SqlCommand SqlCommand = new SqlCommand(queryStr, SqlConnection);

                    //Execute this code to reflect the changes into the database.
                    SqlCommand.ExecuteNonQuery();
                    SqlConnection.Close();

                    //Add custom logic here if needed and remove above method.
                }
            }
            if (value.added != null && value.added.Count > 0)
            {
                foreach (Orders Record in (IEnumerable<Orders>)value.added)
                {
                    //Create query to insert the specific into the database by accessing its properties.
                    string queryStr = $"Insert into Orders(CustomerID,Freight,ShipCity,EmployeeID) values('{Record.CustomerID}','{Record.Freight}','{Record.ShipCity}','{Record.EmployeeID}')";
                    SqlConnection SqlConnection = new SqlConnection(ConnectionString);
                    SqlConnection.Open();

                    //Execute the SQL command.
                    SqlCommand SqlCommand = new SqlCommand(queryStr, SqlConnection);

                    //Execute this code to reflect the changes into the database.
                    SqlCommand.ExecuteNonQuery();
                    SqlConnection.Close();

                    //Add custom logic here if needed and remove above method.
                }
            }
            if (value.deleted != null && value.deleted.Count > 0)
            {
                foreach (Orders Record in (IEnumerable<Orders>)value.deleted)
                {
                    //Create query to remove the specific from database by passing the primary key column value.
                    string queryStr = $"Delete from Orders where OrderID={Record.OrderID}";
                    SqlConnection SqlConnection = new SqlConnection(ConnectionString);
                    SqlConnection.Open();

                    //Execute the SQL command.
                    SqlCommand SqlCommand = new SqlCommand(queryStr, SqlConnection);

                    //Execute this code to reflect the changes into the database.
                    SqlCommand.ExecuteNonQuery();
                    SqlConnection.Close();

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
