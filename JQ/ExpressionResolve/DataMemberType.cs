namespace JQ.ExpressionResolve
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：DataMemberType.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：数据类型
    /// 创建标识：yjq 2017/5/25 17:24:07
    /// </summary>
    public enum DataMemberType
    {
        /// <summary>
        /// 默认（完成组合的）
        /// </summary>
        None = 0,

        /// <summary>
        /// 字段
        /// </summary>
        Key = 1,

        /// <summary>
        /// 值
        /// </summary>
        Value = 2
    }
}