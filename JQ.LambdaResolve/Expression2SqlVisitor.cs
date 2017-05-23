using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace JQ.LambdaResolve
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：Expression2SqlVisitor.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：Expression2SqlVisitor
    /// 创建标识：yjq 2017/5/23 13:29:49
    /// </summary>
    public sealed class Expression2SqlVisitor
    {
        private StringBuilder _sqlBuilder = new StringBuilder();
        private List<SqlParameter> _paramList = new List<SqlParameter>();
        private int _currentParamIndex = 0;

        public Expression2SqlVisitor()
        {
        }

        public SqlMember Resolve(Expression exp)
        {
            if (exp == null)
                return default(SqlMember);
            LogUtil.Debug($"NodeType:{exp.NodeType.ToString()}");
            SqlMember sqlMember = default(SqlMember);
            switch (exp.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    // return this.VisitUnary((UnaryExpression)exp);
                    break;

                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    sqlMember = this.VisitBinary((BinaryExpression)exp);
                    break;
                case ExpressionType.TypeIs:
                    //return this.VisitTypeIs((TypeBinaryExpression)exp);
                    break;

                case ExpressionType.Conditional:
                    //return this.VisitConditional((ConditionalExpression)exp);
                    break;

                case ExpressionType.Constant:
                    sqlMember = this.VisitConstant((ConstantExpression)exp);
                    break;

                case ExpressionType.Parameter:
                    //return this.VisitParameter((ParameterExpression)exp);
                    break;

                case ExpressionType.MemberAccess:
                    //return this.VisitMemberAccess((MemberExpression)exp);
                    sqlMember = VisitMemberAccess((MemberExpression)exp);
                    break;

                case ExpressionType.Call:
                    sqlMember = VisitMethodCall((MethodCallExpression)exp);
                    break;
                case ExpressionType.Lambda:
                    //return this.VisitLambda((LambdaExpression)exp);
                    break;

                case ExpressionType.New:
                    //return this.VisitNew((NewExpression)exp);
                    break;

                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    //return this.VisitNewArray((NewArrayExpression)exp);
                    break;

                case ExpressionType.Invoke:
                    //return this.VisitInvocation((InvocationExpression)exp);
                    break;

                case ExpressionType.MemberInit:
                    //return this.VisitMemberInit((MemberInitExpression)exp);
                    break;

                case ExpressionType.ListInit:
                    //return this.VisitListInit((ListInitExpression)exp);
                    break;

                default:
                    //throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
                    break;
            }
            return sqlMember;
        }

        private SqlMember VisitBinary(BinaryExpression exp)
        {
            if (exp.NodeType == ExpressionType.ArrayIndex)
            {
                throw new NotSupportedException("ArrayIndex");
            }
            var expressionLeft = exp.Left;
            var leftSqlMember = Resolve(expressionLeft);
            var expressionRight = exp.Right;
            var rightSqlMember = Resolve(expressionRight);
            return AppendSqlMember(leftSqlMember, exp.NodeType, rightSqlMember);
        }

        private SqlMember VisitMemberAccess(MemberExpression exp)
        {
            LogUtil.Info(exp.Member.Name);
            SqlMemberType sqlMemberType = default(SqlMemberType);
            List<SqlParameter> paramList = new List<SqlParameter>();
            object value = null;
            if (exp.Expression.NodeType != ExpressionType.Parameter)
            {
                var memberValue = Expression.Lambda(exp).Compile().DynamicInvoke();
                sqlMemberType = SqlMemberType.Value;
                var memberValueType = memberValue.GetType();
                if (memberValueType.IsArray || memberValueType.IsGenericType)
                {
                    foreach (var item in (IEnumerable)memberValue)
                    {
                        paramList.Add(GetSqlParameter(item, item.GetType()));
                    }
                    value = string.Concat("(", string.Join(",", paramList.Select(m => m.ParameterName)), ")");
                }
                else
                {
                    var sqlParameter = GetSqlParameter(memberValue, memberValue.GetType());
                    paramList.Add(sqlParameter);
                    value = sqlParameter.ParameterName;
                }
            }
            else
            {
                value = exp.Member.Name;
                sqlMemberType = SqlMemberType.Key;
            }
            return new SqlMember(value, sqlMemberType, null);

        }

        private SqlMember VisitConstant(ConstantExpression exp)
        {
            var sqlParameter = GetSqlParameter(exp.Value, exp.Type);
            List<SqlParameter> parameterList = new List<SqlParameter>()
            {
                sqlParameter
            };
            return new SqlMember(sqlParameter.ParameterName, SqlMemberType.Value, null);
        }

        private SqlMember VisitMethodCall(MethodCallExpression exp)
        {
            string methodName = exp.Method.Name;
            LogUtil.Debug($"MethodName:{methodName}");
            switch (methodName)
            {
                case "StartsWith":
                    return ResolveMethodStartsWith(exp);

                case "Contains":
                    return ResolveMethodContains(exp);

                case "EndsWith":
                    return ResolveMethodEndsWith(exp);
            }
            return default(SqlMember);
        }

        private SqlMember ResolveMethodStartsWith(MethodCallExpression exp)
        {
            StringBuilder builder = new StringBuilder();
            List<SqlParameter> paramList = new List<SqlParameter>();
            if (exp.Object != null)
            {
                SqlMember sqlMember = Resolve(exp.Object);
                builder.Append(sqlMember.Value);
                if (sqlMember.ParamList != null)
                {
                    paramList.AddRange(sqlMember.ParamList);
                }
            }
            builder.Append(" LIKE '%'+ ");
            if (exp.Arguments != null)
            {
                foreach (var item in exp.Arguments)
                {
                    SqlMember sqlMember = Resolve(item);
                    builder.Append(sqlMember.Value);
                    if (sqlMember.ParamList != null)
                    {
                        paramList.AddRange(sqlMember.ParamList);
                    }
                }
            }
            return new SqlMember(builder.ToString(), SqlMemberType.None, paramList);
        }

        private SqlMember ResolveMethodEndsWith(MethodCallExpression exp)
        {
            StringBuilder builder = new StringBuilder();
            List<SqlParameter> paramList = new List<SqlParameter>();
            if (exp.Object != null)
            {
                SqlMember sqlMember = Resolve(exp.Object);
                builder.Append(sqlMember.Value);
                if (sqlMember.ParamList != null)
                {
                    paramList.AddRange(sqlMember.ParamList);
                }
            }
            builder.Append(" LIKE  ");
            if (exp.Arguments != null)
            {
                foreach (var item in exp.Arguments)
                {
                    SqlMember sqlMember = Resolve(item);
                    builder.Append(sqlMember.Value);
                    if (sqlMember.ParamList != null)
                    {
                        paramList.AddRange(sqlMember.ParamList);
                    }
                }
            }
            builder.Append(" +'%' ");
            return new SqlMember(builder.ToString(), SqlMemberType.None, paramList);
        }

        private SqlMember ResolveMethodContains(MethodCallExpression exp)
        {
            var rightExpType = exp.Arguments[0].NodeType;
            LogUtil.Debug($"RightType:{rightExpType}");
            Expression leftExp = exp.Object;
            Expression rightExp = null;
            if (new ExpressionType[] { ExpressionType.MemberAccess, ExpressionType.Constant }.Contains(rightExpType))
            {
                rightExp = exp.Arguments[0];
            }
            else
            {
                rightExp = Expression.Lambda(exp.Arguments[0]);
            }
            StringBuilder builder = new StringBuilder();
            List<SqlParameter> paramList = new List<SqlParameter>();
            if (exp.Object != null)
            {
                SqlMember sqlMember = Resolve(exp.Object);
                builder.Append(sqlMember.Value);
                if (sqlMember.ParamList != null)
                {
                    paramList.AddRange(sqlMember.ParamList);
                }
            }
            builder.Append(" LIKE '%'+ ");
            if (exp.Arguments != null)
            {
                foreach (var item in exp.Arguments)
                {
                    SqlMember sqlMember = Resolve(item);
                    builder.Append(sqlMember.Value);
                    if (sqlMember.ParamList != null)
                    {
                        paramList.AddRange(sqlMember.ParamList);
                    }
                }
            }
            builder.Append(" +'%' ");
            return new SqlMember(builder.ToString(), SqlMemberType.None, paramList);
        }

        private string ResolveExpressionType(ExpressionType expressionType)
        {
            LogUtil.Warn(expressionType.ToString());
            string operateSign = string.Empty;
            switch (expressionType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.AddChecked:
                    operateSign = "+";
                    break;

                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.AndAssign:
                    operateSign = "AND";
                    break;

                case ExpressionType.LessThan:
                    operateSign = "<";
                    break;

                case ExpressionType.LessThanOrEqual:
                    operateSign = "<=";
                    break;

                case ExpressionType.Equal:
                    operateSign = "=";
                    break;

                case ExpressionType.NotEqual:
                    operateSign = "!=";
                    break;

                case ExpressionType.Not:
                    operateSign = "NOT";
                    break;

                case ExpressionType.GreaterThan:
                    operateSign = ">";
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    operateSign = ">=";
                    break;

                case ExpressionType.OrElse:
                    operateSign = "OR";
                    break;
            }
            return operateSign;
        }

        private SqlMember AppendSqlMember(SqlMember leftSqlMember, ExpressionType expType, SqlMember rightSqlMember)
        {
            StringBuilder builder = new StringBuilder();
            List<SqlParameter> paramList = new List<SqlParameter>();
            if (leftSqlMember.ParamList != null)
            {
                paramList.AddRange(leftSqlMember.ParamList);
            }
            if (rightSqlMember.ParamList != null)
            {
                paramList.AddRange(rightSqlMember.ParamList);
            }

            if (leftSqlMember.MemberType == rightSqlMember.MemberType && leftSqlMember.MemberType == SqlMemberType.None)
            {
                builder.AppendFormat("({0} {1} {2})", leftSqlMember.Value, ResolveExpressionType(expType), rightSqlMember.Value);
            }
            else if (leftSqlMember.MemberType == SqlMemberType.None || rightSqlMember.MemberType == SqlMemberType.None)
            {
                throw new NotSupportedException($"{leftSqlMember.MemberType},{rightSqlMember.MemberType}");
            }
            else
            {
                builder.AppendFormat("({0} {1} {2})", leftSqlMember.Value, ResolveExpressionType(expType), rightSqlMember.Value);
            }
            return new SqlMember(builder.ToString(), SqlMemberType.None, paramList);
        }

        private SqlParameter GetSqlParameter(object value, Type type)
        {
            string parameterName = $"@Parameter_{_currentParamIndex.ToString()}";
            _currentParamIndex++;
            return new SqlParameter { Value = value, SqlDbType = TypeString2SqlType(type.Name.ToLower()), ParameterName = parameterName };
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
    }
}