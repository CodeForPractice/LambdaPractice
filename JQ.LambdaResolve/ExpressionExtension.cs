using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JQ.LambdaResolve
{
    /// <summary>
    /// Copyright (C) 2015 备胎 版权所有。
    /// 类名：ExpressionExtension.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：
    /// 创建标识：yjq 2017/4/26 11:21:39
    /// </summary>
    internal static class ExpressionExtension
    {
        public static bool IsCollection(this string typeName)
        {
            return typeName.StartsWith("System.Collections.Generic.List");
        }

        public static bool IsArray(this string typeName)
        {
            return false;
        }
    }
}
