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
using System.Xml.Serialization;

namespace SharpMap.Entities.Ogc.Kml
{
    /// <remarks/>
    [Serializable]
    [XmlType(TypeName = "UpdateType", Namespace = "http://www.opengis.net/kml/2.2")]
    [XmlRoot("Update", Namespace = "http://www.opengis.net/kml/2.2", IsNullable = false)]
    public class Update
    {
        private object[] itemsField;
        private string targetHrefField;

        private object[] updateExtensionGroupField;

        /// <remarks/>
        [XmlElement(DataType = "anyURI")]
        public string targetHref
        {
            get { return targetHrefField; }
            set { targetHrefField = value; }
        }

        /// <remarks/>
        [XmlElement("Change", typeof (Change))]
        [XmlElement("Create", typeof (Create))]
        [XmlElement("Delete", typeof (Delete))]
        [XmlElement("UpdateOpExtensionGroup", typeof (object))]
        public object[] Items
        {
            get { return itemsField; }
            set { itemsField = value; }
        }

        /// <remarks/>
        [XmlElement("UpdateExtensionGroup")]
        public object[] UpdateExtensionGroup
        {
            get { return updateExtensionGroupField; }
            set { updateExtensionGroupField = value; }
        }
    }
}