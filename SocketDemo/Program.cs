using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("主线程测试开始...");
            AsyncMethod();
            Thread.Sleep(1000);
            Console.WriteLine("主线程测试结束...");
            Console.ReadLine();
        }

        static async void AsyncMethod()
        {
            Console.WriteLine("开始异步代码");
            var result = await MyMethod(10);
            Console.WriteLine("异步代码执行完毕");
        }

        static async Task<int> MyMethod(int index)
        {
            for (int i = 0; i < index; i++)
            {
                Console.WriteLine("异步执行 " + i.ToString() + "...");
                await Task.Delay(1000); //模拟耗时操作
            }
            return 0;
        }
    }
}
