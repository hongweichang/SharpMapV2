﻿// Portions copyright 2005 - 2006: Morten Nielsen (www.iter.dk)
// Portions copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
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
using System.ComponentModel;
using System.Data;
using System.Globalization;
using GeoAPI.Geometries;
using SharpMap.Data;
using SharpMap.Expressions;
using SharpMap.Styles;
using System.Collections.Generic;

namespace SharpMap.Layers
{
    /// <summary>
    /// Represents a map layer of features.
    /// </summary>
    public abstract class FeatureLayer : Layer, IFeatureLayer
    {
        private static readonly PropertyDescriptorCollection _featureLayerTypeProperties;

        static FeatureLayer()
        {
            _featureLayerTypeProperties = TypeDescriptor.GetProperties(typeof(FeatureLayer));
        }

        protected static PropertyDescriptorCollection FeatureLayerTypeProperties
        {
            get { return _featureLayerTypeProperties; }
        }

        public static PropertyDescriptor AreFeaturesSelectableProperty
        {
            get { return FeatureLayerTypeProperties.Find("AreFeaturesSelectable", false); }
        }

        #region Instance fields
        private readonly FeatureDataTable _features;
        private readonly FeatureDataView _selectedFeatures;
        private readonly FeatureDataView _highlightedFeatures;
        #endregion

        /// <summary>
        /// Initializes a new, empty features layer
        /// which handles <see cref="FeatureDataTable.FeaturesNotFound"/> 
        /// events from <see cref="Features"/>.
        /// </summary>
        protected FeatureLayer(IFeatureProvider dataSource)
            : this(String.Empty, dataSource) { }

        /// <summary>
        /// Initializes a new features layer with the given name and datasource
        /// and which handles <see cref="FeatureDataTable.FeaturesNotFound"/> 
        /// events from <see cref="Features"/>.
        /// </summary>
        /// <param name="layername">Name of the layer.</param>
        /// <param name="dataSource">Data source.</param>
        protected FeatureLayer(String layername, IFeatureProvider dataSource)
            : this(layername, new VectorStyle(), dataSource, true) { }

        /// <summary>
        /// Initializes a new features layer with the given name and datasource.
        /// </summary>
        /// <param name="layername">Name of the layer.</param>
        /// <param name="dataSource">Data source.</param>
        /// <param name="handleFeatureDataRequest">
        /// Value to indicate the layer should handle 
        /// <see cref="FeatureDataTable.FeaturesNotFound"/> events from the <see cref="Features"/>
        /// table.
        /// </param>
        protected FeatureLayer(String layername,
                               IFeatureProvider dataSource, 
                               Boolean handleFeatureDataRequest)
            : this(layername, new VectorStyle(), dataSource, handleFeatureDataRequest) { }
        
        /// <summary>
        /// Initializes a new features layer with the given name, style and datasource
        /// and which handles <see cref="FeatureDataTable.FeaturesNotFound"/> 
        /// events from <see cref="Features"/>.
        /// </summary>
        /// <param name="layername">Name of the layer.</param>
        /// <param name="style">Style to apply to the layer.</param>
        /// <param name="dataSource">Data source.</param>
        protected FeatureLayer(String layername, 
                               VectorStyle style,
                               IFeatureProvider dataSource)
            : this(layername, style, dataSource, true) { }

        /// <summary>
        /// Initializes a new features layer with the given name, style and datasource.
        /// </summary>
        /// <param name="layername">Name of the layer.</param>
        /// <param name="style">Style to apply to the layer.</param>
        /// <param name="dataSource">Data source.</param>
        /// <param name="handleFeatureDataRequest">
        /// Value to indicate the layer should handle 
        /// <see cref="FeatureDataTable.FeaturesNotFound"/> events from the 
        /// <see cref="Features"/> table.
        /// </param>
        protected FeatureLayer(String layername, 
                               VectorStyle style,
                               IFeatureProvider dataSource, 
                               Boolean handleFeatureDataRequest)
            : base(layername, style, dataSource)
        {
            //ShouldHandleFeaturesNotFoundEvent = handleFeatureDataRequest;

            // We need to get the schema of the feature table.
            DataSource.Open();
            _features = DataSource.CreateNewTable() 
                        ?? new FeatureDataTable(dataSource.GeometryFactory);
            GeometryFactory = dataSource.GeometryFactory;
            DataSource.Close();

            // We generally want spatial indexing on the feature table...
            _features.IsSpatiallyIndexed = true;

            IGeometry empty = dataSource.GeometryFactory.CreatePoint();
            
            FeatureQueryExpression selectedExpression
                = new FeatureQueryExpression(empty, SpatialOperation.Intersects, this);

            _selectedFeatures = new FeatureDataView(_features, 
                                                    selectedExpression,
                                                    "", 
                                                    DataViewRowState.CurrentRows);

            FeatureQueryExpression highlightExpression 
                = new FeatureQueryExpression(empty, SpatialOperation.Intersects, this);

            _highlightedFeatures = new FeatureDataView(_features,
                                                       highlightExpression, 
                                                       "", 
                                                       DataViewRowState.CurrentRows);
        }

        #region IFeatureLayer Members

        /// <summary>
        /// Gets the data source for this layer as a more 
        /// strongly-typed IFeatureProvider.
        /// </summary>
        public new IFeatureProvider DataSource
        {
            get { return base.DataSource as IFeatureProvider; }
        }

        /// <summary>
        /// Gets a <see cref="FeatureDataTable"/> of cached features for the layer.
        /// </summary>
        public FeatureDataTable Features
        {
            get { return _features; }
        }

        /// <summary>
        /// Gets a <see cref="FeatureDataView"/> of features which have been 
        /// highlighted.
        /// </summary>
        public FeatureDataView HighlightedFeatures
        {
            get { return _highlightedFeatures; }
        }

        //public void LoadFeaturesByOids(IEnumerable oids)
        //{
        //    if (!AsyncQuery)
        //    {
        //        IEnumerable<IFeatureDataRecord> features = DataSource.GetFeatures(oids);
        //        MergeFeatures(features);
        //    }
        //    else
        //    {
        //        DataSource.BeginGetFeatures(oids, getFeaturesCallback, null);
        //    }
        //}

        /// <summary>
        /// Gets the <see cref="CultureInfo"/> used to encode text
        /// and format numbers for this layer.
        /// </summary>
        public CultureInfo Locale
        {
            get { return DataSource.Locale; }
        }

        /// <summary>
        /// Gets a <see cref="FeatureDataView"/> of features which have been 
        /// selected.
        /// </summary>
        public FeatureDataView SelectedFeatures
        {
            get { return _selectedFeatures; }
        }


        public Boolean AreFeaturesSelectable
        {
            get
            {
                FeatureStyle fstyle = Style as FeatureStyle;
                return fstyle != null && fstyle.AreFeaturesSelectable;
            }
            set
            {
                FeatureStyle fstyle = Style as FeatureStyle;

                if (fstyle != null)
                {
                    fstyle.AreFeaturesSelectable = value;
                }
            }
        }

        #endregion

        #region Layer overrides

        public override IEnumerable Select(Expression query)
        {
            throw new NotImplementedException();
        }

        protected override void ProcessLoadResults(Object results)
        {
            _features.SuspendIndexEvents();

            IEnumerable<IFeatureDataRecord> features = results as IEnumerable<IFeatureDataRecord>;
            MergeFeatures(features);

            _features.RestoreIndexEvents(true);
        }
        #endregion

        protected void MergeFeatures(IEnumerable<IFeatureDataRecord> features)
        {
            _features.Merge(features, GeometryFactory);
        }

        #region Private helper methods

        //private void handleFeaturesRequested(Object sender, FeaturesNotFoundEventArgs e)
        //{
        //    IGeometry layerExtents = Extents.ToGeometry();
        //    IGeometry available = layerExtents.Intersection(e.Expression.QueryGeometry);

        //    Boolean hasIntersectionWithLayerData = 
        //        !(available.IsEmpty || Features.Envelope.Contains(available)) && 
        //        e.Expression.QueryType != SpatialExpressionType.Disjoint;

        //    if(hasIntersectionWithLayerData || e.Expression.Oids != null)
        //    {
        //        LoadLayerData(e.Expression);   
        //    }
        //}

        //private void queryCallback(IAsyncResult result)
        //{
        //    FeatureDataTable features = result.AsyncState as FeatureDataTable;

        //    if (features != null)
        //    {
        //        Features.Merge(features);
        //    }

        //    DataSource.EndExecuteFeatureQuery(result);
        //}

        //private void getFeaturesCallback(IAsyncResult result)
        //{
        //    IEnumerable<IFeatureDataRecord> features = DataSource.EndGetFeatures(result);
        //    MergeFeatures(features);
        //}

        #endregion

    }
}