using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Data.SqlClient;
using System.Threading;
using System.IO;
using System.Xml;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServGames
{
	public class ServGames : System.ServiceProcess.ServiceBase
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private LogWriter log;
		private Thread t;
		static private Socket flashGame;


		public ServGames()
		{
			// This call is required by the Windows.Forms Component Designer.
			InitializeComponent();

		}

		// The main entry point for the process
		static void Main()
		{
			System.ServiceProcess.ServiceBase[] ServicesToRun;
	
			// More than one user Service may run within the same process. To add
			// another service to this process, change the following line to
			// create a second service object. For example,
			//
			//   ServicesToRun = new System.ServiceProcess.ServiceBase[] {new Service1(), new MySecondUserService()};
			//
			ServicesToRun = new System.ServiceProcess.ServiceBase[] { new ServGames() };

			System.ServiceProcess.ServiceBase.Run(ServicesToRun);
		}

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.ServiceName = "ServGames";

			this.CanPauseAndContinue = true;
			this.CanShutdown = true;

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		/// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
		protected override void OnStart(string[] args)
		{
			this.log = LogWriter.getLogWriter();

			this.log.GravaLog("Log Error File Started",this.log.LogErrorName);
			this.log.GravaLog("Log Operation File Started",this.log.LogOpName); 


			try
			{
				t = new Thread(new ThreadStart(StartListening));

				t.Priority = ThreadPriority.Lowest; 
				t.Start();
			}
			catch(Exception ex)
			{
				this.log.GravaLog("Erro interno:" + ex.Message,this.log.LogOpName);
			}

		} 
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
			if(t!=null)
			{
		
				t.Abort();
				t.Join(2000);

				this.log.GravaLog("Log Error File Closed", this.log.LogErrorName);		
				this.log.GravaLog("Log Operation File Closed",this.log.LogOpName);		

			}

		}


		public void StartListening() 
		{
			
			IPEndPoint localEP = new IPEndPoint(IPAddress.Parse("200.143.8.148") ,11002);
			// IPEndPoint localEP = new IPEndPoint(IPAddress.Parse("192.168.1.210") ,11002);

			this.log.GravaLog("Local address and port: " + localEP.ToString() ,this.log.LogOpName); 

			Socket listener = new Socket(AddressFamily.InterNetwork,
				SocketType.Stream ,
				ProtocolType.Tcp); 
			

			Thread t;
			ArrayList ListThread = new ArrayList();

			String data = null;
			Socket handler;
			
			try 
			{
				listener.Bind(localEP);
				listener.Listen(10);
		
				byte[] bytes;
				int bytesRec;

				while (true) 
				{
					bytes = new byte[5024];

					// Recebe o Socket
					handler = listener.Accept();
					
					bytesRec = handler.Receive(bytes);
					data = Encoding.ASCII.GetString(bytes,0,bytesRec);
					
					if (data.IndexOf("SITE") > -1)  //
					{
						// Criando a Thread que vai receber os dados da URA
						t = new Thread(new ThreadStart(TrataSocketUra));
						t.Name = data;
						t.Start();

						ListThread.Add(t);

						// Fechando o socket da URA
						handler.Shutdown(SocketShutdown.Both);
						handler.Close();
					}
					else if (data.IndexOf("FLASH") > -1)  
					{
						flashGame = handler;

						// Criando a Thread que vai receber os dados do FLASH
						t = new Thread(new ThreadStart(TrataSocketFlash));
						t.Priority = ThreadPriority.Normal; 
						t.Start();

						ListThread.Add(t);
					}

					// LogWriter.getLogWriter().GravaLog("Text received from socket: " + data,LogWriter.getLogWriter().LogOpName);	

					data = "";
				}
			} 
			catch (Exception e) 
			{
				this.log.GravaLog("Error:" + e.ToString() ,this.log.LogErrorName);
			}
		}	

		static private void TrataSocketFlash()
		{
			try
			{
				String data = null;
								
				while (true) 
				{
					byte[] bytes = new byte[1024];
					int bytesRec = flashGame.Receive(bytes);
					data += Encoding.ASCII.GetString(bytes,0,bytesRec);

					if (data.Equals(""))
					{
						break;
					}
					

					if (data.IndexOf("EOF") > -1)  
					{
						
						// Aqui é verificado o que o flash mandou
	
						// Se tivemos algum status do jogo
						if ((data.IndexOf("ACERTOU") > -1) || (data.IndexOf("ERROU") > -1) || (data.IndexOf("FIM") > -1) )
						{
							// Preciso enviar informações para a URA
							EnviaDadosUra(data);
						}

					
						// Se foi uma desconexão
						if (data.IndexOf("DESCONECTOU") > -1)  
						{
							break;
						}

						
						// break;
						data = "";
					}
					

				}

				// LogWriter.getLogWriter().GravaLog("Flash Socket closed",LogWriter.getLogWriter().LogOpName); 
				flashGame.Shutdown(SocketShutdown.Both);
				flashGame.Close();

			}
			catch (Exception e) 
			{
				LogWriter.getLogWriter().GravaLog("Error in flash socket:"  + e.ToString(),LogWriter.getLogWriter().LogErrorName);
			}

		}

		static private void TrataSocketUra()
		{
			try
			{
				String data = Thread.CurrentThread.Name;
				
				data = data + "\0";  // Deve-se colocar o \0 quando enviar algo para o flash!

				// Enviando para o jogo se o servidor estiver conectado ao jogo
				if (flashGame.Connected)
				{
					byte[] msg = Encoding.ASCII.GetBytes(data);
					flashGame.Send(msg,msg.Length,0);
				}
				else
				{
					LogWriter.getLogWriter().GravaLog("Cannot send URA data to Flash." ,LogWriter.getLogWriter().LogOpName); 
				}
					
			}
			catch (Exception e) 
			{
				LogWriter.getLogWriter().GravaLog("Error in URA socket:"  + e.ToString(),LogWriter.getLogWriter().LogErrorName);
			}
		}

		
		static void EnviaDadosUra(string dados)
		{
			int Maquina = 0;
			int Canal = 0;
			string NumeroA = "";
			string NumeroB = "";
			string Status = "";

			XmlTextReader reader;

			try
			{
				reader = new XmlTextReader(dados,XmlNodeType.Element,null);

				reader.WhitespaceHandling=WhitespaceHandling.None;

				while (reader.Read())
				{

					if (reader.NodeType == XmlNodeType.Element)
					{
						switch (reader.Name )
						{
							case "MAQ":
								Maquina = int.Parse(reader.ReadString());
								break;
							case "CANAL":
								Canal = int.Parse(reader.ReadString());
								break;
							case "NUMA":
								NumeroA = reader.ReadString();
								break;
							case "NUMB":
								NumeroB = reader.ReadString();
								break;
							case "STATUS":
								Status = reader.ReadString();
								break;
						}
					}
				} 

				if (reader != null)
					reader.Close();

				// SqlConnection con1 = new SqlConnection("Server=200.212.36.130;Database=Talk;User id=sa;Password=milano4081;Pooling=false");
				// SqlConnection con1 = new SqlConnection("Server=192.168.1.102;Database=Talk;User id=sa;Password=talk;Pooling=false");
				SqlConnection con1 = new SqlConnection("Server=192.168.3.101;Database=Talk;User id=sa;Password=milano4081;Pooling=false");
				con1.Open();
			
				SqlCommand sqlcmdGravaURA = new SqlCommand();

				sqlcmdGravaURA.Connection = con1;
				sqlcmdGravaURA.CommandType = System.Data.CommandType.Text;

				sqlcmdGravaURA.CommandText  = "UPDATE TB_URA_GAMES SET ";
				sqlcmdGravaURA.CommandText += " FLAG='" + Status.Substring(0,1) +   "'";

				sqlcmdGravaURA.CommandText += " WHERE MAQ =  " + Maquina.ToString();
				sqlcmdGravaURA.CommandText += " AND CANAL = "  + Canal.ToString();
				sqlcmdGravaURA.CommandText += " AND NUMA = '"  + NumeroA + "'";
				sqlcmdGravaURA.CommandText += " AND NUMB = '"  + NumeroB + "'";

				sqlcmdGravaURA.ExecuteNonQuery();
			}
			catch (Exception eX)
			{
				LogWriter.getLogWriter().GravaLog("Error in XML:"  + eX.ToString(),LogWriter.getLogWriter().LogErrorName);
			}

		}

	}


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


}
