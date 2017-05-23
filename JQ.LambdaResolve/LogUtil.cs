using NLog;

namespace JQ.LambdaResolve
{
    /// <summary>
    /// Copyright (C) 2017 yjq 版权所有。
    /// 类名：LogtUtil.cs
    /// 类属性：公共类（非静态）
    /// 类功能描述：LogtUtil
    /// 创建标识：yjq 2017/5/23 13:38:01
    /// </summary>
    public static class LogUtil
    {
        private static Logger GetLogger()
        {
            return NLog.LogManager.GetLogger("*");
        }

        public static void Debug(string msg)
        {
            GetLogger().Debug(msg);
        }

        public static void Info(string msg)
        {
            GetLogger().Info(msg);
        }

        public static void Warn(string msg)
        {
            GetLogger().Warn(msg);
        }

        public static void Error(string msg)
        {
            GetLogger().Error(msg);
        }
    }
}