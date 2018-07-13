using DotNet.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateTaskConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    string queueName = $"TASK:{i}";
                    TestRedis.TestClient.SetEntryInHash(queueName, Guid.NewGuid().ToString("N"), "0");
                }
            }
            catch (Exception ex)
            {
                LogUtilities.WriteException(ex);
                //  throw;
            }

        }
    }
}
