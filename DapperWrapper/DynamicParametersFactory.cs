namespace DapperWrapper
{
    using Dapper;
    using DapperWrapper.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    internal static class DynamicParametersFactory
    {
        internal static DynamicParameters CreateParameters(IList<(string Name, object Value)> input)
        {
            if (input == null || input.Count() < 1)
            {
                return null;
            }

            var parameters = new DynamicParameters();
            for (int i = 0; i < input.Count; i++)
            {
                parameters.Add(input[i].Name.UpperCaseFirstChar(), input[i].Value, GetDbType(input[i].Value));
            }

            return parameters;
        }

        private static DbType GetDbType(object value)
        {
            var valueType = value.GetType();

            if (valueType == typeof(Guid))
            {
                return DbType.Guid;
            }
            else if (valueType == typeof(int))
            {
                return DbType.Int32;
            }
            else if (valueType == typeof(decimal))
            {
                return DbType.Decimal;
            }
            else if (valueType == typeof(bool))
            {
                return DbType.Boolean;
            }
            else if (valueType == typeof(DateTime))
            {
                return DbType.DateTime;
            }

            return DbType.String;
        }
    }
}
