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

using yWorks.Controls;
using yWorks.Controls.Input;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.LabelModels;
using yWorks.Graph.PortLocationModels;
using yWorks.Graph.Styles;

namespace Demo.yFiles.Graph.Bpmn.Styles {

  /// <summary>
  /// An <see cref="ILabelStyle"/> implementation combining an text label, an icon and a connecting line between the icon and the label owner.
  /// </summary>
  internal class ConnectedIconLabelStyle : LabelStyleBase<VisualGroup> {

    #region Properties

    public ILabelModelParameter TextPlacement { get; set; }

    public IPortLocationModelParameter LabelConnectorLocation { get; set; }

    public IPortLocationModelParameter NodeConnectorLocation { get; set; }

    public SizeD IconSize { get; set; }

    public INodeStyle IconStyle { get; set; }

    public ILabelStyle TextStyle { get; set; }

    public IEdgeStyle ConnectorStyle { get; set; }

    #endregion

    #region Initialize static fields

    private static readonly SimpleNode labelAsNode;
    private static readonly SimpleLabel dummyTextLabel;
    private static readonly SimpleEdge dummyEdge;
    private static readonly SimpleNode dummyForLabelOwner;

    static ConnectedIconLabelStyle() {
      labelAsNode = new SimpleNode();
      dummyTextLabel = new SimpleLabel(labelAsNode, "", FreeLabelModel.Instance.CreateDefaultParameter()) {Style = new DefaultLabelStyle()};

      dummyForLabelOwner = new SimpleNode();

      dummyEdge = new SimpleEdge(new SimplePort(labelAsNode, FreeNodePortLocationModel.NodeCenterAnchored),
        new SimplePort(dummyForLabelOwner, FreeNodePortLocationModel.NodeCenterAnchored))
      {
        Style = new BpmnEdgeStyle {Type = EdgeType.Association}
      };
    }

    #endregion

    /// <inheritdoc/>
    protected override VisualGroup CreateVisual(IRenderContext context, ILabel label) {

      Configure(label);
      var container = new VisualGroup();

      IVisual iconVisual = null;
      if (IconStyle != null) {
        iconVisual = IconStyle.Renderer.GetVisualCreator(labelAsNode, labelAsNode.Style).CreateVisual(context);
      }
      container.Add(iconVisual ?? new VisualGroup());

      IVisual textVisual = null;
      if (TextStyle != null && TextPlacement != null) {
        textVisual = TextStyle.Renderer.GetVisualCreator(dummyTextLabel, dummyTextLabel.Style).CreateVisual(context);
      }
      container.Add(textVisual ?? new VisualGroup());

      IVisual connectorVisual = null;
      if (ConnectorStyle != null) {
        connectorVisual = dummyEdge.Style.Renderer.GetVisualCreator(dummyEdge, dummyEdge.Style).CreateVisual(context);
      }
      container.Add(connectorVisual ?? new VisualGroup());

      return container;
    }

    /// <inheritdoc/>
    protected override SizeD GetPreferredSize(ILabel label) {
      if (IconSize != SizeD.Zero) {
        return IconSize;
      } else {
        return label.PreferredSize;
      }
    }

    /// <inheritdoc/>
    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup container, ILabel label) {
      if (container == null) {
        return CreateVisual(context, label);
      }
      Configure(label);

      IVisual oldIconVisual = container.Children[0];
      IVisual newIconVisual = null;
      if (IconStyle != null) {
        newIconVisual = IconStyle.Renderer.GetVisualCreator(labelAsNode, labelAsNode.Style)
          .UpdateVisual(context, oldIconVisual);
      }
      if (oldIconVisual != newIconVisual) {
        container.Children[0] = newIconVisual ?? new VisualGroup();
      }

      IVisual oldTextVisual = container.Children[1];
      IVisual newTextVisual = null;
      if (TextStyle != null && TextPlacement != null) {
        newTextVisual = TextStyle.Renderer.GetVisualCreator(dummyTextLabel, dummyTextLabel.Style)
          .UpdateVisual(context, oldTextVisual);
      }
      if (oldTextVisual != newTextVisual) {
        container.Children[1] = newTextVisual ?? new VisualGroup();
      }

      IVisual oldConnectorVisual = container.Children[2];
      IVisual newConnectorVisual = null;
      if (ConnectorStyle != null) {
        newConnectorVisual = dummyEdge.Style.Renderer.GetVisualCreator(dummyEdge, dummyEdge.Style)
          .UpdateVisual(context, oldConnectorVisual);
      }
      if (oldConnectorVisual != newConnectorVisual) {
        container.Children[2] = newConnectorVisual ?? new VisualGroup();
      }

      return container;
    }

    /// <inheritdoc/>
    private void Configure(ILabel item) {
      labelAsNode.Style = IconStyle;
      labelAsNode.Layout = item.GetLayout().GetBounds();

      var nodeOwner = item.Owner as INode;
      if (nodeOwner != null) {
        dummyForLabelOwner.Style = nodeOwner.Style;
        dummyForLabelOwner.Layout = nodeOwner.Layout;
      }

      dummyTextLabel.Style = TextStyle;
      dummyTextLabel.LayoutParameter = TextPlacement;
      dummyTextLabel.Text = item.Text;
      dummyTextLabel.PreferredSize = dummyTextLabel.Style.Renderer.GetPreferredSize(dummyTextLabel, dummyTextLabel.Style);
      TextBounds = TextPlacement.Model.GetGeometry(dummyTextLabel, TextPlacement);

      BoundingBox = item.GetLayout().GetBounds() + TextBounds.GetBounds();

      // Set source port to the port of the node using a dummy node that is located at the origin.
      ((SimplePort) dummyEdge.SourcePort).LocationParameter = LabelConnectorLocation;
      ((SimplePort) dummyEdge.TargetPort).LocationParameter = NodeConnectorLocation;
    }

    private IOrientedRectangle TextBounds;

    private RectD BoundingBox;

    /// <inheritdoc/>
    protected override bool IsHit(IInputModeContext context, PointD location, ILabel label) {
      Configure(label);
      return label.GetLayout().Contains(location, context.HitTestRadius)
             || TextBounds.Contains(location, context.HitTestRadius)
             || dummyEdge.Style.Renderer.GetHitTestable(dummyEdge, dummyEdge.Style).IsHit(context, location);
    }

    /// <inheritdoc/>
    protected override bool IsInBox(IInputModeContext context, RectD rectangle, ILabel label) {
      Configure(label);
      return rectangle.Intersects(BoundingBox.GetEnlarged(context.HitTestRadius));
    }

    /// <inheritdoc/>
    protected override RectD GetBounds(ICanvasContext context, ILabel label) {
      return BoundingBox +
             dummyEdge.Style.Renderer.GetBoundsProvider(dummyEdge, dummyEdge.Style).GetBounds(context);
    }

    /// <inheritdoc/>
    protected override bool IsVisible(ICanvasContext context, RectD rectangle, ILabel label) {
      // We're computing a (very generous) bounding box here because relying on GetBounds does not work.
      // The visibility test does not call Configure, which means we don't have the dummy edge set up yet.
      var ownerNode = label.Owner as INode;
      if (ownerNode != null) {
        return rectangle.Intersects(BoundingBox + ownerNode.Layout.ToRectD());
      }
      return rectangle.Intersects(BoundingBox);
    }

  }
}
