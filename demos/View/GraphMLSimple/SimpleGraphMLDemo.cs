/****************************************************************************
 ** 
 ** This demo file is part of yFiles.NET 5.2.
 ** Copyright (c) 2000-2019 by yWorks GmbH, Vor dem Kreuzberg 28,
 ** 72070 Tuebingen, Germany. All rights reserved.
 ** 
 ** yFiles demo files exhibit yFiles.NET functionalities. Any redistribution
 ** of demo files in source code or binary form, with or without
 ** modification, is not permitted.
 ** 
 ** Owners of a valid software license for a yFiles.NET version that this
 ** demo is shipped with are allowed to use the demo source code as basis
 ** for their own yFiles.NET powered applications. Use of such programs is
 ** governed by the rights and conditions as set out in the yFiles.NET
 ** license agreement.
 ** 
 ** THIS SOFTWARE IS PROVIDED ''AS IS'' AND ANY EXPRESS OR IMPLIED
 ** WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 ** MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN
 ** NO EVENT SHALL yWorks BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 ** SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
 ** TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 ** PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 ** LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 ** NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 ** SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 ** 
 ***************************************************************************/

using System;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Demo.yFiles.IO.GraphML.Simple.Properties;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Graph;
using yWorks.GraphML;

namespace Demo.yFiles.IO.GraphML.Simple
{

  /// <summary>
  /// Demo that shows how to load and save a 
  /// graph in the xml-based format GraphML using an instance
  /// of <see cref="GraphMLIOHandler"/>
  /// </summary>
  public partial class SimpleGraphMLDemo : Form
  {
    #region GraphML Demo Code

    /// <summary>
    /// Automatically generated by Visual Studio.
    /// Wires up the UI components and adds a 
    /// <see cref="GraphControl"/> to the form.
    /// </summary>
    public SimpleGraphMLDemo() {
      InitializeComponent();
      graphControl.SmoothingMode = SmoothingMode.AntiAlias;
      graphmlIoHandler = graphControl.GraphMLIOHandler;
      // Enable file operations
      // This can also be set in the designer, of course...
      GraphControl.FileOperationsEnabled = true;

      openButton.SetCommand(Commands.Open, graphControl);
      openToolStripMenuItem.SetCommand(Commands.Open, graphControl);
      saveButton.SetCommand(Commands.SaveAs, graphControl);
      saveAsToolStripMenuItem.SetCommand(Commands.SaveAs, graphControl);

      cutButton.SetCommand(Commands.Cut, graphControl);
      cutToolStripMenuItem.SetCommand(Commands.Cut, graphControl);
      copyButton.SetCommand(Commands.Copy, graphControl);
      copyToolStripMenuItem.SetCommand(Commands.Copy, graphControl);
      pasteButton.SetCommand(Commands.Paste, graphControl);
      pasteToolStripMenuItem.SetCommand(Commands.Paste, graphControl);
      deleteToolStripMenuItem.SetCommand(Commands.Delete, graphControl);
      undoButton.SetCommand(Commands.Undo, graphControl);
      undoToolStripMenuItem.SetCommand(Commands.Undo, graphControl);
      redoButton.SetCommand(Commands.Redo, graphControl);
      redoToolStripMenuItem.SetCommand(Commands.Redo, graphControl);

      zoomInButton.SetCommand(Commands.IncreaseZoom, graphControl);
      increaseZoomToolStripMenuItem.SetCommand(Commands.IncreaseZoom, graphControl);
      zoomOutButton.SetCommand(Commands.DecreaseZoom, graphControl);
      decreaseZoomToolStripMenuItem.SetCommand(Commands.DecreaseZoom, graphControl);
      fitContentButton.SetCommand(Commands.FitContent, graphControl);
      fitGraphBoundsToolStripMenuItem.SetCommand(Commands.FitContent, graphControl);
    }

    /// <summary>
    /// The GraphMLIOhandler instance used to serialize and deserialize graphs.
    /// </summary>
    private readonly GraphMLIOHandler graphmlIoHandler;
    
    /// <summary>
    /// Interpret the text content of the text field as graphml
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ApplyGraphMLButton_Click(object sender, EventArgs e) {
      try {
        using (StringReader reader = new StringReader(graphMLText.Text)) {
          graphmlIoHandler.Read(Graph, reader);
          GraphControl.FitGraphBounds();
        }
      } catch (Exception exc) {
        new ExceptionDialog(exc).ShowDialog();
      }
    }

    private void ShowGraphMLButton_Click(object sender, EventArgs e) {
      bool includeGraphSettings = graphSettingsButton.Checked;
      try {
        StringBuilder builder = new StringBuilder();
        using (StringWriter writer = new StringWriter(builder)) {
          // set whether to make the handler write the graph settings.
          graphmlIoHandler.SerializationPropertyOverrides.Set(SerializationProperties.DisableGraphSettings, !includeGraphSettings);
          graphmlIoHandler.Write(Graph, writer);
          // Use SelectAll ans Paste to enable undoability
          graphMLText.SelectAll();
          graphMLText.Paste(builder.ToString());
        }
      } catch (Exception exc) {
        new ExceptionDialog(exc).ShowDialog();
      }
    }

    #endregion GraphML Demo Code

    

    /// <summary>
    /// Called upon loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    /// <seealso cref="InitializeInputModes"/>
    /// <seealso cref="InitializeGraph"/>
    protected override void OnLoad(EventArgs e) {
      base.OnLoad(e);
      description.LoadFile(new MemoryStream(Resources.description), RichTextBoxStreamType.RichText);

      // initialize the graph
      InitializeGraph();

      // initialize the input mode
      InitializeInputModes();
    }

    /// <summary>
    /// Calls <see cref="CreateEditorMode"/> and registers
    /// the result as the <see cref="CanvasControl.InputMode"/>.
    /// </summary>
    protected virtual void InitializeInputModes() {
      GraphControl.InputMode = CreateEditorMode();
    }

    /// <summary>
    /// Creates the default input mode for the GraphControl,
    /// a <see cref="GraphEditorInputMode"/>.
    /// </summary>
    /// <returns>a new GraphEditorInputMode instance</returns>
    protected virtual IInputMode CreateEditorMode() {
      return new GraphEditorInputMode();
    }

    /// <summary>
    /// Initializes the graph instance setting default styles
    /// and creating a small sample graph.
    /// </summary>
    protected virtual void InitializeGraph()
    {
      DefaultGraph defaultGraph = Graph.Lookup<DefaultGraph>();
      if (defaultGraph != null)
      {
        defaultGraph.UndoEngineEnabled = true;
      }
      //Read initial graph from embedded resource
      graphmlIoHandler.Read(GraphControl.Graph, "Resources\\styles.graphml");

      GraphControl.FitGraphBounds();
    }

    #region Standard Demo Actions
    
    /// <summary>
    /// Callback action that is triggered when the user exits the application.
    /// </summary>
    protected virtual void ExitToolStripMenuItem_Click(object sender, EventArgs e) {
      Application.Exit();
    }

    /// <summary>
    /// Returns the GraphControl instance used in the form.
    /// </summary>
    public GraphControl GraphControl {
      get { return graphControl; }
    }

    /// <summary>
    /// Gets the currently registered IGraph instance from the GraphControl.
    /// </summary>
    public IGraph Graph {
      get { return GraphControl.Graph; }
    }

    /// <summary>
    /// Gets the currently registered IGraphSelection instance from the GraphControl.
    /// </summary>
    public IGraphSelection Selection {
      get { return GraphControl.Selection; }
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main() {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new SimpleGraphMLDemo());
    }

    #endregion

  }
}