using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Syncfusion.EJ2.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Grid_EF.Controllers
{
    [ApiController]
    public class GridController : ControllerBase
    {
        string ConnectionString = @"<Enter connection string>";

        /// <summary>
        /// Processes the DataManager request to perform searching, filtering, sorting, and paging operations.
        /// </summary>
        /// <param name="DataManagerRequest">Contains the details of the data operation requested.</param>
        /// <returns>Returns a JSON object along with the total record count.</returns>
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
        /// Retrieves the order data from the database.
        /// </summary>
        /// <returns>Returns a list of orders fetched from the database.</returns>
        [HttpGet]
        [Route("api/[controller]")]
        public List<Orders> GetOrderData()
        {
            using (OrderDbContext Context = new OrderDbContext(ConnectionString))
            {
                // Retrieve orders from the orders DbSet and convert to list asynchronously.
                List<Orders> orders = Context.Orders.ToList();
                return orders;
            }
        }

        // Create a class that inherits from DbContext(Entity Framework Core).
        public class OrderDbContext : DbContext
        {
            //Declare a private variable to store the connection string.
            private readonly string _ConnectionString;

            //Define a constructor that accepts a connection string.
            public OrderDbContext(string ConnectionString)
            {
                //Store the provided connection string.
                _ConnectionString = ConnectionString;
            }

            //Override the onConfiguring method to tell EF Core to use SQL server.
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                // Use the connection string to configure the database connection.
                optionsBuilder.UseSqlServer(_ConnectionString);
            }

            // Define a DbSet to represent the orders table in the database.
            public DbSet<Orders> Orders { get; set; }
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
            using (OrderDbContext Context = new OrderDbContext(ConnectionString))
            {
                // Add the provided order to the orders DbSet.
                Context.Orders.Add(value.value);

                // Save changes to the database.
                Context.SaveChanges();
            }

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
            using (OrderDbContext Context = new OrderDbContext(ConnectionString))
            {
                Orders existingOrder = Context.Orders.Find(value.value.OrderID);
                if (existingOrder != null)
                {
                    // Update the existing order with the new values.
                    Context.Entry(existingOrder).CurrentValues.SetValues(value.value);

                    // Save changes to the database.
                    Context.SaveChanges();
                }
            }

            //Add custom logic here if needed and remove above method.
        }

        /// <summary>
        /// Remove a specific data item from the data collection.
        /// </summary>
        /// <param name="value">It contains the specific record detail which is need to be removed.</param>
        /// <return>Returns void.</return>
        [HttpPost]
        [Route("api/[controller]/Remove")]
        public void Remove([FromBody] CRUDModel<Orders> value)
        {
            int OrderId = Convert.ToInt32(value.key.ToString());
            using (OrderDbContext Context = new OrderDbContext(ConnectionString))
            {
                Orders Order = Context.Orders.Find(OrderId);
                if (Order != null)
                {
                    // Remove the order from the orders DbSet.
                    Context.Orders.Remove(Order);

                    // Save changes to the database.
                    Context.SaveChanges();
                }
            }

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
            using (OrderDbContext Context = new OrderDbContext(ConnectionString))
            {
                if (value.changed != null && value.changed.Count > 0)
                {
                    foreach (Orders Record in (IEnumerable<Orders>)value.changed)
                    {
                        // Update the changed records.
                        Context.Orders.UpdateRange(Record);
                    }
                }

                if (value.added != null && value.added.Count > 0)
                {
                    foreach (Orders Record in (IEnumerable<Orders>)value.added)
                    {
                        foreach (Orders order in value.added)
                        {
                            // This ensures EF does not try to insert OrderID.
                            order.OrderID = default;
                        }
                        // Add new records.
                        Context.Orders.AddRange(value.added);
                    }
                }

                if (value.deleted != null && value.deleted.Count > 0)
                {
                    foreach (Orders Record in (IEnumerable<Orders>)value.deleted)
                    {
                        // Find and delete the records.
                        Orders ExistingOrder = Context.Orders.Find(Record.OrderID);
                        if (ExistingOrder != null)
                        {
                            Context.Orders.Remove(ExistingOrder);
                        }
                    }
                }

                // Save changes to the database.
                Context.SaveChanges();
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
            public decimal Freight { get; set; }
            public string? ShipCity { get; set; }
        }
    }
}