using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using FactomSharp.Factomd;

namespace IOTSASTest
{
    public class IOT_SAS : IDisposable
    {
        SerialPort mySerial;
        
    
        //Set up serial port.
        public IOT_SAS(String device, uint baudRate = 57600)
        {
            mySerial = new SerialPort(device, (int)baudRate, Parity.None, 8, StopBits.One);
            
            mySerial.Handshake = Handshake.None;
            mySerial.Open();        
        }


        //Make an EC address class using the IOT-SAS public key, and override the signing function
        public FactomSharp.ECAddress GetECAddress(FactomdRestClient factomd)
        {
            var ecAddress = new FactomSharp.ECAddress(factomd, GetPublicECAddress());
            ecAddress.SignFunction = (data) =>
            {
                    return SignEd25519(data);
            };
            return ecAddress;
        }
        
        //Get the public key from the IOT-SAS board.
        private string GetPublicECAddress()
        {
            mySerial.DiscardInBuffer();
            var GetEC = new byte[] { 0xFA, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00 };
            mySerial.Write(GetEC, 0, GetEC.Length);

            var key = new byte[52];
            
            var count = 0;
            var bytesRx = 0;
            while (count < key.Length)
            {
                bytesRx = mySerial.Read(key, count, key.Length-count);
                count += bytesRx;
            }

            return Encoding.ASCII.GetString(key);
        }

        
        //Sign data on the IOT-SAS board, unsing the hardware secret key.
        public byte[] SignEd25519(byte[] data)
        {
            mySerial.DiscardInBuffer();
            var toSign = new byte[data.Length + 5];
            toSign[0] = 0xFA;
            toSign[1] = 0x02;
            toSign[2] = 0x02;
            toSign[3] = 0x0;
            toSign[4] = (byte)data.Length;
            Array.Copy(data, 0, toSign, 5, data.Length);            
            
            mySerial.Write(toSign, 0, toSign.Length);

            var signature = new byte[64];
            
            var count = 0;
            var bytesRx = 0;
            while (count < signature.Length)
            {
                bytesRx = mySerial.Read(signature, count, signature.Length-count);
                count += bytesRx;
            }
            
            return signature;
        }
        
        public void Dispose()
        {
            mySerial.Close();
        }
    }    
}
