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
	public class FigPointer
	{
	    private Color cor;
        private String nome;
        private Point p_anterior = new Point(0,0);

        #region FigPointer()
        public FigPointer(Color c, String n) 
        {
		    this.cor = c;
            this.nome =  n;
        }
        #endregion

        #region setCor()
        public void setCor(Color c)
        {
            this.cor = c;
        }
        #endregion

        #region setNome()
        public void setNome(String n)
        {
            this.nome = n;
        }

        #endregion

        #region setLocation()
        public void setLocation(Point p)
        {
            Graphics g = Global.main.CreateGraphics();
 
            // Apagando as coordenadas anteriores
            // TODO: VERFICAR SE HÁ ALGUM POLÍGONO OU CORDA POR TRÁS. Neste caso não utilizar a cor branca para
            // limpar. OU melhor, neste caso nem mostrar o telepoiner
            g.DrawRectangle(new Pen(new SolidBrush(Color.White)),p_anterior.X - 10,p_anterior.Y-2,20,4);
            g.DrawRectangle(new Pen(new SolidBrush(Color.White)),p_anterior.X - 2,p_anterior.Y-10,4,20);

            g.DrawString(this.nome,new Font("Arial",10),new SolidBrush(Color.White),p_anterior.X +4,p_anterior.Y + 4);
            
            // Pintando o TelePointer
            g.DrawRectangle(new Pen(new SolidBrush(this.cor)),p.X - 10,p.Y-2,20,4);
            g.DrawRectangle(new Pen(new SolidBrush(this.cor)),p.X - 2,p.Y-10,4,20);

            g.DrawString(this.nome,new Font("Arial",10),new SolidBrush(this.cor),p.X +4,p.Y + 4);

            // Atualizando o ponto
            p_anterior = new Point(p.X,p.Y);

        }
        #endregion


        #region getCor()
        public Color getCor()
        {
            return this.cor;
        }
        #endregion

        #region getNome()
        public String getNome()
        {
            return this.nome;
        }
        #endregion

    }
}
