using Dapper;
using System;
using JHC.DbManager;
using MySql.Data.MySqlClient;
using JerryClient;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DbManager.Instance.Init("./dbconfig.config");
            MySqlConnection db =(MySqlConnection)DbManager.Instance.GetDbControl("mysql.reader");
            var rest=db.Query<dynamic>("select * from user");
            System.Console.WriteLine(rest);
            System.Console.ReadLine();
        }
    }
}
