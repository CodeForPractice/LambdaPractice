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
                    sqlMember = this.VisitUnary((UnaryExpression)exp);
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
                    throw new NotSupportedException(exp.NodeType.ToString());

                case ExpressionType.Conditional:
                    sqlMember = this.VisitConditional((ConditionalExpression)exp);
                    break;

                case ExpressionType.Constant:
                    sqlMember = this.VisitConstant((ConstantExpression)exp);
                    break;

                case ExpressionType.Parameter:
                    throw new NotSupportedException(exp.NodeType.ToString());

                case ExpressionType.MemberAccess:
                    sqlMember = VisitMemberAccess((MemberExpression)exp);
                    break;

                case ExpressionType.Call:
                    sqlMember = VisitMethodCall((MethodCallExpression)exp);
                    break;

                case ExpressionType.Lambda:
                    throw new NotSupportedException(exp.NodeType.ToString());

                case ExpressionType.New:
                    throw new NotSupportedException(exp.NodeType.ToString());

                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    throw new NotSupportedException(exp.NodeType.ToString());

                case ExpressionType.Invoke:
                    throw new NotSupportedException(exp.NodeType.ToString());

                case ExpressionType.MemberInit:
                    throw new NotSupportedException(exp.NodeType.ToString());

                case ExpressionType.ListInit:
                    throw new NotSupportedException(exp.NodeType.ToString());

                default:
                    throw new NotSupportedException(exp.NodeType.ToString());
            }
            return sqlMember;
        }

        private SqlMember VisitUnary(UnaryExpression exp)
        {
            var memberValue = Expression.Lambda(exp).Compile().DynamicInvoke();
            return Resolve(Expression.Constant(memberValue));
        }

        private SqlMember VisitBinary(BinaryExpression exp)
        {
            if (exp.NodeType == ExpressionType.ArrayIndex)
            {
                var memberValue = Expression.Lambda(exp).Compile().DynamicInvoke();
                return Resolve(Expression.Constant(memberValue));
            }
            var expressionLeft = exp.Left;
            var expressionRight = exp.Right;
            if (exp.NodeType == ExpressionType.Coalesce)
            {
                bool leftIsConstatnt = IsConstantExpression(expressionLeft);
                bool rightIsConstatnt = IsConstantExpression(expressionRight);
                if (leftIsConstatnt && rightIsConstatnt)//如果都是常量则直接返回
                {
                    var memberValue = Expression.Lambda(exp).Compile().DynamicInvoke();
                    return Resolve(Expression.Constant(memberValue));
                }
                else
                {
                    var leftMember = Resolve(expressionLeft);
                    var rightMember = Resolve(expressionRight);
                    return AppendSqlMember(leftMember, exp.NodeType, rightMember);
                }
            }

            var leftSqlMember = Resolve(expressionLeft);
            var rightSqlMember = Resolve(expressionRight);
            return AppendSqlMember(leftSqlMember, exp.NodeType, rightSqlMember);
        }

        private SqlMember VisitConditional(ConditionalExpression exp)
        {
            try
            {
                var value = Expression.Lambda(exp.Test).Compile().DynamicInvoke();
                if (value != null && (bool)value == true)
                {
                    return Resolve(exp.IfTrue);
                }
                return Resolve(exp.IfFalse);
            }
            catch
            {
                StringBuilder builder = new StringBuilder();
                List<SqlParameter> paramList = new List<SqlParameter>();
                var conditionSqlMember = Resolve(exp.Test);
                var ifTrueSqlMember = Resolve(exp.IfTrue);
                var ifFalseSqlMember = Resolve(exp.IfFalse);
                builder.Append("(CASE WHEN ");
                if (conditionSqlMember.MemberType == SqlMemberType.None)
                {
                    builder.Append(conditionSqlMember.Value);
                }
                else
                {
                    builder.AppendFormat("{0}=1", conditionSqlMember.Value);
                }
                builder.AppendFormat(" THEN {0} ELSE {1})", ifTrueSqlMember.Value, ifFalseSqlMember.Value);
                if (conditionSqlMember.ParamList != null)
                {
                    paramList.AddRange(conditionSqlMember.ParamList);
                }
                if (ifTrueSqlMember.ParamList != null)
                {
                    paramList.AddRange(ifTrueSqlMember.ParamList);
                }
                if (ifFalseSqlMember.ParamList != null)
                {
                    paramList.AddRange(ifFalseSqlMember.ParamList);
                }
                return new SqlMember(builder.ToString(), SqlMemberType.None, paramList);
            }
        }

        private SqlMember VisitMemberAccess(MemberExpression exp)
        {
            LogUtil.Info(exp.Member.Name);
            SqlMemberType sqlMemberType = default(SqlMemberType);
            List<SqlParameter> paramList = new List<SqlParameter>();
            object value = null;
            if (IsNotParameterExpression(exp))
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
                if (exp.Member.Name == "Length")
                {
                    SqlMember memberSqlMember = Resolve(exp.Expression);
                    value = $"LEN({memberSqlMember.Value})";
                    sqlMemberType = SqlMemberType.Key;
                    if (memberSqlMember.ParamList != null)
                    {
                        paramList.AddRange(memberSqlMember.ParamList);
                    }
                }
                if (exp.Member.Name == "Count")
                {
                    throw new NotSupportedException("Count");
                }
                else
                {
                    value = exp.Member.Name;
                    sqlMemberType = SqlMemberType.Key;
                }
            }
            return new SqlMember(value, sqlMemberType, paramList);
        }

        private SqlMember VisitConstant(ConstantExpression exp)
        {
            object value = null;
            if (exp.Type == typeof(bool))
            {
                if (exp.Value != null)
                {
                    if ((bool)exp.Value)
                    {
                        value = "1";
                    }
                    else
                    {
                        value = "0";
                    }
                }
            }
            var sqlParameter = GetSqlParameter(value ?? exp.Value, exp.Type);
            List<SqlParameter> parameterList = new List<SqlParameter>()
            {
                sqlParameter
            };
            return new SqlMember(sqlParameter.ParameterName, SqlMemberType.Value, parameterList);
        }

        private SqlMember VisitMethodCall(MethodCallExpression exp)
        {
            try
            {
                var memberValue = Expression.Lambda(exp).Compile().DynamicInvoke();
                return Resolve(Expression.Constant(memberValue));
            }
            catch
            {
                string methodName = exp.Method.Name;
                switch (methodName)
                {
                    case "StartsWith":
                        return ResolveMethodStartsWith(exp);

                    case "Contains":
                        return ResolveMethodContains(exp);

                    case "EndsWith":
                        return ResolveMethodEndsWith(exp);

                    case "TrimEnd":
                        return ResolveMethodTrimEnd(exp);

                    case "TrimStart":
                        return ResolveMethodTrimStart(exp);

                    case "Trim":
                        return ResolveMethodTrim(exp);

                    default:
                        throw new NotSupportedException(methodName);
                }
            }
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
            Expression leftExp = exp.Object ?? exp.Arguments[1];
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
            SqlMember leftSqlMember = Resolve(leftExp);
            SqlMember rightSqlMEember = Resolve(rightExp);
            if (leftSqlMember.ParamList != null && leftSqlMember.ParamList.Count > 1)//是个数组或者集合
            {
                builder.AppendFormat("{0} IN {1}", rightSqlMEember.Value, leftSqlMember.Value);
            }
            else if (rightSqlMEember.ParamList != null && rightSqlMEember.ParamList.Count > 1)
            {
                builder.AppendFormat("{0} IN {1}", leftSqlMember.Value, rightSqlMEember.Value);
            }
            else
            {
                builder.AppendFormat(" {0} LIKE '%'+{1}+'%' ", leftSqlMember.Value, rightSqlMEember.Value);
            }
            if (leftSqlMember.ParamList != null)
            {
                paramList.AddRange(leftSqlMember.ParamList);
            }
            if (rightSqlMEember.ParamList != null)
            {
                paramList.AddRange(rightSqlMEember.ParamList);
            }
            return new SqlMember(builder.ToString(), SqlMemberType.None, paramList);
        }

        private SqlMember ResolveMethodTrimEnd(MethodCallExpression exp)
        {
            StringBuilder builder = new StringBuilder("RTRIM(");
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
            builder.Append(")");
            return new SqlMember(builder.ToString(), SqlMemberType.Key, paramList);
        }

        private SqlMember ResolveMethodTrimStart(MethodCallExpression exp)
        {
            StringBuilder builder = new StringBuilder("LTRIM(");
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
            builder.Append("))");
            return new SqlMember(builder.ToString(), SqlMemberType.Key, paramList);
        }

        private SqlMember ResolveMethodTrim(MethodCallExpression exp)
        {
            StringBuilder builder = new StringBuilder("RTRIM(LTRIM(");
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
            builder.Append(")");
            return new SqlMember(builder.ToString(), SqlMemberType.Key, paramList);
        }

        private SqlMember ResolveMethodCount(MethodCallExpression exp)
        {
            var memberValue = Expression.Lambda(exp).Compile().DynamicInvoke();
            return Resolve(Expression.Constant(memberValue));
        }

        private SqlMember ResolveMethodUnknow(MethodCallExpression exp)
        {
            var memberValue = Expression.Lambda(exp).Compile().DynamicInvoke();
            return Resolve(Expression.Constant(memberValue));
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

        private bool IsNotParameterExpression(MemberExpression exp)
        {
            bool flage = true;
            var checkExp = exp;
            while (checkExp != null && checkExp.Expression != null)
            {
                if (checkExp.Expression.NodeType == ExpressionType.Parameter)
                {
                    flage = false;
                    break;
                }
                checkExp = checkExp.Expression as MemberExpression;
            }
            return flage;
        }

        private bool IsConstantExpression(Expression exp)
        {
            if (exp.NodeType == ExpressionType.Constant)
            {
                return true;
            }
            bool flage = false;
            var checkExp = exp as MemberExpression;
            while (checkExp != null)
            {
                if (checkExp.Expression == null)
                {
                    if (checkExp.NodeType == ExpressionType.Constant)
                    {
                        flage = true;
                    }
                    checkExp = checkExp.Expression as MemberExpression;
                }
                else
                {
                    flage = checkExp.Expression.NodeType == ExpressionType.Constant;
                    checkExp = checkExp.Expression as MemberExpression;
                }
            }
            return flage;
        }

        private SqlMember AppendSqlMember(SqlMember leftSqlMember, ExpressionType expType, SqlMember rightSqlMember)
        {
            SqlMemberType memberType = SqlMemberType.None;
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

            if (expType == ExpressionType.Coalesce)
            {
                builder.AppendFormat("ISNULL({0},{1})", leftSqlMember.MemberType == SqlMemberType.Value ? rightSqlMember.Value : leftSqlMember.Value, leftSqlMember.MemberType == SqlMemberType.Value ? leftSqlMember.Value : rightSqlMember.Value);
                memberType = SqlMemberType.Key;
            }
            else
            {
                if (leftSqlMember.MemberType == rightSqlMember.MemberType && leftSqlMember.MemberType == SqlMemberType.None)
                {
                    builder.AppendFormat("({0} {1} {2})", leftSqlMember.Value, ResolveExpressionType(expType), rightSqlMember.Value);
                }
                else if (leftSqlMember.MemberType == SqlMemberType.None || rightSqlMember.MemberType == SqlMemberType.None)
                {
                    builder.AppendFormat("({0} {1} {2}) ", leftSqlMember.MemberType == SqlMemberType.None ? leftSqlMember.Value : leftSqlMember.Value + "=1", ResolveExpressionType(expType), rightSqlMember.MemberType == SqlMemberType.None ? rightSqlMember.Value : rightSqlMember.Value + "=1");
                }
                else
                {
                    builder.AppendFormat("({0} {1} {2})", leftSqlMember.Value, ResolveExpressionType(expType), rightSqlMember.Value);
                }
            }
            return new SqlMember(builder.ToString(), memberType, paramList);
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