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
	public class ClienteRecebe
	{
	    private Socket socket;
        ClienteRecebe cr;

        #region ClienteRecebe()
        public ClienteRecebe(Socket s) 
        {
		    this.socket = s;
        }
        #endregion

        #region run()
        public void run ()  
        {
            
            System.Threading.Thread.CurrentThread.Priority = ThreadPriority.Highest;
    	    try 
            {

                bool clientTalking = true;
                
                //a loop that reads from and writes to the socket
                while (clientTalking) 
                {
            	    //get what client wants to say...
                    byte[] bytes = new byte[8192]; // 8 KB de dados
				    int bytesRec = this.socket.Receive(bytes);
                    
                    // System.Diagnostics.Debug.WriteLine("----- Recebido... -----");

                    ArrayList list = (ArrayList) getObjectWithByteArray(bytes);          
                    
                    Object o = list[0];
                    String nomeEvento = (String) list[1];
                	

                    // EVENTOS DO ARGO
               	    if (nomeEvento.Equals("PROT_atualiza_modelo_cliente"))
            	    {
                   		
            	    }
                    
               	    if (nomeEvento.Equals("PROT_atualiza_modelo_cliente_inicial"))
            	    {
                        ArrayList ListDoc = (ArrayList) o;	

                        // Global.main.OpenForCollaboration( );

                        Global.main.Invoke(new MainForm.DelegateOpenMetod(Global.main.OpenForCollaboration) ,new object[] { (TextWriter) ListDoc[0] });

            	    }
                   	

    //              Recebeu a notificação que algum cliente entrou na da sessão!
               	    if (nomeEvento.Equals("PROT_inicio_sessao"))
            	    {
            	    }

                   	
    //              Recebeu a notificação que algum cliente saiu da sessão!
               	    if (nomeEvento.Equals("PROT_fim_sessao"))
            	    {
            	    }

                    // Esta ação foi removida para a experiência
               	    /* if (nomeEvento.equals("ActionDeleteFromDiagram-actionPerformed"))
                	    ActionDeleteFromDiagram.SINGLETON.actionPerformedImpl((ActionEvent) o); */
                    
                    

                    // EVENTOS DO GEF

                    // Desenho do TelePointer!
                    #region mouseMovedPointer
                    if (nomeEvento.Equals("mouseMovedPointer"))
                    {
                        
                         
                        /* 
                        // Sem os telePointers por enquanto
                        ArrayList dados = (ArrayList) o;
                        
                        FigPointer fp = Global.main.fpA;
                        String Id_tele  = (String) dados[2];

                        if (Id_tele.Equals("1") ) fp = Global.main.fpA;
                        if (Id_tele.Equals("2") ) fp = Global.main.fpB;
                        if (Id_tele.Equals("3") ) fp = Global.main.fpC;
                        if (Id_tele.Equals("4") ) fp = Global.main.fpD;

                        fp.setCor((Color) dados[1] );
                        fp.setNome((String) dados[3]);
                        
                        fp.setLocation((Point) dados[0]); */

                        

                    }
                    #endregion

                    #region inkoverlay_Stroke
                    if (nomeEvento.Equals("inkoverlay_Stroke"))
                    {
                        ArrayList dados = (ArrayList) o;

                        
                        Ink x = new Ink();
                        x.Load( (byte[]) dados[0] );

                        Stroke s = x.Strokes[x.Strokes.Count-1]; 
                        
                        // Testes: Adicionando a coleção de strokes!
                        Global.main.doc.Ink.CreateStroke(s.GetPoints());


                        InkCollectorStrokeEventArgs e = new InkCollectorStrokeEventArgs(null ,s , false);

                        // Precisa utilizar delegates, pois existem problemas quando uma Thread que não é
                        // o formulário atualiza a interface gráfica
                        Global.main.Invoke(new MainForm.DelegateToMethod(Global.main.inkoverlay_StrokeImpl),new object[] { e });

                    }
                    #endregion

                    #region inkoverlay_StrokesDeleting
                    if (nomeEvento.Equals("inkoverlay_StrokesDeleting"))
                    {
                        ArrayList dados = (ArrayList) o;

                        // Precisa utilizar delegates, pois existem problemas quando uma Thread que não é
                        // o formulário atualiza a interface gráfica
                        Global.main.Invoke(new MainForm.DelegateEraseMethod(Global.main.inkoverlay_StrokesDeletingImpl),new object[] { dados });

                    }
                    #endregion

                    #region inkoverlay_SelectionMovedOrResized
                    if (nomeEvento.Equals("inkoverlay_SelectionMovedOrResized"))
                    {
                        ArrayList dados = (ArrayList) o;

                        // Precisa utilizar delegates, pois existem problemas quando uma Thread que não é
                        // o formulário atualiza a interface gráfica
                        Global.main.Invoke(new MainForm.DelegateEraseMethod(Global.main.inkoverlay_SelectionMovedOrResizedImpl),new object[] { dados });

                    }
                    #endregion
                    
                    #region hover_EditCloneClicked
                    if (nomeEvento.Equals("hover_EditCloneClicked"))
                    {
                        ArrayList dados = (ArrayList) o;

                        // Precisa utilizar delegates, pois existem problemas quando uma Thread que não é
                        // o formulário atualiza a interface gráfica
                        Global.main.Invoke(new MainForm.DelegateEraseMethod(Global.main.hover_EditCloneClickedImpl),new object[] { dados });

                    }
                    #endregion

                    #region hover_EditStraightenClicked
                    if (nomeEvento.Equals("hover_EditStraightenClicked"))
                    {
                        ArrayList dados = (ArrayList) o;

                        // Precisa utilizar delegates, pois existem problemas quando uma Thread que não é
                        // o formulário atualiza a interface gráfica
                        Global.main.Invoke(new MainForm.DelegateEraseMethod(Global.main.hover_EditStraightenClickedImpl),new object[] { dados });

                    }
                    #endregion

                    #region hover_EditPropertiesClicked
                    if (nomeEvento.Equals("hover_EditPropertiesClicked"))
                    {
                        ArrayList dados = (ArrayList) o;

                        // Precisa utilizar delegates, pois existem problemas quando uma Thread que não é
                        // o formulário atualiza a interface gráfica
                        Global.main.Invoke(new MainForm.DelegateEraseMethod(Global.main.hover_EditPropertiesClickedImpl),new object[] { dados });

                    }
                    #endregion

                    // Controles da simulação remota

                    #region hover_AnimateClicked_Start
                    if (nomeEvento.Equals("Start"))
                    {
                        
                        // É preciso criar outra Thread aqui para continuar recebendo os dados....
                        // E depois é preciso terminá-la
                        
                        cr = new ClienteRecebe(this.socket);

                        ThreadStart threadDelegate = new ThreadStart(cr.run);
                        Thread tClienteRecebe = new Thread(threadDelegate);
                        tClienteRecebe.Start();  

                        Object dados = (Object) o;

                        // Precisa utilizar delegates, pois existem problemas quando uma Thread que não é
                        // o formulário atualiza a interface gráfica
                        Global.main.Invoke(new MainForm.DelegateSimulationMetod(Global.main.hover_AnimateClickedStartImpl),new object[] { dados });

                        /* tClienteRecebe.Abort();
                        Thread.Sleep(5);
                        cr = null;   */


                    }
                    #endregion

                    #region hover_AnimateClicked_Stop
                    if (nomeEvento.Equals("Stop"))
                    {
                        Object dados = (Object) o;

                        // Precisa utilizar delegates, pois existem problemas quando uma Thread que não é
                        // o formulário atualiza a interface gráfica
                        Global.main.Invoke(new MainForm.DelegateSimulationMetod(Global.main.hover_AnimateClickedStopImpl),new object[] { dados });

                        // System.Threading.Thread.CurrentThread.Abort();
                        // É preciso cometer suicídio, ou seja, essa Thread deve se auto-terminar

                        if(cr != null)
                            return;

                    }
                    #endregion

                    
                    #region hover_AnimateClicked_Pause
                    if (nomeEvento.Equals("Pause"))
                    {
                        Object dados = (Object) o;

                        // Precisa utilizar delegates, pois existem problemas quando uma Thread que não é
                        // o formulário atualiza a interface gráfica
                        Global.main.Invoke(new MainForm.DelegateSimulationMetod(Global.main.hover_AnimateClickedPauseImpl) ,new object[] { dados });

                    }
                    #endregion 

                    #region hover_AnimateClicked_Resume
                    if (nomeEvento.Equals("Resume"))
                    {
                        Object dados = (Object) o;

                        // Precisa utilizar delegates, pois existem problemas quando uma Thread que não é
                        // o formulário atualiza a interface gráfica
                        Global.main.Invoke(new MainForm.DelegateSimulationMetod(Global.main.hover_AnimateClickedResumeImpl) ,new object[] { dados });

                    }
                    #endregion 



                }
               	
            } 
            catch (ThreadAbortException  e) 
            {
                MessageBox.Show("ThreadAbortException in ClienteRecebe:" + e.Message  , 
			    Application.ProductName, 
			    MessageBoxButtons.OK, 
			    MessageBoxIcon.Warning);
                return;
            }
            catch (Exception e) 
            {
                MessageBox.Show("Exception in ClienteRecebe:" + e.Message  , 
			    Application.ProductName, 
			    MessageBoxButtons.OK, 
			    MessageBoxIcon.Warning);
            }
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



    }
}
