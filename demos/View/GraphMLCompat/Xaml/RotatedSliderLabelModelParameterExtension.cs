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
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using yWorks.Graph.LabelModels;

namespace Demo.yFiles.IO.GraphML.Compat.Xaml {
  [Obfuscation(StripAfterObfuscation = false, Exclude = true, ApplyToMembers = true)]
  public sealed class RotatedSliderLabelModelParameterExtension : MarkupExtension
  {

    public RotatedSliderLabelModelParameterExtension() {
      SegmentRatio = 0.5;
      Location = SliderParameterLocation.FromSource;
    }


    [DefaultValue(null)]
    public ILabelModel Model { get; set; }


    [DefaultValue(SliderParameterLocation.FromSource)]
    public SliderParameterLocation Location { get; set; }


    [DefaultValue(0)]
    public int SegmentIndex { get; set; }


    [DefaultValue(0.5d)]
    public double SegmentRatio { get; set; }



    public override object ProvideValue(IServiceProvider serviceProvider) {
      RotatedSliderEdgeLabelModel exModel = Model as RotatedSliderEdgeLabelModel ?? new RotatedSliderEdgeLabelModel();

      bool fromSource = (Location & SliderParameterLocation.FromSource) == SliderParameterLocation.FromSource;
      if (fromSource) {
        return exModel.CreateParameterFromSource(SegmentIndex, SegmentRatio);
      } else {
        return exModel.CreateParameterFromTarget(SegmentIndex, SegmentRatio);
      }
    }
  }
}