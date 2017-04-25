namespace LambdaPractice
{
    public enum EnumNodeType
    {
        /// <summary>
        /// 二元运算符
        /// </summary>
        BinaryOperator = 1,

        /// <summary>
        /// 一元运算符
        /// </summary>
        UndryOperator = 2,

        /// <summary>
        /// 常量
        /// </summary>
        Constant = 3,

        /// <summary>
        /// 成员（变量）
        /// </summary>
        MemberAccess = 4,

        /// <summary>
        /// 函数
        /// </summary>
        Call = 5,

        /// <summary>
        /// 未知
        /// </summary>
        Unknown = -99,

        /// <summary>
        /// 不支持
        /// </summary>
        NotSupported = -98
    }
}