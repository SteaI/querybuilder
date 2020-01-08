using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SqlKata
{
    public class SqlResult
    {
        public Query Query { get; set; }
        public string RawSql { get; set; } = "";
        public List<object> Bindings { get; set; } = new List<object>();
        public string Sql { get; set; } = "";
        public Dictionary<string, object> NamedBindings = new Dictionary<string, object>();

        private static readonly Type[] NumberTypes =
        {
            typeof(int),
            typeof(long),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(short),
            typeof(ushort),
            typeof(ulong),
        };

        public override string ToString()
        {
            var deepParameters = Helper.Flatten(Bindings).ToList();

            return Helper.ReplaceAll(RawSql, "?", i =>
            {
                if (i >= deepParameters.Count)
                {
                    throw new Exception(
                        $"Failed to retrieve a binding at the index {i}, the total bindings count is {Bindings.Count}");
                }

                var value = deepParameters[i];
                return ChangeToSqlValue(value);
            });
        }

        private static string ChangeToSqlValue(object value)
        {
            switch (value)
            {
                case null:
                case DBNull _:
                    return "NULL";

                case string strValue:
                    return $"'{EscapeSingleQuote(strValue)}'";

                case DateTime date:
                    if (date.Date == date)
                        return $"'{date:yyyy-MM-dd}'";

                    return $"'{date:yyyy-MM-dd HH:mm:ss}'";

                case bool vBool:
                    return vBool ? "true" : "false";

                case Enum vEnum:
                    return Convert.ToInt32(vEnum) + $" /* {vEnum} */";

                case var v when Helper.IsArray(v):
                    return Helper.JoinArray(",", value as IEnumerable);

                case var v when NumberTypes.Contains(v.GetType()):
                    return value.ToString();

                default:
                    return $"'{value}'";
            }
        }

        private static string EscapeSingleQuote(string value)
        {
            return value.Replace("'", "''");
        }
    }
}