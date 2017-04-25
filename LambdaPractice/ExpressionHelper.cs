using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LambdaPractice
{
    internal static class ExpressionHelper
    {
        /// <summary>
        /// 通过Lambda解析为Sql
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public static string GetSqlByExpression(Expression func)
        {
            var funcType = CheckExpressionType(func);
            switch (funcType)
            {
                case EnumNodeType.BinaryOperator:
                    return FormatSqlExpression(VisitBinaryExpression(func as BinaryExpression));
                case EnumNodeType.Constant:
                    return FormatSqlExpression(VisitConstantExpression(func as ConstantExpression));
                case EnumNodeType.Call:
                    return FormatSqlExpression(VisitMethodCallExpression(func as MethodCallExpression));
                case EnumNodeType.UndryOperator:
                    return FormatSqlExpression(VisitUnaryExpression(func as UnaryExpression));
                case EnumNodeType.MemberAccess:
                    return FormatSqlExpression(VisitMemberAccessExpression(func as MemberExpression));
                default:
                    throw new NotSupportedException("不支持" + "：" + funcType.ToString());
            }
        }

        /// <summary>
        /// 格式化生成的sql
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static string FormatSqlExpression(string sql)
        {
            return sql.Replace(" )", ")");
        }

        /// <summary>
        /// 转换ExpressionType为SQL运算符
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        private static string ExpressionTypeToString(ExpressionType func)
        {
            switch (func)
            {
                case ExpressionType.AndAlso: return "AND";
                case ExpressionType.OrElse: return "OR";
                case ExpressionType.Equal: return "=";
                case ExpressionType.NotEqual: return "!=";
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.LessThanOrEqual: return "<=";
                case ExpressionType.Not: return "NOT";
                case ExpressionType.Convert: return "";
                default: return "unknown";
            }
        }

        /// <summary>
        /// 判断表达式类型
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        private static EnumNodeType CheckExpressionType(Expression func)
        {
            switch (func.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.Equal:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThan:
                case ExpressionType.NotEqual:
                    return EnumNodeType.BinaryOperator;
                case ExpressionType.Constant:
                    return EnumNodeType.Constant;
                case ExpressionType.MemberAccess:
                    return EnumNodeType.MemberAccess;
                case ExpressionType.Call:
                    return EnumNodeType.Call;
                case ExpressionType.Not:
                case ExpressionType.Convert:
                    return EnumNodeType.UndryOperator;
                default:
                    return EnumNodeType.Unknown;
            }
        }

        /// <summary>
        /// 判断一元表达式
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        private static string VisitUnaryExpression(UnaryExpression func)
        {
            var result = ExpressionTypeToString(func.NodeType);
            var funcType = CheckExpressionType(func.Operand);
            switch (funcType)
            {
                case EnumNodeType.BinaryOperator:
                    return result + VisitBinaryExpression(func.Operand as BinaryExpression);
                case EnumNodeType.Constant:
                    return result + VisitConstantExpression(func.Operand as ConstantExpression);
                case EnumNodeType.Call:
                    return result + VisitMethodCallExpression(func.Operand as MethodCallExpression);
                case EnumNodeType.UndryOperator:
                    return result + VisitUnaryExpression(func.Operand as UnaryExpression);
                case EnumNodeType.MemberAccess:
                    return result + VisitMemberAccessExpression(func.Operand as MemberExpression);
                default:
                    throw new NotSupportedException("不支持" + "：" + funcType.ToString());
            }
        }

        /// <summary>
        /// 判断常量表达式
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        private static string VisitConstantExpression(ConstantExpression func)
        {
            if (func.Value.ToString() == "")
            {
                return "\'\' ";
            }
            else if (func.Value.ToString() == "True")
            {
                return "1 = 1 ";
            }
            else if (func.Value.ToString() == "False")
            {
                return "0 = 1 ";
            }
            else
            {
                return "'" + func.Value.ToString() + "' ";

            }
        }
        /// <summary>
        /// 判断包含变量的表达式
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        private static string VisitMemberAccessExpression(MemberExpression func)
        {
            try
            {
                var tablename = "table";//EntityHelper.GetTableName(func.Expression.Type);
                return tablename + "." + func.Member.Name + " ";
            }
            catch
            {
                object value;
                switch (func.Type.Name)
                {
                    case "Int32":
                        {
                            var getter = Expression.Lambda<Func<int>>(func).Compile();
                            value = getter();
                        }
                        break;
                    case "String":
                        {
                            var getter = Expression.Lambda<Func<string>>(func).Compile();
                            value = "'" + getter() + "'";
                        }
                        break;
                    case "DateTime":
                        {
                            var getter = Expression.Lambda<Func<DateTime>>(func).Compile();
                            value = "'" + getter() + "'";
                        }
                        break;
                    default:
                        {
                            var getter = Expression.Lambda<Func<object>>(func).Compile();
                            value = getter();
                        }
                        break;
                }
                return value.ToString();
            }
        }

        /// <summary>
        /// 判断包含函数的表达式
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        private static String VisitMethodCallExpression(MethodCallExpression func)
        {
            if (func.Method.Name.Contains("Contains"))
            {
                //获得调用者的内容元素
                var getter = Expression.Lambda<Func<object>>(func.Object).Compile();
                var data = getter() as IEnumerable;
                //获得字段
                var caller = func.Arguments[0];
                while (caller.NodeType == ExpressionType.Call)
                {
                    caller = (caller as MethodCallExpression).Object;
                }
                var field = VisitMemberAccessExpression(caller as MemberExpression);
                var list = (from object i in data select "'" + i + "'").ToList();
                return field + " IN (" + string.Join(",", list.Cast<string>().ToArray()) + ") ";
            }
            else
            {
                throw new NotSupportedException("不支持" + ":" + func.Method.Name);
            }
        }

        /// <summary> 
        /// 判断包含二元运算符的表达式
        /// </summary>
        /// <remarks>注意，这个函数使用了递归，修改时注意不要修改了代码顺序和逻辑</remarks>
        /// <param name="func"></param>
        private static string VisitBinaryExpression(BinaryExpression func)
        {
            var result = "(";
            var leftType = CheckExpressionType(func.Left);
            switch (leftType)
            {
                case EnumNodeType.BinaryOperator:
                    result += VisitBinaryExpression(func.Left as BinaryExpression); break;
                case EnumNodeType.Constant:
                    result += VisitConstantExpression(func.Left as ConstantExpression); break;
                case EnumNodeType.MemberAccess:
                    result += VisitMemberAccessExpression(func.Left as MemberExpression); break;
                case EnumNodeType.UndryOperator:
                    result += VisitUnaryExpression(func.Left as UnaryExpression); break;
                case EnumNodeType.Call:
                    result += VisitMethodCallExpression(func.Left as MethodCallExpression); break;
                default:
                    throw new NotSupportedException("不支持" + "：" + leftType.ToString());
            }

            result += ExpressionTypeToString(func.NodeType) + " ";

            var rightType = CheckExpressionType(func.Right);
            switch (rightType)
            {
                case EnumNodeType.BinaryOperator:
                    result += VisitBinaryExpression(func.Right as BinaryExpression); break;
                case EnumNodeType.Constant:
                    result += VisitConstantExpression(func.Right as ConstantExpression); break;
                case EnumNodeType.MemberAccess:
                    result += VisitMemberAccessExpression(func.Right as MemberExpression); break;
                case EnumNodeType.UndryOperator:
                    result += VisitUnaryExpression(func.Right as UnaryExpression); break;
                case EnumNodeType.Call:
                    result += VisitMethodCallExpression(func.Right as MethodCallExpression); break;
                default:
                    throw new NotSupportedException("不支持" + "：" + rightType.ToString());
            }

            result += ") ";
            return result;
        }
    }
}
