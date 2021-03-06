// Copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using GeoAPI.Diagnostics;
using SharpMap.Data;
using SharpMap.Diagnostics;
using SharpMap.Layers;
using SharpMap.Presentation.Views;
using SharpMap.Expressions;
using Trace = GeoAPI.Diagnostics.Trace;

namespace SharpMap.Presentation.Presenters
{
    /// <summary>
    /// Manages views of attribute data and coordinates layer attribute interaction.
    /// </summary>
    public class AttributePresenter : FeatureLayersListenerPresenter<IAttributeView>
    {
        #region Private instance fields
        private Boolean _highlightUpdating;
        #endregion

        #region Object construction / disposal
        /// <summary>
        /// Creates a new <see cref="AttributePresenter"/> with the given map and view.
        /// </summary>
        /// <param name="map">The map model to observe and control.</param>
        /// <param name="view">The view to use to display attribute data.</param>
        public AttributePresenter(Map map, IAttributeView view)
            : base(map, view)
        {
            Trace.Info(TraceCategories.Presentation, "Creating AttributePresenter");
            View.FeaturesHighlightedChanged += handleViewFeaturesHighlightedChanged;
            View.Layers = Map.Layers;
        }
        #endregion

        #region Private helper methods

        private static IEnumerable<Object> getFeatureIdsFromIndexes(IFeatureLayer layer,
                                                                    IEnumerable<Int32> indexes)
        {
            foreach (Int32 index in indexes)
            {
                FeatureDataRow feature = layer.SelectedFeatures[index].Row as FeatureDataRow;
                Debug.Assert(feature != null);

                if (!feature.HasOid)
                {
                    throw new InvalidOperationException("Feature must have Object identifier " +
                                                        "in order to highlight.");
                }

                yield return feature.GetOid();
            }
        }

        private static IEnumerable<Int32> getSelectedFeatureIndexesFromHighlighedFeatures(
                                                                    FeatureDataView selectedFeatures,
                                                                    FeatureDataView highlightedFeatures)
        {
            foreach (FeatureDataRow feature in highlightedFeatures)
            {
                yield return selectedFeatures.Find(feature);
            }
        }
        #endregion

        protected override ListChangedEventHandler GetHighlightedChangedEventHandler()
        {
            return handleHighlightedFeaturesChanged;
        }

        #region Event handlers
        //private void handleLayersChanged(Object sender, ListChangedEventArgs e)
        //{
        //    IFeatureLayer newLayer = null;
        //    IFeatureLayer oldLayer = null;

        //    if (e.NewIndex >= 0 && e.NewIndex < Map.Layers.Count)
        //    {
        //        newLayer = Map.Layers[e.NewIndex] as IFeatureLayer;
        //    }

        //    if (e.OldIndex >= 0 && e.OldIndex < Map.Layers.Count && _highlightUpdating)
        //    {
        //        oldLayer = Map.Layers[e.OldIndex] as IFeatureLayer;
        //    }

        //    if (newLayer != null && e.ListChangedType == ListChangedType.ItemAdded)
        //    {
        //        wireupFeatureLayer(newLayer);
        //    }

        //    // BUG: this shouldn't work... e.OldIndex won't be the old layer
        //    // index if it was deleted...
        //    // TODO: make unit test which confirms or refutes the above bug
        //    if (oldLayer != null &&
        //        e.NewIndex < 0 &&
        //        e.ListChangedType == ListChangedType.ItemDeleted)
        //    {
        //        unwireFeatureLayer(oldLayer);
        //    }
        //}

        private void handleViewFeaturesHighlightedChanged(Object sender,
                                                          FeaturesHighlightedChangedEventArgs e)
        {
            Trace.Info(TraceCategories.Presentation, "AttributePresenter handling " +
                                                     "view features highlighted changed");
#if DEBUG
            Trace.Debug(TraceCategories.Presentation, "Layer on which highlight change: " +
                                                      e.LayerName);
            Trace.Debug(TraceCategories.Presentation, "Highlighted features: " +
                                                      EnumerablePrinter.Print(e.HighlightedFeatures));
#endif

            if (_highlightUpdating)
            {
                return;
            }

            ILayer layer = Map.Layers[e.LayerName];

            IFeatureLayer featureLayer = layer as IFeatureLayer;

            if (featureLayer == null)
            {
                LayerGroup group = layer as LayerGroup;

                if (group != null)
                {
                    featureLayer = group.MasterLayer as IFeatureLayer;
                }
            }

            Assert.IsNotNull(featureLayer);

            FeatureQueryExpression viewDefinition = featureLayer.HighlightedFilter;

            IEnumerable oids = getFeatureIdsFromIndexes(featureLayer, e.HighlightedFeatures);
            OidCollectionExpression oidExpression = new OidCollectionExpression(oids);

            if (viewDefinition == null)
            {
                viewDefinition = new FeatureQueryExpression(new AllAttributesExpression(),
                                                            oidExpression, null);
            }
            else
            {
                ProjectionExpression projection = viewDefinition.Projection;

                if (viewDefinition.SpatialPredicate == null)
                {
                    viewDefinition = new FeatureQueryExpression(projection, oidExpression, null);
                }
                else
                {
                    PredicateExpression predicate = new BinaryExpression(viewDefinition.SpatialPredicate,
                                                                         BinaryOperator.And,
                                                                         oidExpression);
                    viewDefinition = new FeatureQueryExpression(projection, predicate, null);
                }
            }

            _highlightUpdating = true;
            featureLayer.HighlightedFilter = viewDefinition;
            _highlightUpdating = false;
        }

        private void handleHighlightedFeaturesChanged(Object sender, ListChangedEventArgs e)
        {
            if (_highlightUpdating)
            {
                return;
            }

            IFeatureLayer featureLayer = null;

            foreach (ILayer layer in Map.Layers)
            {
                featureLayer = layer as IFeatureLayer;

                if (featureLayer == null)
                {
                    LayerGroup group = layer as LayerGroup;

                    if (group != null)
                    {
                        featureLayer = group.MasterLayer as IFeatureLayer;
                    }
                }

                if (featureLayer == null)
                {
                    continue;
                }

                if (ReferenceEquals(featureLayer.HighlightedFeatures, sender))
                {
                    break;
                }
            }

            Assert.IsNotNull(featureLayer);

            // When the user selects features in the view, 
            // we need to highlight those features
            IEnumerable<Int32> indexes = getSelectedFeatureIndexesFromHighlighedFeatures(
                                                                    featureLayer.SelectedFeatures,
                                                                    featureLayer.HighlightedFeatures);

            _highlightUpdating = true;
            View.SetHighlightedFeatures(featureLayer.LayerName, indexes);
            _highlightUpdating = false;
        }
        #endregion
    }
}
