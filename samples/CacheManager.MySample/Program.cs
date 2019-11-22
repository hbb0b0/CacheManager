using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CacheManager.Core;

namespace CacheManager.MySample
{
    class Program
    {
        static void Main(string[] args)
        {
            //MultiThread();
            //GetCacheData();
            //UpdateForSlidingExpireTime();
            UpdateForAbsoluteExpireTime();
            Console.Read();
        }



        public static void UpdateForSlidingExpireTime()
        {
            var cache = CacheFactory.Build<string>(s => s.WithDictionaryHandle());
            Console.WriteLine($"Testing update...{ExpirationMode.Sliding.ToString()}");
            string myKey = "test1";
            CacheManager.Core.CacheItem<string> cacheItem = new CacheItem<string>(myKey, "100", ExpirationMode.Sliding, TimeSpan.FromMilliseconds(50000));
            cache.Add(cacheItem);
            bool needUpdate = true;
            int i = 0;
            string dbValue = "";
            while (needUpdate)
            {
                Thread.Sleep(2000);
                var currentCacheItem = cache.GetCacheItem(myKey);
                if (currentCacheItem == null)
                {
                    break;
                }
                Console.WriteLine($"get:{ cache.Get<string>(myKey)}  CreatedUtc:{ currentCacheItem.CreatedUtc} ExpirationMode:{currentCacheItem.ExpirationMode} ExpirationTimeout:{ currentCacheItem.ExpirationTimeout} LastAccessedUtc:{currentCacheItem.LastAccessedUtc}");
                var result = cache.TryUpdate(myKey, p =>
                {
                    return i.ToString();
                }, out dbValue);

            }
        }

        public static void GetCacheData()
        {
            const int MAXCOUNTER = 1000;

            //var cache = CacheFactory.Build<int>(s => s.WithDictionaryHandle());
            var cache = CacheFactory.Build<int>(settings =>
            {
                settings.WithDictionaryHandle("CacheForInt");

            });
            /*
            string testKey = "testKey";
            Task[] tasks = new Task[MAXCOUNTER];

            int myCounter = 1;

            for (int i = 0; i < MAXCOUNTER; i++)
            {

                var t = Task.Run(() =>
                {
                    cache.Add($"testKey_{i}", i);
                });
                tasks[i] = t;
            }

            Task.WaitAll(tasks);
            */
            cache.Add($"testKey_{0}", 100);

            var data = cache.CacheHandles.First().GetInternalData();

        

           // Console.WriteLine(myCounter);

        }


        public static void MultiThread()
        {
            const int MAXCOUNTER = 10000;

            var cache = CacheFactory.Build<int>(s => s.WithDictionaryHandle());

            string testKey = "testKey";
            Task[] tasks = new Task[MAXCOUNTER];

            int myCounter = 1;

            for (int i = 0; i < MAXCOUNTER; i++)
            {
               
                var t = Task.Run(() =>
                {
                    myCounter += 1;
                    cache.AddOrUpdate(testKey, 1, (p) =>
                    {

                        return p + 1;
                    });
                });
                tasks[i] = t;
            }

            Task.WaitAll(tasks);

            Console.WriteLine(cache.Get<int>(testKey) );

            Console.WriteLine(myCounter);

        }


        public static void UpdateForAbsoluteExpireTime()
        {
            var cache = CacheFactory.Build<string>(s => s.WithDictionaryHandle());

            cache.OnRemoveByHandle += Cache_OnRemoveByHandle;

            Console.WriteLine("Testing update...");

            //if (!cache.TryUpdate("test", v => "item has not yet been added", out string newValue))
            //{
            //    Console.WriteLine("Value not added?: {0}", newValue == null);
            //}
            string myKey = "test1";
            CacheManager.Core.CacheItem<string> cacheItem = new CacheItem<string>(myKey, "100", ExpirationMode.Absolute, TimeSpan.FromMilliseconds(50000));
            cache.Add(cacheItem);
            bool needUpdate = true;
            int i = 0;
            string dbValue = "";
            while (needUpdate)
            {
                Thread.Sleep(2000);
                var currentCacheItem = cache.GetCacheItem(myKey);
                if (currentCacheItem == null)
                {
                    break;
                }
                Console.WriteLine($"get:{ cache.Get<string>(myKey)}  CreatedUtc:{ currentCacheItem.CreatedUtc} ExpirationMode:{currentCacheItem.ExpirationMode} ExpirationTimeout:{ currentCacheItem.ExpirationTimeout.TotalMilliseconds} LastAccessedUtc:{currentCacheItem.LastAccessedUtc}");
                var result = cache.TryUpdate(myKey, p =>
                {
                    return i.ToString();
                }, out dbValue);

            }
        }



        /// <summary>
        /// 当缓存项被移除时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Cache_OnRemoveByHandle(object sender, Core.Internal.CacheItemRemovedEventArgs e)
        {
            Console.WriteLine($"Cache_OnRemove1 :{sender.ToString()}:{e.Key}");
        }
    }
}
