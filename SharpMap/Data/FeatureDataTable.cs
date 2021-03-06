// Portions copyright 2005 - 2006: Morten Nielsen (www.iter.dk)
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Threading;
using GeoAPI.Diagnostics;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using SharpMap.Expressions;
using SharpMap.Indexing.RTree;
using SharpMap.Utilities;
using GeoAPI.CoordinateSystems.Transformations;

namespace SharpMap.Data
{
    /// <summary>
    /// Represents an in-memory table of feature data. 
    /// </summary>
    [Serializable]
    public class FeatureDataTable : DataTable,
                                    IEnumerable<FeatureDataRow>,
                                    IEnumerable<IFeatureDataRecord>
    {
        #region Nested types

        private delegate DataRow MergeRowDelegate(FeatureDataTable table, FeatureDataRow sourceRow,
                                                  FeatureDataRow targetRow, Boolean preserveChanges, Object index);

        private delegate Boolean GetEnforceConstraintsDelegate(FeatureDataTable table);

        private delegate void SetEnforceConstraintsDelegate(FeatureDataTable table, Boolean value);

        private delegate void EvaluateExpressionsDelegate(FeatureDataTable table);

        private delegate void EnableConstraintsDelegate(FeatureDataTable table);

        private delegate Boolean GetSuspendEnforceConstraintsDelegate(FeatureDataTable table);

        private delegate void SetSuspendEnforceConstraintsDelegate(FeatureDataTable table, Boolean value);

        private delegate UniqueConstraint GetPrimaryKeyConstraintDelegate(FeatureDataTable table);

        private delegate Boolean GetMergingDataDelegate(FeatureDataTable table);

        private delegate void SetMergingDataDelegate(FeatureDataTable table, Boolean isMerging);

        private delegate FeatureDataView GetDefaultViewDelegate(FeatureDataTable table);

        private delegate DataTable CloneToDelegate(DataTable src, DataTable dst, DataSet dataSet, Boolean skipExpressions);

        private delegate void SuspendIndexEventsDelegate(DataTable table);

        private delegate void RestoreIndexEventsDelegate(DataTable table, Boolean forcesReset);

        private delegate FeatureDataRow FindMergeTargetDelegate(
            FeatureDataTable table, FeatureDataRow sourceRow, Object dataKey, Object index);

        #endregion

        #region Type fields

        private static readonly MergeRowDelegate _mergeRow;
        private static readonly GetEnforceConstraintsDelegate _getEnforceConstraints;
        private static readonly SetEnforceConstraintsDelegate _setEnforceConstraints;
        private static readonly EvaluateExpressionsDelegate _evaluateExpression;
        private static readonly GetSuspendEnforceConstraintsDelegate _getSuspendEnforceConstraints;
        private static readonly SetSuspendEnforceConstraintsDelegate _setSuspendEnforceConstraints;
        private static readonly EnableConstraintsDelegate _enableConstraints;
        private static readonly GetPrimaryKeyConstraintDelegate _getPrimaryKeyConstraint;
        private static readonly GetMergingDataDelegate _getMergingData;
        private static readonly SetMergingDataDelegate _setMergingData;
        private static readonly GetDefaultViewDelegate _getDefaultView;
        private static readonly CloneToDelegate _cloneTo;
        private static readonly SuspendIndexEventsDelegate _suspendIndexEvents;
        private static readonly RestoreIndexEventsDelegate _restoreIndexEvents;
        private static readonly FindMergeTargetDelegate _findMergeTarget;

        #endregion

        #region Static constructors

        static FeatureDataTable()
        {
            _getPrimaryKeyConstraint = generatePrimaryKeyConstraintDelegate();
            _getMergingData = generateGetMergingDataDelegate();
            _setMergingData = generateSetMergingDataDelegate();
            _getDefaultView = generateGetDefaultViewDelegate();
            _cloneTo = generateCloneToDelegate();
            _suspendIndexEvents = generateSuspendIndexEventsDelegate();
            _restoreIndexEvents = generateRestoreIndexEventsDelegate();
            _findMergeTarget = generateFindMergeTargetDelgate();
            _getEnforceConstraints = generateGetEnforceConstraintsDelegate();
            _setEnforceConstraints = generateSetEnforceConstraintsDelegate();
            _evaluateExpression = generateEvaluateExpressionsDelegate();
            _getSuspendEnforceConstraints = generateGetSuspendEnforceConstraintsDelegate();
            _setSuspendEnforceConstraints = generateSetSuspendEnforceConstraintsDelegate();
            _enableConstraints = generateEnableConstraintsDelegate();
            _mergeRow = generateMergeRowDelegate();
        }

        #endregion

        #region Instance fields

        private readonly IGeometryFactory _geoFactory;
        private IUpdatableSpatialIndex<IExtents, FeatureDataRow> _rTreeIndex;
        //private IGeometry _loadedRegion;
        private IExtents _extents;
        //private IGeometry _empty;

        #endregion

        #region Object constructors
        /// <summary>
        /// Creates a <see cref="FeatureDataTable"/> with an name of <see cref="String.Empty"/> and 
        /// the given <see cref="IGeometryFactory"/>
        /// instance used to create <see cref="IGeometry"/> instances.
        /// </summary>
        /// <param name="geometryFactory">
        /// The <see cref="IGeometryFactory"/> to use to create 
        /// <see cref="IGeometry"/> instances.
        /// </param>
        public FeatureDataTable(IGeometryFactory geometryFactory)
            : this(String.Empty, geometryFactory) { }

        /// <summary>
        /// Creates a <see cref="FeatureDataTable"/> with the given <paramref name="tableName"/> 
        /// and <see cref="IGeometryFactory"/> instance used to create <see cref="IGeometry"/> 
        /// instances.
        /// </summary>
        /// </summary>
        /// <param name="tableName">The name to give the table.</param>
        /// <param name="geometryFactory">
        /// The <see cref="IGeometryFactory"/> to use to create 
        /// <see cref="IGeometry"/> instances.
        /// </param>
        public FeatureDataTable(String tableName, IGeometryFactory geometryFactory)
            : base(tableName)
        {
            _geoFactory = geometryFactory;
            //_empty = _geoFactory.CreatePoint();
            Constraints.CollectionChanged += OnConstraintsChanged;
        }

        /// <summary>
        /// Intitalizes a new instance of the FeatureDataTable class and
        /// copies the name and structure of the given <paramref name="table"/>.
        /// </summary>
        public FeatureDataTable(DataTable table, IGeometryFactory factory)
            : base(table.TableName)
        {
            _geoFactory = factory;
            //_empty = _geoFactory.CreatePoint();
            Constraints.CollectionChanged += OnConstraintsChanged;

            if (table.DataSet == null || (table.CaseSensitive != table.DataSet.CaseSensitive))
            {
                CaseSensitive = table.CaseSensitive;
            }

            if (table.DataSet == null || (table.Locale.ToString() != table.DataSet.Locale.ToString()))
            {
                Locale = table.Locale;
            }

            if (table.DataSet == null || (table.Namespace != table.DataSet.Namespace))
            {
                Namespace = table.Namespace;
            }

            Prefix = table.Prefix;
            MinimumCapacity = table.MinimumCapacity;
            DisplayExpression = table.DisplayExpression;
        }

        protected FeatureDataTable(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Put extra stuff to serialize here...
            throw new NotImplementedException();
        }

        #endregion

        #region Events
        /// <summary>
        /// Event which occurs when a <see cref="Select"/> method is called
        /// on the <see cref="FeatureDataTable"/>.
        /// </summary>
        public event EventHandler<SelectRequestedEventArgs> SelectRequested;

        /// <summary>
        /// Occurs after a FeatureDataRow has been changed successfully. 
        /// </summary>
        public event FeatureDataRowChangeEventHandler FeatureDataRowChanged;

        /// <summary>
        /// Occurs when a FeatureDataRow is changing. 
        /// </summary>
        public event FeatureDataRowChangeEventHandler FeatureDataRowChanging;

        /// <summary>
        /// Occurs after a row in the table has been deleted.
        /// </summary>
        public event FeatureDataRowChangeEventHandler FeatureDataRowDeleted;

        /// <summary>
        /// Occurs before a row in the table is about to be deleted.
        /// </summary>
        public event FeatureDataRowChangeEventHandler FeatureDataRowDeleting;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the full extents of all features in the feature table.
        /// </summary>
        public IExtents Extents
        {
            get
            {
                if (_extents == null)
                {
                    computeExtents();
                }

                return _extents;
            }
        }

        /// <summary>
        /// Gets the number of feature rows in the feature table.
        /// </summary>
        [Browsable(false)]
        public Int32 FeatureCount
        {
            get { return base.Rows.Count; }
        }

        /// <summary>
        /// Gets the feature data row at the specified index.
        /// </summary>
        /// <param name="index">Index of the row to retrieve.</param>
        /// <returns>
        /// The <see cref="FeatureDataRow"/> at the given <paramref name="index"/>.
        /// </returns>
        public FeatureDataRow this[Int32 index]
        {
            get { return base.Rows[index] as FeatureDataRow; }
        }

        /// <summary>
        /// Gets or sets a value indicating if the table is spatially indexed.
        /// </summary>
        public Boolean IsSpatiallyIndexed
        {
            get { return _rTreeIndex != null; }
            set
            {
                if (value && _rTreeIndex == null)
                {
                    initializeSpatialIndex();
                }
                else if (!value && _rTreeIndex != null)
                {
                    _rTreeIndex.Dispose();
                    _rTreeIndex = null;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="IGeometryFactory"/> assigned to the 
        /// <see cref="FeatureDataTable"/> when it was 
        /// <see cref="FeatureDataTable(IGeometryFactory)">created</see>.
        /// </summary>
        public IGeometryFactory GeometryFactory
        {
            get { return _geoFactory; }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds a feature row to the <see cref="FeatureDataTable"/>.
        /// </summary>
        /// <param name="row">The <see cref="FeatureDataRow"/> to add.</param>
        public void AddRow(FeatureDataRow row)
        {
            base.Rows.Add(row);
        }

        /// <summary>
        /// Clones the structure of the <see cref="FeatureDataTable"/>, 
        /// including all <see cref="FeatureDataTable"/> schemas and constraints. 
        /// </summary>
        /// <returns>
        /// A <see cref="FeatureDataTable"/> with the same
        /// schema and constraints.
        /// </returns>
        public new FeatureDataTable Clone()
        {
            return base.Clone() as FeatureDataTable;
        }

        /// <summary>
        /// Clones the schema of the <see cref="FeatureDataTable"/>
        /// to another <see cref="FeatureDataTable"/> instance.
        /// </summary>
        /// <param name="table">
        /// <see cref="FeatureDataTable"/> to copy the schema to.
        /// </param>
        public void CloneTo(FeatureDataTable table)
        {
            _cloneTo(this, table, null, false);
        }

        /// <summary>
        /// Finds a feature given its id.
        /// </summary>
        /// <param name="key">The feature object id.</param>
        /// <returns>
        /// The feature row identified by <paramref name="key"/>, or <see langword="null"/>
        /// if the feature row is not found.
        /// </returns>
        public FeatureDataRow Find(Object key)
        {
            return Rows.Find(key) as FeatureDataRow;
        }

        /// <summary>
        /// Gets a copy of the <see cref="FeatureDataTable"/> that contains all 
        /// changes made to it since it was loaded or <see cref="DataTable.AcceptChanges"/> 
        /// was last called.
        /// </summary>
        /// <returns>
        /// A copy of the changes from this <see cref="FeatureDataTable"/>, 
        /// or <see langword="null"/> if no changes are found.
        /// </returns>
        public new FeatureDataTable GetChanges()
        {
            FeatureDataTable changes = Clone();
            FeatureDataRow row;

            for (Int32 i = 0; i < Rows.Count; i++)
            {
                row = Rows[i] as FeatureDataRow;
                Debug.Assert(row != null);

                if (row.RowState != DataRowState.Unchanged)
                {
                    changes.ImportRow(row);
                }
            }

            return changes.Rows.Count == 0
                       ? null
                       : changes;
        }

        /// <summary>
        /// Returns an enumerator for enumering the rows of the 
        /// <see cref="FeatureDataTable"/>.
        /// </summary>
        /// <returns>An enumeration of all <see cref="FeatureDataRow"/>s in the table.</returns>
        public virtual IEnumerator<FeatureDataRow> GetEnumerator()
        {
            foreach (FeatureDataRow row in Rows)
            {
                yield return row;
            }
        }

        /// <summary>
        /// Imports a <see cref="FeatureDataRow"/> into the FeatureDataTable.
        /// </summary>
        /// <param name="featureRow">The feature row to import.</param>
        public void ImportRow(FeatureDataRow featureRow)
        {
            FeatureDataRow newRow = NewRow();
            copyRow(featureRow, newRow);
            AddRow(newRow);
        }

        /// <summary>
        /// Fills a <see cref="FeatureDataTable"/> with values from a data source 
        /// using the supplied <see cref="IDataReader"/>. If the DataTable already 
        /// contains rows, the incoming data from the data source is merged with the 
        /// existing rows.
        /// </summary>
        /// <param name="reader">
        /// <see cref="IDataReader"/> used as source of row data.
        /// </param>
        /// <param name="loadOption">
        /// A <see cref="LoadOption"/> value to control how rows are imported.
        /// </param>
        /// <param name="errorHandler">
        /// A callback method to get notification of errors in the load.
        /// </param>
        public override void Load(IDataReader reader,
                                  LoadOption loadOption,
                                  FillErrorEventHandler errorHandler)
        {
            if (!(reader is IFeatureDataReader))
            {
                throw new NotSupportedException("Only IFeatureDataReader instances are " +
                                                "supported to load from.");
            }

            Load(reader as IFeatureDataReader, loadOption, errorHandler);
        }

        // TODO: FeatureDataTable.Load would be more convenient if 'reader' parameter were IEnumerable<IFeatureDataRow>
        /// <summary>
        /// Fills a <see cref="FeatureDataTable"/> with values from a data source 
        /// using the supplied <see cref="IFeatureDataReader"/>. If the DataTable already 
        /// contains rows, the incoming data from the data source is merged with the 
        /// existing rows.
        /// </summary>
        /// <param name="reader">
        /// <see cref="IFeatureDataReader"/> used as source of feature row data.
        /// </param>
        /// <param name="loadOption">
        /// A <see cref="LoadOption"/> value to control how rows are imported.
        /// </param>
        /// <param name="errorHandler">
        /// A callback method to get notification of errors in the load.
        /// </param>
        public void Load(IFeatureDataReader reader,
                         LoadOption loadOption,
                         FillErrorEventHandler errorHandler)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            LoadFeaturesAdapter adapter = new LoadFeaturesAdapter(GeometryFactory);
            adapter.FillLoadOption = loadOption;
            adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

            if (errorHandler != null)
            {
                adapter.FillError += errorHandler;
            }

            adapter.Fill(this, reader);

            if (!reader.IsClosed && !reader.NextResult())
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Merges the features of <paramref name="features"/> into the 
        /// <see cref="FeatureDataTable"/>, overwriting any changes.
        /// </summary>
        /// <param name="features">
        /// The <see cref="FeatureDataTable"/> source from which features are merged.
        /// </param>
        public void Merge(FeatureDataTable features)
        {
            Merge(features, false, MissingSchemaAction.Add);
        }

        /// <summary>
        /// Merges the features of <paramref name="features"/> into the 
        /// <see cref="FeatureDataTable"/>, indicating whether current feature changes
        /// should be preserved or not.
        /// </summary>
        /// <param name="features">
        /// The <see cref="FeatureDataTable"/> source from which features are merged.
        /// </param>
        /// <param name="preserveChanges">
        /// Determines whether changes in the current <see cref="FeatureDataTable"/> are preserved:
        /// <see langword="true"/> preserves them, <see langword="false"/> overwrites them.</param>
        public void Merge(FeatureDataTable features, Boolean preserveChanges)
        {
            Merge(features, preserveChanges, SchemaMergeAction.AddAll);
        }

        /// <summary>
        /// Merges the features of <paramref name="features"/> into the 
        /// <see cref="FeatureDataTable"/>, indicating whether current feature changes
        /// should be preserved or not.
        /// </summary>
        /// <param name="features">
        /// The <see cref="FeatureDataTable"/> source from which features are merged.
        /// </param>
        /// <param name="preserveChanges">
        /// Determines whether changes in the current <see cref="FeatureDataTable"/> are preserved:
        /// <see langword="true"/> preserves them, <see langword="false"/> overwrites them.</param>
        public void Merge(FeatureDataTable features,
                          Boolean preserveChanges,
                          SchemaMergeAction schemaMergeAction)
        {
            FeatureMerger merger = new FeatureMerger(this, null, GeometryFactory, preserveChanges, schemaMergeAction);
            merger.MergeFeatures(features);
        }

        public void Merge(IFeatureDataRecord record, IGeometryFactory factory)
        {
            Merge(record, factory, SchemaMergeAction.AddWithKey);
        }

        public void Merge(IFeatureDataRecord record,
                          IGeometryFactory factory,
                          SchemaMergeAction schemaMergeAction)
        {
            FeatureMerger merger = new FeatureMerger(this, null, GeometryFactory, true, schemaMergeAction);
            merger.MergeFeature(record, factory);
        }

        public void Merge(IEnumerable<IFeatureDataRecord> records, IGeometryFactory factory)
        {
            Merge(records, null, factory);
        }

        public void Merge(IEnumerable<IFeatureDataRecord> records,
                          ICoordinateTransformation transform,
                          IGeometryFactory factory)
        {
            Merge(records, transform, factory, SchemaMergeAction.AddWithKey );
        }

        public void Merge(IEnumerable<IFeatureDataRecord> records,
                          ICoordinateTransformation transform,
                          IGeometryFactory factory,
                          SchemaMergeAction schemaMergeAction)
        {
            FeatureMerger merger = new FeatureMerger(this, transform, factory, true, schemaMergeAction);
            merger.MergeFeatures(records);
        }

        /// <summary>
        /// Merges schema from the FeatureDataTable to a target FeatureDataTable,
        /// adding columns and key information to the target.
        /// </summary>
        /// <param name="target">The target of the schema merge.</param>
        /// <remarks>
        /// Calls <see cref="MergeSchemaTo(FeatureDataTable, SchemaMergeAction)"/>
        /// with SchemaMergeAction.Add | SchemaMergeAction.Key.
        /// </remarks>
        public void MergeSchemaTo(FeatureDataTable target)
        {
            MergeSchemaTo(target, SchemaMergeAction.AddWithKey);
        }

        /// <summary>
        /// Merges schema from the FeatureDataTable to a target FeatureDataTable.
        /// </summary>
        /// <param name="target">The target of the schema merge.</param>
        /// <param name="schemaMergeAction">
        /// Option to specify how to merge the schemas.
        /// </param>
        public void MergeSchemaTo(FeatureDataTable target, SchemaMergeAction schemaMergeAction)
        {
            FeatureMerger merger = new FeatureMerger(target, null, GeometryFactory, true, schemaMergeAction);
            merger.MergeSchema(this);
        }

        /// <summary>
        /// Merges schema from a source <see cref="DataTable"/> 
        /// to the <see cref="FeatureDataTable"/>,
        /// adding columns and key information.
        /// </summary>
        /// <param name="source">The source of the schema merge.</param>
        /// <remarks>
        /// Calls <see cref="MergeSchemaFrom(FeatureDataTable, SchemaMergeAction)"/>
        /// with SchemaMergeAction.Add | SchemaMergeAction.Key.
        /// </remarks>
        public void MergeSchemaFrom(DataTable source)
        {
            MergeSchemaFrom(source, SchemaMergeAction.AddWithKey);
        }

        /// <summary>
        /// Merges schema to the <see cref="FeatureDataTable"/> from a source 
        /// <see cref="DataTable"/>.
        /// </summary>
        /// <param name="source">The source of the schema merge.</param>
        /// <param name="schemaMergeAction">
        /// Option to specify how to merge the schemas.
        /// </param>
        public void MergeSchemaFrom(DataTable source, SchemaMergeAction schemaMergeAction)
        {
            FeatureMerger merger = new FeatureMerger(this, null, GeometryFactory, true, schemaMergeAction);
            merger.MergeSchema(source);
        }

        /// <summary>
        /// Creates a new FeatureDataRow with the same schema as the table.
        /// </summary>
        /// <returns>
        /// A new <see cref="FeatureDataRow"/> with the same schema as the table.
        /// </returns>
        public new FeatureDataRow NewRow()
        {
            return base.NewRow() as FeatureDataRow;
        }

        /// <summary>
        /// Creates a new FeatureDataRow with the same schema as the table,
        /// and a value indicating whether it is fully loaded.
        /// </summary>
        /// <param name="isFullyLoaded">
        /// A value indicating whether the row is fully loaded from the data source
        /// or not (lazy-loaded).
        /// </param>
        /// <returns>
        /// A new <see cref="FeatureDataRow"/> with the same schema as the table.
        /// </returns>
        public FeatureDataRow NewRow(Boolean isFullyLoaded)
        {
            FeatureDataRow row = NewRow();
            row.IsFullyLoaded = isFullyLoaded;
            return row;
        }

        /// <summary>
        /// Removes the row from the table
        /// </summary>
        /// <param name="row">Row to remove</param>
        public void RemoveRow(FeatureDataRow row)
        {
            base.Rows.Remove(row);
        }

        /// <summary>
        /// Selects a set of <see cref="FeatureDataRow"/> instances 
        /// which intersect with the given <paramref name="bounds"/>.
        /// </summary>
        /// <param name="bounds">
        /// The <see cref="IExtents"/> to perform intersection testing with.
        /// </param>
        /// <returns>
        /// The set of <see cref="FeatureDataRow"/>s which intersect 
        /// <paramref name="bounds"/>.
        /// </returns>
        public IEnumerable<FeatureDataRow> Select(IExtents bounds)
        {
            IGeometry boundsGeometry = bounds.ToGeometry();

            return Select(boundsGeometry);
        }

        /// <summary>
        /// Selects a set of <see cref="FeatureDataRow"/> instances 
        /// which intersect with the given <paramref name="geometry"/>.
        /// </summary>
        /// <param name="geometry">
        /// The <see cref="IGeometry"/> to perform intersection testing with.
        /// </param>
        /// <returns>
        /// The set of <see cref="FeatureDataRow"/>s which intersect 
        /// <paramref name="geometry"/>.
        /// </returns>
        public IEnumerable<FeatureDataRow> Select(IGeometry geometry)
        {
            OnSelectRequested(FeatureQueryExpression.Intersects(GetAttributesExpression(), geometry));

            if (IsSpatiallyIndexed)
            {
                foreach (FeatureDataRow row in _rTreeIndex.Query(geometry.Extents))
                {
                    if (geometry.Intersects(row.Geometry))
                    {
                        yield return row;
                    }
                }
            }
            else
            {
                foreach (FeatureDataRow feature in this)
                {
                    if (geometry.Intersects(feature.Geometry))
                    {
                        yield return feature;
                    }
                }
            }
        }

        public IEnumerable<FeatureDataRow> Select(SpatialBinaryExpression query)
        {
            if (query == null) throw new ArgumentNullException("query");

            SpatialExpression spatialExpression = query.SpatialExpression;

            if (SpatialExpression.IsNullOrEmpty(spatialExpression))
            {
                yield break;
            }

            if (query.Expression != null &&
                (!(query.Expression is ThisExpression) &&
                ((query.Expression is FeaturesCollectionExpression) &&
                (query.Expression as FeaturesCollectionExpression).Collection != this)))
            {
                throw new ArgumentException("The Expression value of the query cannot be evaluated.");
            }

            OnSelectRequested(new FeatureQueryExpression(GetAttributesExpression(), query, null));

            ExtentsExpression extentsExpression = spatialExpression as ExtentsExpression;
            GeometryExpression geometryExpression = spatialExpression as GeometryExpression;

            IExtents filterExtents = extentsExpression != null
                                         ? extentsExpression.Extents
                                         : null;
            IGeometry filterGeometry = geometryExpression != null
                                           ? geometryExpression.Geometry
                                           : null;

            Assert.IsTrue(filterExtents != null || filterGeometry != null);

            if (filterExtents == null)
            {
                filterExtents = filterGeometry.Extents;
            }

            Boolean isLeft = query.IsSpatialExpressionLeft;
            SpatialOperation op = query.Op;

            IEnumerable<FeatureDataRow> features = IsSpatiallyIndexed
                                                       ? _rTreeIndex.Query(filterExtents)
                                                       : this;

            foreach (FeatureDataRow row in features)
            {
                if (filterGeometry != null)
                {
                    if (SpatialBinaryExpression.IsMatch(op, isLeft, filterGeometry, row.Geometry))
                    {
                        yield return row;
                    }
                }
                else
                {
                    if (SpatialBinaryExpression.IsMatch(op, isLeft, filterExtents, row.Geometry.Extents))
                    {
                        yield return row;
                    }
                }
            }

            // Handle the case where we are asking for disjoint geometries
            // but used an index intersection to narrow the candidates. All
            // geometries which have an extent which doesn't intersect is disjoint.
            if (op == SpatialOperation.Disjoint && IsSpatiallyIndexed)
            {
                foreach (FeatureDataRow row in _rTreeIndex.Query(_rTreeIndex.Bounds))
                {
                    if (!row.Extents.Intersects(filterExtents))
                    {
                        yield return row;
                    }
                }
            }
        }

        #endregion

        #region Protected members

        protected ProjectionExpression GetAttributesExpression()
        {
            // TODO: compute correct attribute expression
            return new AllAttributesExpression();
        }

        protected virtual void OnConstraintsChanged(Object sender, CollectionChangeEventArgs args)
        {
            if (args.Action == CollectionChangeAction.Add && args.Element is UniqueConstraint)
            {
                UniqueConstraint constraint = args.Element as UniqueConstraint;

                Debug.Assert(constraint != null);

                // If more than one column is added to the primary key, throw
                // an exception - we don't support it for now.
                if (constraint.IsPrimaryKey && constraint.Columns.Length > 1)
                {
                    throw new NotSupportedException("Compound primary keys not supported.");
                }
            }
        }

        protected virtual void OnSelectRequested(FeatureQueryExpression query)
        {
            EventHandler<SelectRequestedEventArgs> e = SelectRequested;

            if (e != null)
            {
                SelectRequestedEventArgs args = new SelectRequestedEventArgs(query);
                e(this, args);
            }
        }

        #endregion

        #region DataTable overrides and shadows

        /// <summary>
        /// Gets the containing FeatureDataSet.
        /// </summary>
        public new FeatureDataSet DataSet
        {
            get { return (FeatureDataSet)base.DataSet; }
        }

        /// <summary>
        /// Gets the default FeatureDataView for the table.
        /// </summary>
        public new FeatureDataView DefaultView
        {
            get
            {
                FeatureDataView defaultView = DefaultViewInternal;

                if (defaultView == null)
                {
                    defaultView = DataSet != null
                                      ? DataSet.DefaultViewManager.CreateDataView(this)
                                      : new FeatureDataView(this, true);

                    FeatureDataView baseDefaultView = _getDefaultView(this);
                    defaultView = Interlocked.CompareExchange(ref baseDefaultView, defaultView, null) ??
                                  baseDefaultView;
                }

                return defaultView;
            }
        }

        /// <summary>
        /// Creates and returns a new instance of a FeatureDataTable.
        /// </summary>
        /// <returns>An empty FeatureDataTable.</returns>
        protected override DataTable CreateInstance()
        {
            return new FeatureDataTable(_geoFactory);
        }

        /// <summary>
        /// Returns the FeatureDataRow type.
        /// </summary>
        /// <returns>The <see cref="Type"/> <see cref="FeatureDataRow"/>.</returns>
        protected override Type GetRowType()
        {
            return typeof(FeatureDataRow);
        }

        /// <summary>
        /// Clears the spatial index (if one exists) and calls 
        /// <see cref="DataTable.OnTableCleared"/> to have the 
        /// <see cref="DataTable.TableCleared"/> event raised.
        /// </summary>
        /// <param name="e">Event args identifying table being cleared.</param>
        protected override void OnTableCleared(DataTableClearEventArgs e)
        {
            if (_rTreeIndex != null)
            {
                _rTreeIndex.Clear();
            }

            base.OnTableCleared(e);
        }

        /// <summary>
        /// Creates a new FeatureDataRow with the same schema as the table, 
        /// based on a datarow builder.
        /// </summary>
        /// <param name="builder">
        /// The DataRowBuilder instance to use to construct
        /// a new row.
        /// </param>
        /// <returns>A new DataRow using the schema in the DataRowBuilder.</returns>
        protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
        {
            return new FeatureDataRow(builder);
        }

        /// <summary>
        /// Gets the collection of rows that belong to this table.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// Thrown if this property is set.
        /// </exception>
        public new DataRowCollection Rows
        {
            get { return base.Rows; }
            set { throw new NotSupportedException(); }
        }

        #endregion

        #region Event generators

        /// <summary>
        /// Raises the FeatureDataRowChanged event. 
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnRowChanged(DataRowChangeEventArgs e)
        {
            FeatureDataRow r = e.Row as FeatureDataRow;
            Debug.Assert(r != null);
            IGeometry geometry = r.Geometry;

            switch (e.Action)
            {
                case DataRowAction.Add:
                    if (geometry != null)
                    {
                        //_cache.AddExpressionResult(null, geometry);

                        if (IsSpatiallyIndexed)
                        {
                            _rTreeIndex.Insert(r);
                        }
                    }
                    break;
                case DataRowAction.Change:
                case DataRowAction.ChangeCurrentAndOriginal:
                    break;
                case DataRowAction.ChangeOriginal:
                    throw new NotImplementedException("Geometry versioning not implemented.");
                case DataRowAction.Delete:
                    if (geometry != null)
                    {
                        //_cache.Expire(geometry);

                        if (IsSpatiallyIndexed)
                        {
                            _rTreeIndex.Remove(r);
                        }
                    }
                    break;
                /*
            case DataRowAction.Commit:
            case DataRowAction.Rollback:
            case DataRowAction.Nothing:
                 */
                default:
                    break;
            }

            if ((FeatureDataRowChanged != null))
            {
                FeatureDataRowChanged(this, new FeatureDataRowChangeEventArgs(r, e.Action));
            }

            base.OnRowChanged(e);
        }

        /// <summary>
        /// Raises the FeatureDataRowChanging event. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRowChanging(DataRowChangeEventArgs e)
        {
            base.OnRowChanging(e);

            if ((FeatureDataRowChanging != null))
            {
                FeatureDataRowChanging(this, new FeatureDataRowChangeEventArgs((FeatureDataRow)e.Row,
                                                                               e.Action));
            }
        }

        /// <summary>
        /// Raises the FeatureDataRowDeleted event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRowDeleted(DataRowChangeEventArgs e)
        {
            base.OnRowDeleted(e);

            if ((FeatureDataRowDeleted != null))
            {
                FeatureDataRowDeleted(this, new FeatureDataRowChangeEventArgs((FeatureDataRow)e.Row,
                                                                              e.Action));
            }
        }

        /// <summary>
        /// Raises the FeatureDataRowDeleting event. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRowDeleting(DataRowChangeEventArgs e)
        {
            base.OnRowDeleting(e);

            if ((FeatureDataRowDeleting != null))
            {
                FeatureDataRowDeleting(this, new FeatureDataRowChangeEventArgs((FeatureDataRow)e.Row,
                                                                               e.Action));
            }
        }

        #endregion

        #region IEnumerable members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IEnumerable<IFeatureDataRecord> members

        IEnumerator<IFeatureDataRecord> IEnumerable<IFeatureDataRecord>.GetEnumerator()
        {
            foreach (FeatureDataRow row in this)
            {
                yield return row;
            }
        }

        #endregion

        #region Internal helper methods and properties

        //internal Boolean Contains(IGeometry geometry)
        //{
        //    return _cache.Contains(geometry);
        //}

        internal FeatureDataView DefaultViewInternal
        {
            get { return _getDefaultView(this); }
        }

        internal FeatureDataRow FindMergeTarget(FeatureDataRow sourceRow, Object dataKey, Object index)
        {
            return _findMergeTarget(this, sourceRow, dataKey, index);
        }

        internal void EnableConstraints()
        {
            _enableConstraints(this);
        }

        internal Boolean EnforceConstraints
        {
            get { return _getEnforceConstraints(this); }
            set { _setEnforceConstraints(this, value); }
        }

        internal void EvaluateExpressions()
        {
            _evaluateExpression(this);
        }

        internal Boolean MergingData
        {
            get { return _getMergingData(this); }
            set { _setMergingData(this, value); }
        }

        //internal void NotifyFeaturesNotFound(FeatureSpatialExpression notFoundExpression)
        //{
        //    EventHandler<FeaturesNotFoundEventArgs> e = FeaturesNotFound;

        //    if (e != null)
        //    {
        //        FeaturesNotFoundEventArgs args = new FeaturesNotFoundEventArgs(notFoundExpression);
        //        e(this, args);
        //    }
        //}

        internal void RestoreIndexEvents(Boolean forceReset)
        {
            _restoreIndexEvents(this, forceReset);
        }

        internal void RowGeometryChanged(FeatureDataRow row, IGeometry oldGeometry)
        {
            if (IsSpatiallyIndexed)
            {
                _rTreeIndex.Remove(row);
                _rTreeIndex.Insert(row);
            }

            // TODO: how to update the cache when the row geometry changes?
            // Replacing the geometry in the expression would break the expression
            // What about adding a new expression (and perhaps a new expression type)?
            //_cache.Expire(oldGeometry);
        }

        internal Boolean SuspendEnforceConstraints
        {
            get { return _getSuspendEnforceConstraints(this); }
            set { _setSuspendEnforceConstraints(this, value); }
        }

        internal void SuspendIndexEvents()
        {
            _suspendIndexEvents(this);
        }

        internal UniqueConstraint PrimaryKeyConstraint
        {
            get { return _getPrimaryKeyConstraint(this); }
        }

        internal FeatureDataRow MergeRowInternal(FeatureDataRow sourceRow, FeatureDataRow targetRow, Boolean preserveChanges,
                                       Object index)
        {
            FeatureDataRow mergedRow = _mergeRow(this, sourceRow, targetRow, preserveChanges, index) as FeatureDataRow;
            return mergedRow;
        }

        #endregion

        #region Private static helper methods

        private static GetEnforceConstraintsDelegate generateGetEnforceConstraintsDelegate()
        {
            DynamicMethod get_EnforceConstraintsMethod = new DynamicMethod("get_EnforceConstraints_DynamicMethod",
                                                                           typeof(Boolean),
                                                                           new Type[] { typeof(FeatureDataTable) },
                                                                           typeof(DataTable));

            ILGenerator il = get_EnforceConstraintsMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            PropertyInfo enforceConstraintsProperty = typeof(DataTable).GetProperty("EnforceConstraints",
                                                                                    nonPublicInstance);
            MethodInfo getMethod = enforceConstraintsProperty.GetGetMethod(true);
            il.Emit(OpCodes.Call, getMethod);
            il.Emit(OpCodes.Ret);

            return get_EnforceConstraintsMethod.CreateDelegate(typeof(GetEnforceConstraintsDelegate))
                   as GetEnforceConstraintsDelegate;
        }

        private static SetEnforceConstraintsDelegate generateSetEnforceConstraintsDelegate()
        {
            DynamicMethod set_EnforceConstraintsMethod = new DynamicMethod("set_EnforceConstraints_DynamicMethod",
                                                                           null,
                                                                           new Type[]
                                                                               {
                                                                                   typeof (FeatureDataTable), typeof (Boolean)
                                                                               },
                                                                           typeof(DataTable));

            ILGenerator il = set_EnforceConstraintsMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            PropertyInfo enforceConstraintsProperty = typeof(DataTable).GetProperty("EnforceConstraints",
                                                                                    nonPublicInstance);
            MethodInfo setMethod = enforceConstraintsProperty.GetSetMethod(true);
            il.Emit(OpCodes.Call, setMethod);
            il.Emit(OpCodes.Ret);

            return set_EnforceConstraintsMethod.CreateDelegate(typeof(SetEnforceConstraintsDelegate))
                   as SetEnforceConstraintsDelegate;
        }

        private static EvaluateExpressionsDelegate generateEvaluateExpressionsDelegate()
        {
            DynamicMethod evaluateExpressionsMethod =
                new DynamicMethod("FeatureDataTable_EvaluateExpressionsMethod_DynamicMethod",
                                  MethodAttributes.Public | MethodAttributes.Static,
                                  CallingConventions.Standard,
                                  null,
                                  new Type[] { typeof(DataTable) },
                                  typeof(DataTable),
                                  false);

            ILGenerator il = evaluateExpressionsMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            MethodInfo evaluateExpressionsInfo = typeof(DataTable).GetMethod("EvaluateExpressions",
                                                                              nonPublicInstance,
                                                                              null,
                                                                              Type.EmptyTypes,
                                                                              null);
            il.Emit(OpCodes.Call, evaluateExpressionsInfo);
            il.Emit(OpCodes.Ret);

            return evaluateExpressionsMethod.CreateDelegate(typeof(EvaluateExpressionsDelegate))
                   as EvaluateExpressionsDelegate;
        }

        private static GetSuspendEnforceConstraintsDelegate generateGetSuspendEnforceConstraintsDelegate()
        {
            DynamicMethod get_SuspendEnforceConstraintsMethod =
                new DynamicMethod("get_SuspendEnforceConstraints_DynamicMethod",
                                  typeof(Boolean),
                                  new Type[] { typeof(FeatureDataTable) },
                                  typeof(DataTable));

            ILGenerator il = get_SuspendEnforceConstraintsMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            PropertyInfo suspendConstraintsProperty = typeof(DataTable).GetProperty("SuspendEnforceConstraints",
                                                                                    nonPublicInstance);
            MethodInfo getMethod = suspendConstraintsProperty.GetGetMethod(true);
            il.Emit(OpCodes.Call, getMethod);
            il.Emit(OpCodes.Ret);

            return get_SuspendEnforceConstraintsMethod.CreateDelegate(typeof(GetSuspendEnforceConstraintsDelegate))
                   as GetSuspendEnforceConstraintsDelegate;
        }

        private static SetSuspendEnforceConstraintsDelegate generateSetSuspendEnforceConstraintsDelegate()
        {
            DynamicMethod set_SuspendEnforceConstraintsMethod =
                new DynamicMethod("set_SuspendEnforceConstraints_DynamicMethod",
                                  null,
                                  new Type[] { typeof(FeatureDataTable), typeof(Boolean) },
                                  typeof(DataTable));

            ILGenerator il = set_SuspendEnforceConstraintsMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            PropertyInfo suspendConstraintsProperty = typeof(DataTable).GetProperty("SuspendEnforceConstraints",
                                                                                    nonPublicInstance);
            MethodInfo setMethod = suspendConstraintsProperty.GetSetMethod(true);
            il.Emit(OpCodes.Call, setMethod);
            il.Emit(OpCodes.Ret);

            return set_SuspendEnforceConstraintsMethod.CreateDelegate(typeof(SetSuspendEnforceConstraintsDelegate))
                   as SetSuspendEnforceConstraintsDelegate;
        }

        private static EnableConstraintsDelegate generateEnableConstraintsDelegate()
        {
            DynamicMethod enableConstraintsMethod =
                new DynamicMethod("FeatureDataTable_EnableConstraints_DynamicMethod",
                                  MethodAttributes.Public | MethodAttributes.Static,
                                  CallingConventions.Standard,
                                  null,
                                  new Type[] { typeof(DataTable) },
                                  typeof(DataTable),
                                  false);

            ILGenerator il = enableConstraintsMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            MethodInfo enableConstraintsInfo = typeof(DataTable).GetMethod("EnableConstraints",
                                                                            nonPublicInstance,
                                                                            null,
                                                                            Type.EmptyTypes,
                                                                            null);
            il.Emit(OpCodes.Call, enableConstraintsInfo);
            il.Emit(OpCodes.Ret);

            return enableConstraintsMethod.CreateDelegate(typeof(EnableConstraintsDelegate))
                   as EnableConstraintsDelegate;
        }

        private const BindingFlags nonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;
        private static GetMergingDataDelegate generateGetMergingDataDelegate()
        {
            DynamicMethod get_MergingDataMethod = new DynamicMethod("FeatureDataTable_get_mergingData_DynamicMethod",
                                                                    typeof(Boolean),
                                                                    new Type[] { typeof(FeatureDataTable) },
                                                                    typeof(DataTable));

            ILGenerator il = get_MergingDataMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, typeof(DataTable).GetField("mergingData", nonPublicInstance));
            il.Emit(OpCodes.Ret);

            return get_MergingDataMethod.CreateDelegate(typeof(GetMergingDataDelegate))
                   as GetMergingDataDelegate;
        }

        private static SetMergingDataDelegate generateSetMergingDataDelegate()
        {
            DynamicMethod set_MergingDataMethod = new DynamicMethod("FeatureDataTable_set_mergingData_DynamicMethod",
                                                                    null,
                                                                    new Type[] { typeof(FeatureDataTable), typeof(Boolean) },
                                                                    typeof(DataTable));

            ILGenerator il = set_MergingDataMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, typeof(DataTable).GetField("mergingData", nonPublicInstance));
            il.Emit(OpCodes.Ret);

            return set_MergingDataMethod.CreateDelegate(typeof(SetMergingDataDelegate))
                   as SetMergingDataDelegate;
        }

        private static GetDefaultViewDelegate generateGetDefaultViewDelegate()
        {
            DynamicMethod get_DefaultViewMethod = new DynamicMethod("FeatureDataTable_get_defaultView_DynamicMethod",
                                                                    typeof(FeatureDataView),
                                                                    new Type[] { typeof(FeatureDataTable) },
                                                                    typeof(DataTable));

            ILGenerator il = get_DefaultViewMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, typeof(DataTable).GetField("defaultView", nonPublicInstance));
            il.Emit(OpCodes.Ret);

            return get_DefaultViewMethod.CreateDelegate(typeof(GetDefaultViewDelegate))
                   as GetDefaultViewDelegate;
        }

        private static RestoreIndexEventsDelegate generateRestoreIndexEventsDelegate()
        {
            DynamicMethod restoreIndexEventsMethod =
                new DynamicMethod("FeatureDataTable_RestoreIndexEvents_DynamicMethod",
                                  MethodAttributes.Public | MethodAttributes.Static,
                                  CallingConventions.Standard,
                                  null,
                                  new Type[] { typeof(DataTable), typeof(Boolean) },
                                  typeof(DataTable),
                                  false);

            ILGenerator il = restoreIndexEventsMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            MethodInfo restoreIndexEventsInfo = typeof(DataTable).GetMethod("RestoreIndexEvents",
                                                                             nonPublicInstance,
                                                                             null,
                                                                             new Type[] { typeof(Boolean) },
                                                                             null);
            il.Emit(OpCodes.Call, restoreIndexEventsInfo);
            il.Emit(OpCodes.Ret);

            return
                (RestoreIndexEventsDelegate)
                restoreIndexEventsMethod.CreateDelegate(typeof(RestoreIndexEventsDelegate));
        }

        private static SuspendIndexEventsDelegate generateSuspendIndexEventsDelegate()
        {
            DynamicMethod suspendIndexEventsMethod =
                new DynamicMethod("FeatureDataTable_SuspendIndexEvents_DynamicMethod",
                                  MethodAttributes.Public | MethodAttributes.Static,
                                  CallingConventions.Standard,
                                  null,
                                  new Type[] { typeof(DataTable) },
                                  typeof(DataTable),
                                  false);

            ILGenerator il = suspendIndexEventsMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            MethodInfo suspendIndexEventsInfo = typeof(DataTable).GetMethod("SuspendIndexEvents",
                                                                             nonPublicInstance,
                                                                             null,
                                                                             Type.EmptyTypes,
                                                                             null);
            il.Emit(OpCodes.Call, suspendIndexEventsInfo);
            il.Emit(OpCodes.Ret);

            return suspendIndexEventsMethod.CreateDelegate(typeof(SuspendIndexEventsDelegate))
                   as SuspendIndexEventsDelegate;
        }

        private static CloneToDelegate generateCloneToDelegate()
        {
            DynamicMethod cloneToMethod = new DynamicMethod("FeatureDataTable_CloneTo_DynamicMethod",
                                                            MethodAttributes.Public | MethodAttributes.Static,
                                                            CallingConventions.Standard,
                                                            typeof(DataTable),
                                                            new Type[]
                                                                {
                                                                    typeof (DataTable), typeof (DataTable),
                                                                    typeof (DataSet), typeof (Boolean)
                                                                },
                                                            typeof(DataTable),
                                                            false);

            ILGenerator il = cloneToMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_3);
            MethodInfo cloneToInfo = typeof(DataTable).GetMethod("CloneTo", nonPublicInstance);
            il.Emit(OpCodes.Call, cloneToInfo);
            il.Emit(OpCodes.Ret);

            return (CloneToDelegate)cloneToMethod.CreateDelegate(typeof(CloneToDelegate));
        }

        private static FindMergeTargetDelegate generateFindMergeTargetDelgate()
        {
            DynamicMethod findMergeTargetMethod =
                new DynamicMethod("FeatureDataTable_FindMergeTargetDelegate_DynamicMethod",
                                  typeof(FeatureDataRow),
                                  new Type[] { typeof(DataTable), typeof(DataRow), typeof(Object), typeof(Object) },
                                  typeof(DataTable));

            // No error or null checking here, since it's only internal code which calls
            // this method
            ILGenerator il = findMergeTargetMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Unbox_Any, AdoNetInternalTypes.DataKeyType);
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Castclass, AdoNetInternalTypes.IndexType);
            MethodInfo findMergeTargetInfo = typeof(DataTable).GetMethod("FindMergeTarget", nonPublicInstance);
            il.Emit(OpCodes.Call, findMergeTargetInfo);
            il.Emit(OpCodes.Ret);

            return findMergeTargetMethod.CreateDelegate(typeof(FindMergeTargetDelegate))
                   as FindMergeTargetDelegate;
        }

        private static GetPrimaryKeyConstraintDelegate generatePrimaryKeyConstraintDelegate()
        {
            DynamicMethod get_PrimaryKeyConstraintMethod =
                new DynamicMethod("FeatureDataTable_get_primaryKey_DynamicMethod",
                                  typeof(UniqueConstraint),
                                  new Type[] { typeof(FeatureDataTable) },
                                  typeof(DataTable));

            ILGenerator il = get_PrimaryKeyConstraintMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, typeof(DataTable).GetField("primaryKey", nonPublicInstance));
            il.Emit(OpCodes.Ret);

            return get_PrimaryKeyConstraintMethod.CreateDelegate(typeof(GetPrimaryKeyConstraintDelegate))
                   as GetPrimaryKeyConstraintDelegate;
        }

        private static MergeRowDelegate generateMergeRowDelegate()
        {
            DynamicMethod mergeRowMethod = new DynamicMethod("FeatureDataTable_MergeRow_DynamicMethod",
                                                             typeof(DataRow),
                                                             new Type[]
                                                                 {
                                                                     typeof (FeatureDataTable),
                                                                     typeof (FeatureDataRow),
                                                                     typeof (FeatureDataRow),
                                                                     typeof (Boolean),
                                                                     typeof (Object)
                                                                 },
                                                             typeof(DataTable));

            ILGenerator il = mergeRowMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_3);
            il.Emit(OpCodes.Ldarg_S, 4);
            il.Emit(OpCodes.Castclass, AdoNetInternalTypes.IndexType);
            MethodInfo mergeRowInfo = typeof(DataTable).GetMethod(
                "MergeRow",
                nonPublicInstance,
                null,
                new Type[]
                    {
                        typeof (FeatureDataRow), typeof (FeatureDataRow),
                        typeof (Boolean), AdoNetInternalTypes.IndexType
                    },
                null);
            il.Emit(OpCodes.Call, mergeRowInfo);
            il.Emit(OpCodes.Ret);

            return mergeRowMethod.CreateDelegate(typeof(MergeRowDelegate))
                   as MergeRowDelegate;
        }

        #endregion

        #region Private helper methods

        private void computeExtents()
        {
            foreach (FeatureDataRow row in this)
            {
                if (_extents == null)
                {
                    _extents = _geoFactory.CreateExtents(row.Extents);
                }
                else
                {
                    _extents.ExpandToInclude(row.Extents);
                }
            }
        }

        //private void notifyIfCacheDoesNotContain(IGeometry geometry)
        //{
        //    if (geometry.IsEmpty || _cache.Contains(geometry))
        //    {
        //        return;
        //    }

        //    FeatureSpatialExpression notFound = new FeatureSpatialExpression(geometry,
        //                                                                     SpatialExpressionType.Intersects);

        //    NotifyFeaturesNotFound(notFound);
        //}

        private static void copyRow(FeatureDataRow source, FeatureDataRow target)
        {
            if (source.Table.Columns.Count != target.Table.Columns.Count)
            {
                throw new InvalidOperationException("Can't import a feature row with a different " +
                                                    "number of columns.");
            }

            for (Int32 columnIndex = 0; columnIndex < source.Table.Columns.Count; columnIndex++)
            {
                target[columnIndex] = source[columnIndex];
            }

            target.Geometry = source.Geometry == null
                                  ? null
                                  : source.Geometry.Clone();
        }

        private void initializeSpatialIndex()
        {
            // TODO: implement Post-optimization restructure strategy
            IIndexRestructureStrategy<IExtents, FeatureDataRow> restructureStrategy
                = new NullRestructuringStrategy<IExtents, FeatureDataRow>();
            RestructuringHuristic restructureHeuristic
                = new RestructuringHuristic(RestructureOpportunity.None, 4.0);
            IItemInsertStrategy<IExtents, FeatureDataRow> insertStrategy
                = new GuttmanQuadraticInsert<FeatureDataRow>(_geoFactory);
            INodeSplitStrategy<IExtents, FeatureDataRow> nodeSplitStrategy
                = new GuttmanQuadraticSplit<FeatureDataRow>(_geoFactory);
            IdleMonitor idleMonitor = null;

            _rTreeIndex = new SelfOptimizingDynamicSpatialIndex<FeatureDataRow>(_geoFactory,
                                                                                restructureStrategy,
                                                                                restructureHeuristic, insertStrategy,
                                                                                nodeSplitStrategy,
                                                                                new DynamicRTreeBalanceHeuristic(),
                                                                                idleMonitor);
        }

        #endregion

        #region Obsoleted commented-out code

        ///// <summary>
        ///// Initializes a new instance of the FeatureDataTable class with no arguments.
        ///// </summary>
        //public FeatureDataTable()
        //    : this(null, null)
        //{ }

        ///// <summary>
        ///// Initializes a new instance of the FeatureDataTable class with the given
        ///// table name.
        ///// </summary>
        //public FeatureDataTable(String tableName)
        //    : base(tableName, null)
        //{
        //    Constraints.CollectionChanged += OnConstraintsChanged;
        //}

        //public IGeometry LoadedRegion
        //{
        //    get
        //    {
        //        if (_loadedRegion == null)
        //        {
        //            computeLoadedRegion();
        //        }

        //        return _loadedRegion ?? _empty;
        //    }
        //}

        //private void computeLoadedRegion()
        //{

        //}
        #endregion
    }
}