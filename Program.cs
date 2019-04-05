using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FactomSharp;
using FactomSharp.Factomd;

namespace IOTSASTest
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            //Open a connection to the IOT-SAS board
            using (var iotsas = new IOT_SAS("/dev/ttyAMA0", 57600))
            {
                //Open a connection to a Factom node
                var factomd = new FactomdRestClient("https://api.factomd.net/v2");

                //Get an EC address from the IOS-SAS
                var address = iotsas.GetECAddress(factomd);  
                    
                Console.WriteLine($"EC Address: {address.Public}");
                Console.WriteLine($"Balance {address.GetBalance()}");

                using (var chain = new Chain(address))
                {
                    chain.ChainStatusChange += (o, a) =>
                    {
                        Console.WriteLine($"Chain: {a.ToString()}");
                    };

                    chain.QueueItemStatusChange += (o, a) =>
                    {
                        var item = (EntryItem)o;
                        Console.WriteLine($"Entry: {a.ToString()} {item?.ApiError?.error}");
                    };

                    chain.Create(Encoding.ASCII.GetBytes("This is my new chain"));

                    var text = $"This is a test - hello!";

                    var entry = chain.AddEntry(Encoding.ASCII.GetBytes(text));
                    Console.WriteLine($"ChainID: {chain.ChainID}");

                    Console.ReadLine(); //Pause
                }
            }
        }
    }
}
