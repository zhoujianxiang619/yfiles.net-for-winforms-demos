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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Demo.yFiles.Graph.Collapse.Properties;
using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;
using yWorks.Layout.Organic;
using yWorks.Layout.Tree;

namespace Demo.yFiles.Graph.Collapse
{
  /// <summary>
  /// A form that demonstrates the wrapping and decorating of <see cref="IGraph"/> instances.
  /// </summary>
  /// <remarks>
  /// This demo shows a collapsible tree structure. Subtrees can be collapsed or expanded by clicking on 
  /// their root nodes.
  /// </remarks>
  /// <seealso cref="FilteredGraphWrapper"/>
  public partial class GraphCollapseForm : Form
  {
    // list that stores collapsed nodes
    private readonly ICollection<INode> collapsedNodes = new List<INode>();
    // graph that contains visible nodes
    private FilteredGraphWrapper filteredGraph;
    // graph containing all nodes
    private DefaultGraph fullGraph;
    private readonly INodeStyle leafNodeStyle = new LeafNodeStyle();
    // currently selected layouter
    private ILayoutAlgorithm currentLayouter;
    // list of all layouts
    private readonly List<ILayoutAlgorithm> layouts = new List<ILayoutAlgorithm>();
    // mapper for mapping layouts to their string representation in the combobox
    private IMapper<string, ILayoutAlgorithm> layouterMapper = new DictionaryMapper<string, ILayoutAlgorithm>();
    // the node that has just been toggled and should stay fixed.
    private INode toggledNode;

    /// <summary>
    /// Returns all available layouts.
    /// </summary>
    public List<ILayoutAlgorithm> Layouts {
      get { return layouts; }
    }

    /// <summary>
    /// Create a new instance of this class.
    /// </summary>
    public GraphCollapseForm() {
      InitializeComponent();
      // assign zoom commands
      ZoomInButton.SetCommand(Commands.IncreaseZoom, graphControl);
      ZoomOutButton.SetCommand((Commands.DecreaseZoom), graphControl);
      FitContentButton.SetCommand(Commands.FitContent, graphControl);
      // load description
      try {
        descriptionTextBox.LoadFile(new MemoryStream(Resources.description), RichTextBoxStreamType.RichText);
      } catch (MissingMethodException) {
        // Workaround for https://github.com/microsoft/msbuild/issues/4581
        descriptionTextBox.Text = "The description is not available with this version of .NET Core.";
      }
    }

    #region Handling Expand/Collapse Clicks

    /// <summary>
    /// The command that can be used by the node buttons to toggle the visibilit of the child nodes.
    /// </summary>
    /// <remarks>
    /// This command requires the corresponding <see cref="INode"/> as the <see cref="ExecutedCommandEventArgs.Parameter"/>.
    /// </remarks>
    public static readonly ICommand ToggleChildrenCommand = Commands.CreateCommand("Toggle Children");

    
    /// <summary>
    /// Called when the ToggleChildren command has been executed. 
    /// </summary>
    /// <remarks>
    /// Toggles the visiblity of the node's children.
    /// </remarks>
    public void ToggleChildrenExecuted(object sender, ExecutedCommandEventArgs e) {
      var node = (e.Parameter ?? graphControl.CurrentItem) as INode;
      if (node != null) {
        bool canExpand = filteredGraph.OutDegree(node) != filteredGraph.WrappedGraph.OutDegree(node);
        if (canExpand) {
          Expand(node);
        } else {
          Collapse(node);
        }
      }
    }

    /// <summary>
    /// Show the children of a collapsed node.
    /// </summary>
    /// <param name="node">The node that should be expanded</param>
    private void Expand(INode node) {
      if (collapsedNodes.Contains(node)) {
        toggledNode = node;
        SetCollapsedTag(node, false);
        AlignChildren(node);
        collapsedNodes.Remove(node);
        filteredGraph.NodePredicateChanged();
        RunLayout(false);
      }
    }

    /// <summary>
    /// Hide the children of a expanded node.
    /// </summary>
    /// <param name="node">The node that should be collapsed</param>
    private void Collapse(INode node) {
      if (!collapsedNodes.Contains(node)) {
        toggledNode = node;
        SetCollapsedTag(node, true);
        collapsedNodes.Add(node);
        filteredGraph.NodePredicateChanged();
        RunLayout(false);
      }
    }

    private void AlignChildren(INode node) {
      // This method is used to set the initial positions of the children
      // of a node which gets expanded to the position of the expanded node.
      // This looks nicer in the following animated layout. Try commenting
      // out the method body to see the difference.
      PointD center = node.Layout.GetCenter();
      foreach (IEdge edge in fullGraph.EdgesAt(node)) {
        if (edge.SourcePort.Owner == node) {
          fullGraph.ClearBends(edge);
          INode child = (INode)edge.TargetPort.Owner;
          fullGraph.SetNodeCenter(child, center);
          AlignChildren(child);
        }
      }
    }

    private void SetCollapsedTag(INode node, bool collapsed) {
      var style = node.Style as CollapseNodeStyle;
      if (style != null) {
        style.CollapsedState = collapsed ? CollapsedState.Collapsed : CollapsedState.Expanded;
      }
    }
    
    #endregion

    /// <summary>
    /// Builds a sample graph
    /// </summary>
    private void BuildTree(IGraph graph, int children, int levels, int collapseLevel) {
      INode root = graph.CreateNode(new PointD(20, 20));
      SetCollapsedTag(root, false);
      AddChildren(levels, graph, root, children, collapseLevel);
    }

    private readonly Random random = new Random(666);

    /// <summary>
    /// Recusively add children to the tree
    /// </summary>
    private void AddChildren(int level, IGraph graph, INode root, int childCount, int collapseLevel) {
      int actualChildCount = random.Next(1, childCount + 1);
      for (int i = 0; i < actualChildCount; i++) {
        INode child = graph.CreateNode(new PointD(20, 20));
        graph.CreateEdge(root, child);
        if (level < collapseLevel) {
          collapsedNodes.Add(child);
          SetCollapsedTag(child, true);
        }
        else {
          SetCollapsedTag(child, false);
        }
        if (level > 0) {
          AddChildren(level - 1, graph, child, 4, 2);
        } else {
          graph.SetStyle(child, leafNodeStyle);
        }
      }
    }

    private bool EdgePredicate(IEdge obj) {
      // return true for any edge
      return true;
    }

    /// <summary>
    /// Predicate for the filtered graph wrapper that 
    /// indicates whether a node should be visible
    /// </summary>
    /// <returns> true if the node should be visible</returns>
    private bool NodePredicate(INode node) {
      // return true if none of the parent nodes is collapsed
      foreach (IEdge edge in fullGraph.InEdgesAt(node)) {
        INode parent = (INode)edge.SourcePort.Owner;
        return !collapsedNodes.Contains(parent) && NodePredicate(parent);
      }
      return true;
    }

    #region Initialization

    /// <summary>
    /// Called upon the loading of the form.
    /// This method initializes the graph and the input mode.
    /// </summary>
    /// <param name="e"></param>
    /// <seealso cref="InitializeInputModes"/>
    /// <seealso cref="InitializeGraph"/>
    protected override void OnLoad(EventArgs e) {
      base.OnLoad(e);

      // initialize the input mode
      InitializeInputModes();

      // initialize the graph
      InitializeGraph();

      layouterComboBox.SelectedIndex = 0;
    }

    /// <summary>
    /// Initializes the graph instance, setting default styles
    /// and creating a small sample graph.
    /// </summary>
    protected virtual void InitializeGraph() {
      // Create the graph instance that will hold the complete graph.
      fullGraph = new DefaultGraph();

      // Create a nice default style for the nodes
      CollapseNodeStyle style = new CollapseNodeStyle();
      fullGraph.NodeDefaults.Style = style;
      fullGraph.NodeDefaults.Size = new SizeD(60, 30);
      fullGraph.NodeDefaults.ShareStyleInstance = false;


      // and a style for the labels
      DefaultLabelStyle labelStyle = new DefaultLabelStyle();
      Font font = new Font(FontFamily.GenericSansSerif, 12, GraphicsUnit.Pixel);
      labelStyle.Font = font;
      fullGraph.NodeDefaults.Labels.Style = labelStyle;


      // now build a simple sample tree
      BuildTree(fullGraph, 3, 3, 3);

      // create a view of the graph that contains only non-collapsed subtrees.
      // use a predicate method to decide what nodes should be part of the graph.
      filteredGraph =
        new FilteredGraphWrapper(fullGraph, NodePredicate, EdgePredicate);

      // display the filtered graph in our control.
      graphControl.Graph = filteredGraph;
      // center the graph to prevent the initial layout fading in from the top left corner
      graphControl.FitGraphBounds();

      // create layouts
      SetupLayouters();
      // calculate and run the initial layout.
      RunLayout(true);
    }

    /// <summary>
    /// Creates a mode and registers it as the
    /// <see cref="CanvasControl.InputMode"/>.
    /// </summary>
    protected virtual void InitializeInputModes() {
      // create a simple mode that reacts to mouse clicks on nodes.
      var graphViewerInputMode = new GraphViewerInputMode();
      graphViewerInputMode.ClickableItems = GraphItemTypes.Node;
      graphViewerInputMode.ItemClicked += delegate(object sender, ItemClickedEventArgs<IModelItem> args) {
        if (args.Item is INode) {
          // toggle the collapsed state of the clicked node
          ToggleChildrenCommand.Execute((INode) args.Item, graphControl);
        }
      };
			graphViewerInputMode.KeyboardInputMode.AddCommandBinding(ToggleChildrenCommand, ToggleChildrenExecuted);
      graphControl.InputMode = graphViewerInputMode;
    }

    private void SetupLayouters() {
      // create TreeLayout
      var treeLayout = new TreeLayout();
      treeLayout.PrependStage(new FixNodeLayoutStage());
      layouts.Add(treeLayout);
      layouterMapper["Tree"] = treeLayout;
      layouterComboBox.Items.Add("Tree");

      // set it as initial value
      currentLayouter = treeLayout;

      // create BalloonLayout
      var balloonLayouter = new BalloonLayout
      {
        FromSketchMode = true,
        CompactnessFactor = 1.0,
        AllowOverlaps = true
      };
      balloonLayouter.PrependStage(new FixNodeLayoutStage());
      layouts.Add(balloonLayouter);
      layouterMapper["Balloon"] = balloonLayouter;
      layouterComboBox.Items.Add("Balloon");

      // create OrganicLayouter
      var organicLayouter = new ClassicOrganicLayout { InitialPlacement = InitialPlacement.AsIs };
      organicLayouter.PrependStage(new FixNodeLayoutStage());
      layouts.Add(organicLayouter);
      layouterMapper["Organic"] = organicLayouter;
      layouterComboBox.Items.Add("Organic");
    }

    #endregion

    #region Actions

    /// <summary>
    /// Exit the demo
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>            
    private void ExitMenuItem_Click(object sender, EventArgs e) {
      Application.Exit();
    }

    #endregion

    private async void RunLayout(bool animateViewport) {
      if (currentLayouter != null) {
        // provide additional data to configure the FixNodeLayoutStage
        FixNodeLayoutData fixNodeLayoutData = new FixNodeLayoutData();
        // specify the node whose position is to be fixed during layout
        fixNodeLayoutData.FixedNodes.Item = toggledNode;

        var layoutExecutor = new LayoutExecutor(graphControl, currentLayouter)
        {
          UpdateContentRect = true,
          AnimateViewport = animateViewport,
          Duration = TimeSpan.FromSeconds(0.3d),
          LayoutData = fixNodeLayoutData
        };

        await layoutExecutor.Start();

        toggledNode = null;
      }
    }

    private void layouterComboBox_SelectedIndexChanged(object sender, EventArgs e) {
      currentLayouter = layouterMapper[layouterComboBox.SelectedItem as string];
      RunLayout(true);
    }

    #region Main

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main() {
      Application.EnableVisualStyles();
      Application.Run(new GraphCollapseForm());
    }

    #endregion

  }

  /// <summary>
  /// Enum that holds the collapse states
  /// </summary>
  public enum CollapsedState
  {
    Unknown = 0,
    Collapsed = 1,
    Expanded = 2,
  }
}