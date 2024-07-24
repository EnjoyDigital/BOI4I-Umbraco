using BOI.Core.Search.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NPoco;

namespace BOI.Core.Search.Queries.SQL
{
    public interface IGetAllMediaLogs
    {
        Task<Page<MediaRequestLog>> Execute(long page);
    }

    public class GetAllMediaLogs : IGetAllMediaLogs
    {
        private readonly IConfiguration configuration;

        /// <summary>
        /// Creates a new query
        /// </summary>
        /// <param name="configuration">A configuration instance</param>
        public GetAllMediaLogs(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        
        public async Task<Page<MediaRequestLog>> Execute(long page)
        {
            var connectionString = configuration.GetConnectionString("cdb");

            var connection = new SqlConnection(connectionString);
            connection.Open();

            var db = new Database(connection) { OneTimeCommandTimeout = 3600 };

            var casualties = await db.PageAsync<MediaRequestLog>(page, 100000,
                @"SELECT [id],[MediaUrl],[DateViewed], mediaItemId FROM [MediaRequestLog]");

            connection.Close();

            return casualties;
        }
    }
}
