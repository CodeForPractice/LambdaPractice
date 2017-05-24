using JQ.LambdaResolve;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LambdaPractice
{
    class Program
    {
        static StringBuilder builder = new StringBuilder();
        static int currentParameterIndex = 0;
        static List<SqlParameter> dbParameterList = new List<SqlParameter>();
        static string address = "11";

        static void Main(string[] args)
        {

            //TODO   ExpressionType.ArrayIndex

            var aaa = new object[] { 11111, 111 };
            Console.WriteLine(aaa.GetType().IsArray);
            IEnumerable<Program> list = new List<Program>();
            Console.WriteLine(list.GetType().IsGenericType);
            int? aaaas = 11;
            Console.WriteLine(aaaas.GetType().IsValueType);
            int[] ages = new int[] { 1, 2, 5, 67 };
            //Expression<Func<User, bool>> expression2 = m => (m.Age == 10 && m.Address.StartsWith("浙江") && m.Address.Contains("省") && m.Address.EndsWith("省")) || (m.Sex == 0 || "" == m.Address && ages.Contains(m.Age) && m.Age == ages[3]);
            //Expression<Func<User, bool>> expression2 = m => m.Address.TrimStart() == "11" && m.Name.TrimEnd() == "3434" && m.Age == 1 && m.IsDelete == true;
            Expression<Func<User, bool>> expression2 = m => GetAge() ? m.Address == "" : m.Age == 1;

            //Expression<Func<User, bool>> expression2 = m => m.Age == 10;
            Expression2SqlVisitor sqlVisitor = new Expression2SqlVisitor();
            var sqlMember = sqlVisitor.Resolve(expression2.Body);
            sqlMember.Output();

            Console.Read();
        }

        static bool GetAge()
        {
            return false;
        }

        static void ResolveExpression(Expression expression)
        {
            var nodeType = expression.NodeType;
            Console.WriteLine("类型" + nodeType.ToString());
            if (expression is BinaryExpression)
            {
                Warn("BinaryExpression");
                ResolveBinaryExpression(expression as BinaryExpression);
            }
            else if (expression is BlockExpression)
            {
                Warn("BlockExpression");
            }
            else if (expression is ConditionalExpression)
            {
                Warn("ConditionalExpression");
            }
            else if (expression is ConstantExpression)
            {
                Warn("ConstantExpression");
                ResolveConstantExpression(expression as ConstantExpression);
            }
            else if (expression is DebugInfoExpression)
            {
                Warn("DebugInfoExpression");
            }
            else if (expression is DefaultExpression)
            {
                Warn("DefaultExpression");
            }
            else if (expression is DynamicExpression)
            {
                Warn("DynamicExpression");
            }
            else if (expression is GotoExpression)
            {
                Warn("GotoExpression");
            }
            else if (expression is IndexExpression)
            {
                Warn("IndexExpression");
            }
            else if (expression is InvocationExpression)
            {
                Warn("InvocationExpression");
            }
            else if (expression is ListInitExpression)
            {
                Warn("ListInitExpression");
            }
            else if (expression is LambdaExpression)
            {
                Warn("LambdaExpression");
            }
            else if (expression is LoopExpression)
            {
                Warn("LoopExpression");
            }
            else if (expression is MemberExpression)
            {
                Warn("MemberExpression");
                ResolveMemberExpression(expression as MemberExpression);
            }
            else if (expression is MemberInitExpression)
            {
                Warn("MemberInitExpression");
            }
            else if (expression is MethodCallExpression)
            {
                Warn("MethodCallExpression");
                ResolveMethodCallExpression(expression as MethodCallExpression);
            }
            else if (expression is NewArrayExpression)
            {
                Warn("NewArrayExpression");
            }
            else if (expression is NewExpression)
            {
                Warn("NewExpression");
            }
            else if (expression is ParameterExpression)
            {
                Warn("ParameterExpression");
                ResolveParameterExpression(expression as ParameterExpression);
            }
            else if (expression is SwitchExpression)
            {
                Warn("SwitchExpression");
            }
            else if (expression is TryExpression)
            {
                Warn("TryExpression");
            }
            else if (expression is RuntimeVariablesExpression)
            {
                Warn("RuntimeVariablesExpression");
            }
        }

        static void ResolveBinaryExpression(BinaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.ArrayIndex)
            {
                throw new NotSupportedException("ArrayIndex");
            }
            else
            {
                builder.Append("(");
                var expressionLeft = expression.Left;
                ResolveExpression(expressionLeft);
                ResolveExpressionType(expression.NodeType);
                var expressionRight = expression.Right;
                ResolveExpression(expressionRight);
                builder.Append(")");
            }
        }
        static void ResolveParameterExpression(ParameterExpression expression)
        {
            Console.WriteLine(expression.Name);
            Console.WriteLine(expression.Type.Name);
        }

        static void ResolveMemberExpression(MemberExpression expression)
        {
            Info(expression.Member.Name);
            builder.Append(expression.Member.Name);
        }

        static void ResolveConstantExpression(ConstantExpression expression)
        {
            string parameterName = $"@Parameter_{currentParameterIndex.ToString()}";
            currentParameterIndex++;
            builder.Append(parameterName);
            dbParameterList.Add(new SqlParameter { Value = expression.Value, SqlDbType = TypeString2SqlType(expression.Type.Name.ToLower()), ParameterName = parameterName });
        }

        static void ResolveMethodCallExpression(MethodCallExpression expression)
        {
            if (expression.Object != null)
            {
                ResolveExpression(expression.Object);
            }
            string methodName = expression.Method.Name;
            Debug($"MethodName:{methodName}");
            switch (methodName)
            {
                case "StartsWith":
                    ResolveMethodStartsWith(expression);
                    break;
            }

        }
        static void ResolveMethodStartsWith(MethodCallExpression expression)
        {
            builder.Append(" LIKE '%'+");
            if (expression.Arguments != null)
            {
                Error("方法参数:");
                foreach (var item in expression.Arguments)
                {
                    ResolveExpression(item);
                }
                Error("===");
            }
        }


        public static void ResolveExpressionType(ExpressionType expressionType)
        {
            Warn(expressionType.ToString());
            switch (expressionType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.AddChecked:
                    builder.Append("+");
                    break;
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.AndAssign:
                    builder.Append(" AND ");
                    break;

                case ExpressionType.LessThan:
                    builder.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    builder.Append(" <= ");
                    break;
                case ExpressionType.Equal:
                    builder.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    builder.Append(" != ");
                    break;
                case ExpressionType.Not:
                    builder.Append(" NOT ");
                    break;
                case ExpressionType.GreaterThan:
                    builder.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    builder.Append(" >= ");
                    break;
                case ExpressionType.OrElse:
                    builder.Append(" OR ");
                    break;
            }
        }

        public static Expression<Func<int, int, int, int, int>> ToExpression3()
        {
            ParameterExpression parameter1 = Expression.Parameter(typeof(int), "a");
            ParameterExpression parameter2 = Expression.Parameter(typeof(int), "b");
            BinaryExpression binaryExpression1 = Expression.Multiply(parameter1, parameter2);
            ParameterExpression parameter3 = Expression.Parameter(typeof(int), "c");
            ParameterExpression parameter4 = Expression.Parameter(typeof(int), "d");
            BinaryExpression binaryExpression2 = Expression.Add(parameter3, parameter4);
            return Expression.Lambda<Func<int, int, int, int, int>>(Expression.Multiply(binaryExpression1, binaryExpression2), parameter1, parameter2, parameter3, parameter4);
        }

        public static Expression<Func<User, bool>> ToExpression4(int age)
        {
            ParameterExpression parameter1 = Expression.Parameter(typeof(User), "m");
            MemberExpression member1 = Expression.Property(parameter1, "Age");
            BinaryExpression binary = Expression.LessThanOrEqual(member1, Expression.Constant(age, typeof(int)));
            return Expression.Lambda<Func<User, bool>>(binary, parameter1);
        }

        /// <summary>
        /// 根据类型对应类型获取对应数据库对应的类型
        /// </summary>
        /// <param name="infoTypeString">类型类型</param>
        /// <returns>数据库对应的类型</returns>
        public static SqlDbType TypeString2SqlType(string infoTypeString)
        {
            SqlDbType dbType = SqlDbType.Binary;//默认为Object

            switch (infoTypeString)
            {
                case "int16":
                case "int32":
                    dbType = SqlDbType.Int;
                    break;

                case "string":
                case "varchar":
                    dbType = SqlDbType.NVarChar;
                    break;

                case "boolean":
                case "bool":
                case "bit":
                    dbType = SqlDbType.Bit;
                    break;

                case "datetime":
                    dbType = SqlDbType.DateTime;
                    break;

                case "decimal":
                    dbType = SqlDbType.Decimal;
                    break;

                case "float":
                    dbType = SqlDbType.Float;
                    break;

                case "image":
                    dbType = SqlDbType.Image;
                    break;

                case "money":
                    dbType = SqlDbType.Money;
                    break;

                case "ntext":
                    dbType = SqlDbType.NText;
                    break;

                case "nvarchar":
                    dbType = SqlDbType.NVarChar;
                    break;

                case "smalldatetime":
                    dbType = SqlDbType.SmallDateTime;
                    break;

                case "smallint":
                    dbType = SqlDbType.SmallInt;
                    break;

                case "text":
                    dbType = SqlDbType.Text;
                    break;

                case "int64":
                case "bigint":
                    dbType = SqlDbType.BigInt;
                    break;

                case "binary":
                    dbType = SqlDbType.Binary;
                    break;

                case "char":
                    dbType = SqlDbType.Char;
                    break;

                case "nchar":
                    dbType = SqlDbType.NChar;
                    break;

                case "numeric":
                    dbType = SqlDbType.Decimal;
                    break;

                case "real":
                    dbType = SqlDbType.Real;
                    break;

                case "smallmoney":
                    dbType = SqlDbType.SmallMoney;
                    break;

                case "sql_variant":
                    dbType = SqlDbType.Variant;
                    break;

                case "timestamp":
                    dbType = SqlDbType.Timestamp;
                    break;

                case "tinyint":
                    dbType = SqlDbType.TinyInt;
                    break;

                case "uniqueidentifier":
                    dbType = SqlDbType.UniqueIdentifier;
                    break;

                case "varbinary":
                    dbType = SqlDbType.VarBinary;
                    break;

                case "xml":
                    dbType = SqlDbType.Xml;
                    break;
            }
            return dbType;
        }

        static Logger GetLogger()
        {
            return NLog.LogManager.GetLogger("*");
        }

        public static void Debug(string msg)
        {
            GetLogger().Debug(msg);
        }

        public static void Info(string msg)
        {
            GetLogger().Info(msg);
        }
        public static void Warn(string msg)
        {
            GetLogger().Warn(msg);
        }
        public static void Error(string msg)
        {
            GetLogger().Error(msg);
        }
    }
    class User
    {
        public string Name { get; set; }

        public string Address { get; set; }


        public int? Age { get; set; }

        public int Sex { get; set; }

        public bool IsDelete { get; set; }

        public DateTime? CreateTime { get; set; }
    }
}
