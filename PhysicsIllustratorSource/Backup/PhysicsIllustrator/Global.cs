//-------------------------------------------------------------------------
//
//  Copyright (C) 2004 Microsoft Corporation
//  All rights reserved.
//
//  File: Global.cs
//  
//  Description: Global/static members and main entry point.
//--------------------------------------------------------------------------
using System;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;
using Physics.Collab;

public class Global
{
	//
	// Entry point
    
    public static ClienteConecta clienteEnvia;
    public static Thread tClienteEnvia; // Precisa 
    public static MainForm main;

    #region Main()
    [STAThread]
	public static void Main(string[] args)
	{
		// Main method-body as simple as possible, to postpone the majority of 
		// JIT compilation cycles until after the splash screen is displayed.
		Form splash = ShowSplashScreen();
		MainImpl(args,splash);
    }
    #endregion

    #region ShowSplashScreen()
    private static Form ShowSplashScreen()
	{
		SplashScreen splash = new SplashScreen();
		splash.Show();

		// Spin, pumping messages for a few secs.
		
        // Retirando a SlashScreen
        /* for (int i=0; i < 20; ++i)
		{
			Application.DoEvents();
			System.Threading.Thread.Sleep(100); // take it slow
		} */

		return splash;
    }
    #endregion

    #region MainImpl()
    private static void MainImpl(string[] args, Form splash)
	{
		try // To handle exceptions in mainform jit compilation
		{
			// Load latest ink version, in case the version we built against is missing.
			latestInk = Assembly.LoadWithPartialName("Microsoft.Ink, PublicKeyToken=31bf3856ad364e35");

			// Close the splash screen.
			if (!splash.IsDisposed) splash.Close();

			// No Ink?  Tell the user.
			if (latestInk == null)
			{
				string text = "Tablet PC runtime library (Microsoft.Ink.dll) not found!";
				MessageBox.Show(text,Application.ProductName,MessageBoxButtons.OK,MessageBoxIcon.Stop);
				Application.Exit();
				return;
			}

			// Check to ensure that we got a version we're willing to support.
			Version v = latestInk.GetName().Version;
			Version vmin = new Version(1,0,2201,2); // RTM release lacks IDisposable support
			Version vmax = new Version(1,7,2600,2180); // stop forward-support at Lonestar

			if (v < vmin || v > vmax)
			{
				string text = String.Format(
					"This application has not been tested with your version ({0}) \n of the Tablet PC runtime library (Microsoft.Ink.dll). \n"+
					"Do you wish to continue?",
					v);
				DialogResult dr = MessageBox.Show(text,Application.ProductName,MessageBoxButtons.YesNo,MessageBoxIcon.Warning);
				if (dr != DialogResult.Yes)
				{
					Application.Exit();
					return;
				}
			}

			// Hook into fusion.
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(HandleAssemblyResolve);

			// Install a catch-all exception filter for UI threads, allowing the app to 
			// continue in some cases.
			Application.ThreadException += 
				new ThreadExceptionEventHandler(HandleThreadException);

            // Cria a variável de armazenará as informações de conexão com o servidor
            clienteEnvia  = new ClienteConecta();


			// Factored out, to delay JIT compilation of Ink-dependent types until 
			// assembly-resolve handler is firmly in place.
			MainImplImpl(args);
		}
		catch (Exception ex)
		{
			HandleThreadException(null, new ThreadExceptionEventArgs(ex));
		}
    }
    #endregion

    #region MainImplImpl()
    private static void MainImplImpl(string[] args)
	{
		// Create main form, and open document if specified on commandline.
		main = new MainForm();
        
		if (args.Length == 1)
			main.OpenDocument(args[0]);

        // System.Threading.Thread.CurrentThread.SetApartmentState(ApartmentState.MTA);
		// Start the message pump.
		Application.Run(main);
    }
    #endregion

    //
    // Interface

    #region LoadImage()
    public static Image LoadImage(string name)
	{
		Assembly thisassem = Assembly.GetExecutingAssembly();
		Stream stream = thisassem.GetManifestResourceStream(name);
		return Image.FromStream(stream);

        
    }
    #endregion

    #region HandleThreadException()
    public static void HandleThreadException(object sender, ThreadExceptionEventArgs e)
	{
		string message = e.Exception.Message;
		string tempfile = Path.Combine(Path.GetTempPath(),"PhysicsIllustrator.txt");
		string text = String.Format("The following error occurred:\n\n{0}\n\nDetails will be saved to '{1}'.",message,tempfile);

		MessageBox.Show(text,Application.ProductName,MessageBoxButtons.OK,MessageBoxIcon.Stop);

		using (TextWriter tw = File.CreateText(tempfile))
		{
			tw.WriteLine(e.Exception.ToString());
		}

		System.Diagnostics.Process.Start(tempfile);
    }
    #endregion

    //
	// Implementation

	static Assembly latestInk;

    #region HandleAssemblyResolve()
    private static Assembly HandleAssemblyResolve(object sender, ResolveEventArgs args)
	{
		if (args.Name.StartsWith("Microsoft.Ink"))
			return latestInk;
		else
			return null;
    }
    #endregion
}
