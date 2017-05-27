using JQ.ExpressionResolve;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LambdaPractice
{
    internal class Program
    {
        private static StringBuilder builder = new StringBuilder();
        private static int currentParameterIndex = 0;
        private static List<SqlParameter> dbParameterList = new List<SqlParameter>();
        private static string address = "11";

        private static void Main(string[] args)
        {
            int[] ages = new int[] { 1, 2, 5, 67 };
            Expression<Func<User, bool>> expression2 = m => (m.Age == 10 && m.Address.StartsWith("浙江") && m.Address.Contains("省") && m.Address.EndsWith("省")) || (m.Sex == 0 || "" == m.Address && ages.Contains(m.Age) && m.Age == ages[3]);
            // Expression<Func<User, bool>> expression2 = m => m.Address.TrimStart() == "11" && m.Name.TrimEnd() == "3434" && m.Age == 1 && m.IsDelete == true;
            //Expression<Func<User, bool>> expression2 = m => GetAge() ? m.Address == "" : m.Age == 1;

            //Expression<Func<User, bool>> expression2 = m => m.Age == 10 && m.Age == 1;
            SqlServerVisitor sqlVisitor = new SqlServerVisitor("A.");
            var sqlMember = sqlVisitor.GetSqlWhere(expression2.Body);
            Console.WriteLine(sqlMember.Item1);
            if (sqlMember.Item2 != null)
            {
                foreach (var item in sqlMember.Item2)
                {
                    Console.WriteLine($"{item?.ParameterName},{item?.Value}");
                }
            }

            Console.Read();
        }

        private static bool GetAge()
        {
            return false;
        }
    }

    internal class User
    {
        public string Name { get; set; }

        public string Address { get; set; }

        public int Age { get; set; }

        public int Sex { get; set; }

        public bool IsDelete { get; set; }

        public DateTime? CreateTime { get; set; }
    }
}