using System;
using System.Threading.Tasks;

namespace RestSharp.InteractiveTests
{
    class Program
    {
        static async Task Main()
        {
            var keys = new AuthenticationTests.TwitterKeys
            {
                ConsumerKey = Prompt("Consumer key"),
                ConsumerSecret = Prompt("Consumer secret"),
            };
            
           AuthenticationTests.Can_Authenticate_With_OAuth(keys);
           await AuthenticationTests.Can_Authenticate_With_OAuth_Async_With_Callback(keys);
        }

        static string Prompt(string message)
        {
            Console.Write(message + ": ");
            return Console.ReadLine();
        }
    }
}
