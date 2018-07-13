using DotNet.Utilities;
using DotNet.Utilities.CommonRedis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateTaskConsole
{
    public static class TestRedis
    {

        /// <summary>
        /// test消费数据存储redisDb
        /// </summary>
        private static long testDb = BaseBusinessLogic.ConvertToInt(ConfigurationHelper.AppSettings("redis_testDb", false));


        /// <summary>
        /// redis管理类
        /// </summary>
        private static CommonRedisClientManager _redisInstanceManager = null;


        /// <summary>
        /// test锁对象
        /// </summary>
        private static object _redisInstanceManagerLock = new object();


        /// <summary>
        /// reddis管理类
        /// </summary>
        public static CommonRedisClientManager RedisManagerInstance
        {
            get
            {
                if (_redisInstanceManager == null)
                {
                    lock (_redisInstanceManagerLock)
                    {
                        if (_redisInstanceManager == null)
                        {
                            _redisInstanceManager =
                                new CommonRedisClientManager(ConfigurationHelper.AppSettings("TestRedisConn", false),
                                    RedisClientDriverType.StackExchange);
                        }
                    }

                }
                return _redisInstanceManager;
            }
        }


        /// <summary>
        /// 获取Test消费数据存储redis客户端
        /// </summary>
        /// <returns></returns>
        public static ICommonRedisClient TestClient
        {
            get
            {
                if (_TestClient == null)
                {
                    lock (_TestClientLock)
                    {
                        if (_TestClient == null)
                        {
                            _TestClient = RedisManagerInstance.GetClient();
                            _TestClient.Db = testDb;
                        }
                    }
                }
                return _TestClient;
            }
        }
        private static ICommonRedisClient _TestClient = null;
        private static object _TestClientLock = new object();







    }
}
