# DapperWrapper
 Simple wrapper for DB interactions using the Dapper micro ORM

 Currently this only handles simple CRUD operations on single entities, at some point in the future I might add support for joins and more complex operations.
 
 
 Usage in a simple repository would look something like this:
 
 ```csharp
     public class StorageRepository : DapperBase<Storage>, IStorageRepository
    {
        private const string TableName = "Storage";

        public StorageRepository(NpgsqlConnection connection) : base(connection, DapperConstants.Schema, TableName)
        {
        }

        public IEnumerable<IStorageModel> GetStorageOptions()
        {
            var response = this.Query(this.baseSelectQueryString, null);
            return response.Select(x => new StorageModel
            {
                Id = x.Id,
                Capacity = x.Capacity,
                Technology = x.Technology,
                Price = x.Price
            }).ToList();
        }

        public IStorageModel GetStorage(Guid id)
        {
            var props = new List<(string Name, object Value)>
            {
                (nameof(id), id)
            };

            var response = this.QuerySingle($"{this.baseSelectQueryString} {this.ConstructWhereClause(props)}", new DapperRequest { Properties = props });

            return response == null ? null : new StorageModel
            {
                Id = response.Id,
                Capacity = response.Capacity,
                Technology = response.Technology,
                Price = response.Price
            };
        }
    }
 ```
 
 On construction the DapperBase class discovers all properties of the object type it is given to build a collection of vanilla queries to handle CRUD operations, these queries are assigned to protected readonly strings that the inheriting class can make use of (this.baseSelectQueryString, etc)
 
 For the time being where clauses need to be constructed by explicitly calling the this.ConstructWhereClause(IList<(string Name, object Value)> properties) method.
