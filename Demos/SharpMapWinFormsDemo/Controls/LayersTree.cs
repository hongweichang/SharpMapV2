/*
 *	This file is part of SharpMap.MapViewer
 *  SharpMapMapViewer is free software � 2008 Newgrove Consultants Limited, 
 *  http://www.newgrove.com; you can redistribute it and/or modify it under the terms 
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
using System;
using System.ComponentModel;
using System.Windows.Forms;
using SharpMap.Layers;

namespace MapViewer.Controls
{
    public class LayersTree : CustomTreeView
    {

        private BindingList<ILayer> layers;

        public LayersTree()
        {
            CheckBoxes = true;

        }

        public BindingList<ILayer> Layers
        {
            get { return layers; }
            set
            {
                UnwireLayers(layers);
                layers = value;
                if (value != null)
                    WireLayers(layers);
            }
        }



        private void WireLayers(BindingList<ILayer> newLayers)
        {
            if (newLayers != null)
            {
                newLayers.ListChanged += layers_ListChanged;
                foreach (ILayer lyr in newLayers)
                {
                    TreeNode n = NodeFactory.CreateLayerNode(lyr);
                    n.Checked = lyr.Enabled;
                    Nodes.Add(n);
                }
            }
            else
            {
                Nodes.Clear();
            }
        }

        private void layers_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                TreeNode n = NodeFactory.CreateLayerNode(layers[e.NewIndex]);
                n.Checked = layers[e.NewIndex].Enabled;
                Nodes.Insert(e.NewIndex, n);
                return;
            }

            if (e.ListChangedType == ListChangedType.ItemDeleted)
            {
                if (e.OldIndex > -1 && e.OldIndex < Nodes.Count)
                {
                    TreeNode tn = Nodes[e.OldIndex];
                    Nodes.Remove(tn);
                }
            }
        }

        private void UnwireLayers(BindingList<ILayer> layers)
        {
            if (layers != null)
                layers.ListChanged -= layers_ListChanged;
            Nodes.Clear();
        }


        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                Layers = null;
                base.Dispose(disposing);
            }
        }
    }
}