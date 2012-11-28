using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;

namespace MandaDigitos
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	class MandaDigitos
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			
		    Envia.EnviaDigitos("192.168.1.32",100); 

		}
	}

	public class Envia
	{

		public static void EnviaDigitos(string server,int port) 
		{
			try
			{
				String strRetPage = null;
				
				int conPort = port;

				IPEndPoint remoteEP = new IPEndPoint( IPAddress.Parse(server),conPort);

				Socket s = new Socket(AddressFamily.InterNetwork,
					SocketType.Stream ,
					ProtocolType.Tcp); 

				s.Connect(remoteEP);

				if (!s.Connected)
				{
					// Connection failed, try next IPaddress.
					strRetPage = "Unable to connect to host";
					Console.WriteLine("Message : " + strRetPage);
					s = null;
					return;
				}

                
                //Set up variables and String to write to the server.
				Encoding ASCII = Encoding.ASCII;
				string Get = "";
				Byte[] ByteGet;
				
                ArrayList lista = new ArrayList();

                lista.Add(10);
                lista.Add("b");

				// ByteGet = getByteArrayWithObject(10);

                ByteGet = getByteArrayWithObject(lista);
	
				s.Send(ByteGet, ByteGet.Length, 0); 
                
				s.Shutdown(SocketShutdown.Both);
				s.Close();

			}
			catch(SocketException e) 
			{
				Console.WriteLine("SocketException caught!!!");
				Console.WriteLine("Source : " + e.Source);
				Console.WriteLine("Message : " + e.Message);
			}
			catch(ArgumentNullException e)
			{
				Console.WriteLine("ArgumentNullException caught!!!");
				Console.WriteLine("Source : " + e.Source);
				Console.WriteLine("Message : " + e.Message);
			}
			catch(NullReferenceException e)
			{
				Console.WriteLine("NullReferenceException caught!!!");
				Console.WriteLine("Source : " + e.Source);
				Console.WriteLine("Message : " + e.Message);
			}
			catch(Exception e)
			{
				Console.WriteLine("Exception caught!!!");
				Console.WriteLine("Source : " + e.Source);
				Console.WriteLine("Message : " + e.Message);
			}

		}


        public static byte[] getByteArrayWithObject(Object o)
        {
            /*

                1) Create a new MemoryStream class with the CanWrite property set to true
                (should be by default, using the default constructor).

                2) Create a new instance of the BinaryFormatter class.

                3) Pass the MemoryStream instance and your object to be serialized to the
                Serialize method of the BinaryFormatter class.

                4) Call the ToArray method on the MemoryStream class to get a byte array
                with the serialized data.

            */


            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf1 = new BinaryFormatter();
            bf1.Serialize(ms, o);
            return ms.ToArray();
        }

        


    }


	
}

		