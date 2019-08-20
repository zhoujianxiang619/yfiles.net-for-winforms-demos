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
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Markup;
using Demo.yFiles.Graph.SelectionStyling.Properties;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.Styles;

[assembly : XmlnsDefinition("http://www.yworks.com/yFiles.net/demos/SelectionStyling/1.0", "Demo.yFiles.Graph.SelectionStyling")]
[assembly: XmlnsPrefix("http://www.yworks.com/yFiles.net/demos/SelectionStyling/1.0", "demo")]

namespace Demo.yFiles.Graph.SelectionStyling
{
  /// <summary>
  /// Demonstrates customized selecting painting of nodes, edges and labels by decorating these items with a corresponding style.
  /// </summary>
  public partial class SelectionStylingForm : Form
  {
    private IContextLookupChainLink nodeDecorationLookupChainLink;
    private IContextLookupChainLink edgeDecorationLookupChainLink;
    private IContextLookupChainLink labelDecorationLookupChainLink;
    private NodeStyleDecorationInstaller nodeDecorationInstaller;
    private EdgeStyleDecorationInstaller edgeDecorationInstaller;
    private LabelStyleDecorationInstaller labelDecorationInstaller;
    private StyleDecorationZoomPolicy[] styleDecorationZoomPolicies;

    /// <summary>
    /// Automatically generated by Visual Studio.
    /// Wires up the UI components and adds a 
    /// <see cref="GraphControl"/> to the form.
    /// </summary>
    public SelectionStylingForm() {
      InitializeComponent();
      graphControl.FileOperationsEnabled = true;
      zoomInButton.SetCommand(Commands.IncreaseZoom, graphControl);
      zoomOutButton.SetCommand(Commands.DecreaseZoom, graphControl);
      resetZoomButton.SetCommand(Commands.Zoom, 1, graphControl);
      zoomFitButton.SetCommand(Commands.FitContent, graphControl);

      saveButton.SetCommand(Commands.SaveAs, graphControl);
      openButton.SetCommand(Commands.Open, graphControl);
    }

    /// <summary>
    /// Called upon loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    protected override void OnLoad(EventArgs e) {
      base.OnLoad(e);
      try {
        description.LoadFile(new MemoryStream(Resources.description), RichTextBoxStreamType.RichText);
      } catch (MissingMethodException) {
        // Workaround for https://github.com/microsoft/msbuild/issues/4581
        description.Text = "The description is not available with this version of .NET Core.";
      }
      // initialize default graph styles
      var graph = graphControl.Graph;
      graph.NodeDefaults.Style = new ShinyPlateNodeStyle { Brush = Brushes.DarkOrange };
      graph.NodeDefaults.Size = new SizeD(50, 30);

      graph.NodeDefaults.Labels.Style = new MySimpleLabelStyle();
      graph.NodeDefaults.Labels.LayoutParameter =
        new ExteriorLabelModel {Insets = new InsetsD(15)}.CreateParameter(ExteriorLabelModel.Position.North);
      graph.EdgeDefaults.Labels.Style = new MySimpleLabelStyle();

      // initialize the graph
      graphControl.ImportFromGraphML("Resources\\SelectionStyling.graphml");

      // initialize the input mode
      // disable label editing, since this would be really confusing in combination with our style.
      var graphEditorInputMode = new GraphEditorInputMode
                                   {
                                     AllowEditLabel = true,
                                     SnapContext = new GraphSnapContext()
                                   };
      graphControl.InputMode = graphEditorInputMode;

      InitializeDecoration();
      UpdateDecoration();

      SelectAllNodes(graphEditorInputMode);
      SelectAllLabels(graphEditorInputMode);
    }

    private void InitializeDecoration() {
      nodeDecorationInstaller = new NodeStyleDecorationInstaller
      {
        NodeStyle = new ShapeNodeStyle {Shape = ShapeNodeShape.Rectangle, Pen = Pens.DeepSkyBlue, Brush = Brushes.Transparent},
        Margins = new InsetsD(10.0)
      };
      edgeDecorationInstaller = new EdgeStyleDecorationInstaller
      {
        EdgeStyle = new PolylineEdgeStyle {Pen = new Pen(Brushes.DeepSkyBlue, 3)}
      };
      labelDecorationInstaller = new LabelStyleDecorationInstaller
      {
        LabelStyle = new NodeStyleLabelStyleAdapter(
          new ShapeNodeStyle { Shape = ShapeNodeShape.RoundRectangle, Pen = Pens.DeepSkyBlue, Brush = Brushes.Transparent },
          VoidLabelStyle.Instance),
        Margins = new InsetsD(5.0)
      };

      styleDecorationZoomPolicies = new[]
                              {
                                StyleDecorationZoomPolicy.Mixed,
                                StyleDecorationZoomPolicy.WorldCoordinates,
                                StyleDecorationZoomPolicy.ViewCoordinates
                              };
      zoomModeComboBox.ComboBox.DataSource = styleDecorationZoomPolicies;
      zoomModeComboBox.SelectedIndex = 0;
    }

    /// <summary>
    /// Sets, removes and updates the custom selection decoration for nodes,
    /// edges, and labels according to the current settings.
    /// </summary>
    public void UpdateDecoration() {
      var selectedZoomMode = (StyleDecorationZoomPolicy) zoomModeComboBox.SelectedItem;
      nodeDecorationInstaller.ZoomPolicy = selectedZoomMode;
      edgeDecorationInstaller.ZoomPolicy = selectedZoomMode;
      labelDecorationInstaller.ZoomPolicy = selectedZoomMode;

      var graphDecorator = graphControl.Graph.GetDecorator();
      var nodeDecorator = graphDecorator.NodeDecorator;
      var edgeDecorator = graphDecorator.EdgeDecorator;
      var labelDecorator = graphDecorator.LabelDecorator;

      if (!IsChecked(CustomNodeDecoratorButton) && nodeDecorationLookupChainLink != null) {
        nodeDecorator.Remove(nodeDecorationLookupChainLink);
        nodeDecorationLookupChainLink = null;
      } else if (IsChecked(CustomNodeDecoratorButton) && nodeDecorationLookupChainLink == null) {
        nodeDecorationLookupChainLink = nodeDecorator.SelectionDecorator.SetFactory(node => nodeDecorationInstaller);
      }
      if (!IsChecked(CustomEdgeDecoratorButton) && edgeDecorationLookupChainLink != null) {
        edgeDecorator.Remove(edgeDecorationLookupChainLink);
        edgeDecorationLookupChainLink = null;
      } else if (IsChecked(CustomEdgeDecoratorButton) && edgeDecorationLookupChainLink == null) {
        edgeDecorationLookupChainLink = edgeDecorator.SelectionDecorator.SetFactory(edge => edgeDecorationInstaller);
      }
      if (!IsChecked(CustomLabelDecoratorButton) && labelDecorationLookupChainLink != null) {
        labelDecorator.Remove(labelDecorationLookupChainLink);
        labelDecorationLookupChainLink = null;
      } else if (IsChecked(CustomLabelDecoratorButton) && labelDecorationLookupChainLink == null) {
        labelDecorationLookupChainLink = labelDecorator.SelectionDecorator.SetFactory(label => labelDecorationInstaller);
      }
    }

    private static bool IsChecked(ToolStripButton button) {
      return button.Checked;
    }

    private void SelectAllNodes(GraphEditorInputMode inputMode) {
      foreach (var node in graphControl.Graph.Nodes) {
        inputMode.SetSelected(node, true);
      }
    }

    private void SelectAllEdges(GraphEditorInputMode inputMode) {
      foreach (var edge in graphControl.Graph.Edges) {
        inputMode.SetSelected(edge, true);
      }
    }

    private void SelectAllLabels(GraphEditorInputMode inputMode) {
      foreach (var label in graphControl.Graph.Labels) {
        inputMode.SetSelected(label, true);
      }
    }

    #region UI event handler

    private void CustomNodeDecorationChanged(object sender, EventArgs e) {
      UpdateDecoration();
      var inputMode = ((GraphEditorInputMode) graphControl.InputMode);
      inputMode.ClearSelection();
      SelectAllNodes(inputMode);
    }

    private void CustomEdgeDecorationChanged(object sender, EventArgs e) {
      UpdateDecoration();
      var inputMode = ((GraphEditorInputMode) graphControl.InputMode);
      inputMode.ClearSelection();
      SelectAllEdges(inputMode);
    }

    private void CustomLabelDecorationChanged(object sender, EventArgs e) {
      UpdateDecoration();
      var inputMode = ((GraphEditorInputMode) graphControl.InputMode);
      inputMode.ClearSelection();
      SelectAllLabels(inputMode);
    }

    private void ZoomModeChanged(object sender, EventArgs e) {
      UpdateDecoration();
      graphControl.Invalidate();
    }

    #endregion

    #region Main

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new SelectionStylingForm());
    }

    #endregion
  }
}
