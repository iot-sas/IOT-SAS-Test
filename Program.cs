using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FactomSharp;
using FactomSharp.Factomd;
using System.Linq;

namespace IOTSASTest
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            //Open a connection to the IOT-SAS board
            using (var iotsas = new IOT_SAS("/dev/ttyUSB0", 57600))
            {
                //Open a connection to a Factom node
                var factomd = new FactomdRestClient("http://87.117.232.37:8088"); //"https://api.factomd.net/v2");

                //Get an EC address from the IOS-SAS
                var address = iotsas.GetECAddress(factomd);  
                    
                Console.WriteLine($"EC Address: {address.Public}");
                Console.WriteLine($"Balance {address.GetBalance()}");

                using (var chain = new Chain(address))
                {
                    //As this is an asynchronous class, we need to capture events.
                    chain.ChainStatusChange += (o, a) =>
                    {
                        Console.WriteLine($"Chain: {a.ToString()}");
                    };

                    chain.QueueItemStatusChange += (o, a) =>
                    {
                        var item = (EntryItem)o;
                        Console.WriteLine($"Entry: {a.ToString()} {item?.ApiError?.error}");
                    };

                    //Create a Chain.
                    chain.Create(Encoding.ASCII.GetBytes(String.Join(" ",args)));
                    Console.WriteLine($"ChainID: {chain.ChainID}");

                    //Make a second entry to the chain.
                    var entry = chain.AddEntry(Encoding.ASCII.GetBytes("This is a test - hello!"));                   

                    Console.ReadLine(); //Pause
                }
            }
        }
    }
}
