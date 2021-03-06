﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace JQ.LambdaResolve
{
    public class LambdaResolve
    {
        public LambdaResolve()
        {
        }

        public string Resolve(Expression funcExp)
        {
            string result = string.Empty;
            var nodeType = GetNodeType(funcExp);
            result = GetSqlInfo(nodeType, funcExp);
            return result;
        }

        /// <summary>
        /// 判断包含二元运算符的表达式
        /// </summary>
        /// <param name="funcExp">表达式</param>
        private string VisitBinaryExpression(BinaryExpression funcExp)
        {
            string result = "(";
            var leftNodeType = GetNodeType(funcExp.Left);
            result += GetSqlInfo(leftNodeType, funcExp.Left);
            result += ExpressionTypeToOperate(funcExp.NodeType) + " ";
            var rightNodeType = GetNodeType(funcExp.Right);
            result += GetSqlInfo(rightNodeType, funcExp.Right);
            result += ")";
            return result;
        }

        /// <summary>
        /// 判断常量表达式
        /// </summary>
        /// <param name="funcExp"></param>
        /// <returns></returns>
        private string VisitConstantExpression(ConstantExpression funcExp)
        {
            var value = funcExp.Value;
            if (value == null)
            {
                return " IS NULL ";
            }
            else
            {
                string expressionResult = string.Empty;
                switch (value.ToString().ToLower().Trim())
                {
                    case "true":
                        expressionResult = " 1=1 ";
                        break;

                    case "false":
                        expressionResult = " 1=0 ";
                        break;

                    default:
                        expressionResult = value.ToString();
                        break;
                }
                return expressionResult;
            }
        }

        /// <summary>
        /// 判断对方法成员的调用
        /// </summary>
        /// <param name="funcExp"></param>
        /// <returns></returns>
        private string VisitMethodCallExpression(MethodCallExpression funcExp)
        {
            if (funcExp.Method.Name.Contains("Contains"))
            {
                Expression leftExpression = null;
                Expression rightExpression = null;
                bool funcExpObjectIsnull = funcExp.Object == null;
                if (funcExpObjectIsnull)
                {
                    leftExpression = funcExp.Arguments[0];
                    rightExpression = funcExp.Arguments[1];
                }
                else
                {
                    leftExpression = funcExp.Object;
                    rightExpression = funcExp.Arguments[0];
                }

                var resultLeft = GetSqlInfo(GetNodeType(leftExpression),leftExpression);
                //判断左边是不是字段
                var resultRight = GetSqlInfo(GetNodeType(rightExpression), rightExpression);
                if (funcExpObjectIsnull)
                {
                    return $"{resultRight} IN {resultLeft}";
                }else
                {
                    return $"{resultLeft} {resultRight}";
                }
            }
            else
            {
                throw new Exception($"不支持{funcExp.Method.Name}方法");
            }
        }

        /// <summary>
        /// 判断包含变量的表达式
        /// </summary>
        /// <param name="funcExp"></param>
        /// <returns></returns>
        private string VisitMemberExpression(MemberExpression funcExp)
        {
            return funcExp.Member.Name;
        }
        /// <summary>
        /// 判断包含数组的表达式
        /// </summary>
        /// <param name="funcexp"></param>
        /// <returns></returns>
        private string VisitNewArrayExpression(NewArrayExpression funcexp)
        {
            List<string> builder = new List<string>();
            foreach (var item in funcexp.Expressions)
            {
                builder.Add(GetSqlInfo(GetNodeType(item), item));
            }
            return $"({string.Join(",", builder)})";
        }
        /// <summary>
        /// 判断包含一元运算符的表达式
        /// </summary>
        /// <param name="funcExp"></param>
        /// <returns></returns>
        private string VisitUnaryExpression(UnaryExpression funcExp)
        {
            var result = ExpressionTypeToOperate(funcExp.NodeType);
            var operandNodeType = GetNodeType(funcExp.Operand);
            result += GetSqlInfo(operandNodeType, funcExp.Operand);
            return result;
        }

        private string GetSqlInfo(NodeType notyType, Expression funcExp)
        {
            string sqlInfo = string.Empty;
            switch (notyType)
            {
                case NodeType.BinaryOperator:
                    sqlInfo = VisitBinaryExpression(funcExp as BinaryExpression);
                    break;

                case NodeType.Constant:
                    sqlInfo = VisitConstantExpression(funcExp as ConstantExpression);
                    break;

                case NodeType.Call:
                    sqlInfo = VisitMethodCallExpression(funcExp as MethodCallExpression);
                    break;

                case NodeType.MemberAccess:
                    sqlInfo = VisitMemberExpression(funcExp as MemberExpression);
                    break;

                case NodeType.UnaryOperator:
                    sqlInfo = VisitUnaryExpression(funcExp as UnaryExpression);
                    break;

                case NodeType.NewArrayInit:
                    sqlInfo = VisitNewArrayExpression(funcExp as NewArrayExpression);
                    break;

                default:
                    throw new NotSupportedException($"不支持：{notyType.ToString()}");
            }
            return sqlInfo;
        }

        private NodeType GetNodeType(Expression funcExp)
        {
            switch (funcExp.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.Equal:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThan:
                case ExpressionType.NotEqual:
                    return NodeType.BinaryOperator;

                case ExpressionType.Constant:
                    return NodeType.Constant;

                case ExpressionType.MemberAccess:
                    return NodeType.MemberAccess;

                case ExpressionType.Call:
                    return NodeType.Call;

                case ExpressionType.Not:
                case ExpressionType.Convert:
                    return NodeType.UnaryOperator;

                case ExpressionType.NewArrayInit:
                    return NodeType.NewArrayInit;

                default:
                    return NodeType.Unknown;
            }
        }

        private string ExpressionTypeToOperate(ExpressionType expressType)
        {
            switch (expressType)
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
    }
}