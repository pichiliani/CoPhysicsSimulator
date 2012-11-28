//-------------------------------------------------------------------------
//
//  Copyright (C) 2004 Microsoft Corporation
//  All rights reserved.
//
//  File: CollabDialog.cs
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
internal class CollabDialog : System.Windows.Forms.Form
{
    private Button button2;
    private Label label1;
    private Label label2;
    private Label label3;
    private Label label4;
    private TextBox txtServer;
    private TextBox txtPort;
    private TextBox txtUser;
    private TextBox txtPassword;
    private System.Windows.Forms.Button button1;

    // 


	// 
	// Initialization 

	public CollabDialog()
	{
		// Required for Windows Form Designer support.
		InitializeComponent();

		// Load our icon.
		this.Icon = new Icon(typeof(CollabDialog),"PhysicsIllustrator.ico");

		// Set the version label.
		
		// Load image for panel, tracking the size-change.
		
	}

	#region Windows Form Designer generated code
	private void InitializeComponent()
	{
        this.button1 = new System.Windows.Forms.Button();
        this.button2 = new System.Windows.Forms.Button();
        this.label1 = new System.Windows.Forms.Label();
        this.label2 = new System.Windows.Forms.Label();
        this.label3 = new System.Windows.Forms.Label();
        this.label4 = new System.Windows.Forms.Label();
        this.txtServer = new System.Windows.Forms.TextBox();
        this.txtPort = new System.Windows.Forms.TextBox();
        this.txtUser = new System.Windows.Forms.TextBox();
        this.txtPassword = new System.Windows.Forms.TextBox();
        this.SuspendLayout();
        // 
        // button1
        // 
        this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
        this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
        this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
        this.button1.Location = new System.Drawing.Point(172, 150);
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
        this.button2.Location = new System.Drawing.Point(47, 150);
        this.button2.Name = "button2";
        this.button2.Size = new System.Drawing.Size(75, 23);
        this.button2.TabIndex = 5;
        this.button2.Text = "Connect";
        this.button2.Click += new System.EventHandler(this.button2_Click);
        // 
        // label1
        // 
        this.label1.AutoSize = true;
        this.label1.Location = new System.Drawing.Point(26, 34);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(41, 13);
        this.label1.TabIndex = 2;
        this.label1.Text = "Server:";
        // 
        // label2
        // 
        this.label2.AutoSize = true;
        this.label2.Location = new System.Drawing.Point(26, 63);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(29, 13);
        this.label2.TabIndex = 3;
        this.label2.Text = "Port:";
        // 
        // label3
        // 
        this.label3.AutoSize = true;
        this.label3.Location = new System.Drawing.Point(26, 93);
        this.label3.Name = "label3";
        this.label3.Size = new System.Drawing.Size(32, 13);
        this.label3.TabIndex = 4;
        this.label3.Text = "User:";
        // 
        // label4
        // 
        this.label4.AutoSize = true;
        this.label4.Location = new System.Drawing.Point(26, 117);
        this.label4.Name = "label4";
        this.label4.Size = new System.Drawing.Size(56, 13);
        this.label4.TabIndex = 5;
        this.label4.Text = "Password:";
        // 
        // txtServer
        // 
        this.txtServer.Location = new System.Drawing.Point(83, 25);
        this.txtServer.Name = "txtServer";
        this.txtServer.Size = new System.Drawing.Size(198, 20);
        this.txtServer.TabIndex = 1;
        this.txtServer.Text = "127.0.0.1";
        // 
        // txtPort
        // 
        this.txtPort.Location = new System.Drawing.Point(83, 54);
        this.txtPort.Name = "txtPort";
        this.txtPort.Size = new System.Drawing.Size(198, 20);
        this.txtPort.TabIndex = 2;
        this.txtPort.Text = "100";
        // 
        // txtUser
        // 
        this.txtUser.Location = new System.Drawing.Point(83, 84);
        this.txtUser.Name = "txtUser";
        this.txtUser.Size = new System.Drawing.Size(198, 20);
        this.txtUser.TabIndex = 3;
        this.txtUser.Text = "A";
        // 
        // txtPassword
        // 
        this.txtPassword.Location = new System.Drawing.Point(83, 116);
        this.txtPassword.Name = "txtPassword";
        this.txtPassword.Size = new System.Drawing.Size(198, 20);
        this.txtPassword.TabIndex = 4;
        this.txtPassword.Text = "A";
        // 
        // CollabDialog
        // 
        this.AcceptButton = this.button1;
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(309, 185);
        this.Controls.Add(this.txtPassword);
        this.Controls.Add(this.txtUser);
        this.Controls.Add(this.txtPort);
        this.Controls.Add(this.txtServer);
        this.Controls.Add(this.label4);
        this.Controls.Add(this.label3);
        this.Controls.Add(this.label2);
        this.Controls.Add(this.label1);
        this.Controls.Add(this.button2);
        this.Controls.Add(this.button1);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "CollabDialog";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Connect to the CollaborationServer";
        this.ResumeLayout(false);
        this.PerformLayout();

	}
	#endregion

    // O button1 fecha a janela
    #region button1_Click(object sender, EventArgs e)
    private void button1_Click(object sender, EventArgs e)
    {

    }
    #endregion

    // O button2 abre a conexão
    #region button2_Click(object sender, EventArgs e)
    private void button2_Click(object sender, EventArgs e)
    {

        // SessionDialog dlg = new SessionDialog();
        // dlg.ShowDialog(this);

        try
        {

            if ( Global.clienteEnvia.SetaConecta(txtServer.Text,int.Parse(txtPort.Text)) )   
            {

                if ( Global.clienteEnvia.SetaUser(txtUser.Text,txtPassword.Text) )
                {

                    // Aqui vou iniciat a Thread do cliente envia!

                        ThreadStart threadDelegate = new ThreadStart(Global.clienteEnvia.run);
                        Global.tClienteEnvia = new Thread(threadDelegate);
                        Global.tClienteEnvia.Start();

                        this.Hide();
                        this.Close();

                        // tClienteEnvia.

                    // Aqui vou iniciar a nova janela com as informações para conexão

                    SessionDialog dlg = new SessionDialog();
                    dlg.ShowDialog(this);

                }
            }
        } catch(SocketException e1) 
        {
	       
                MessageBox.Show(
				"Exception caught!!!" + e1.Message , 
				Application.ProductName, 
				MessageBoxButtons.OK, 
				MessageBoxIcon.Warning);
        }


    }
    #endregion
    //
	// Implementation

}