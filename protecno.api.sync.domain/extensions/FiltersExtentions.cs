using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace protecno.api.sync.domain.extensions
{
    public static class FiltersExtentions<T> where T : class
    {
        public static void FilterBuilder(T filterRQ, List<string> blackListFilds, out DynamicParameters dbArgs, out string sqlFilter)
        {
            dbArgs = new DynamicParameters();
            sqlFilter = " WHERE 1=1 ";
            DateTime date;
            foreach (var pair in filterRQ.GetType().GetProperties())
            {
                object value = GetPropertyValue(filterRQ, pair.Name);
                if (value != null && value.ToString() != "0" && !string.IsNullOrEmpty(value.ToString()) && !blackListFilds.Any(x => x == pair.Name))
                {
                    switch (pair.Name)
                    {
                        case "BaseInventarioId":
                            dbArgs.Add(pair.Name, value);
                            sqlFilter += $" AND T.BaseInventarioId = @{pair.Name} ";
                            break;

                        case "DataAquisicao":
                            date = Convert.ToDateTime(value).Date;
                            if (date.Year > 1900)
                            {
                                dbArgs.Add(pair.Name, date.ToString("yyyy/MM/d"));
                                sqlFilter += $" AND DATE_FORMAT(T.DataAquisicao, '%Y/%m/%d') = @{pair.Name}";
                            }
                            break; 

                        case "DataEntradaOperacao":
                            date = Convert.ToDateTime(value).Date;
                            if (date.Year > 1900)
                            {
                                dbArgs.Add(pair.Name, date.ToString("yyyy/MM/d"));
                                sqlFilter += $" AND DATE_FORMAT(T.DataEntradaOperacao, '%Y/%m/%d') = @{pair.Name}";
                            }
                            break; 

                        case "DataSync":
                            date = Convert.ToDateTime(value).Date;
                            if (date.Year > 1900)
                            {
                                dbArgs.Add(pair.Name, value, DbType.DateTime);
                                sqlFilter += $" AND T.DataSync >=  @{pair.Name} ";
                            }
                            break;

                        case "DataCadastro":
                            date = Convert.ToDateTime(value).Date;
                            if (date.Year > 1900)
                            {
                                dbArgs.Add(pair.Name, date.ToString("yyyy/MM/d"));
                                sqlFilter += $" AND DATE_FORMAT(T.DataCadastro, '%Y/%m/%d') = @{pair.Name}";
                            }
                            break;

                        case "DataAtualizacao":
                            date = Convert.ToDateTime(value).Date;
                            if (date.Year > 1900)
                            {
                                dbArgs.Add(pair.Name, date.ToString("yyyy/MM/d"));
                                sqlFilter += $" AND DATE_FORMAT(T.DataAtualizacao, '%Y/%m/%d') = @{pair.Name}";
                            }
                            break;

                        default:
                            if (value.ToString().Contains('%'))
                            {
                                dbArgs.Add(pair.Name, "%" + value.ToString().Replace("%", "").Replace("'", "") + "%");
                                sqlFilter += $" AND T.{pair.Name} LIKE @{pair.Name}";
                            }
                            else
                            {
                                dbArgs.Add(pair.Name, value);
                                sqlFilter += $" AND T.{pair.Name} = @{pair.Name}";
                            }
                            break;
                    }
                }
            }
        }

        private static object GetPropertyValue(object source, string propertyName)
        {
            var property = source.GetType().GetRuntimeProperties().FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));
            var value = property?.GetValue(source);

            return value;
        }
    }
}
