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

using System.Drawing;
using System.Linq;
using System.Reflection;
using yWorks.Controls;
using yWorks.Geometry;
using yWorks.Graph;
using yWorks.Graph.Styles;
using yWorks.Layout;

namespace Demo.yFiles.Graph.Bpmn.Styles {
  /// <summary>
  /// Custom stripe style that alternates the visualizations for the leaf nodes and uses a different style for all parent stripes.
  /// </summary>
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = false)]
  public class AlternatingLeafStripeStyle : StripeStyleBase<VisualGroup> {
    /// <summary>
    /// Visualization for all leaf stripes that have an even index
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public StripeDescriptor EvenLeafDescriptor { get; set; }

    /// <summary>
    /// Visualization for all stripes that are not leafs
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public StripeDescriptor ParentDescriptor { get; set; }

    /// <summary>
    /// Visualization for all leaf stripes that have an odd index
    /// </summary>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    public StripeDescriptor OddLeafDescriptor { get; set; }

    #region IVisualCreator methods

    ///<inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    protected override VisualGroup CreateVisual(IRenderContext context, IStripe stripe) {
      var layout = stripe.Layout.ToRectD();
      var cc = new VisualGroup();
      InsetsD stripeInsets;

      StripeDescriptor descriptor;
      //Depending on the stripe type, we need to consider horizontal or vertical insets
      if (stripe is IColumn) {
        var col = (IColumn) stripe;
        var actualInsets = col.GetActualInsets();
        stripeInsets = new InsetsD(0, actualInsets.Top, 0, actualInsets.Bottom);
      } else {
        var row = (IRow) stripe;
        var actualInsets = row.GetActualInsets();
        stripeInsets = new InsetsD(actualInsets.Left, 0, actualInsets.Right, 0);
      }

      if (stripe.GetChildStripes().Any()) {
        //Parent stripe - use the parent descriptor
        descriptor = ParentDescriptor;
      } else {
        int index;
        if (stripe is IColumn) {
          var col = (IColumn) stripe;
          //Get all leaf columns
          var leafs = col.Table.RootColumn.GetLeaves().ToList();
          //Determine the index
          index = leafs.FindIndex(curr => col == curr);
          //Use the correct descriptor
          descriptor = index%2 == 0 ? EvenLeafDescriptor : OddLeafDescriptor;
        } else {
          var row = (IRow) stripe;
          var leafs = row.Table.RootRow.GetLeaves().ToList();
          index = leafs.FindIndex((curr) => row == curr);
          descriptor = index%2 == 0 ? EvenLeafDescriptor : OddLeafDescriptor;
        }
      }
      cc.Add(new RectangleVisual(layout) {Brush = descriptor.BackgroundBrush});
      //Draw the insets
      if (stripeInsets.Left > 0) {
        cc.Add(new RectangleVisual(layout.X, layout.Y, stripeInsets.Left, layout.Height) {Brush = descriptor.InsetBrush});
      }
      if (stripeInsets.Top > 0) {
        cc.Add(new RectangleVisual(layout.X, layout.Y, layout.Width, stripeInsets.Top) {Brush = descriptor.InsetBrush});
      }
      if (stripeInsets.Right > 0) {
        cc.Add(new RectangleVisual(layout.MaxX - stripeInsets.Right, layout.Y, stripeInsets.Right, layout.Height) { Brush = descriptor.InsetBrush });
      }
      if (stripeInsets.Bottom > 0) {
        cc.Add(new RectangleVisual(layout.X, layout.MaxY - stripeInsets.Bottom, layout.Width, stripeInsets.Bottom) { Brush = descriptor.InsetBrush });
      }
      cc.Add(new RectangleVisual(layout) {Pen = new Pen(descriptor.BorderBrush, descriptor.BorderThickness)});
      return cc;
    }

    ///<inheritdoc/>
    [Obfuscation(StripAfterObfuscation = false, Exclude = true)]
    protected override VisualGroup UpdateVisual(IRenderContext context, VisualGroup cc, IStripe stripe) {
      if (cc == null || cc.Children.Count < 2) {
        return CreateVisual(context, stripe);
      }
      var layout = stripe.Layout.ToRectD();
      InsetsD stripeInsets;

      StripeDescriptor descriptor;
      //Depending on the stripe type, we need to consider horizontal or vertical insets
      if (stripe is IColumn) {
        var col = (IColumn) stripe;
        var actualInsets = col.GetActualInsets();
        stripeInsets = new InsetsD(0, actualInsets.Top, 0, actualInsets.Bottom);
      } else {
        var row = (IRow) stripe;
        var actualInsets = row.GetActualInsets();
        stripeInsets = new InsetsD(actualInsets.Left, 0, actualInsets.Right, 0);
      }

      if (stripe.GetChildStripes().Any()) {
        //Parent stripe - use the parent descriptor
        descriptor = ParentDescriptor;
      } else {
        int index;
        if (stripe is IColumn) {
          var col = (IColumn) stripe;
          //Get all leaf columns
          var leafs = col.Table.RootColumn.GetLeaves().ToList();
          //Determine the index
          index = leafs.FindIndex(curr => col == curr);
          //Use the correct descriptor
          descriptor = index%2 == 0 ? EvenLeafDescriptor : OddLeafDescriptor;
        } else {
          var row = (IRow) stripe;
          var leafs = row.Table.RootRow.GetLeaves().ToList();
          index = leafs.FindIndex((curr) => row == curr);
          descriptor = index%2 == 0 ? EvenLeafDescriptor : OddLeafDescriptor;
        }
      }
      RectangleVisual rect = (RectangleVisual)cc.Children[0];
      rect.Rectangle = layout;
      rect.Brush = descriptor.BackgroundBrush;
      var childIndex = 1;
      //Draw the insets
      if (stripeInsets.Left > 0) {
        if (childIndex < cc.Children.Count) {
          rect = (RectangleVisual) cc.Children[childIndex];
          rect.Brush = descriptor.InsetBrush;
          rect.Rectangle = new RectD(layout.X, layout.Y, stripeInsets.Left, layout.Height);
        } else {
          cc.Add(new RectangleVisual(layout.X, layout.Y, stripeInsets.Left, layout.Height) {
            Brush = descriptor.InsetBrush
          });
        }
        childIndex++;
      }
      if (stripeInsets.Top > 0) {
        if (childIndex < cc.Children.Count) {
          rect = (RectangleVisual) cc.Children[childIndex];
          rect.Brush = descriptor.InsetBrush;
          rect.Rectangle = new RectD(layout.X, layout.Y, layout.Width, stripeInsets.Top);
        } else {
          cc.Add(new RectangleVisual(layout.X, layout.Y, layout.Width, stripeInsets.Top) {Brush = descriptor.InsetBrush});
        }
        childIndex++;
      }
      if (stripeInsets.Right > 0) {
        if (childIndex < cc.Children.Count) {
          rect = (RectangleVisual) cc.Children[childIndex];
          rect.Brush = descriptor.InsetBrush;
          rect.Rectangle = new RectD(layout.MaxX - stripeInsets.Right, layout.Y, stripeInsets.Right, layout.Height);
        } else {
          cc.Add(new RectangleVisual(layout.MaxX - stripeInsets.Right, layout.Y, stripeInsets.Right, layout.Height) {
            Brush = descriptor.InsetBrush
          });
        }
        childIndex++;
      }
      if (stripeInsets.Bottom > 0) {
        if (childIndex < cc.Children.Count) {
          rect = (RectangleVisual) cc.Children[childIndex];
          rect.Brush = descriptor.InsetBrush;
          rect.Rectangle = new RectD(layout.X, layout.MaxY - stripeInsets.Bottom, layout.Width, stripeInsets.Bottom);
        } else {
          cc.Add(new RectangleVisual(layout.X, layout.MaxY - stripeInsets.Bottom, layout.Width, stripeInsets.Bottom) {
            Brush = descriptor.InsetBrush
          });
        }
        childIndex++;
      }
      if (childIndex < cc.Children.Count) {
        rect = (RectangleVisual) cc.Children[childIndex];
        rect.Pen = new Pen(descriptor.BorderBrush, descriptor.BorderThickness);
        rect.Rectangle = layout;
      } else {
        cc.Add(new RectangleVisual(layout) {Pen = new Pen(descriptor.BorderBrush, descriptor.BorderThickness)});
      }
      childIndex++;
      while (cc.Children.Count > childIndex) {
        cc.Children.RemoveAt(childIndex);
      }
      return cc;
    }

    #endregion

  }
}
