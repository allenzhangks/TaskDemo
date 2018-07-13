using DotNet.Utilities;
using DotNet.Utilities.CommonRedis;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskDemo
{
    public class TestQueueService
    {
        //Reids 等待队列
        private string CONST_WAIT_PROCESS_QUEUES = "STO-WAIT-PROCESS-QUEUE";
        //Redis 正在处理队列
        private string CONST_IN_PROCESSING_QUEUES = "STO-IN-PROCESSING-QUEUE";
        //Redis 最后处理时间队列
        private string CONST_LAST_PROCESS_TIME_QUEUES = "STO-LAST-PROCESS-TIME-QUEUE";


        //扫描队列,加入到等待队列
        public void ScanQueue()
        {
            var redisClient = TestRedis.TestClient;
            for (int i = 0; i < 1000; i++)
            {
                string queueName = $"TASK:{i}";
                var queueCount = redisClient.GetHashCount(queueName);
                if (queueCount > 100)
                {
                    AddProcessQueue(redisClient, queueName);
                }
                var strLastProcessTime = redisClient.GetValueFromHash(CONST_LAST_PROCESS_TIME_QUEUES, queueName);
                var lastProcessTime = DateTime.MinValue;
                if (!string.IsNullOrWhiteSpace(strLastProcessTime))
                {
                    DateTime.TryParse(strLastProcessTime, out lastProcessTime);
                }
                if (queueCount > 0&&
                    ((lastProcessTime == DateTime.MinValue) || (lastProcessTime > DateTime.MinValue && lastProcessTime.AddMilliseconds(10) < DateTime.Now)))
                {
                    AddProcessQueue(redisClient, queueName);

                }

                //更新最后处理队列时间
                redisClient.SetEntryInHash(CONST_LAST_PROCESS_TIME_QUEUES, queueName, DateTime.Now.ToString());

            }
        }
        //处理任务
        public void StartToQueueProcess(ICommonRedisClient redisClient)
        {
            var queueName = redisClient.PopItemWithLowestScoreFromSortedSet(CONST_WAIT_PROCESS_QUEUES);
            bool needProcess = false;
            if (!string.IsNullOrWhiteSpace(queueName))
            {
                string lockKey = $"{CONST_IN_PROCESSING_QUEUES}-{queueName}-lockKey";
                using (var dl = redisClient.AcquireLock(lockKey, new TimeSpan(0, 0, 0, 10)))
                {
                    if (dl != null)
                    {
                        DateTime inProcessTime = DateTime.MinValue;
                        var strInprocessTime = redisClient.GetValueFromHash(CONST_IN_PROCESSING_QUEUES, queueName);
                        if (!string.IsNullOrWhiteSpace(strInprocessTime))
                        {
                            DateTime.TryParse(strInprocessTime, out inProcessTime);
                        }
                        if (inProcessTime == DateTime.MinValue)
                        {
                            redisClient.SetEntryInHash(CONST_IN_PROCESSING_QUEUES, queueName, DateTime.Now.ToString(BaseSystemInfo.DateFormat));
                            needProcess = true;
                        }
                        else
                        {
                            if (inProcessTime.AddMinutes(5) < DateTime.Now)
                            {
                                redisClient.RemoveEntryFromHash(CONST_IN_PROCESSING_QUEUES, queueName);
                            }
                        }


                    }
                    else
                    {
                        LogUtilities.WriteLine("获取锁失败");
                    }
                }

            }
            if (needProcess)
            {
                if (!string.IsNullOrEmpty(queueName))
                {
                    LogUtilities.WriteLine($"处理的任务队列是{queueName}");
                    redisClient.Remove(queueName);
                }
            }
        }

        public void AddProcessQueue(ICommonRedisClient redisClient, string queueName)
        {
            string lockKey = string.Format("{0}-{1}-LockKey", CONST_WAIT_PROCESS_QUEUES, queueName);
            using (var dl = redisClient.AcquireLock(lockKey, new TimeSpan(0, 0, 0, 10)))
            {
                if (dl != null)
                {
                    if (!redisClient.SortedSetContainsItem(CONST_WAIT_PROCESS_QUEUES, queueName))
                    {
                        redisClient.AddItemToSortedSet(CONST_WAIT_PROCESS_QUEUES, queueName, DateTime.Now.Ticks);

                    }
                }
            }
            redisClient.SetEntryInHash(CONST_LAST_PROCESS_TIME_QUEUES, queueName, DateTime.Now.ToString(BaseSystemInfo.DateTimeFormat));
        }
    }
}
