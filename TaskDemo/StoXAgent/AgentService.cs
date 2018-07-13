using System;

namespace StoXAgent
{
    using NewLife.Log;

    /// <summary>代理服务例子。自定义服务程序可参照该类实现。</summary>
    public class AgentService<TService> : AgentServiceBase<TService> where TService : AgentServiceBase<TService>, new()
    {
        public override int ThreadCount
        {
            get
            {
                return 1;
            }
        }
        #region 这些参数可以配置文件中设置
        public AgentService()
        {
            ServiceName = "StoXAgent";
        }
        #endregion
    }

    namespace System.Runtime.CompilerServices
    {
        public class ExtensionAttribute : Attribute { }
    }
}