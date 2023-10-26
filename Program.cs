using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

public class Program
{
    private const string ConnectionString = "Data Source=localhost;Initial Catalog=YOUR_DATABASE;Integrated Security=True;";

    public static void Main()
    {
        var result = GetTablesWithAndWithoutData();

        Console.WriteLine($"Total number of tables: {result.TablesWithData.Count + result.TablesWithoutData.Count}");

        Console.WriteLine($"\nCount of tables with data: {result.TablesWithData.Count}");
        Console.WriteLine("Tables with data:");
        foreach (var table in result.TablesWithData)
        {
            Console.WriteLine(table);
        }

        Console.WriteLine($"\nCount of tables without data: {result.TablesWithoutData.Count}");
        Console.WriteLine("Tables without data:");
        foreach (var table in result.TablesWithoutData)
        {
            Console.WriteLine(table);
        }
    }

    public static (List<string> TablesWithData, List<string> TablesWithoutData) GetTablesWithAndWithoutData()
    {
        var tablesWithData = new List<string>();
        var tablesWithoutData = new List<string>();

        using (var connection = new SqlConnection(ConnectionString))
        {
            connection.Open();

            var query = @"
                SELECT s.name + '.' + t.name AS TableName, SUM(p.rows) as TotalRows
                FROM sys.tables t
                INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                INNER JOIN sys.partitions p ON t.object_id = p.object_id
                WHERE p.index_id IN (0, 1) 
                GROUP BY s.name, t.name";

            using (var command = new SqlCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var tableName = reader["TableName"].ToString();
                    var rowCount = Convert.ToInt32(reader["TotalRows"]);

                    if (rowCount > 0)
                        tablesWithData.Add(tableName);
                    else
                        tablesWithoutData.Add(tableName);
                }
            }
        }

        return (tablesWithData, tablesWithoutData);
    }
}
