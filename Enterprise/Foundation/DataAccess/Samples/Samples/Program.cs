using RP.Enterprise.Foundation.DataAccess.Component;
using System;

namespace Samples
{
    public abstract class BaseRepository
    {
        IConnectionFactory connectionFactory = new ConnectionFactory();
        IUnitOfWork uow;
        IRepository repository;

        public IRepository GetRepository()
        {
            uow = new DapperUnitOfWork(connectionFactory);
            repository = new DapperRepository(uow);
            repository.UnitOfWork.Initialize("server=localhost; initial catalog=Identity; integrated security=yes");
            return repository;
        }
    }
    class Program
    {

        static void Main(string[] args)
        {
            var ds = new DapperSamples();
            ds.GetUser(1);
        }

    }

    public class DapperSamples : BaseRepository
    {
        public void GetUser(int userId)
        {
            //dynamic parameter =  new   {userId,zip,param1};
            try
            {
                using (var repo = GetRepository())
                {
                    var x = repo.GetOne<Scope>("GetAllScopes", null);
                }
            }
            catch (Exception EX)
            {
                Console.Write(EX.Message);
            }
        }
    }

    public class Scope
    {
        public int ScopeId { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public bool Emphasize { get; set; }
        public int Type { get; set; }
        public bool IncludeAllClaimsForUser { get; set; }
        public string ClaimsRule { get; set; }
        public bool ShowInDiscoveryDocument { get; set; }
        public bool AllowUnrestrictedIntrospection { get; set; }
    }
}
