//-------------------------------------------------------------------------
//
//  Copyright (C) 2004 Microsoft Corporation
//  All rights reserved.
//
//  File: MainForm.cs
//  
//  Description: Main full-screen window and drawing surface.
//--------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.Ink;
using Physics.Collab;



using dbg=System.Diagnostics.Debug;

[System.ComponentModel.DesignerCategory("Code")]
public class MainForm : System.Windows.Forms.Form
{
	//
	// Instance data

    // Este MagicDocument é que contém todos os elementos!
	public MagicDocument doc;

	private InkOverlay inkoverlay;
	private HoverRegion hover;
	private ToolTip menuttip;
	private PhysicsIllustrator.SmartTag.SmartTag bodytag;

	private string savedfilename;
	private InkOverlayEditingMode declaredmode;
	private static readonly Guid BarrelSwitchId = 
		new Guid("f0720328-663b-418f-85a6-9531ae3ecdfa"); 

	private AnimationEngine engine;

    private RigidBodyBase tappedbody;

        private int[] neweststrokeids; // Note: Ink.AddStrokesAtRectangle doesn't return 
    // the new stroke ids, so we must get them ourselves, from the InkAdded event.

    // Necessário para regular a freqüência de envio do telepointer!
    private int ContaMouseMove = 0;

    // Telepointers
    public FigPointer fpA = new FigPointer(Color.Black, "A");
    public FigPointer fpB = new FigPointer(Color.Blue , "B");
    public FigPointer fpC = new FigPointer(Color.Yellow, "C");
    public FigPointer fpD = new FigPointer(Color.Orange, "D");

    public delegate void DelegateToMethod(InkCollectorStrokeEventArgs e);

    public delegate void DelegateEraseMethod(ArrayList l);

    public delegate void DelegateSimulationMetod(Object o);

    public delegate void DelegateOpenMetod(TextWriter t);

    public delegate void DelegateGenericMetod();
	// 
    // Initialization 

    #region MainForm-Construtor
    public MainForm()
	{
		// Required for Windows Form Designer support.
		InitializeComponent();

		// Load our icon.
		this.Icon = new Icon(typeof(MainForm),"PhysicsIllustrator.ico");

		// Initialize the per-item smart tag.
		bodytag = new PhysicsIllustrator.SmartTag.SmartTag();
		bodytag.Image = Global.LoadImage("Resources.PenMenu.ico");
		bodytag.Visible = false;

		bodytag.ContextMenu = new ContextMenu();
		bodytag.ContextMenu.MenuItems.Add("Clone", new EventHandler(hover_EditCloneClicked));
		bodytag.ContextMenu.MenuItems.Add("Delete", new EventHandler(hover_EditDeleteClicked));
		bodytag.ContextMenu.MenuItems.Add("-");
		bodytag.ContextMenu.MenuItems.Add("Straighten", new EventHandler(hover_EditStraightenClicked));
		bodytag.ContextMenu.MenuItems.Add("Properties...", new EventHandler(hover_EditPropertiesClicked));

		this.Controls.Add(bodytag);

		// Go fullscreen.  Note: this works even with the taskbar set 
		// to "keep on top of other windows".  
		this.WindowState = System.Windows.Forms.FormWindowState.Normal;
		// this.Bounds = Screen.PrimaryScreen.Bounds;

		// Declare repaint optimizations.
		base.SetStyle(
			ControlStyles.UserPaint|
			ControlStyles.AllPaintingInWmPaint|
			ControlStyles.DoubleBuffer,
			true);

		// Init inkoverlay.
		inkoverlay = new InkOverlay(this.Handle,true);
		inkoverlay.CollectionMode = CollectionMode.InkOnly; // no gestures

		inkoverlay.AutoRedraw = false; // Dynamic rendering only; we do all the painting.

		DrawingAttributes da = new DrawingAttributes();
		da.AntiAliased = false;
		inkoverlay.DefaultDrawingAttributes = da;

		inkoverlay.Stroke += new InkCollectorStrokeEventHandler(inkoverlay_Stroke);

		inkoverlay.CursorInRange += new InkCollectorCursorInRangeEventHandler(inkoverlay_CursorInRange);
		inkoverlay.StrokesDeleting += new InkOverlayStrokesDeletingEventHandler(inkoverlay_StrokesDeleting);

		inkoverlay.SelectionChanging += new InkOverlaySelectionChangingEventHandler(inkoverlay_SelectionChanging);
		inkoverlay.SelectionChanged += new InkOverlaySelectionChangedEventHandler(inkoverlay_SelectionChanged);
		inkoverlay.SelectionMoved += new InkOverlaySelectionMovedEventHandler(inkoverlay_SelectionMoved);
		inkoverlay.SelectionResized += new InkOverlaySelectionResizedEventHandler(inkoverlay_SelectionResized);

        // inkoverlay.
		declaredmode = inkoverlay.EditingMode;

		// Spin up SDI model (ink+doc).
		doc = new MagicDocument();
		inkoverlay.Ink = doc.Ink;

		inkoverlay.Enabled = !DesignMode;

		inkoverlay.Ink.InkAdded += new StrokesEventHandler(inkoverlay_Ink_InkAdded);

        

    }
    #endregion

    #region Windows Form Designer generated code
    private void InitializeComponent()
	{
		// 
		// MainForm
		// 
		this.BackColor = System.Drawing.Color.Linen;
		this.ClientSize = new System.Drawing.Size(1024, 768);
		
        // Colocando as bordas naturais da janela
        // this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
		
        this.Name = "MainForm";
		this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
		this.Text = "CoPhysics Illustrator for Tablet PC";
	}
	#endregion

	//
	// Overrides
    #region OnLoad()

	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);

		// Declare hover-region for controls in upper-right corner (upper-left if right 
		// handed).  We do this here, not in the constructor, so our layout changes 
		// (resize/maximized) will have had a chance to kick in.
		hover = new HoverRegion(this,Config.PinHoverPanel);
		menuttip = new ToolTip();
		hover.InitializeControls(menuttip);
		hover.EnablePerItemEditCommands(false);
		this.Controls.AddRange(hover.Controls);

		// Subscribe to logical application-events, exposed from hover-bar.
		hover.FileNewClicked += new EventHandler(hover_FileNewClicked);
		hover.FileOpenClicked += new EventHandler(hover_FileOpenClicked);
		hover.FileSaveClicked += new EventHandler(hover_FileSaveClicked);
		hover.FileSaveAsClicked += new EventHandler(hover_FileSaveAsClicked);


        // Colaboração
        hover.FileCollabClicked  += new EventHandler(hover_CollabClicked);

		hover.FileHelpClicked += new EventHandler(hover_FileHelpClicked);
		hover.FileAboutClicked += new EventHandler(hover_FileAboutClicked);
		hover.FileExitClicked += new EventHandler(hover_FileExitClicked);

		hover.EditCloneClicked += new EventHandler(hover_EditCloneClicked);
		hover.EditDeleteClicked += new EventHandler(hover_EditDeleteClicked);
		hover.EditStraightenClicked += new EventHandler(hover_EditStraightenClicked);
		hover.EditPropertiesClicked += new EventHandler(hover_EditPropertiesClicked);

		hover.PenDrawClicked += new EventHandler(hover_PenDrawClicked);
		hover.PenEraseClicked += new EventHandler(hover_PenEraseClicked);
		hover.PenLassoClicked += new EventHandler(hover_PenLassoClicked);

		hover.AnimateClicked += new EventHandler(hover_AnimateClicked);
        hover.PauseClicked   += new EventHandler(hover_PauseClicked);
		hover.MinimizeClicked += new EventHandler(hover_MinimizeClicked);


        

		// Respond to portrait/landscape orientation changes.
		Microsoft.Win32.SystemEvents.DisplaySettingsChanged += 
			new EventHandler(SystemEvents_DisplaySettingsChanged);

		// Show controls initially, for a few seconds, as a hint to the user.
		hover.DisplayInitial();

        
        hover.getPauseTag().Visible = false;
	}

    #endregion

    // O evento onMouseMove() será utilizado para caputar a posição do mouse!    
    #region onMouseMove()
    protected override void OnMouseMove(MouseEventArgs e)
	{
        // dbg.WriteLine("----- OnMouseMove -----" + e.X.ToString() + " " + e.Y.ToString());   
        base.OnMouseMove(e);


        // Enviando a posição do cursor do mouse!

        ContaMouseMove++;
    	
    	if(ContaMouseMove.Equals(4))
    	{
    		ArrayList list = new ArrayList();

        	list.Add(e.Location); // objeto 'genérico' (pode ser um mouse event ou outro)
        	
        	// Mandando a cor atual do telepointer
        	list.Add(Global.clienteEnvia.getCorTelepointer());    	                           
        	
        	// 	Mandando id do telepointer
        	list.Add(Global.clienteEnvia.getIdTelepointer());
        	
        	// Mandando o nome do proprietario do telepointer
        	list.Add(Global.clienteEnvia.getLogin());

            // Este evento eh para reproduzir o telepointer
        	// Teste: Desabilitando o Telepointer por enquanto
            //Global.clienteEnvia.EnviaEvento((Object) list,"mouseMovedPointer");
        	
        	ContaMouseMove = 0;
    	}
    }

    #endregion

    #region OnPaint()
    protected override void OnPaint(PaintEventArgs e)
	{
   
            base.OnPaint(e);

            Graphics g = e.Graphics;

		    // Set "world" transform == ink-to-pixel space transform.
		    Rectangle rect = new Rectangle(0,0,10000,10000);
		    Point[] plgpts = new Point[] { 
										     new Point(0,0), 
										     new Point(10000,0), 
										     new Point(0,10000)
									     };
		    inkoverlay.Renderer.InkSpaceToPixel(g, ref plgpts);
		    using (Matrix world = new Matrix(rect,plgpts))
		    {
			    g.Transform = world;
		    }

		    // Use actual doc when editing; throwaway clone when animating.
		    MagicDocument doc = this.doc;

		    if (!inkoverlay.Enabled && engine != null && engine.Document != null)
			    doc = engine.Document;

		    // Draw gravity vector in background.
		    if (doc.Gravity != null)
			    doc.Gravity.Render(g);

            
		    // Draw bodies. 
		    foreach (RigidBodyBase body in doc.Bodies)
		    {
			    body.Render(g);
		    }

		    // Draw mechs.
		    foreach (MechanismBase mech in doc.Mechanisms)
		    {
			    mech.Render(g);
		    }

    }
    #endregion
    
    #region OnMove()
    protected override void OnMove(EventArgs e)
	{
		dbg.WriteLine("----- OnMove -----");
		base.OnMove(e);

		// Layout hover-region target area and child controls.
		if (hover != null) hover.UpdateTargetRectLayout();
    }
    #endregion

    #region OnResize()
    protected override void OnResize(EventArgs e)
	{
		dbg.WriteLine("----- OnResize -----");
		base.OnResize(e);

		// Layout hover-region target area and child controls.
		if (hover != null) hover.UpdateTargetRectLayout();
    }
    #endregion

    #region OnClosing()
    protected override void OnClosing(CancelEventArgs e)
	{
		dbg.WriteLine("----- OnClosing -----");
		base.OnClosing(e);


        try
        {
            if(Global.tClienteEnvia != null)
            {
                if(Global.tClienteEnvia.IsAlive)
                    Global.tClienteEnvia.Abort();
            }

            if(engine != null)
            {
                engine.setPause(false);
                Thread.Sleep(10);
                engine.Stop();
            }

        }
        catch{}

		if (!IsUserSure())
			e.Cancel = true;
        
        Application.Exit();
        Application.ExitThread(); 


    }
    #endregion

    //
    // Implementation
    #region InvalidateInkSpaceRectangle()
    private void InvalidateInkSpaceRectangle(Rectangle rect)
	{
		using (Graphics g = this.CreateGraphics())
		{
			Point ul = new Point(rect.Left,rect.Top);
			Point lr = new Point(rect.Right,rect.Bottom);
			Point[] pts = { ul, lr };
			inkoverlay.Renderer.InkSpaceToPixel(g, ref pts);

			ul = pts[0]; lr = pts[1];
			rect = new Rectangle(ul.X, ul.Y, lr.X-ul.X, lr.Y-ul.Y);

			rect.Inflate(7,7);
			Invalidate(rect);
		}
    }

    #endregion

    #region IsUserSure() - Pergunta se quer salvar
    private bool IsUserSure()
	{
		// Don't bother the user if the form's not dirty.
		if (!doc.Ink.Dirty || doc.Ink.Strokes.Count < 1) return true;

		DialogResult dr = MessageBox.Show(
			"Do you want to save the changes to this document?", 
			Application.ProductName, 
			MessageBoxButtons.YesNoCancel, 
			MessageBoxIcon.Warning, 
			MessageBoxDefaultButton.Button1);

		if (dr == DialogResult.Yes)
		{
			if (this.savedfilename == null)
			{
				dr = ShowSaveAsDialog();
			}
			else
			{
				JustSaveIt();
			}
		}

		return (dr != DialogResult.Cancel);
    }
    #endregion
    
    #region MakeBodyFromClosedStroke() - Faz corpo fechado
    private RigidBodyBase MakeBodyFromClosedStroke(Stroke stroke, Point[] vertices)
	{
		// Form a new body -- ellipse or polygon?
		RigidBodyBase newbody = null;

		Point[] points = stroke.GetPoints();
		Ellipse elli = Ellipse.FromRegression(points);
		if (!elli.IsEmpty && elli.IsFit(points))
		{
			dbg.WriteLine("new EllipticalBody");

			newbody = new EllipticalBody();
			EllipticalBody body = newbody as EllipticalBody;

			body.CenterPoint = elli.Center;
			body.MajorAxis = elli.MajorAxis;
			body.MinorAxis = elli.MinorAxis;
			body.Orientation = elli.Orientation;

			// Close to circle? Snap to it.
			if ((float)elli.MajorAxis/(float)elli.MinorAxis < 1.25)
			{
				int r = (elli.MajorAxis+elli.MinorAxis)/2;
				body.MajorAxis = body.MinorAxis = r;
				body.Orientation = 0;
			}
		}
		else
		{
			dbg.WriteLine("new PolygonalBody");

			newbody = new PolygonalBody();
			PolygonalBody body = newbody as PolygonalBody;
			body.Vertices = vertices;
		}

		dbg.WriteLine(String.Format("Mass={0}, I={1}",newbody.Mass,newbody.I));

		newbody.strokeid = stroke.Id;
		doc.Bodies.Add(newbody);

		return newbody;
    }

    #endregion

    #region MakeRodRopeOrSpring() - Faz corda ou alavanca
    private MechanismBase MakeRodRopeOrSpring(Stroke stroke, RigidBodyBase headbody, RigidBodyBase tailbody)
	{
		// Rod, Rope, or Spring: we decide based on straightness, curvyness, and loopiness.
		int np = stroke.PacketCount;
		Point head = stroke.GetPoint(0), tail = stroke.GetPoint(np-1);
		double distance = Geometry.DistanceBetween(head,tail);

		StrokeGeometry sg = new StrokeGeometry(stroke);
		double length = sg.IntegrateLength();

		// Consider spring: analyze total net curvature of the stroke, and call it a 
		// spring if it makes at least 540 degrees (1.5 loops) in the same direction.
		double tt = StrokeAnalyzer.AnalyzeTotalCurvature(stroke,100.0);
		if (Math.Abs(Geometry.Rad2Deg(tt)) > 540.0)
		{
			dbg.WriteLine("new SpringMech");

			SpringMech newmech = new SpringMech();
			newmech.EndpointA = BodyRef.For(headbody,head);
			newmech.EndpointB = BodyRef.For(tailbody,tail);
			newmech.extension = distance;
			newmech.stiffness = MathEx.Square(tt)/100; //Heuristic: th²/100 feels about right.

			newmech.strokeid = stroke.Id;
			doc.Mechanisms.Add(newmech);
			return newmech;
		}

		// Straight and narrow?
		double rodropethreshold = 1.1; //heuristic
		if (length/distance < rodropethreshold)
		{
			dbg.WriteLine("new RodMech");

			RodMech newmech = new RodMech();
			newmech.EndpointA = BodyRef.For(headbody,head);
			newmech.EndpointB = BodyRef.For(tailbody,tail);
			newmech.length = distance;

			newmech.strokeid = stroke.Id;
			doc.Mechanisms.Add(newmech);
			return newmech;
		}
		else
		{
			dbg.WriteLine("new RopeMech");

			RopeMech newmech = new RopeMech();
			newmech.EndpointA = BodyRef.For(headbody,head);
			newmech.EndpointB = BodyRef.For(tailbody,tail);
			newmech.length = length;

			newmech.strokeid = stroke.Id;
			doc.Mechanisms.Add(newmech);
			return newmech;
		}
    }
    #endregion

    #region inkoverlay_StrokeImpl() --> Faz o stroke remoto
    public void inkoverlay_StrokeImpl(InkCollectorStrokeEventArgs e)
	{
        dbg.WriteLine("----- inkoverlay_StrokeImpl -----");


        // Previso verificar o modo. Se estiver em um modo que não seja a edição, salva
        // o modo atual e coloca na edição. No final volto ao modo que estava

        // Primeiro passo é salvar o modo de edição autal e colocar no modo de deleção
        InkOverlayEditingMode em =  inkoverlay.EditingMode;
        SetEditingMode(InkOverlayEditingMode.Ink,true);


		try // To prevent exceptions from propagating back to ink runtime.
		{
			/* 
            // Hook for tap-to-select feature, in lasso-mode.
			if (inkoverlay.EditingMode == InkOverlayEditingMode.Select)
			{
				//////////////// USO DO STROKE //////////////////////
                TryTapToSelect(e.Stroke);
				return;
			} */

			// Analyze stroke geometry.
			bool closed;
			Point[] vertices;
			double tolerance = 500.0; //Heuristic: 500 seems about right.
			//////////////// USO DO STROKE //////////////////////
            StrokeAnalyzer.AnalyzeClosedness(e.Stroke, tolerance, out closed, out vertices);

			// Se for um desenho fechado...
            // Interpret stroke in document-context: first, consider closed strokes.
			if (closed)
			{
				// Check for a small elliptical-gesture over two or more bodies. 
				// If so, it's a pin joint!
				//////////////// USO DO STROKE //////////////////////
                Rectangle bbox = e.Stroke.GetBoundingBox();
				Point midp = Geometry.Midpoint(bbox);

				RigidBodyBase[] hits = doc.HitTestBodies(midp);

				if (hits.Length >= 2 && bbox.Width < 1000 && bbox.Height < 1000)
				{
					RigidBodyBase top = hits[0];
					RigidBodyBase bottom = hits[1];

					// Snap to CG if close to either top's or bottom's.
					if (Geometry.DistanceBetween(midp,top.CG) < 500.0)
						midp = top.CG;
					else if (Geometry.DistanceBetween(midp,bottom.CG) < 500.0)
						midp = bottom.CG;

					dbg.WriteLine("new JointMech");

					JointMech newmech = new JointMech();
					newmech.EndpointA = BodyRef.For(bottom,midp);
					newmech.EndpointB = BodyRef.For(top,midp);

                    //////////////// USO DO STROKE //////////////////////
					newmech.strokeid = e.Stroke.Id;
					doc.Mechanisms.Add(newmech);

					// Repaint area around the newmech (unions with stroke bbox, below).
					Rectangle dirtybbox = newmech.BoundingBox;
					InvalidateInkSpaceRectangle(dirtybbox);
					return;
				}
				else
				{
					// Larger stroke, and/or no centerpoint hits -- form a new solid body.
					//////////////// USO DO STROKE //////////////////////
                    RigidBodyBase newbody = MakeBodyFromClosedStroke(e.Stroke,vertices);

					// Repaint area around the newbody (unions with stroke bbox, below).
					Rectangle dirtybbox = newbody.BoundingBox;
					InvalidateInkSpaceRectangle(dirtybbox);

					// Select it, to show the smart tag.
					//////////////// USO DO STROKE //////////////////////
                    
                    // TESTE: Retirada da selecion
                    // TESTE: inkoverlay.Selection = this.MakeSingleStrokeCollection(e.Stroke);
                    // inkoverlay.Selection = this.MakeSingleStrokeCollectionImpl();
					return;
				}
			}


            // Caso senha um desenho aberto...
			// An unclosed stroke -- 
			// Check if head and/or tail is hit on existing bodies.
			//////////////// USO DO STROKE //////////////////////
            int np = e.Stroke.PacketCount;
			Point head = e.Stroke.GetPoint(0), tail = e.Stroke.GetPoint(np-1);

			RigidBodyBase[] headhits = doc.HitTestBodies(head);
			RigidBodyBase[] tailhits = doc.HitTestBodies(tail);

			if (headhits.Length == 0 && tailhits.Length == 0)
			{
				// Neither head or tail hit, so let's try harder to make a body 
				// out of this stroke.
				Point[] dummy;
				tolerance = 2000.0; //Heuristic: vastly relax closure tolerance.
				//////////////// USO DO STROKE //////////////////////
                StrokeAnalyzer.AnalyzeClosedness(e.Stroke, tolerance, out closed, out dummy);

				if (closed)
				{
					//////////////// USO DO STROKE //////////////////////
                    RigidBodyBase newbody = MakeBodyFromClosedStroke(e.Stroke,vertices);

					// Repaint area around the newbody (unions with stroke bbox, below).
					Rectangle dirtybbox = newbody.BoundingBox;
					InvalidateInkSpaceRectangle(dirtybbox);

					// Select it, to show the smart tag.
					//////////////// USO DO STROKE //////////////////////
                    // TESTE: Retirada da selecion
                    // inkoverlay.Selection = this.MakeSingleStrokeCollection(e.Stroke);
					return;
				}
				else if (Geometry.DistanceBetween(head,tail) > 500.0)
				{
					// Interpret this stroke as a gravity-vector.
					GravitationalForceMech newgrav = new GravitationalForceMech();
					newgrav.Body = null; // Applies to all bodies!
					newgrav.origin = head;
					newgrav.vector = Vector.FromPoints(head,tail);

					// Repaint area around the gravity vector (unions with stroke bbox, below).
					Rectangle dirtybbox = newgrav.BoundingBox;
					InvalidateInkSpaceRectangle(dirtybbox);

					// Throw out the current gravity-vector stroke, if it exists.
					if (doc.Gravity != null)
					{
						dirtybbox = doc.Gravity.BoundingBox;
						InvalidateInkSpaceRectangle(dirtybbox);

                        // Testes de remoçao do Gravity Strike

                        if(doc.Ink.Strokes.Count > 0)
                        {
                            foreach (Stroke S in doc.Ink.Strokes)
                            {
                                if (S.Id == doc.Gravity.strokeid)
                                {
                                    doc.Ink.DeleteStroke(S);
                                }
                            }
                        }
					}

					//////////////// USO DO STROKE //////////////////////
                    newgrav.strokeid = e.Stroke.Id;
					doc.Gravity = newgrav;
					return;
				}
				else
				{
					// This stroke is probably an accidental tap -- discard it.
					e.Cancel = true;
					return;
				}
			}

			if (headhits.Length > 0 && tailhits.Length == 0)
			{
				// If only the head is hit, it must be an 'attractive force'.
				RigidBodyBase body = headhits[0];

				dbg.WriteLine("new ExternalForceMech");

				ExternalForceMech newmech = new ExternalForceMech();
				newmech.Body = BodyRef.For(body,head);
				newmech.vector = Vector.FromPoints(head,tail);

				//////////////// USO DO STROKE //////////////////////
                newmech.strokeid = e.Stroke.Id;
				doc.Mechanisms.Add(newmech);

				// Repaint area around the newmech (unions with stroke bbox, below).
				Rectangle dirtybbox = newmech.BoundingBox;
				InvalidateInkSpaceRectangle(dirtybbox);
				return;
			}

			if (headhits.Length == 0 && tailhits.Length > 0)
			{
				// If only the tail is hit, it must be a 'propulsive force'.
				RigidBodyBase body = tailhits[0];

				dbg.WriteLine("new PropulsiveForceMech");

				PropulsiveForceMech newmech = new PropulsiveForceMech();
				newmech.Body = BodyRef.For(body,tail);
				newmech.vector = Vector.FromPoints(head,tail);

                //////////////// USO DO STROKE //////////////////////
				newmech.strokeid = e.Stroke.Id;
				doc.Mechanisms.Add(newmech);

				// Repaint area around the newmech (unions with stroke bbox, below).
				Rectangle dirtybbox = newmech.BoundingBox;
				InvalidateInkSpaceRectangle(dirtybbox);
				return;
			}

			if (true) // scope
			{
				// Create a binding mechanism between two bodies.
				RigidBodyBase headbody = headhits[0], tailbody = tailhits[0];

				// If both the head and the tail hit same object, 
				// attach the head to the one behind.
				if (Object.ReferenceEquals(headbody,tailbody))
				{
					if (headhits.Length > 1)
						headbody = headhits[1];
					else if (tailhits.Length > 1)
						tailbody = tailhits[1];
					else
					{
						// Aqui é feita a seleção. Remotamente não faço nada
                        
                        // Don't self-connect. We will perhaps interpret the stroke as an 
						// anchor-gesture or a selection-gesture, 
						// but if we cannot, we will cancel.
						//////////////// USO DO STROKE //////////////////////
                        int nc = e.Stroke.PolylineCusps.Length;
						if (np <= 25)
						{
							// TESTE: Retirada da selecion
                            // inkoverlay.Selection = MakeSingleStrokeCollection(headbody.strokeid);
							SetEditingMode(InkOverlayEditingMode.Select,false);
						}
						else if (np <= 150 && (nc >= 3 && nc <= 5))
						{
							headbody.anchored = !headbody.anchored; //toggle
						}

                       
						e.Cancel = true;

						// Repaint area around the headbody (unions with stroke bbox, below).
						Rectangle dirtybbox = headbody.BoundingBox;
						InvalidateInkSpaceRectangle(dirtybbox);
						return;
					}
				}

				// Create a rope, rod, or spring out of the stroke.
				//////////////// USO DO STROKE //////////////////////
                MechanismBase newmech = MakeRodRopeOrSpring(e.Stroke,headbody,tailbody);

				if (newmech != null)
				{
					// Repaint area around the newmech (unions with stroke bbox, below).
					Rectangle dirtybbox = newmech.BoundingBox;
					InvalidateInkSpaceRectangle(dirtybbox);
					return;
				}
				else
				{
					// Throw out the stroke, and return.
					e.Cancel = true;
					return;
				}
			}
		}
		catch (Exception ex)
		{
			// Cancel the stroke, and log the error.
			e.Cancel = true;
			Global.HandleThreadException(this, new System.Threading.ThreadExceptionEventArgs(ex));
		}
		finally
		{
			// Repaint the area around the stroke (unions with newbody/mech region, above).
			//////////////// USO DO STROKE //////////////////////
            Rectangle dirtybbox = e.Stroke.GetBoundingBox();
			InvalidateInkSpaceRectangle(dirtybbox);

            SetEditingMode(em,true);
		}

    }

    #endregion

    #region inkoverlay_Stroke() -> Verifica o que criar! Evento chave a ser replicado!

    // É neste método que ocorre a criação do elemento (corpo, corda, força gravitacional, etc...)
    private void inkoverlay_Stroke(object sender, InkCollectorStrokeEventArgs e)
	{
		dbg.WriteLine("----- inkoverlay_Stroke -----");

		// Ensure we're on the UI thread.
		dbg.Assert(!this.InvokeRequired);

        // Check to make sure we're not erasing.
		if (inkoverlay.EditingMode == InkOverlayEditingMode.Delete)
			return;

		try // To prevent exceptions from propagating back to ink runtime.
		{
			// Hook for tap-to-select feature, in lasso-mode.
			if (inkoverlay.EditingMode == InkOverlayEditingMode.Select)
			{
				//////////////// USO DO STROKE //////////////////////
                TryTapToSelect(e.Stroke);
				return;
			}


            // Aqui vou mandar o evento para o servidor!

            ArrayList list = new ArrayList();

        	// list.Add(e.Stroke); // objeto 'genérico' (pode ser um mouse event ou outro)
            
            list.Add(inkoverlay.Ink.Save(PersistenceFormat.InkSerializedFormat)); 
        	
            // Este evento eh para reproduzir o inkoverlay_Stroke
        	Global.clienteEnvia.EnviaEvento((Object) list,"inkoverlay_Stroke");



			// Analyze stroke geometry.
			bool closed;
			Point[] vertices;
			double tolerance = 500.0; //Heuristic: 500 seems about right.
			//////////////// USO DO STROKE //////////////////////
            StrokeAnalyzer.AnalyzeClosedness(e.Stroke, tolerance, out closed, out vertices);

			// Se for um desenho fechado...
            // Interpret stroke in document-context: first, consider closed strokes.
			if (closed)
			{
				// Check for a small elliptical-gesture over two or more bodies. 
				// If so, it's a pin joint!
				//////////////// USO DO STROKE //////////////////////
                Rectangle bbox = e.Stroke.GetBoundingBox();
				Point midp = Geometry.Midpoint(bbox);

				RigidBodyBase[] hits = doc.HitTestBodies(midp);

				if (hits.Length >= 2 && bbox.Width < 1000 && bbox.Height < 1000)
				{
					RigidBodyBase top = hits[0];
					RigidBodyBase bottom = hits[1];

					// Snap to CG if close to either top's or bottom's.
					if (Geometry.DistanceBetween(midp,top.CG) < 500.0)
						midp = top.CG;
					else if (Geometry.DistanceBetween(midp,bottom.CG) < 500.0)
						midp = bottom.CG;

					dbg.WriteLine("new JointMech");

					JointMech newmech = new JointMech();
					newmech.EndpointA = BodyRef.For(bottom,midp);
					newmech.EndpointB = BodyRef.For(top,midp);

                    //////////////// USO DO STROKE //////////////////////
					newmech.strokeid = e.Stroke.Id;
					doc.Mechanisms.Add(newmech);

					// Repaint area around the newmech (unions with stroke bbox, below).
					Rectangle dirtybbox = newmech.BoundingBox;
					InvalidateInkSpaceRectangle(dirtybbox);
					return;
				}
				else
				{
					// Larger stroke, and/or no centerpoint hits -- form a new solid body.
					//////////////// USO DO STROKE //////////////////////
                    RigidBodyBase newbody = MakeBodyFromClosedStroke(e.Stroke,vertices);

					// Repaint area around the newbody (unions with stroke bbox, below).
					Rectangle dirtybbox = newbody.BoundingBox;
					InvalidateInkSpaceRectangle(dirtybbox);

					// Select it, to show the smart tag.
					//////////////// USO DO STROKE //////////////////////
                    inkoverlay.Selection = this.MakeSingleStrokeCollection(e.Stroke);
					return;
				}
			}


            // Caso seja um desenho aberto...
			// An unclosed stroke -- 
			// Check if head and/or tail is hit on existing bodies.
			//////////////// USO DO STROKE //////////////////////
            int np = e.Stroke.PacketCount;
			Point head = e.Stroke.GetPoint(0), tail = e.Stroke.GetPoint(np-1);

			RigidBodyBase[] headhits = doc.HitTestBodies(head);
			RigidBodyBase[] tailhits = doc.HitTestBodies(tail);

			if (headhits.Length == 0 && tailhits.Length == 0)
			{
				// Neither head or tail hit, so let's try harder to make a body 
				// out of this stroke.
				Point[] dummy;
				tolerance = 2000.0; //Heuristic: vastly relax closure tolerance.
				//////////////// USO DO STROKE //////////////////////
                StrokeAnalyzer.AnalyzeClosedness(e.Stroke, tolerance, out closed, out dummy);

				if (closed)
				{
					//////////////// USO DO STROKE //////////////////////
                    RigidBodyBase newbody = MakeBodyFromClosedStroke(e.Stroke,vertices);

					// Repaint area around the newbody (unions with stroke bbox, below).
					Rectangle dirtybbox = newbody.BoundingBox;
					InvalidateInkSpaceRectangle(dirtybbox);

					// Select it, to show the smart tag.
					//////////////// USO DO STROKE //////////////////////
                    inkoverlay.Selection = this.MakeSingleStrokeCollection(e.Stroke);
					return;
				}
				else if (Geometry.DistanceBetween(head,tail) > 500.0)
				{
					// Interpret this stroke as a gravity-vector.
					GravitationalForceMech newgrav = new GravitationalForceMech();
					newgrav.Body = null; // Applies to all bodies!
					newgrav.origin = head;
					newgrav.vector = Vector.FromPoints(head,tail);

					// Repaint area around the gravity vector (unions with stroke bbox, below).
					Rectangle dirtybbox = newgrav.BoundingBox;
					InvalidateInkSpaceRectangle(dirtybbox);

					// Throw out the current gravity-vector stroke, if it exists.
					if (doc.Gravity != null)
					{
						dirtybbox = doc.Gravity.BoundingBox;
						InvalidateInkSpaceRectangle(dirtybbox);

                        if(doc.Ink.Strokes.Count > 0)
                        {
	    					
                            foreach (Stroke S in doc.Ink.Strokes)
                            {
                                if (S.Id == doc.Gravity.strokeid)
                                {
                                    doc.Ink.DeleteStroke(S);
                                }
                            }
                        }
					}

					//////////////// USO DO STROKE //////////////////////
                    newgrav.strokeid = e.Stroke.Id;
					doc.Gravity = newgrav;
					return;
				}
				else
				{
					// This stroke is probably an accidental tap -- discard it.
					e.Cancel = true;
					return;
				}
			}

			if (headhits.Length > 0 && tailhits.Length == 0)
			{
				// If only the head is hit, it must be an 'attractive force'.
				RigidBodyBase body = headhits[0];

				dbg.WriteLine("new ExternalForceMech");

				ExternalForceMech newmech = new ExternalForceMech();
				newmech.Body = BodyRef.For(body,head);
				newmech.vector = Vector.FromPoints(head,tail);

				//////////////// USO DO STROKE //////////////////////
                newmech.strokeid = e.Stroke.Id;
				doc.Mechanisms.Add(newmech);

				// Repaint area around the newmech (unions with stroke bbox, below).
				Rectangle dirtybbox = newmech.BoundingBox;
				InvalidateInkSpaceRectangle(dirtybbox);
				return;
			}

			if (headhits.Length == 0 && tailhits.Length > 0)
			{
				// If only the tail is hit, it must be a 'propulsive force'.
				RigidBodyBase body = tailhits[0];

				dbg.WriteLine("new PropulsiveForceMech");

				PropulsiveForceMech newmech = new PropulsiveForceMech();
				newmech.Body = BodyRef.For(body,tail);
				newmech.vector = Vector.FromPoints(head,tail);

                //////////////// USO DO STROKE //////////////////////
				newmech.strokeid = e.Stroke.Id;
				doc.Mechanisms.Add(newmech);

				// Repaint area around the newmech (unions with stroke bbox, below).
				Rectangle dirtybbox = newmech.BoundingBox;
				InvalidateInkSpaceRectangle(dirtybbox);
				return;
			}

			if (true) // scope
			{
				// Create a binding mechanism between two bodies.
				RigidBodyBase headbody = headhits[0], tailbody = tailhits[0];

				// If both the head and the tail hit same object, 
				// attach the head to the one behind.
				if (Object.ReferenceEquals(headbody,tailbody))
				{
					if (headhits.Length > 1)
						headbody = headhits[1];
					else if (tailhits.Length > 1)
						tailbody = tailhits[1];
					else
					{
						// Don't self-connect. We will perhaps interpret the stroke as an 
						// anchor-gesture or a selection-gesture, 
						// but if we cannot, we will cancel.
						//////////////// USO DO STROKE //////////////////////
                        int nc = e.Stroke.PolylineCusps.Length;
						if (np <= 25)
						{
                            inkoverlay.Selection = MakeSingleStrokeCollection(headbody.strokeid);
							SetEditingMode(InkOverlayEditingMode.Select,false);
						}
						else if (np <= 150 && (nc >= 3 && nc <= 5))
						{
							headbody.anchored = !headbody.anchored; //toggle
						}

						e.Cancel = true;

						// Repaint area around the headbody (unions with stroke bbox, below).
						Rectangle dirtybbox = headbody.BoundingBox;
						InvalidateInkSpaceRectangle(dirtybbox);
						return;
					}
				}

				// Create a rope, rod, or spring out of the stroke.
				//////////////// USO DO STROKE //////////////////////
                MechanismBase newmech = MakeRodRopeOrSpring(e.Stroke,headbody,tailbody);

				if (newmech != null)
				{
					// Repaint area around the newmech (unions with stroke bbox, below).
					Rectangle dirtybbox = newmech.BoundingBox;
					InvalidateInkSpaceRectangle(dirtybbox);
					return;
				}
				else
				{
					// Throw out the stroke, and return.
					e.Cancel = true;
					return;
				}
			}
		}
		catch (Exception ex)
		{
			// Cancel the stroke, and log the error.
			e.Cancel = true;
			Global.HandleThreadException(this, new System.Threading.ThreadExceptionEventArgs(ex));
		}
		finally
		{
			// Repaint the area around the stroke (unions with newbody/mech region, above).
			//////////////// USO DO STROKE //////////////////////
            Rectangle dirtybbox = e.Stroke.GetBoundingBox();
			InvalidateInkSpaceRectangle(dirtybbox);
		}
    }

    #endregion

    #region TryTapToSelect()
    private void TryTapToSelect(Stroke stroke)
	{
		// We interpret strokes with fewer than 25 packets, 
		// starting and ending on the same body, as tap gestures.
		int np = stroke.PacketCount;
		Point head = stroke.GetPoint(0), tail = stroke.GetPoint(np-1);
		RigidBodyBase[] headhits = doc.HitTestBodies(head);
		RigidBodyBase[] tailhits = doc.HitTestBodies(tail);

		if (np <= 25 && 
			headhits.Length > 0 && 
			tailhits.Length > 0 && 
			Object.ReferenceEquals(headhits[0],tailhits[0]))
		{
			tappedbody = headhits[0];

			// Ensure we didn't just tap what's already selected.
			ArrayList selbodies = new ArrayList(
				doc.GetBodiesFor(inkoverlay.Selection));
			if (selbodies.Contains(tappedbody))
				return;

			// We must delay the call to set_Selection, until after InkOverlay is 
			// finished looking at this stroke.  BeginInvoke seems to work nicely 
			// for this purpose.
			dbg.WriteLine("-------TTS--------");
			base.BeginInvoke(new MethodInvoker(DelayTapToSelect));
		}
    }

    #endregion
        

    #region DelayTapToSelect()
    private void DelayTapToSelect()
	{
		dbg.WriteLine("-------TTSx--------");
		if (tappedbody != null)
			inkoverlay.Selection = MakeSingleStrokeCollection(tappedbody.strokeid);
    }
    #endregion

    #region inkoverlay_CursorInRange()
    private void inkoverlay_CursorInRange(object sender, InkCollectorCursorInRangeEventArgs e)
	{
		dbg.WriteLine("----- inkoverlay_CursorInRange -----");

		// Ensure we're on the UI thread.
		dbg.Assert(!this.InvokeRequired);

		try // To prevent exceptions from propagating back to ink runtime.
		{
			if (e.Cursor.Inverted)
				SetEditingMode(InkOverlayEditingMode.Delete,false);
			else
				SetEditingMode(declaredmode,false);
		}
		catch (Exception ex)
		{
			// Log the error.
			Global.HandleThreadException(this, new System.Threading.ThreadExceptionEventArgs(ex));
		}
    }
    #endregion

    #region inkoverlay_StrokesDeleting() -> Evento que faz a deleteção dos Strokes
    private void inkoverlay_StrokesDeleting(object sender, InkOverlayStrokesDeletingEventArgs e)
	{
		dbg.WriteLine("----- inkoverlay_StrokesDeleting -----");

		// Ensure we're on the UI thread.
		dbg.Assert(!this.InvokeRequired);

		// Track region to repaint.
		Rectangle dirtybbox = Rectangle.Empty;

		try // To prevent exceptions from propagating back to ink runtime.
		{
            // Get objects for stroke(s) to delete.
			RigidBodyBase[] bodies = doc.GetBodiesFor(e.StrokesToDelete);
			MechanismBase[] mechs = doc.GetMechanismsFor(e.StrokesToDelete);

            // Arrays de inteiros para remover os elementos posteriormente!
            ArrayList mechs_int = new ArrayList();
            ArrayList bodies_int = new ArrayList();
            ArrayList gravits_int = new ArrayList();


			// Delete mechanisms.
			foreach (MechanismBase mech in mechs)
			{
				// Para posterior envio
                mechs_int.Add( doc.Mechanisms.IndexOf(mech) );

                doc.Mechanisms.Remove(mech);
				dirtybbox = Rectangle.Union(dirtybbox,mech.BoundingBox);
			}

			// Delete bodies and their attached mechanisms.
			foreach (RigidBodyBase body in bodies)
			{
				// Para posterior envio
                bodies_int.Add( doc.Bodies.IndexOf(body) );
                
                mechs = doc.GetMechanismsForBody(body);
				foreach (MechanismBase mech in mechs)
				{
					doc.Mechanisms.Remove(mech);
					dirtybbox = Rectangle.Union(dirtybbox,mech.BoundingBox);

					Strokes mstrokes = doc.Ink.CreateStrokes(new int[] { mech.strokeid });
					doc.Ink.DeleteStrokes(mstrokes);
				}

				doc.Bodies.Remove(body);
				dirtybbox = Rectangle.Union(dirtybbox,body.BoundingBox);
			}

            

			// Check if this stroke was the gravity vector's?
			if (doc.Gravity != null)
			{
				foreach (Stroke s in e.StrokesToDelete)
					if (s.Id == doc.Gravity.strokeid) 
					{
						gravits_int.Add(s.Id);

                        this.InvalidateInkSpaceRectangle(doc.Gravity.BoundingBox);
						doc.Gravity = null;
					}
			}

            // Aqui vou mandar o evento para o servidor!
            // Um ArrayList com os MechanismBase a serem removidos
            // Um ArrayList com os Bodies
            // Um arraylist com os strokes a serem deletados
            
            ArrayList list = new ArrayList();

            list.Add(mechs_int); 
            list.Add(bodies_int); 
            list.Add(gravits_int); 
            	
            // Este evento eh para reproduzir o inkoverlay_Stroke
            Global.clienteEnvia.EnviaEvento((Object) list,"inkoverlay_StrokesDeleting");

		}
		catch (Exception ex)
		{
			// Log the error.
			Global.HandleThreadException(this, new System.Threading.ThreadExceptionEventArgs(ex));
		}
		finally
		{
			// Repaint the affected area.
			InvalidateInkSpaceRectangle(dirtybbox);
		}
    }
    #endregion

    #region inkoverlay_StrokesDeletingImpl() -> Evento que faz a deleteção remota dos elementos
    public void inkoverlay_StrokesDeletingImpl(ArrayList l)
	{
	 
        // Track region to repaint.
		Rectangle dirtybbox = Rectangle.Empty;

        try 
		{
            // Primeiro passo é salvar o modo de edição autal e colocar no modo de deleção
            InkOverlayEditingMode em =  inkoverlay.EditingMode;
            SetEditingMode(InkOverlayEditingMode.Delete,true);

            // Segundo passo é remover os elementos
            ArrayList mechs_int = (ArrayList) l[0];
            ArrayList bodies_int = (ArrayList) l[1];
            ArrayList gravits_int = (ArrayList) l[2];

            MechanismBase[] mechs;

            for(int i=0;i<mechs_int.Count;i++)
            {
                MechanismBase mech = (MechanismBase) doc.Mechanisms[ (int) mechs_int[i] ];

                doc.Mechanisms.Remove(mech);
				dirtybbox = Rectangle.Union(dirtybbox,mech.BoundingBox);

            }

            for(int i=0;i<bodies_int.Count;i++)
            {
                RigidBodyBase body = (RigidBodyBase) doc.Bodies[ (int) bodies_int[i] ];

                mechs = doc.GetMechanismsForBody(body);
				foreach (MechanismBase mech in mechs)
				{
					doc.Mechanisms.Remove(mech);
					dirtybbox = Rectangle.Union(dirtybbox,mech.BoundingBox);

					Strokes mstrokes = doc.Ink.CreateStrokes(new int[] { mech.strokeid });
					doc.Ink.DeleteStrokes(mstrokes);
				}

				doc.Bodies.Remove(body);
				dirtybbox = Rectangle.Union(dirtybbox,body.BoundingBox);
            }

			// Check if this stroke was the gravity vector's?
			if (doc.Gravity != null)
			{
                for(int i=0;i<gravits_int.Count;i++)
                {
                    if( ( (int) gravits_int[i] ) == doc.Gravity.strokeid )
                    {
                        this.InvalidateInkSpaceRectangle(doc.Gravity.BoundingBox);
						doc.Gravity = null;
                    }
                }
            }

            // Voltar para o modo de edição que estava antes!
            SetEditingMode(em,true);

		}
		catch (Exception ex)
		{
			// Log the error.
			Global.HandleThreadException(this, new System.Threading.ThreadExceptionEventArgs(ex));
		}
		finally
		{
			// Repaint the affected area.
			InvalidateInkSpaceRectangle(dirtybbox);
		}
     
    }
    #endregion
 
    #region GetStrokeById()
    private Stroke GetStrokeById(int id)
	{
		int[] ids = new int[] { id };
		Strokes strokes = doc.Ink.CreateStrokes(ids);
		if (strokes.Count != 1) throw new ApplicationException("Bogus stroke id");
		return strokes[0];
    }
    #endregion

    #region inkoverlay_SelectionChanging()
    private void inkoverlay_SelectionChanging(object sender, InkOverlaySelectionChangingEventArgs e)
	{
		dbg.WriteLine("----- inkoverlay_SelectionChanging -----");

		// Ensure we're on the UI thread.
		dbg.Assert(!this.InvokeRequired);

		try // To prevent exceptions from propagating back to ink runtime.
		{
			if (e.NewSelection.Count > 0)
			{
				// Get objects for selected stroke(s); remove mechanisms from bucket.
				MechanismBase[] mechs = doc.GetMechanismsFor(e.NewSelection);
				foreach (MechanismBase mech in mechs)
				{
					Stroke mechstroke = GetStrokeById(mech.strokeid);
					e.NewSelection.Remove(mechstroke);
				}

				// Don't allow gravity to be selected, either.
				if (doc.Gravity != null)
				{
                        if(doc.Ink.Strokes.Count > 0)
                        {
                            foreach (Stroke S in doc.Ink.Strokes)
                            {
                                if (S.Id == doc.Gravity.strokeid)
                                {
                                    e.NewSelection.Remove(S);
                                    return;
                                }
                            }
                        }

                    
                    // Stroke gravstroke = GetStrokeById(doc.Gravity.strokeid);
					// e.NewSelection.Remove(gravstroke);
				}
			}
		}
		catch (Exception ex)
		{
			// Log the error.
			Global.HandleThreadException(this, new System.Threading.ThreadExceptionEventArgs(ex));
		}
    }
    #endregion

    #region inkoverlay_SelectionChanged()
    private void inkoverlay_SelectionChanged(object sender, EventArgs e)
	{
		dbg.WriteLine("----- inkoverlay_SelectionChanged -----");

		// Ensure we're on the UI thread.
		dbg.Assert(!this.InvokeRequired);

		try // To prevent exceptions from propagating back to ink runtime.
		{
			// Show smarttag if only single-selection.
			RigidBodyBase[] bodies = doc.GetBodiesFor(inkoverlay.Selection);
			if (bodies.Length != 1)
			{
				hover.EnablePerItemEditCommands(false);
				if (bodytag.Visible) bodytag.Hide();
				return;
			}

			RigidBodyBase body = bodies[0];
			hover.EnablePerItemEditCommands(true);

			ShowBodyTag(body);
		}
		catch (Exception ex)
		{
			// Log the error.
			Global.HandleThreadException(this, new System.Threading.ThreadExceptionEventArgs(ex));
		}
    }

    #endregion

    #region ShowBodyTag()
    private void ShowBodyTag(RigidBodyBase body)
	{
		// Establish point on lower-right of selected body's bbox.
		Point p = new Point(body.BoundingBox.Right,body.BoundingBox.Bottom);
		using (Graphics g = this.CreateGraphics())
		{
			// Convert to pixel-space.
			inkoverlay.Renderer.InkSpaceToPixel(g, ref p);
		}

		// Make sure we're not off the edge of the screen.
		if (p.X > this.Right-bodytag.Width) p.X = this.Right-bodytag.Width;
		if (p.Y > this.Bottom-bodytag.Height) p.Y = this.Bottom-bodytag.Height;

		bodytag.Show(p);
    }

    #endregion

    #region inkoverlay_SelectionMoved()
    private void inkoverlay_SelectionMoved(object sender, InkOverlaySelectionMovedEventArgs e)
	{
		// Share impl with resize.
		inkoverlay_SelectionMovedOrResized(sender,e.OldSelectionBoundingRect);
    }
    #endregion

    #region inkoverlay_SelectionResized()
    private void inkoverlay_SelectionResized(object sender, InkOverlaySelectionResizedEventArgs e)
	{
		// Share impl with move.
		inkoverlay_SelectionMovedOrResized(sender,e.OldSelectionBoundingRect);
    }
    #endregion

    #region inkoverlay_SelectionMovedOrResized()
    private void inkoverlay_SelectionMovedOrResized(object sender, Rectangle oldbbox)
	{
		dbg.WriteLine("----- inkoverlay_SelectionMovedOrResized -----");

		// Ensure we're on the UI thread.
		dbg.Assert(!this.InvokeRequired);

        ArrayList bodies_int = new ArrayList();

		try // To prevent exceptions from propagating back to ink runtime.
		{
			// Formulate matrix to represent old->new transform.
			Strokes selection = inkoverlay.Selection;
			Rectangle newbbox = selection.GetBoundingBox();

			using (Matrix m = Geometry.MatrixFromRects(oldbbox,newbbox))
			{
				// Move the bodies and any attached mechanisms.
				ArrayList mechlist = new ArrayList();
				RigidBodyBase[] bodies = doc.GetBodiesFor(selection);
				foreach (RigidBodyBase body in bodies)
				{
                    // Para posterior envio
                    bodies_int.Add( doc.Bodies.IndexOf(body) );
                    
                    body.Transform(m);
					MechanismBase[] mechs = doc.GetMechanismsForBody(body);
					foreach (MechanismBase mech in mechs)
						if (!mechlist.Contains(mech)) mechlist.Add(mech);
					
					// Move the bodytag to match.
					if (bodytag.Visible) ShowBodyTag(body);
				}

				foreach (MechanismBase mech in mechlist)
					mech.Transform(m);
			}


            // Aqui vou mandar o evento para o servidor!
            // Enviar elementos:
            // 1 - oldbbox
            // 2  - newbbox
            // 3 - array coms os id de bodies
            ArrayList list = new ArrayList();

            list.Add(oldbbox); 
            list.Add(newbbox); 
            list.Add(bodies_int); 
            	
            // Este evento eh para reproduzir o inkoverlay_Stroke
            Global.clienteEnvia.EnviaEvento((Object) list,"inkoverlay_SelectionMovedOrResized");


			// Repaint everything.  Note: if excess flicker becomes apparent, it 
			// might be reduced by invalidating the individual items' bounding boxes.
			Invalidate();
		}
		catch (Exception ex)
		{
			// Log the error.
			Global.HandleThreadException(this, new System.Threading.ThreadExceptionEventArgs(ex));
		}
    }

    #endregion


    #region inkoverlay_SelectionMovedOrResizedImpl() --> Cuida do redimensionamento e posição dos elementos
    public void inkoverlay_SelectionMovedOrResizedImpl(ArrayList l)
	{
		try // To prevent exceptions from propagating back to ink runtime.
		{
			// Formulate matrix to represent old->new transform.
			Rectangle oldbbox = (Rectangle) l[0];
			Rectangle newbbox = (Rectangle) l[1];

            ArrayList bodies_int = (ArrayList) l[2];

			using (Matrix m = Geometry.MatrixFromRects(oldbbox,newbbox))
			{
				// Move the bodies and any attached mechanisms.
				ArrayList mechlist = new ArrayList();

                for(int i=0;i<bodies_int.Count;i++)
                {
                    RigidBodyBase body = (RigidBodyBase) doc.Bodies[ (int) bodies_int[i] ];

                    body.Transform(m);
					MechanismBase[] mechs = doc.GetMechanismsForBody(body);

					foreach (MechanismBase mech in mechs)
						if (!mechlist.Contains(mech)) mechlist.Add(mech);
					
					// Move the bodytag to match.
					if (bodytag.Visible) ShowBodyTag(body);
				}

				foreach (MechanismBase mech in mechlist)
					mech.Transform(m);
             }
			
			// Repaint everything.  Note: if excess flicker becomes apparent, it 
			// might be reduced by invalidating the individual items' bounding boxes.
			Invalidate();
		}
		catch (Exception ex)
		{
			// Log the error.
			Global.HandleThreadException(this, new System.Threading.ThreadExceptionEventArgs(ex));
		}
         
    }

    #endregion
    
    #region inkoverlay_Ink_InkAdded() -> Evento que ocorrem em background
    private void inkoverlay_Ink_InkAdded(object sender, StrokesEventArgs e)
	{
		// This event fires on a background thread, which creates a race condition 
		// when you access neweststrokeids.  
		// Return to the UI thread before continuing.
		if (this.InvokeRequired)
		{
			this.Invoke(
				new StrokesEventHandler(inkoverlay_Ink_InkAdded), 
				new object[] { sender, e });
			return;
		}

		dbg.WriteLine("----- inkoverlay_Ink_InkAdded -----");

		try // To prevent exceptions from propagating back to ink runtime.
		{
			neweststrokeids = e.StrokeIds;
		}
		catch (Exception ex)
		{
			// Log the error.
			Global.HandleThreadException(this, new System.Threading.ThreadExceptionEventArgs(ex));
		}
    }

    #endregion

    #region SystemEvents_DisplaySettingsChanged()
    private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
	{
		dbg.WriteLine("----- SystemEvents_DisplaySettingsChanged -----");

		// Layout hover-region target area and child controls.
		if (hover != null) hover.UpdateTargetRectLayout();
    }
    #endregion

    //
    // Menu item handlers

    #region hover_FileNewClicked()
    private void hover_FileNewClicked(object sender, System.EventArgs e)
	{
		if (!IsUserSure())
			return;

		doc = new MagicDocument();
		HookupOverlayToNewInk();
		savedfilename = null;

		Invalidate();
    }
    #endregion

    #region HookupOverlayToNewInk()
    private void HookupOverlayToNewInk()
	{
		// Hook up the new ink to the InkOverlay, and induce a repaint.
		DisableInkOverlay();
		try
		{
			inkoverlay.Ink = doc.Ink;
			inkoverlay.Ink.InkAdded += new StrokesEventHandler(inkoverlay_Ink_InkAdded);
		}
		finally
		{ inkoverlay.Enabled = true; }

		Invalidate();
    }
    #endregion

    #region DisableInkOverlay()
    private void DisableInkOverlay()
	{
		while (inkoverlay.CollectingInk) Application.DoEvents();  //review: racey spin?
		inkoverlay.Enabled = false;
		inkoverlay.Selection = EmptyStrokes;
    }
    #endregion

    #region hover_FileOpenClicked()
    private void hover_FileOpenClicked(object sender, EventArgs e)
	{
		if (!IsUserSure())
			return;

		OpenFileDialog ofd = new OpenFileDialog();
		ofd.DefaultExt = "physi";
		ofd.Filter = "Physics Illustrator files (*.physi)|*.physi|All files (*.*)|*.*";
		if (ofd.ShowDialog() == DialogResult.OK)
		{
			OpenDocument(ofd.FileName);
		}
    }

    #endregion

    #region OpenDocument()
    public void OpenDocument(string filename)
	{
		try
		{
			using (System.IO.TextReader reader = File.OpenText(filename))
			{
				// Load it!  And hook up the new Ink object.
				doc = MagicDocument.LoadDocument(reader);
				savedfilename = filename;
				doc.Ink.Dirty = false;

				HookupOverlayToNewInk();
			}
		}
		catch (IOException ex)
		{
			MessageBox.Show(
				ex.Message, 
				Application.ProductName, 
				MessageBoxButtons.OK, 
				MessageBoxIcon.Warning);
		}
    }
    #endregion

    #region OpenForCollaboration()
    public void OpenForCollaboration(TextWriter x)
	{
            try
		    {
                StringReader y = new StringReader(x.ToString());
        
    		    doc = MagicDocument.LoadDocument(y);
			    doc.Ink.Dirty = false;

			    HookupOverlayToNewInk();
            }
       		catch (IOException ex)
    		{
			    MessageBox.Show(
				    ex.Message, 
				    Application.ProductName, 
				    MessageBoxButtons.OK, 
				    MessageBoxIcon.Warning);
    		}
    }
    #endregion

    #region hover_FileSaveClicked()
    private void hover_FileSaveClicked(object sender, EventArgs e)
	{
		// If we haven't saved yet, do a save-as.  Else, just save.
		if (this.savedfilename == null)
		{
			ShowSaveAsDialog();
		}
		else
		{
			JustSaveIt();
		}
    }
    #endregion

    #region hover_FileSaveAsClicked()
    private void hover_FileSaveAsClicked(object sender, EventArgs e)
	{
		ShowSaveAsDialog();
    }
    #endregion

    #region JustSaveIt()
    private void JustSaveIt()
	{
		// Write it! And remember to set the Dirty property to false.
		dbg.Assert(savedfilename != null);

		using (TextWriter writer = File.CreateText(savedfilename))
		{
			doc.WriteDocument(writer);
			doc.Ink.Dirty = false;
		}
    }
    #endregion

    #region SaveForCollaboration()
    public StringWriter SaveForCollaboration()
	{
		// Write it! And remember to set the Dirty property to false.

       //  TextWriter writer = File.Create(

        StringWriter x = new StringWriter();
        doc.WriteDocument(x);
        doc.Ink.Dirty = false;

        return x;

    }
    #endregion



    #region ShowSaveAsDialog()
    private DialogResult ShowSaveAsDialog()
	{
		SaveFileDialog sfd = new SaveFileDialog();
		sfd.AddExtension = true;
		sfd.DefaultExt = "physi";
		sfd.Filter = "Physics Illustrator files (*.physi)|*.physi|All files (*.*)|*.*";

		DialogResult dr = sfd.ShowDialog();
		if (dr == DialogResult.OK)
		{
			savedfilename = sfd.FileName;
			JustSaveIt();
		}

		return dr;
    }
    #endregion

    #region hover_FileHelpClicked()
    private void hover_FileHelpClicked(object sender, EventArgs e)
	{
		// Kickoff help file in separate process.
		Assembly thisassem = Assembly.GetExecutingAssembly();
		string help = Path.Combine(
			Path.GetDirectoryName(thisassem.Location),
			"PhysicsIllustratorForTabletPC.chm");
		System.Diagnostics.Process.Start(help);
    }

    #endregion


    #region hover_CollabClicked()
    private void hover_CollabClicked(object sender, EventArgs e)
	{
		// Chama a janela de colaboração
		
        CollabDialog  dlg = new CollabDialog();
		dlg.ShowDialog(this);
    }

    #endregion


    #region hover_FileAboutClicked()
    private void hover_FileAboutClicked(object sender, EventArgs e)
	{
		AboutBox dlg = new AboutBox();
		dlg.ShowDialog(this);
    }

    #endregion

    #region hover_FileExitClicked()
    private void hover_FileExitClicked(object sender, EventArgs e)
	{
		this.Close();
    }

    #endregion

    #region hover_EditCloneClicked()
    private void hover_EditCloneClicked(object sender, EventArgs e)
	{
		Strokes selected = inkoverlay.Selection;
		if (selected == null || selected.Count != 1) return;

        ArrayList bodies_int = new ArrayList();
        // Gerando o array com os ids contidos em select
        foreach(Stroke ss in selected)
        {
            bodies_int.Add(ss.Id);
        }

		// Get objects for the targeted stroke(s).  Ensure that only one is selected.
		RigidBodyBase[] bodies = doc.GetBodiesFor(selected);
		if (bodies.Length != 1) return;

		RigidBodyBase body = bodies[0];

        // First, clone the ink stroke.  Move it down and to the right a bit.
		Rectangle newrect = selected.GetBoundingBox();
		newrect.Offset(1000,1000);
		doc.Ink.AddStrokesAtRectangle(selected,newrect);

		// Next, clone the body, binding it to the new stroke id. 
		// Note: we got the new Strokes' ids by listening to the InkAdded event
		// AddStrokesAtRectangle doesn't return the strokes' ids.
		RigidBodyBase newbody = body.Clone(neweststrokeids[0]);

		doc.Bodies.Add(newbody);

		// Repaint the area around the newbody.
		Rectangle dirtybbox = newbody.BoundingBox;
		InvalidateInkSpaceRectangle(dirtybbox);

		// Select it, to show the smart tag.
		inkoverlay.Selection = doc.Ink.CreateStrokes(neweststrokeids);



        // Aqui vou mandar o evento para o servidor!
        // Mandar para evento remoto:
        // 1 - um array contendo os strokeids dos strokes contidos em selected (A coleção de strokes)
        ArrayList list = new ArrayList();

        list.Add(bodies_int); 
        	
        // Este evento eh para reproduzir o inkoverlay_Stroke
        Global.clienteEnvia.EnviaEvento((Object) list,"hover_EditCloneClicked");

    }

    #endregion

    #region hover_EditCloneClickedImpl() --> Faz a operação de clonagem
    public void hover_EditCloneClickedImpl(ArrayList l)
	{
	
	    // Primeiro passo é salvar o modo de edição autal e colocar no modo de deleção
        InkOverlayEditingMode em =  inkoverlay.EditingMode;
        SetEditingMode(InkOverlayEditingMode.Ink,true);

        // Obtendo os elementos que foram selecionados
        Strokes selected = doc.Ink.CreateStrokes();
        ArrayList bodies_int = (ArrayList) l[0];

        for(int i = 0;i< bodies_int.Count;i++)
        {
            foreach(Stroke ss in inkoverlay.Ink.Strokes)
            {
                if( ss.Id == (int) bodies_int[i] )
                {
                    selected.Add(ss);
                }
            }
        }

        if (selected == null || selected.Count != 1) return;

		// Get objects for the targeted stroke(s).  Ensure that only one is selected.
		RigidBodyBase[] bodies = doc.GetBodiesFor(selected);
		if (bodies.Length != 1) return;

		RigidBodyBase body = bodies[0];

        // First, clone the ink stroke.  Move it down and to the right a bit.
		Rectangle newrect = selected.GetBoundingBox();
		newrect.Offset(1000,1000);
		doc.Ink.AddStrokesAtRectangle(selected,newrect);

		// Next, clone the body, binding it to the new stroke id. 
		// Note: we got the new Strokes' ids by listening to the InkAdded event
		// AddStrokesAtRectangle doesn't return the strokes' ids.
		RigidBodyBase newbody = body.Clone(neweststrokeids[0]);

		doc.Bodies.Add(newbody);

		// Repaint the area around the newbody.
		Rectangle dirtybbox = newbody.BoundingBox;
		InvalidateInkSpaceRectangle(dirtybbox);

		// Select it, to show the smart tag.
		inkoverlay.Selection = doc.Ink.CreateStrokes(neweststrokeids);

        // Voltar para o modo de edição que estava antes!
        SetEditingMode(em,true);
    }

    #endregion
    
    #region hover_EditDeleteClicked()
    private void hover_EditDeleteClicked(object sender, EventArgs e)
	{
		Strokes doomed = inkoverlay.Selection;
		if (doomed == null || doomed.Count != 1) return;

		// Simply delegate to the UI handler for deleting strokes.
		this.inkoverlay_StrokesDeleting(this, 
			new InkOverlayStrokesDeletingEventArgs(doomed));

		// Now, actually delete the strokes, too.
		doc.Ink.DeleteStrokes(doomed);

		// Clear the selection rectangle.
		inkoverlay.Selection = EmptyStrokes;
    }
    #endregion

    #region hover_EditStraightenClicked()
    private void hover_EditStraightenClicked(object sender, EventArgs e)
	{
		Strokes selected = inkoverlay.Selection;
        
        if (selected == null || selected.Count != 1) return;

		// Get objects for the targeted stroke(s).  Ensure only one selected.
		RigidBodyBase[] bodies = doc.GetBodiesFor(selected);
		if (bodies.Length != 1) return;

        // Gerando o array com os ids contidos em select
        ArrayList bodies_int = new ArrayList();
        
        foreach(Stroke ss in selected)
        {
            bodies_int.Add(ss.Id);
        }

		RigidBodyBase body = bodies[0];

		// Repaint the area around the original body.
		Rectangle dirtybbox = body.BoundingBox;
		InvalidateInkSpaceRectangle(dirtybbox);

		// Straighten it.
		body.Straighten();

		// Repaint the area around the newbody.
		dirtybbox = body.BoundingBox;
		InvalidateInkSpaceRectangle(dirtybbox);

        // Aqui vou mandar o evento para o servidor!
        // Mandar para evento remoto:
        // 1 - um array contendo os strokeids dos strokes contidos em selected (A coleção de strokes)
        ArrayList list = new ArrayList();

        list.Add(bodies_int); 
        	
        // Este evento eh para reproduzir o inkoverlay_Stroke
        Global.clienteEnvia.EnviaEvento((Object) list,"hover_EditStraightenClicked");
    }
    #endregion

    #region hover_EditStraightenClickedImpl() --> Faz a operação Straighten
    public void hover_EditStraightenClickedImpl(ArrayList l)
	{
		// Primeiro passo é salvar o modo de edição autal e colocar no modo de deleção
        InkOverlayEditingMode em =  inkoverlay.EditingMode;
        SetEditingMode(InkOverlayEditingMode.Ink,true);

        // Obtendo os elementos que foram selecionados
        Strokes selected = doc.Ink.CreateStrokes();
        ArrayList bodies_int = (ArrayList) l[0];

        for(int i = 0;i< bodies_int.Count;i++)
        {
            foreach(Stroke ss in inkoverlay.Ink.Strokes)
            {
                if( ss.Id == (int) bodies_int[i] )
                {
                    selected.Add(ss);
                }
            }
        }

		if (selected == null || selected.Count != 1) return;

		// Get objects for the targeted stroke(s).  Ensure only one selected.
		RigidBodyBase[] bodies = doc.GetBodiesFor(selected);
		if (bodies.Length != 1) return;

		RigidBodyBase body = bodies[0];

		// Repaint the area around the original body.
		Rectangle dirtybbox = body.BoundingBox;
		InvalidateInkSpaceRectangle(dirtybbox);

		// Straighten it.
		body.Straighten();

		// Repaint the area around the newbody.
		dirtybbox = body.BoundingBox;
		InvalidateInkSpaceRectangle(dirtybbox);

        // Voltar para o modo de edição que estava antes!
        SetEditingMode(em,true);


    }
    #endregion

    #region hover_EditPropertiesClicked()
    private void hover_EditPropertiesClicked(object sender, EventArgs e)
	{
		Strokes selected = inkoverlay.Selection;
		if (selected == null || selected.Count != 1) return;

		// Get objects for the targeted stroke(s).  Ensure that only one is selected.
		RigidBodyBase[] bodies = doc.GetBodiesFor(selected);
		if (bodies.Length != 1) return;

		RigidBodyBase body = bodies[0];

		using (BodyPropertiesForm bpf = new BodyPropertiesForm(body))
		{
			bpf.ShowDialog(this);
			Invalidate();
		}
    }
    #endregion

    #region hover_EditPropertiesClickedImpl() --> Faz a operação de modificação das propriedades
    public void hover_EditPropertiesClickedImpl(ArrayList l)
	{
		
        foreach(RigidBodyBase b in doc.Bodies)
        {
            if(b.strokeid == ( (int) l[0] ) )
            {
                b.density    = (double) l[1];
		        b.elasticity = (double) l[2];
		        b.cfriction  = (double) l[3];
        		b.fillcolor = (Color)  l[4];
                
                Invalidate();

                return;
            }
        }

        
    }
    #endregion

    #region hover_PenDrawClicked()
    private void hover_PenDrawClicked(object sender, EventArgs e)
	{
		SetEditingMode(InkOverlayEditingMode.Ink,true);
    }
    #endregion

    #region hover_PenEraseClicked()
    private void hover_PenEraseClicked(object sender, EventArgs e)
	{
		SetEditingMode(InkOverlayEditingMode.Delete,true);
    }
    #endregion

    #region hover_PenLassoClicked()
    private void hover_PenLassoClicked(object sender, EventArgs e)
	{
		SetEditingMode(InkOverlayEditingMode.Select,true);
    }

    #endregion

    #region SetEditingMode()
    private void SetEditingMode(InkOverlayEditingMode em, bool @explicit)
	{
		if (inkoverlay.EditingMode == em) return;

		if (em == InkOverlayEditingMode.Delete) 
			inkoverlay.Selection = EmptyStrokes;

		inkoverlay.EditingMode = em;
		if (@explicit) declaredmode = em;
    }
    #endregion
    
    #region EmptyStrokes()
    private Strokes EmptyStrokes
	{
		get { return doc.Ink.CreateStrokes(); }
    }
    #endregion

    #region MakeSingleStrokeCollection()
    private Strokes MakeSingleStrokeCollection(int id)
	{
		int[] ids = new int[] { id };
		
        // A linha abaixo dá problema quando o dado vem remotamente...
        Strokes strokes = doc.Ink.CreateStrokes(ids);

		if (strokes.Count != 1) throw new ApplicationException("Bogus stroke id");
		return strokes;
    }
    #endregion
        
    #region MakeSingleStrokeCollection()
    private Strokes MakeSingleStrokeCollection(Stroke s)
	{
		return MakeSingleStrokeCollection(s.Id);
    }
    #endregion


    // Controles da simulação local

    #region hover_AnimateClicked()
    private void hover_AnimateClicked(object sender, EventArgs e)
	{
		dbg.WriteLine("----- hover_AnimateClicked -----");

		PhysicsIllustrator.SmartTag.SmartTag tag = sender as PhysicsIllustrator.SmartTag.SmartTag;
        
		// Running or stopping?
		if (inkoverlay.Enabled)
		{
            // Aqui vou mandar o evento para o servidor!    	
            // Este evento eh para o início da simulação
            Global.clienteEnvia.EnviaEvento(new ArrayList(),"Start");
            
            // Lock down all editing operations.
			DisableInkOverlay();
			if (bodytag.Visible) bodytag.Hide();

			// Habilita o botão pause.
			hover.Enabled = false;
            
            tag.Image = Global.LoadImage("Resources.PauseAnimate.ico");
			menuttip.SetToolTip(tag,"Pause");
			tag.Show();  

			// Fork the document for animation.
			MagicDocument animedoc = doc.Clone();

            // Transformar o engine de simulação em uma Thread!

			// Run the animation!
			engine = new AnimationEngine();
            engine.Start(animedoc,this);

            engine.setPause(false);

		}
		else // Pausa
		{
            if(engine.getPause()) // Está pausado!
            {
                // Aqui vou mandar o evento para o servidor!    	
                // Este evento eh para o resume
                Global.clienteEnvia.EnviaEvento(new ArrayList(),"Resume");
                
                engine.setPause(false);
                tag.Image  =  Global.LoadImage("Resources.PauseAnimate.ico");
                menuttip.SetToolTip(tag,"Pause");
                tag.Visible = true;
                tag.Show(); 

                 hover.getPauseTag().Visible = false;

            }
            else // Está rodando
            {
                
                // Aqui vou mandar o evento para o servidor!    	
                // Este evento eh para o pause
                Global.clienteEnvia.EnviaEvento(new ArrayList(),"Pause");

                engine.setPause(true);
                // Devo pausar e mudar o ícone do botão

                tag.Image  =  Global.LoadImage("Resources.ResumeAnimate.ico");
                menuttip.SetToolTip(tag,"Resume");

                hover.getPauseTag().Image  =  Global.LoadImage("Resources.StopAnimation.ico");
                menuttip.SetToolTip(hover.getPauseTag(),"Stop");
                hover.getPauseTag().Visible = true;
                hover.getPauseTag().Show();  

            } 
		}
        
    }
    #endregion

    #region hover_PauseClicked()
    private void hover_PauseClicked(object sender, EventArgs e)
	{
        dbg.WriteLine("----- hover_AnimateClickedImpl -----");

		PhysicsIllustrator.SmartTag.SmartTag tag = hover.getAnimateTag();
        
        // Aqui vou mandar o evento para o servidor!    	
        // Este evento eh para o Stop
        Global.clienteEnvia.EnviaEvento(new ArrayList(),"Stop");

        engine.setPause(false);
        Thread.Sleep(10);
        engine.Stop();

		inkoverlay.Enabled = true;

		tag.Image = Global.LoadImage("Resources.Animate.ico");
		menuttip.SetToolTip(tag,"Animate!");
		hover.Enabled = true;
		hover.DisplayInitial();
		Invalidate();

        hover.getPauseTag().Visible = false;


    }
    #endregion


    // Controles da simulação remota
    #region hover_AnimateClickedStartImpl()
    public void hover_AnimateClickedStartImpl(object sender)
	{
        // Aqui vou começar a simulação remotamente

        // Lock down all editing operations.
        DisableInkOverlay();
        if (bodytag.Visible) 
            bodytag.Hide();

        // Habilita o botão pause.
        hover.Enabled = false;

        
        hover.getAnimateTag().Image = Global.LoadImage("Resources.PauseAnimate.ico");
        menuttip.SetToolTip(hover.getAnimateTag(),"Pause");
        hover.getAnimateTag().Show();  

        // Fork the document for animation.
        MagicDocument animedoc = doc.Clone();

        // Run the animation!
        engine = new AnimationEngine();
        engine.Start(animedoc,this);

        engine.setPause(false);
    }
    #endregion

    #region hover_AnimateClickedStopImpl()
    public void hover_AnimateClickedStopImpl(object sender)
	{
		PhysicsIllustrator.SmartTag.SmartTag tag = hover.getAnimateTag();

        engine.setPause(false);
        Thread.Sleep(10);
        engine.Stop();

		inkoverlay.Enabled = true;

		tag.Image = Global.LoadImage("Resources.Animate.ico");
		menuttip.SetToolTip(tag,"Animate!");
		hover.Enabled = true;
		hover.DisplayInitial();
		Invalidate();

        hover.getPauseTag().Visible = false;
     
    }
    #endregion

    #region hover_AnimateClickedPauseImpl()
    public void hover_AnimateClickedPauseImpl(object sender)
	{
        engine.setPause(true);
        // Devo pausar e mudar o ícone do botão

        PhysicsIllustrator.SmartTag.SmartTag tag = hover.getAnimateTag();

        tag.Image  =  Global.LoadImage("Resources.ResumeAnimate.ico");
        menuttip.SetToolTip(tag,"Resume");

        hover.getPauseTag().Image  =  Global.LoadImage("Resources.StopAnimation.ico");
        menuttip.SetToolTip(hover.getPauseTag(),"Stop");
        hover.getPauseTag().Visible = true;
        hover.getPauseTag().Show();  
     
    }
    #endregion

    #region hover_AnimateClickedResumeImpl()
    public void hover_AnimateClickedResumeImpl(object sender)
	{
        PhysicsIllustrator.SmartTag.SmartTag tag = hover.getAnimateTag();

        engine.setPause(false);
        tag.Image  =  Global.LoadImage("Resources.PauseAnimate.ico");
        menuttip.SetToolTip(tag,"Pause");
        tag.Visible = true;
        tag.Show(); 

        hover.getPauseTag().Visible = false;

    }
    #endregion

    #region hover_MinimizeClicked()
    private void hover_MinimizeClicked(object sender, EventArgs e)
	{
		dbg.WriteLine("----- hover_MinimizeClicked -----");

		PhysicsIllustrator.SmartTag.SmartTag tag = sender as PhysicsIllustrator.SmartTag.SmartTag;

		this.WindowState = FormWindowState.Minimized;
    }
    #endregion
}