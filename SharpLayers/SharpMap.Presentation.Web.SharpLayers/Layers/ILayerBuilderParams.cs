﻿/*
 *  The attached / following is part of SharpMap.Presentation.Web.SharpLayers
 *  SharpMap.Presentation.Web.SharpLayers is free software © 2008 Newgrove Consultants Limited, 
 *  www.newgrove.com; you can redistribute it and/or modify it under the terms 
 *  of the current GNU Lesser General Public License (LGPL) as published by and 
 *  available from the Free Software Foundation, Inc., 
 *  59 Temple Place, Suite 330, Boston, MA 02111-1307 USA: http://fsf.org/    
 *  This program is distributed without any warranty; 
 *  without even the implied warranty of merchantability or fitness for purpose.  
 *  See the GNU Lesser General Public License for the full details. 
 *  
 *  Author: John Diss 2008
 * 
 */
namespace SharpMap.Presentation.Web.SharpLayers.Layers
{
    public interface ILayerBuilderParams : IBuilderParams
    {
        bool IsBaseLayer { get; set; }

        bool DisplayInLayerSwitcher { get; set; }

        bool Visibility { get; set; }

        MapUnits Units { get; set; }

        Bounds MaxExtent { get; set; }

        Bounds MinExtent { get; set; }

        bool DisplayOutsideMaxExtent { get; set; }

        bool WrapDateLine { get; set; }

        string Attribution { get; set; }

        bool AlwaysInRange { get; set; }

        double? MinScale { get; set; }

        CollectionBase<DoubleValue> Resolutions { get; }
        CollectionBase<DoubleValue> Scales { get; }

        double? MaxScale { get; set; }
        double? MinResolution { get; set; }
        double? MaxResolution { get; set; }
        int? Gutter { get; set; }
    }
}