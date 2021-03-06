/****************************************************************************
 ** 
 ** This demo file is part of yFiles.NET 5.3.
 ** Copyright (c) 2000-2020 by yWorks GmbH, Vor dem Kreuzberg 28,
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using System.Windows.Markup;
using Demo.yFiles.Graph.StyleDecorators.Properties;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

[assembly : XmlnsDefinition("http://www.yworks.com/yFiles.net/demos/StyleDecorators/1.0",
    "Demo.yFiles.Graph.StyleDecorators")]
[assembly : XmlnsPrefix("http://www.yworks.com/yFiles.net/demos/StyleDecorators/1.0", "demo")]

namespace Demo.yFiles.Graph.StyleDecorators
{
  /// <summary>
  /// This demo shows how to wrap existing node, edge and label styles 
  /// and add visual decorators.
  /// </summary>
  public partial class StyleDecoratorsForm : Form
  {
    private readonly Random random = new Random();

    /// <summary>
    /// Automatically generated by Visual Studio.
    /// Wires up the UI components and adds a 
    /// <see cref="GraphControl"/> to the form.
    /// </summary>
    public StyleDecoratorsForm() {
      InitializeComponent();
	  graphControl.FileOperationsEnabled = true;
      RegisterCommands();
    }

    private void RegisterCommands() {
      zoomInButton.SetCommand(Commands.IncreaseZoom, graphControl);
      zoomOutButton.SetCommand(Commands.DecreaseZoom, graphControl);
      fitContentButton.SetCommand(Commands.FitContent, graphControl);
      openButton.SetCommand(Commands.Open, graphControl);
      saveButton.SetCommand(Commands.SaveAs, graphControl);
    }

    /// <summary>
    /// Called upon loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    /// <seealso cref="InitializeInputModes"/>
    /// <seealso cref="InitializeDefaultStyles"/>
	protected override void OnLoad(EventArgs e) {
      base.OnLoad(e);

      description.LoadFile(new MemoryStream(Resources.description), RichTextBoxStreamType.RichText);

      // initialize the graph
      InitializeDefaultStyles();

      // initialize the input mode
      InitializeInputModes();
      
      CreateSampleGraph();
      graphControl.FitGraphBounds();
    }

    /// <summary>
    /// Creates an input mode and registers
    /// the result as the <see cref="CanvasControl.InputMode"/>.
    /// </summary>
    protected virtual void InitializeInputModes() {
      var graphEditorInputMode = new GraphEditorInputMode();
      // create random data for new edges to show different visualizations
      graphEditorInputMode.CreateEdgeInputMode.EdgeCreated +=
        (source, evt) => { evt.Item.Tag = Enum.ToObject(typeof (TrafficLoad), random.Next(4)); };
      graphControl.InputMode = graphEditorInputMode;
    }

    /// <summary>
    /// Sets a style wrappers as the default for newly created
    /// elements in the graph.
    /// </summary>
    protected virtual void InitializeDefaultStyles() {
      Pen pen = new Pen(new SolidBrush(Color.FromArgb(0xFF, 0x24, 0x9A, 0xE7)), 1);
      LinearGradientBrush brush = new LinearGradientBrush(new PointF(0,0), new PointF(0, 1.01f), Color.FromArgb(0xFF, 0xCC, 0xFF, 0xFF), Color.FromArgb(0xFF, 0x24, 0x9A, 0xE7));
      // Create a new style and use it as default node style
      Graph.NodeDefaults.Style = new NodeStyleDecorator(
        new ShapeNodeStyle { Shape = ShapeNodeShape.RoundRectangle, Pen = pen, Brush = brush },
        "Resources/computer.png");
      Graph.NodeDefaults.Size = new SizeD(80, 40);

      // Create a new style and use it as default edge style
      Graph.EdgeDefaults.Style =
          new EdgeStyleDecorator(
              new NodeStylePortStyleAdapter(new ShapeNodeStyle {
                Shape = ShapeNodeShape.Ellipse,
                Brush = Brushes.DarkGray,
                Pen = null
              }) {RenderSize = new SizeD(3, 3)});
      // Create a new style and use it as default label style

      ILabelStyle labelStyle = new LabelStyleDecorator(new DefaultLabelStyle());
      Graph.NodeDefaults.Labels.Style = labelStyle;
      Graph.EdgeDefaults.Labels.Style = labelStyle;
      Graph.NodeDefaults.Labels.LayoutParameter = new InteriorLabelModel().CreateParameter(InteriorLabelModel.Position.Center);
    }

    private void CreateSampleGraph() {
      INodeStyle nodeStyle = new ShapeNodeStyle {
        Shape = ShapeNodeShape.RoundRectangle,
        Pen = new Pen(new SolidBrush(Color.FromArgb(0xFF, 0x24, 0x9A, 0xE7)), 1),
        Brush = new LinearGradientBrush(new PointF(0, 0), new PointF(0, 1.01f),
            Color.FromArgb(0xFF, 0xCC, 0xFF, 0xFF),
            Color.FromArgb(0xFF, 0x24, 0x9A, 0xE7))
      };

      var node1 = CreateNode(new PointD(0, 0), new NodeStyleDecorator(nodeStyle, "Resources/internet.png"), "Root");
      var node2 = CreateNode(new PointD(120, -50), new NodeStyleDecorator(nodeStyle, "Resources/switch.png"), "Switch");
      var node3 = CreateNode(new PointD(-130, 60), new NodeStyleDecorator(nodeStyle, "Resources/switch.png"), "Switch");
      var node4 = CreateNode(new PointD(95, -180), new NodeStyleDecorator(nodeStyle, "Resources/scanner.png"), "Scanner");
      var node5 = CreateNode(new PointD(240, -110), new NodeStyleDecorator(nodeStyle, "Resources/printer.png"), "Printer");
      var node6 = CreateNode(new PointD(200, 50), new NodeStyleDecorator(nodeStyle, "Resources/computer.png"), "Workstation");
      var node7 = CreateNode(new PointD(-160, -60), new NodeStyleDecorator(nodeStyle, "Resources/printer.png"), "Printer");
      var node8 = CreateNode(new PointD(-260, 40), new NodeStyleDecorator(nodeStyle, "Resources/scanner.png"), "Scanner");
      var node9 = CreateNode(new PointD(-200, 170), new NodeStyleDecorator(nodeStyle, "Resources/computer.png"), "Workstation");
      var node10 = CreateNode(new PointD(-50, 160), new NodeStyleDecorator(nodeStyle, "Resources/computer.png"), "Workstation");

      Graph.CreateEdge(node1, node2, Graph.EdgeDefaults.Style, TrafficLoad.VeryHigh);
      Graph.CreateEdge(node1, node3, Graph.EdgeDefaults.Style, TrafficLoad.High);
      Graph.CreateEdge(node2, node4, Graph.EdgeDefaults.Style, TrafficLoad.High);
      Graph.CreateEdge(node2, node5, Graph.EdgeDefaults.Style, TrafficLoad.Normal);
      Graph.CreateEdge(node2, node6, Graph.EdgeDefaults.Style, TrafficLoad.High);
      Graph.CreateEdge(node3, node7, Graph.EdgeDefaults.Style, TrafficLoad.Low);
      Graph.CreateEdge(node3, node8, Graph.EdgeDefaults.Style, TrafficLoad.Low);
      Graph.CreateEdge(node3, node9, Graph.EdgeDefaults.Style, TrafficLoad.Normal);
      Graph.CreateEdge(node3, node10, Graph.EdgeDefaults.Style, TrafficLoad.Low);
    }

    private INode CreateNode(PointD location, INodeStyle nodeStyle, string label) {
      var node = Graph.CreateNode(location, nodeStyle);
      Graph.AddLabel(node, label);
      return node;
    }

    /// <summary>
    /// Gets the currently registered <see cref="IGraph"/> instance from the <see cref="GraphControl"/>.
    /// </summary>
    public IGraph Graph {
      get { return graphControl.Graph; }
    }

    private void ReloadGraphButtonClick(object sender, EventArgs e) {
      Graph.Clear();
      CreateSampleGraph();
      graphControl.FitGraphBounds();
    }

	#region Main

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new StyleDecoratorsForm());
    }

    #endregion

  }

  /// <summary>
  /// A simple enum used in the edges' tags.
  /// </summary>
  public enum TrafficLoad
  {
    VeryHigh = 0,
    High,
    Normal,
    Low
  }
}
