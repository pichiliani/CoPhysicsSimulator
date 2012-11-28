using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using Microsoft.Ink;
using System.Windows.Forms;

namespace Physics.Collab
{
	public class ClienteConecta
	{
        private String serverName;
        private int serverPort;
        private String lo; //login
        private String pass; //senhs
    
        private Socket socket;

        public bool connected;
        private ClienteRecebe cr;
        public Thread tClienteRecebe; // Precisa, pois esta treadhchama a instânci do clienteReceve


        public ArrayList objAenviar;         // vector of currently connected clients
        public ArrayList objFila = new ArrayList();
    
        public ArrayList listaSessoes;  
	    public String AcaoAnterior = "";
        private Color cor_telepointer;
	    private String id_telepointer;



        #region ClienteConecta() 
        public ClienteConecta() 
        {
            this.connected = false;
            this.objAenviar = new ArrayList();
        }
        #endregion

        #region getLogin()
        public String getLogin()
	    {
		    return this.lo;
        }
        #endregion

        // Método que vai empacotar os dados antes deles serem enviados        
        #region getByteArrayWithObject()
        
        public static byte[] getByteArrayWithObject(Object o)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf1 = new BinaryFormatter();
            bf1.Serialize(ms, o);
            return ms.ToArray();
        }
        #endregion

        // Método que vai desempacotar os dados antes deles serem enviados        
        #region getObjectWithByteArray()
        public static object getObjectWithByteArray(byte[] theByteArray)
        {
            MemoryStream ms = new MemoryStream(theByteArray);
            BinaryFormatter bf1 = new BinaryFormatter();
            ms.Position = 0;

            return bf1.Deserialize(ms);
        }
        #endregion

        // monta o que vai ser enviado
        #region EnviaEvento()
        public void EnviaEvento(Object me,String evento)
        {
    	        if(connected) 
    	        {
			        ArrayList list = new ArrayList();
                    list.Add(me); // objeto 'genérico' (pode ser um mouse event ou outro)
        	        list.Add(evento); // o nome do evento
                
        	        this.objAenviar.Add(list);



    	        }
            }
        #endregion

        #region EnviaEventoAtraso()
        public void EnviaEventoAtraso(Object me,String evento)
        {
    	    if(connected) 
    	    {
        		
    		    ArrayList list = new ArrayList();
	    	    list.Add(me); // objeto 'genérico' (pode ser um mouse event ou outro)
        	    list.Add(evento); // o nome do evento
            
        	    this.objFila.Add(list);
            	
    	    }
        }
        #endregion

        #region EnviaAtraso()
        public void EnviaAtraso()
        {
    	    // Neste método são enviados todos os
    	    // eventos pendentes .

            System.Collections.IEnumerator myEnumerator = objFila.GetEnumerator();
          
            while ( myEnumerator.MoveNext() )
            {
                this.objAenviar.Add(myEnumerator.Current);
            }
        	 
    	     objFila.Clear();
         }

        #endregion

        #region SetaConecta()
        public bool SetaConecta(String s,int port) 
	    {
            
		    if(!this.connected)
		    {
			    this.serverName = s;
	            this.serverPort = port;
    	        
	            return  this.connect();
		    }
		    else
			    return true;
        }

        #endregion

        #region connect()
        protected bool connect() 
         {
             try
			{

                if(socket != null) 
                {
                    try 
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    } catch (Exception e) {}
                }
    
				IPEndPoint remoteEP = new IPEndPoint( IPAddress.Parse(serverName),serverPort);

				socket = new Socket(AddressFamily.InterNetwork,
					SocketType.Stream ,
					ProtocolType.Tcp); 

				socket.Connect(remoteEP);

				if (!socket.Connected)
				{
					// Connection failed, try next IPaddress.
                    MessageBox.Show("Não conectado!" , 
				    Application.ProductName, 
				    MessageBoxButtons.OK, 
				    MessageBoxIcon.Warning);



					socket = null;
                    connected = false;
        	        return false;
				}
                
                connected = true;
                return true;
            }
			catch(Exception e)
			{
                MessageBox.Show(
				"Exception on ClienteConecta:" + e.Message , 
				Application.ProductName, 
				MessageBoxButtons.OK, 
				MessageBoxIcon.Warning);

                connected = false;
        	    return false;
			}
        }

        #endregion

        #region run()  
        public void run()  
        {
            try {
                Object envia;
                
                while(true) 
                {
                    for (int i = 0; i < objAenviar.Count; i++) 
                    {
                	    envia = (Object) objAenviar[i];

                    	// Aqui efetivamente envia os dados!
                        Byte[] ByteGet;

                        ByteGet = getByteArrayWithObject(envia);

                        socket.Send(ByteGet, ByteGet.Length, 0); 
                    }
                    
                    // Limpando o vetor de itens enviados
                    objAenviar.Clear();
                   
                    // System.Diagnostics.Debug.WriteLine("----- Testando... -----");

                    // Esperando alguns segundos
                    System.Threading.Thread.Sleep(150); 
                }
            
            } catch (ThreadAbortException  e) 
            {
                this.disconnect();
                return;
            }
            catch (Exception e) 
            {

                MessageBox.Show(
				"Exception on ClienteConecta:" + e.Message , 
				Application.ProductName, 
				MessageBoxButtons.OK, 
				MessageBoxIcon.Warning);
            }    	

            this.disconnect();
        }

        #endregion

        #region disconnect() 

        public void disconnect () 
        {
            try 
            {
        	    // System.Threading.Thread.Sleep(3000); 

                this.socket.Shutdown(SocketShutdown.Both);
                this.socket.Close();

                connected = false;

                if (tClienteRecebe.IsAlive)
                    tClienteRecebe.Abort();
                

            } catch (Exception e) 
            {

                /* MessageBox.Show(
				"Exception on ClienteConecta:" + e.Message , 
				Application.ProductName, 
				MessageBoxButtons.OK, 
				MessageBoxIcon.Warning); */
            }
        }

        #endregion

        #region SetaUser()
        public bool SetaUser(String login,String senha) 
	   {
            try 
		    {
			    this.lo = login;
	            this.pass = senha;
    	    
    	        
	            // O primeiro objeto a ser enviado é uma string que vai indicar
	            // ao servidor se eh uma conexão para trocas de objetos no nível do 
	            // GEF ou nível do ArgoUML. Devo enviar também o login e a senha
	    	    ArrayList l = new ArrayList();
	      	    l.Add(this.lo); // 
	            l.Add(this.pass); // 
    	        
	            this.EnviaEvento(l,"ARGO");

                // Aqui efetivamente envia os dados!
                Byte[] ByteGet;

                ByteGet = getByteArrayWithObject((Object) objAenviar[0]);

                socket.Send(ByteGet, ByteGet.Length, 0); 

                objAenviar.Clear();
    			
                // Recebendo a resposta
                byte[] bytes = new byte[1024];
				int bytesRec = socket.Receive(bytes);
                	
                ArrayList list = (ArrayList) getObjectWithByteArray(bytes);          
                
    	
                Object o = list[0];
                String nomeEvento = (String) list[1];
    	
	    	    if (nomeEvento.Equals("ERRO"))
	    	    {
                    
                    MessageBox.Show("Login ou senha incorretos!" , 
				    Application.ProductName, 
				    MessageBoxButtons.OK, 
				    MessageBoxIcon.Warning);

    	    	
	    		    return false;
	    	    }
	    	    else
	    	    {
	    		    // Aqui vou armazenar as informações que vão ser colocadas na tabela!

            	    if (nomeEvento.Equals("PROT_lista_sessoes"))
            	    {
            		    ArrayList se = (ArrayList) o;
            		    // Colocando os nomes das sessões colaborativas
                		
            		    this.listaSessoes = se; 

            		    // Agora colocando a cor do telepointer
            		    String id = (String) list[2];
            		    Color c = (Color) list[3];
                		
                		this.setaCorTelepointer(c);
                        this.setaIdTelepointer(id);
        	        	
            	    } 

            	    // Iniciando a Thread que vai receber os dados
                    this.cr = new ClienteRecebe(socket);

                    ThreadStart threadDelegate = new ThreadStart(this.cr.run);
                    tClienteRecebe = new Thread(threadDelegate);
                    tClienteRecebe.Start();

                    // Iniciando a Thread...
    	            
	    		    return true;
	    	    }
		    }
            catch (Exception e) 
		    {

                    MessageBox.Show("Exception in ClienteConecta " + e.Message  , 
				    Application.ProductName, 
				    MessageBoxButtons.OK, 
				    MessageBoxIcon.Warning);

        	    return false;
            }

        }

        #endregion

        #region setaCorTelepointer()
        public void setaCorTelepointer(Color c)
        {
            cor_telepointer = c;
        }
        #endregion

        #region getCorTelepointer()
        public Color getCorTelepointer()
        {
            return cor_telepointer;
        }
        #endregion

        #region setaIdTelepointer()
        public void setaIdTelepointer(String c)
        {
            id_telepointer = c;
        }
        #endregion

        #region getIdTelepointer()
        public String getIdTelepointer()
        {
            return id_telepointer;
        }
        #endregion


    }
}
