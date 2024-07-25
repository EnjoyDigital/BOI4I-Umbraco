using NPoco;
using System.Data.Common;

namespace BOI.Core.Web.Factories
{
    public interface IDatabaseFactory
    {
        Database GetDatabase();
    }

    public class DatabaseFactory : IDatabaseFactory
    {
        private static NPoco.DatabaseFactory internalFactory;

        public DatabaseFactory()
        {
            Configure();
        }
        private static void Configure()
        {
            //internalFactory = NPoco.DatabaseFactory.Config(x => x.UsingDatabase(() => new EnjoyDb("umbracoDbDSN")));
        }

        public Database GetDatabase()
        {
            return internalFactory.GetDatabase();
        }
    }

    public class EnjoyDb : Database
    {
        public EnjoyDb(DbConnection connection) : base(connection)
        {
        }

        //public EnjoyDb(string connectionStringName) : base(connectionStringName) { }

        protected override void OnException(Exception e)
        {
            base.OnException(e);
            e.Data["LastSQL"] = LastSQL;
        }
    }
}
