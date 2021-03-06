﻿using System;
using System.Data.SqlClient;

namespace JQ.ExpressionResolve
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：SqlServerVisitro.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：SqlServerVisitro
    /// 创建标识：yjq 2017/5/27 14:47:02
    /// </summary>
    public sealed class SqlServerVisitor : BaseParamExpressionVisitor<SqlParameter>
    {
        private int _currentParamIndex = 0;//当前已有参数个数

        public SqlServerVisitor(string prefix = "") : base(prefix: prefix)
        {
        }

        /// <summary>
        /// 根据值获取参数的方法
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected override Tuple<string, SqlParameter> GetParameter(object value)
        {
            string parameterName = $"@Parameter_{_currentParamIndex.ToString()}";
            _currentParamIndex++;
            return Tuple.Create(parameterName, new SqlParameter
            {
                Value = value,
                SqlDbType = TypeUtil.TypeString2SqlType(value.GetType().Name.ToLower()),
                ParameterName = parameterName
            });
        }

    }
}