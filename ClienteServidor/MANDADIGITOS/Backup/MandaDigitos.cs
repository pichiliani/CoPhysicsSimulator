using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
			
			string [] split = null;

			split = args[0].ToString().Split(",".ToCharArray());

			// Console.WriteLine(args[0].ToString());	
			// Console.WriteLine(split[0].ToString() + " " + split[1].ToString() + " " + split[2].ToString());
			
			if ( (split.Length == 8 ) && (split[7].ToString() != "") )
			{

				 Envia.EnviaDigitos(split[0].ToString(), // server
					System.Convert.ToInt32(split[1]), //porta
					split[2].ToString(), // site
					System.Convert.ToInt32(split[3]), // maq
					System.Convert.ToInt32(split[4]), // canal
					split[5].ToString(), // numa
					split[6].ToString(), // numb 
					split[7].ToString() ); //digito 

			} 

		}
	}

	public class Envia
	{

		public static void EnviaDigitos(string server,int port,string site,int maq,int canal_ura,string numa,string numb,string digito) 
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
				
				Get = "<SITE>" + site.ToString() + "</SITE>";
				Get += "<MAQ>" + maq.ToString() + "</MAQ>";
				Get += "<CANAL>" + canal_ura.ToString() + "</CANAL>";
				Get += "<NUMA>" + numa +  "</NUMA>";
				Get += "<NUMB>" + numb + "</NUMB>";
				Get += "<DIGITO>" + digito+ "</DIGITO>";

				Get += "<EOF>";

				ByteGet = ASCII.GetBytes(Get);
	
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
	}
	
}

		