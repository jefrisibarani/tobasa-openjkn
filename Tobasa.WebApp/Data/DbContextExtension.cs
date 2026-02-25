/*
    Tobasa OpenJKN Bridge
    Copyright (C) 2020-2026 Jefri Sibarani
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

// NOTE
// https://stackoverflow.com/questions/35631903/raw-sql-query-without-dbset-entity-framework-core
// https://stackoverflow.com/a/53918008/3339585

using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Tobasa.Data
{
    public enum ProviderType
    {
        SQLITE,
        MYSQL,
        OLEDB
    }

    public static class DbContextCommandExtensions
    {
        public static string GetDateTimeSqlString(this DbContext context)
        {
            if (context.GetClassName() == "DataContextAntrianSQLite")
            {
                return "strftime('%Y-%m-%d %H:%M:%f','now', 'localtime')";
            }
            else if (context.GetClassName() == "DataContextAntrianMySql")
            {
                return "CURDATE()";
            }
            else
            {
                return "getdate()";
            }
        }

        public static string GetClassName(this DbContext context)
        {
            Type type = context.GetType();
            return type.Name;
        }

        public static DbConnection Connection(this DbContext context)
        {
            var conn = context.Database.GetDbConnection();
            
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            return conn;
        }

        public static DbParameter AddParameter(this DbContext context, DbCommand command, string paramName, object value, DbType dataType)
        {
            DbParameter param = command.CreateParameter();
            param.ParameterName = paramName;
            param.Value = value;
            param.DbType = dataType;

            command.Parameters.Add(param);
            return param;
        }

        public static DbParameter Parameter(
                                    this DbContext context,
                                    string parName,
                                    DbType parType,
                                    object parValue,
                                    int parSize)
        {
            var conn = context.Database.GetDbConnection();
            var parameter =  conn.CreateCommand().CreateParameter();

            parameter.Value           = parValue;
            parameter.ParameterName   = parName;
            parameter.DbType          = parType;
            parameter.Size            = parSize;
            
            return parameter;
        }

        public static async Task<int> ExecuteNonQueryAsync(
                                        this DbContext context,
                                        string rawSql,
                                        params object[] parameters)
        {
            var conn = context.Database.GetDbConnection();

            using var command = conn.CreateCommand();
            command.CommandText = rawSql;

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    command.Parameters.Add(p);
                }
            }

            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }

            return await command.ExecuteNonQueryAsync();
        }

        public static async Task<T> ExecuteScalarAsync<T>(
                                        this DbContext context,
                                        string rawSql,
                                        params object[] parameters)
        {
            var conn = context.Database.GetDbConnection();

            using (var command = conn.CreateCommand())
            {
                command.CommandText = rawSql;

                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        command.Parameters.Add(p);
                    }
                }

                if (conn.State != ConnectionState.Open) {
                    await conn.OpenAsync();
                }

                return (T) await command.ExecuteScalarAsync();
            }
        }

        public static async Task<DbDataReader> ExecuteQueryAsync(
                                        this DbContext context,
                                        string rawSql,
                                        params object[] parameters)
        {
            var conn = context.Database.GetDbConnection();

            using (var command = conn.CreateCommand())
            {
                command.CommandText = rawSql;

                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        command.Parameters.Add(p);
                    }
                }

                if (conn.State != ConnectionState.Open)
                {
                    await conn.OpenAsync();
                }

                var dataReader = await command.ExecuteReaderAsync();

                return dataReader;
            }
        }


        public static async Task<int> ExecuteStoredProcedureAsync(
                                        this DbContext context,
                                        string procedureName,
                                        params object[] parameters)
        {
            var conn = context.Database.GetDbConnection();

            using (var command = conn.CreateCommand()/*new SqlCommand(procedureName,(SqlConnection)conn)*/ )
            {
                command.CommandText = procedureName;
                command.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        command.Parameters.Add(p);
                    }
                }

                if (conn.State != ConnectionState.Open) {
                    await conn.OpenAsync();
                }

                return await command.ExecuteNonQueryAsync();
            }
        }

    }
}