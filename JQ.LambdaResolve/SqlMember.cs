using System.Collections.Generic;
using System.Data.SqlClient;

namespace JQ.LambdaResolve
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：SqlMember.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：SqlMember
    /// 创建标识：yjq 2017/5/23 16:00:21
    /// </summary>
    public struct SqlMember
    {
        public SqlMember(object value, SqlMemberType memberType, List<SqlParameter> paramList)
        {
            Value = value;
            MemberType = memberType;
            ParamList = paramList;
        }

        /// <summary>
        /// 值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 成员类型
        /// </summary>
        public SqlMemberType MemberType { get; set; }

        public List<SqlParameter> ParamList { get; set; }

        public void Output()
        {
            LogUtil.Debug(Value.ToString());
            if (ParamList != null)
            {
                foreach (var item in ParamList)
                {
                    LogUtil.Debug($"{item.ParameterName},{item.Value},{item.Size},{item.SqlDbType}");
                }
            }

        }
    }
}