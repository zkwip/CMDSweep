﻿using CMDSweep.Geometry;
using CMDSweep.Rendering;

namespace CMDSweep.Layout.Popup;

internal interface IPopup
{

    public Dimensions ContentDimensions { get; }

    public void RenderContent(Rectangle bounds, IRenderer renderer);
}