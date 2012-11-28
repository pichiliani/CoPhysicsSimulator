using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Threading;
using System.IO;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;
using System.Runtime.Serialization.Formatters.Binary;

namespace ServerTeste
{
	class CollabServer
	{

        public ArrayList clientVector;         // vector of currently connected clients
	    private int id_telepointer = 1;
	    private ArrayList telepointers = new ArrayList();
	    private ArrayList userList = new ArrayList(); // Lista contendo os usuários e senhas

        #region CollabServer()
        public  CollabServer()
        {

        }
        #endregion

        #region Main(string[] args)
        [STAThread]
		static void Main(string[] args)
		{
			
            // TODO: Inserir endereço IP do servidor e IP como parâmetro
            CollabServer s = new CollabServer();
            s.IniciaServico(100);
        }
        #endregion

        #region IniciaServico
        public void IniciaServico(int port)
        {
            // Lista de usuários
             String []x = {"A" ,"A"};
    	    userList.Add (x);

    	    String []y = {"B" ,"B"};
		    userList.Add (y);


            
            IPHostEntry ipHostInfo = Dns.GetHostByName("localhost"); 
			IPEndPoint localEP = new IPEndPoint(ipHostInfo.AddressList[0],100);

			Console.WriteLine("Local address and port: " + localEP.ToString());

			Socket listener = new Socket( localEP.Address.AddressFamily,
				SocketType.Stream, ProtocolType.Tcp );

			Thread t;
			ArrayList ListThread = new ArrayList();

			int i;
			String data = null;
			Socket handler;
			
			try 
			{
				listener.Bind(localEP);
				listener.Listen(10);
		
				i = 0;
				while (true) 
				{
					// Recebe o Socket
					handler = listener.Accept();

					byte[] bytes = new byte[1024];
					int bytesRec = handler.Receive(bytes);
					
                    // int dado = (int) getObjectWithByteArray(bytes);
                    // Console.WriteLine("Valor: " + dado.ToString());

                    ArrayList l = (ArrayList) getObjectWithByteArray(bytes);                     
                    Console.WriteLine("Objeto: " + l.ToString());
                    Console.WriteLine("Valor: " + l.Count.ToString());
                    // Console.WriteLine("Valor: " + l.IndexOf(1).ToString());
			
					i++;
				}
			} 

			catch (Exception e) 
			{
				Console.WriteLine("Error:" + e.ToString());

			}


        }
        #endregion


        #region getObjectWithByteArray(byte[] theByteArray)
        public static object getObjectWithByteArray(byte[] theByteArray)
        {
            MemoryStream ms = new MemoryStream(theByteArray);
            BinaryFormatter bf1 = new BinaryFormatter();
            ms.Position = 0;

            return bf1.Deserialize(ms);
        }
        #endregion

    }



    #region Classe LogWriter
    public class LogWriter
	{
		private static LogWriter p;

		private String LogErrorFileName;
		private String LogOpFileName;
		// Propriedades do Destino

		
		public String LogErrorName
		{
			get {return this.LogErrorFileName;}
			set {this.LogErrorFileName= value;}
		}

		public String LogOpName
		{
			get {return this.LogOpFileName;}
			set {this.LogOpFileName= value;}
		}

		
		private LogWriter()
		{

			string caminho = @System.Environment.CommandLine;
			caminho = 	caminho.Substring(1,caminho.LastIndexOf(@"\")-1);
		
			this.LogErrorFileName=caminho + @"\LogError" + DateTime.Now.ToString("yyyy-MM-dd-HH_mm_ss")+  ".log";
			this.LogOpFileName=caminho + @"\LogOp" + DateTime.Now.ToString("yyyy-MM-dd-HH_mm_ss")+  ".log";;

		}

		public static LogWriter getLogWriter()
		{
			if (p == null)
				p = new LogWriter();

			return p;
		}

		
		public void GravaLog(String msg,String filename)
		{
			// Gravando os logs de erro
			StreamWriter w =  File.AppendText(@filename);
			
			w.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " - " +  msg + "\n\n");	
			w.Flush();
			w.Close();
		}
    }
    #endregion


}
