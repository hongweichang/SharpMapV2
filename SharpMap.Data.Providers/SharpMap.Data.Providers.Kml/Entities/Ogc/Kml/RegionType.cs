// /*
//  *  The attached / following is part of SharpMap.Data.Providers.Kml
//  *  SharpMap.Data.Providers.Kml is free software � 2008 Newgrove Consultants Limited, 
//  *  www.newgrove.com; you can redistribute it and/or modify it under the terms 
//  *  of the current GNU Lesser General Public License (LGPL) as published by and 
//  *  available from the Free Software Foundation, Inc., 
//  *  59 Temple Place, Suite 330, Boston, MA 02111-1307 USA: http://fsf.org/    
//  *  This program is distributed without any warranty; 
//  *  without even the implied warranty of merchantability or fitness for purpose.  
//  *  See the GNU Lesser General Public License for the full details. 
//  *  
//  *  Author: John Diss 2009
//  * 
//  */
using System;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SharpMap.Entities.Ogc.Kml
{
    [XmlType(TypeName = "RegionType", Namespace = Declarations.SchemaVersion), Serializable]
    [XmlInclude(typeof (DataType))]
    [XmlInclude(typeof (AbstractTimePrimitiveType))]
    [XmlInclude(typeof (SchemaDataType))]
    [XmlInclude(typeof (ItemIconType))]
    [XmlInclude(typeof (AbstractLatLonBoxType))]
    [XmlInclude(typeof (OrientationType))]
    [XmlInclude(typeof (AbstractStyleSelectorType))]
    [XmlInclude(typeof (ResourceMapType))]
    [XmlInclude(typeof (LocationType))]
    [XmlInclude(typeof (AbstractSubStyleType))]
    [XmlInclude(typeof (RegionType))]
    [XmlInclude(typeof (AliasType))]
    [XmlInclude(typeof (AbstractViewType))]
    [XmlInclude(typeof (AbstractFeatureType))]
    [XmlInclude(typeof (AbstractGeometryType))]
    [XmlInclude(typeof (BasicLinkType))]
    [XmlInclude(typeof (PairType))]
    [XmlInclude(typeof (ImagePyramidType))]
    [XmlInclude(typeof (ScaleType))]
    [XmlInclude(typeof (LodType))]
    [XmlInclude(typeof (ViewVolumeType))]
    public class RegionType : AbstractObjectType
    {
        [XmlIgnore] private LatLonAltBox _latLonAltBox;

        [XmlIgnore] private Lod _lod;
        [XmlIgnore] private List<RegionObjectExtensionGroup> _regionObjectExtensionGroup;

        [XmlIgnore] private List<string> _regionSimpleExtensionGroup;

        [XmlElement(Type = typeof (LatLonAltBox), ElementName = "LatLonAltBox", IsNullable = false,
            Form = XmlSchemaForm.Qualified, Namespace = Declarations.SchemaVersion)]
        public LatLonAltBox LatLonAltBox
        {
            get { return _latLonAltBox; }
            set { _latLonAltBox = value; }
        }

        [XmlElement(Type = typeof (Lod), ElementName = "Lod", IsNullable = false, Form = XmlSchemaForm.Qualified,
            Namespace = Declarations.SchemaVersion)]
        public Lod Lod
        {
            get { return _lod; }
            set { _lod = value; }
        }

        [XmlElement(Type = typeof (string), ElementName = "RegionSimpleExtensionGroup", IsNullable = false,
            Form = XmlSchemaForm.Qualified, Namespace = Declarations.SchemaVersion)]
        public List<string> RegionSimpleExtensionGroup
        {
            get
            {
                if (_regionSimpleExtensionGroup == null) _regionSimpleExtensionGroup = new List<string>();
                return _regionSimpleExtensionGroup;
            }
            set { _regionSimpleExtensionGroup = value; }
        }

        [XmlElement(Type = typeof (RegionObjectExtensionGroup), ElementName = "RegionObjectExtensionGroup",
            IsNullable = false, Form = XmlSchemaForm.Qualified, Namespace = Declarations.SchemaVersion)]
        public List<RegionObjectExtensionGroup> RegionObjectExtensionGroup
        {
            get
            {
                if (_regionObjectExtensionGroup == null)
                    _regionObjectExtensionGroup = new List<RegionObjectExtensionGroup>();
                return _regionObjectExtensionGroup;
            }
            set { _regionObjectExtensionGroup = value; }
        }

        public new void MakeSchemaCompliant()
        {
            base.MakeSchemaCompliant();
        }
    }
}