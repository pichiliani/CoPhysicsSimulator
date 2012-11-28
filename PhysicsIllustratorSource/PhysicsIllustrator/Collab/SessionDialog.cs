//-------------------------------------------------------------------------
//
//  Copyright (C) 2004 Microsoft Corporation
//  All rights reserved.
//
//  File: SessionDialog.cs
//  
//  Description: About box form.
//--------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Reflection;

using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;
using System.Drawing.Drawing2D;
using Microsoft.Ink;


using dbg=System.Diagnostics.Debug;

//[System.ComponentModel.DesignerCategory("Code")]
internal class SessionDialog : System.Windows.Forms.Form
{
    private Button button2;
    private Label label1;
    private Label label2;
    private TextBox txtSession;
    private System.Windows.Forms.Button button1;
    private Label lblUser;
    private Label label3;
    private ListBox lstSessions;

    // 
    private Thread tClienteEnvia; // Precisa 

	// 
    // Initialization 

    #region SessionDialog()
    public SessionDialog()
	{
		// Required for Windows Form Designer support.
		InitializeComponent();

		// Load our icon.
		this.Icon = new Icon(typeof(SessionDialog),"PhysicsIllustrator.ico");

		// Set the version label.
		
		// Load image for panel, tracking the size-change.
		
        this.lblUser.Text = Global.clienteEnvia.getLogin();

        // Aqui carrego as sessões existentes:

        ArrayList todas_se = Global.clienteEnvia.listaSessoes;
 
        for (int i = 0; i < todas_se.Count; i++) 
        {
            String []sessao = (String []) todas_se[i];

            // this.lstSessions.Items.Add("Session: " + sessao[0] + " + User: " + sessao[1]  );
            this.lstSessions.Items.Add(sessao[0]);
        }
    }

    #endregion

    #region Windows Form Designer generated code
    private void InitializeComponent()
	{
        this.button1 = new System.Windows.Forms.Button();
        this.button2 = new System.Windows.Forms.Button();
        this.label1 = new System.Windows.Forms.Label();
        this.label2 = new System.Windows.Forms.Label();
        this.txtSession = new System.Windows.Forms.TextBox();
        this.lblUser = new System.Windows.Forms.Label();
        this.label3 = new System.Windows.Forms.Label();
        this.lstSessions = new System.Windows.Forms.ListBox();
        this.SuspendLayout();
        // 
        // button1
        // 
        this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
        this.button1.Location = new System.Drawing.Point(175, 253);
        this.button1.Name = "button1";
        this.button1.Size = new System.Drawing.Size(75, 23);
        this.button1.TabIndex = 6;
        this.button1.Text = "Close";
        this.button1.Click += new System.EventHandler(this.button1_Click);
        // 
        // button2
        // 
        this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.button2.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
        this.button2.Location = new System.Drawing.Point(50, 253);
        this.button2.Name = "button2";
        this.button2.Size = new System.Drawing.Size(75, 23);
        this.button2.TabIndex = 5;
        this.button2.Text = "Start";
        this.button2.Click += new System.EventHandler(this.button2_Click);
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(26, 34);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(32, 13);
        this.label1.TabIndex = 2;
        this.label1.Text = "User:";
        // 
        // label2
        // 
        this.label2.AutoSize = true;
        this.label2.Location = new System.Drawing.Point(25, 60);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(76, 13);
        this.label2.TabIndex = 3;
        this.label2.Text = "Session name:";
        // 
        // txtSession
        // 
        this.txtSession.Location = new System.Drawing.Point(104, 57);
        this.txtSession.Name = "txtSession";
        this.txtSession.Size = new System.Drawing.Size(177, 20);
        this.txtSession.TabIndex = 2;
        // 
        // lblUser
        // 
        this.lblUser.AutoSize = true;
        this.lblUser.Location = new System.Drawing.Point(80, 34);
        this.lblUser.Name = "lblUser";
        this.lblUser.Size = new System.Drawing.Size(0, 13);
        this.lblUser.TabIndex = 7;
        // 
        // label3
        // 
        this.label3.AutoSize = true;
        this.label3.Location = new System.Drawing.Point(26, 107);
        this.label3.Name = "label3";
        this.label3.Size = new System.Drawing.Size(87, 13);
        this.label3.TabIndex = 8;
        this.label3.Text = "Current sessions:";
        // 
        // lstSessions
        // 
        this.lstSessions.FormattingEnabled = true;
        this.lstSessions.Location = new System.Drawing.Point(29, 135);
        this.lstSessions.MultiColumn = true;
        this.lstSessions.Name = "lstSessions";
        this.lstSessions.Size = new System.Drawing.Size(271, 95);
        this.lstSessions.TabIndex = 9;
        this.lstSessions.SelectedIndexChanged += new System.EventHandler(this.lstSessions_SelectedIndexChanged);
        // 
        // SessionDialog
        // 
        this.AcceptButton = this.button1;
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(312, 288);
        this.Controls.Add(this.lstSessions);
        this.Controls.Add(this.label3);
        this.Controls.Add(this.lblUser);
        this.Controls.Add(this.txtSession);
        this.Controls.Add(this.label2);
        this.Controls.Add(this.label1);
        this.Controls.Add(this.button2);
        this.Controls.Add(this.button1);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "SessionDialog";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Choose or create a new session";
        this.Load += new System.EventHandler(this.SessionDialog_Load);
        this.ResumeLayout(false);
        this.PerformLayout();

	}
	#endregion

    // O button1 fecha a janela
    #region button1_Click(object sender, EventArgs e)
    private void button1_Click(object sender, EventArgs e)
    {
        this.Close();
        this.Hide();
    }
    #endregion

    // O button2 abre a conexão
    #region button2_Click(object sender, EventArgs e)
    private void button2_Click(object sender, EventArgs e)
    {
        // Usuário digitou o nome da sessão
        if(this.lstSessions.SelectedIndex < 0)
        {
        
            // Aqui vai a programação para enviar todo o arquivo de uma vez só!
            ArrayList o = new ArrayList();
            ArrayList l = new ArrayList();

            o.Add(Global.main.SaveForCollaboration());

            l.Add(this.txtSession.Text); // Primeiro elemento contém apenas o nome da sessão
            l.Add(o);                    // Este segundo elemento contém todos os dados do documento
            l.Add(new ArrayList()); // o terceiro elemento eh um arraylist contendo os Ids globais das Figs

            Global.clienteEnvia.EnviaEvento(l,"PROT_nova_sessao");

            this.Close();
            this.Hide();


        }
        else // Usuário escolheu uma sessão existente
        {

            String nome_sessao = lstSessions.Items[lstSessions.SelectedIndex].ToString();

            Global.clienteEnvia.EnviaEvento(nome_sessao,"PROT_sessao_existente");

            this.Close();
            this.Hide();


        }

    }
    #endregion


    #region lstSessions_SelectedIndexChanged()
    private void lstSessions_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.txtSession.Text  = lstSessions.Items[lstSessions.SelectedIndex].ToString();
    }
    #endregion

    private void SessionDialog_Load(object sender, EventArgs e)
    {

    }

    //
	// Implementation

}