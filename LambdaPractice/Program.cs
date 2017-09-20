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

        private static Delegate FuncHandle;

        private static void Main(string[] args)
        {
            //Expression<Func<User, int>> userLambda = (User u) => u.Age;

            var p = Expression.Parameter(typeof(User), "m");
            var memberExpression = Expression.Property(p, "Name");
            var expression = Expression.Lambda(memberExpression, p);
            Console.WriteLine(expression);

            //Func<int, int, int> calculate = (x, y) => { return x + y; };//计算x+y的lambda
            //Console.WriteLine(calculate(1, 2).ToString());//输出3
            //Console.WriteLine(calculate(2, 5).ToString());//输出7

            //ParameterExpression leftExpression = Expression.Parameter(typeof(int), "m");//int类型的，参数名称为m
            //ConstantExpression rightExpression = Expression.Constant(10, typeof(int));//常量表达式树，10
            ////进行左边是否大于右边的判断
            //var binaryExpression =Expression.GreaterThan(leftExpression, rightExpression);
            //Console.WriteLine(binaryExpression.ToString());
            //var lambda =Expression.Lambda<Func<int,bool>>(binaryExpression, leftExpression).Compile();//构建成lambda表达式
            //Console.WriteLine(lambda(5).ToString());

            //ParameterExpression left1Expression= Expression.Parameter(typeof(int), "a");//int类型的，参数名称为a
            //ParameterExpression right1Expression = Expression.Parameter(typeof(int), "b");//int类型的，参数名称为b
            //var aAndbExpression = Expression.Add(left1Expression, right1Expression);//进行相加拼接
            //ParameterExpression cExpression= Expression.Parameter(typeof(int), "c");//int类型的，参数名称为c
            //var finallyExpression = Expression.GreaterThan(aAndbExpression, cExpression);
            //Console.WriteLine(finallyExpression.ToString());
            //var finallyLambda= Expression.Lambda<Func<int,int,int, bool>>(finallyExpression, left1Expression, right1Expression, cExpression).Compile();//构建成lambda表达式
            //Console.WriteLine(finallyLambda(1, 6, 10).ToString());


            int[] ages = new int[] { 1, 2, 5, 67 };
            //Expression<Func<User, bool>> expression2 = m => (m.Age == 10 && m.Address.StartsWith("浙江") && m.Address.Contains("省") && m.Address.EndsWith("省")) || (m.Sex == 0 || "" == m.Address && ages.Contains(m.Age) && m.Age == ages[3]);
            //Expression<Func<User, bool>> expression2 = m => m.Address.TrimStart() == "11" && m.Name.TrimEnd() == "3434" && m.Age == 1 && m.IsDelete == true;
            Expression<Func<User, bool>> expression2 = m => GetAge() ? m.Address == "" : m.Age == 1;

            //Expression<Func<User, bool>> expression2 = m => m.Age == 10 && m.Age == 1 && m.Sex == GeneralType.Man && Convert.ToInt32("1") == 1;
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

        public GeneralType Sex { get; set; }

        public bool IsDelete { get; set; }

        public DateTime? CreateTime { get; set; }
    }

    public enum GeneralType
    {
        Man,
        WoMan,
        Other
    }
}