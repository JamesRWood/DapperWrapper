namespace DapperWrapper
{
    using Dapper;
    using DapperWrapper.Contracts;
    using DapperWrapper.Contracts.Attributes;
    using Extensions;
    using Npgsql;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public abstract class DapperBase<T> where T : IDbEntity
    {
        private const string Select = "SELECT";
        private const string InsertInto = "INSERT INTO";
        private const string Delete = "DELETE";
        private const string Update = "UPDATE";
        private const string From = "FROM";
        private const string Where = "WHERE";
        private const string Set = "SET";
        private const string Values = "VALUES";
        private const string And = "AND";

        private readonly NpgsqlConnection connection;

        protected readonly string baseSelectQueryString;
        protected readonly string baseInsertQueryString;
        protected readonly string baseDeleteQueryString;
        protected readonly string baseUpdateQueryString;

        protected DapperBase(
            NpgsqlConnection connection,
            string schema,
            string tableName)
        {
            this.connection = connection ?? throw new ArgumentNullException("Connection");

            var primaryKeyProperties = typeof(T).GetProperties()
                .Where(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(PrimaryKeyAttribute)))
                .Select(z => z.Name).ToList();

            var allProperties = typeof(T).GetProperties().Select(x => x.Name).ToList();

            var formattedTableName = $"{schema}.{tableName.ToDbIdentifier()}";

            this.baseSelectQueryString = ConstructBaseSelectQueryString(allProperties, formattedTableName);
            this.baseInsertQueryString = ConstructBaseInsertQueryString(allProperties, formattedTableName);
            this.baseDeleteQueryString = ConstructBaseDeleteQueryString(primaryKeyProperties, formattedTableName);
            this.baseUpdateQueryString = ConstructBaseUpateQueryString(formattedTableName);
        }

        protected T QuerySingle(string sql, DapperRequest request)
        {
            return this.connection.QuerySingle<T>(sql, DynamicParametersFactory.CreateParameters(request.Properties));
        }

        protected async Task<T> QuerySingleAsync(string sql, DapperRequest request)
        {
            return await this.connection.QuerySingleAsync<T>(sql, DynamicParametersFactory.CreateParameters(request.Properties));
        }

        protected IEnumerable<T> Query(string sql, DapperRequest request)
        {
            return this.connection.Query<T>(sql, DynamicParametersFactory.CreateParameters(request?.Properties));
        }

        protected async Task<IEnumerable<T>> QueryAsync(string sql, DapperRequest request)
        {
            return await this.connection.QueryAsync<T>(sql, DynamicParametersFactory.CreateParameters(request?.Properties));
        }

        protected int ExecuteCommand(string sql, DapperRequest request)
        {
            return this.connection.Execute(sql, DynamicParametersFactory.CreateParameters(request?.Properties));
        }

        protected async Task<int> ExecuteCommandAsync(string sql, DapperRequest request)
        {
            return await this.connection.ExecuteAsync(sql, DynamicParametersFactory.CreateParameters(request?.Properties));
        }

        protected string ConstructWhereClause(IList<(string Name, object Value)> properties)
        {
            return $"{Where} {ConvertToColumnEqualsString(properties.Select(x => x.Name)).AggregateWithDelimeter($" {And} ")};";
        }

        private static string ConstructBaseSelectQueryString(IEnumerable<string> properties, string tableName)
        {
            return $"{Select} {ConvertToColumnNamesString(properties).AggregateWithDelimeter(", ")}" +
                   $" {From} {tableName}";
        }

        private static string ConstructBaseInsertQueryString(IEnumerable<string> properties, string tableName)
        {
            return $"{InsertInto} {tableName} ({ConvertToColumnNamesString(properties).AggregateWithDelimeter(", ")})" +
                   $" {Values} ({ConvertToParamPointersString(properties).AggregateWithDelimeter(", ")});";
        }

        private static string ConstructBaseDeleteQueryString(IEnumerable<string> properties, string tableName)
        {
            return $"{Delete} {From} {tableName} {Where} {ConvertToColumnEqualsString(properties).AggregateWithDelimeter($" {And} ")};";
        }

        private static string ConstructBaseUpateQueryString(string tableName)
        {
            var primaryKeyPropertyAndParamString = new List<string>();
            var propertyAndParamString  =new List<string>();

            var props = typeof(T).GetProperties();
            for (int i = 0; i < props.Length; i++)
            {
                var cur = props[i];
                if (cur.CustomAttributes.Any(y => y.AttributeType == typeof(PrimaryKeyAttribute)))
                {
                    primaryKeyPropertyAndParamString.Add(cur.Name);
                }
                else
                {
                    propertyAndParamString.Add(cur.Name);
                }
            }

            return $"{Update} {tableName} {Set} {ConvertToColumnEqualsString(propertyAndParamString).AggregateWithDelimeter(", ")}" +
                   $" {Where} {ConvertToColumnEqualsString(primaryKeyPropertyAndParamString).AggregateWithDelimeter($" {And} ")};";
        }

        private static IEnumerable<string> ConvertToColumnNamesString(IEnumerable<string> input)
        {
            return input.Select(x => x.ToDbIdentifier());
        }

        private static IEnumerable<string> ConvertToParamPointersString(IEnumerable<string> input)
        {
            return input.Select(x => x.ToParamPointer());
        }

        private static IEnumerable<string> ConvertToColumnEqualsString(IEnumerable<string> input)
        {
            return input.Select(x => $"{x.ToDbIdentifier()} = {x.ToParamPointer()}");
        }
    }
}
