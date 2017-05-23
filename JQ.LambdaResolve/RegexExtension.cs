using System.Text.RegularExpressions;

namespace JQ.LambdaResolve
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：RegexExtension.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：RegexExtension
    /// 创建标识：yjq 2017/5/23 19:18:13
    /// </summary>
    public static class RegexExtension
    {
        public static bool IsMatch(this object thisValue, string pattern)
        {
            if (thisValue == null) return false;
            Regex reg = new Regex(pattern);
            return reg.IsMatch(thisValue.ToString());
        }
    }
}