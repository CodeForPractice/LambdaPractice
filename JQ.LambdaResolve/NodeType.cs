namespace JQ.LambdaResolve
{
    /// <summary>
    /// 节点类型
    /// </summary>
    internal enum NodeType
    {
        /// <summary>
        /// 二元运算符
        /// </summary>
        BinaryOperator = 1,

        /// <summary>
        /// 一员运算符
        /// </summary>
        UnaryOperator = 2,

        /// <summary>
        /// 常量
        /// </summary>
        Constant = 3,

        /// <summary>
        /// 成员
        /// </summary>
        MemberAccess = 4,

        /// <summary>
        /// 方法
        /// </summary>
        Call = 5,

        /// <summary>
        /// 一维数组
        /// </summary>
        NewArrayInit = 6,

        /// <summary>
        /// 未知类型
        /// </summary>
        Unknown = -99,

        /// <summary>
        /// 不支持
        /// </summary>
        NotSupport = -100
    }
}