using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TaskDemo
{
    using DotNet.Utilities;
    using StoXAgent;
    public class TaskAgent : AgentServiceBase<TaskAgent>
    {
        public override int ThreadCount => BaseBusinessLogic.ConvertToInt(ConfigurationHelper.AppSettings("ThreadCount", false));


        public TaskAgent()
        {
            ServiceName = ConfigurationHelper.AppSettings("ServiceName", false);
            Intervals = new Int32[ThreadCount];
            for (int i = 0; i < ThreadCount; i++)
            {
                if (i == 0)
                {
                    Intervals[i] = 60;
                }
                else
                {
                    Intervals[i] = 20;
                }
            }

        }
        public override void StartWork()
        {
            base.StartWork();
        }
        public override void StopWork()
        {
            base.StopWork();
        }
        public override bool Work(int index)
        {
            try
            {
                switch (index)
                {
                    //0 号线程 主进程是扫描任务加入到等待队列，不是主进程就处理任务。
                    case 0:
                        {
                            var mainProcessKey = "MainSevice-Process";
                            string lockKey = $"{mainProcessKey}-lock";
                            var isMain = false;
                            //第一次进入时，判断当前是否存在主。
                            bool existMain = false;
                            using (var dl = TestRedis.TestClient.AcquireLock(lockKey, new TimeSpan(0, 0, 0, 0, 1000)))
                            {
                                if (dl != null)
                                {
                                    existMain = TestRedis.TestClient.ContainsKey(mainProcessKey);
                                    if (existMain)
                                    {
                                        KeyValueObject<string, DateTime> kvpMainFlag = TestRedis.TestClient.Get<KeyValueObject<string, DateTime>>(mainProcessKey);
                                        if (kvpMainFlag.Key.Equals(ServiceName))
                                        {
                                            isMain = true;
                                            TestRedis.TestClient.Set(mainProcessKey, new KeyValueObject<string, DateTime>(ServiceName, DateTime.Now));
                                        }
                                        else
                                        {
                                            if (kvpMainFlag.Value.AddMinutes(5d) < DateTime.Now)
                                            {
                                                isMain = true;
                                                TestRedis.TestClient.Set(mainProcessKey, new KeyValueObject<string, DateTime>(ServiceName, DateTime.Now));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        isMain = true;
                                        TestRedis.TestClient.Set(mainProcessKey, new KeyValueObject<string, DateTime>(ServiceName, DateTime.Now));

                                    }
                                }
                                else
                                {
                                    LogUtilities.WriteLine("获取锁失败！！！");
                                }
                            }
                            TestQueueService testQueueService = new TestQueueService();
                            if (isMain)
                            {
                                testQueueService.ScanQueue();
                                LogUtilities.WriteLog("加入等待队列完毕");
                                //ScanProcess
                                //加入到等待队列
                            }
                            else
                            {
                                testQueueService.StartToQueueProcess(TestRedis.TestClient);
                                LogUtilities.WriteLog("处理任务完毕");
                                //处理任务
                            }

                            break;

                        }
                    default:
                        {
                            TestQueueService testQueueService = new TestQueueService();
                            testQueueService.StartToQueueProcess(TestRedis.TestClient);

                        }
                        break;
                }

            }
            catch (Exception ex)
            {

                LogUtilities.WriteException(ex);
            }

            return base.Work(index);
        }

    }


}