﻿/*
 *  The attached / following is part of SharpMap.Data.Providers.Db
 *  SharpMap.Data.Providers.Db is free software © 2008 Newgrove Consultants Limited, 
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using SharpMap.Data.Providers.Db.Expressions;
using SharpMap.Expressions;
#if DOTNET35
using Processor = System.Linq.Enumerable;
using Enumerable = System.Linq.Enumerable;
using Caster = GeoAPI.DataStructures.Caster;
#else

#endif

namespace SharpMap.Data.Providers.Db
{
    public abstract class ExpressionTreeToSqlCompilerBase<TOid>
    {
        private readonly Expression _expression;
        private readonly Dictionary<object, IDataParameter> _parameterCache = new Dictionary<object, IDataParameter>();
        private readonly List<string> _parameterDeclarations = new List<string>();
        private readonly List<string> _projectedColumns = new List<string>();
        private readonly List<ProviderPropertyExpression> _providerProperties = new List<ProviderPropertyExpression>();
        private readonly List<string> _tableJoinStrings = new List<string>();
        private string _sqlColumns;
        private string _sqlJoinClauses;
        private string _sqlParamDeclarations;
        private string _sqlWhereClause;
        private string _orderByClause;
        private bool built;


        protected ExpressionTreeToSqlCompilerBase(
            SpatialDbProviderBase<TOid> provider,
            Expression expression)
        {
            _expression = expression;
            Provider = provider;
        }

        protected SpatialDbProviderBase<TOid> Provider { get; set; }

        public string SqlWhereClause
        {
            get
            {
                EnsureBuilt();
                return _sqlWhereClause;
            }
        }

        public string OrderByClause
        {
            get
            {
                EnsureBuilt();
                return _orderByClause;
            }
        }

        public string SqlParamDeclarations
        {
            get
            {
                EnsureBuilt();
                _sqlParamDeclarations = _sqlParamDeclarations ??
                                        string.Join(" ", Enumerable.ToArray(ParameterDeclarations));
                return _sqlParamDeclarations;
            }
        }

        public string SqlJoinClauses
        {
            get
            {
                EnsureBuilt();
                _sqlJoinClauses = _sqlJoinClauses ?? string.Join(" ", Enumerable.ToArray(TableJoinStrings));
                return _sqlJoinClauses;
            }
        }

        public string SqlColumns
        {
            get
            {
                EnsureBuilt();
                _sqlColumns = _sqlColumns ??
                              string.Join(", ",
                                          Enumerable.ToArray(Provider.FormatColumnNames(true, true,
                                                                                        InternalProjectedColumns)));
                return _sqlColumns;
            }
        }

        public Dictionary<object, IDataParameter> ParameterCache
        {
            get { return _parameterCache; }
        }

        protected IList<string> TableJoinStrings
        {
            get { return _tableJoinStrings; }
        }

        protected IList<string> ParameterDeclarations
        {
            get { return _parameterDeclarations; }
        }

        protected IList<string> InternalProjectedColumns
        {
            get { return _projectedColumns; }
        }

        public IList<string> ProjectedColumns
        {
            get
            {
                EnsureBuilt();
                return InternalProjectedColumns;
            }
        }

        public IList<ProviderPropertyExpression> ProviderProperties
        {
            get
            {
                EnsureBuilt();
                return _providerProperties;
            }
        }

        #region database info

        private static readonly Dictionary<Type, Func<object, object, IDataParameter>> _CreateParameterDelegateTypeMap =
            new Dictionary<Type, Func<object, object, IDataParameter>>();


        /// <summary>
        /// Create or retrieves from cache a parameter using the classes supplied parameter prefix.
        /// The name is autogenerated. Use the ParameterName property of the returned param in your queries.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual IDataParameter CreateParameter<TValue>(TValue value)
        {
            ///if TValue is System.Object we need to expand it to generate the correct parameter type
            if (typeof(TValue) == typeof(object) && value.GetType() != typeof(object))
                return CreateParameterFromObject(value);

            object key = value;
            if (Equals(null, value))
                key = new NullValue<TValue>();

            if (ParameterCache.ContainsKey(key))
                return ParameterCache[key];

            IDataParameter p;
            if (value is IGeometry)
                p = Provider.DbUtility.CreateParameter(string.Format("iparam{0}", ParameterCache.Count),
                                                       ((IGeometry)value).AsBinary(),
                                                       ParameterDirection.Input);


            else
                p = Provider.DbUtility.CreateParameter(string.Format("iparam{0}", ParameterCache.Count),
                                                       value,
                                                       ParameterDirection.Input);

            ParameterCache.Add(key, p);

            return p;
        }

        protected IDataParameter CreateParameterFromObject(object value)
        {
            Func<object, object, IDataParameter> dlgt;
            ///method sig for our delegate method. The method created is static and requires the first parameter to be a 
            /// reference to an instance the calling class
            Type tValue = value.GetType();

            if (!_CreateParameterDelegateTypeMap.TryGetValue(tValue, out dlgt))
            //see if we have already created this method.
            {
                lock (_CreateParameterDelegateTypeMap)
                {
                    //aquire a lock and test again
                    if (!_CreateParameterDelegateTypeMap.TryGetValue(tValue, out dlgt))
                    {
                        DynamicMethod m = new DynamicMethod(string.Format("CreateParam_{0}", tValue),
                                                            MethodAttributes.Public | MethodAttributes.Static,
                                                            CallingConventions.Standard, typeof(IDataParameter),
                                                            new[] { typeof(object), typeof(object) }, GetType(), true);

                        Type classType = GetType();

                        ///get the method info for the method we want to call e.g CreateParameter<classType>(classType val)
                        MethodInfo mi = classType.GetMethod("CreateParameter",
                                                            BindingFlags.NonPublic | BindingFlags.Public |
                                                            BindingFlags.Instance);

                        //construct a generic method with the same type argument as the value
                        MethodInfo mig = mi.MakeGenericMethod(tValue);

                        //create our ILGenerator
                        ILGenerator generator = m.GetILGenerator();
                        generator.Emit(OpCodes.Ldarg_0);
                        // load the first argument onto the stack - equivalent to 'this' within the method
                        generator.Emit(OpCodes.Ldarg_1); // load the first argument (the value) onto the stack 
                        if (tValue.IsValueType)
                            generator.Emit(OpCodes.Unbox_Any, tValue); //unbox value types
                        else
                            generator.Emit(OpCodes.Castclass, tValue); //cast ref types to the correct arg type
                        generator.EmitCall(OpCodes.Callvirt, mig, null); // call the method passing in the param
                        generator.Emit(OpCodes.Ret); //return the value

                        //Put it all together into a callable method
                        dlgt =
                            (Func<object, object, IDataParameter>)
                            m.CreateDelegate(typeof(Func<object, object, IDataParameter>));

                        _CreateParameterDelegateTypeMap.Add(tValue, dlgt);
                    }
                }
            }

            return dlgt(this, value);
        }

        #endregion

        private void EnsureBuilt()
        {
            if (!built)
            {
                built = true;
                _sqlWhereClause = BuildSql();
                _orderByClause = BuildOrderBy();
            }
        }

        private string BuildOrderBy()
        {
            QueryExpression query = _expression as QueryExpression;
            if (query == null)
                return string.Empty;
            StringBuilder sb = new StringBuilder();
            VisitExpression(sb, query.Sort);
            return sb.ToString();
        }

        private string BuildSql()
        {
            StringBuilder sb = new StringBuilder();
            VisitExpression(sb, _expression);
            return sb.ToString();
        }

        protected virtual void VisitExpression(StringBuilder builder, Expression exp)
        {
            if (exp == null)
                return;
            if (exp is ProviderQueryExpression)
                VisitProviderQueryExpression(builder, (ProviderQueryExpression)exp);
            else if (exp is SortExpressionCollectionExpression)
                VisitSortExpressionCollectionExpression(builder, exp as SortExpressionCollectionExpression);
            else if (exp is FeatureQueryExpression)
                VisitFeatureQueryExpression(builder, (FeatureQueryExpression)exp);
            else if (exp is ProviderPropertiesExpression)
                VisitProviderPropertiesExpression(builder, (ProviderPropertiesExpression)exp);
            else if (exp is ProviderPropertyExpression)
                VisitProviderPropertyExpression(builder, (ProviderPropertyExpression)exp);
            else if (exp is ProjectionExpression)
                VisitProjectionExpression((ProjectionExpression)exp);
            else if (exp is SpatialBinaryExpression)
                VisitSpatialBinaryExpression(builder, (SpatialBinaryExpression)exp);
            else if (exp is QueryExpression)
                VisitQueryExpression(builder, (QueryExpression)exp);
            else if (exp is CollectionBinaryExpression)
                VisitCollectionBinaryExpression(builder, (CollectionBinaryExpression)exp);
            else if (exp is BinaryExpression)
                VisitBinaryExpression(builder, (BinaryExpression)exp);
            else if (exp is AttributeBinaryStringExpression)
                VisitBinaryStringExpression(builder, (AttributeBinaryStringExpression)exp);
            else if (exp is LiteralExpression)
                VisitValueExpression(builder, (LiteralExpression)exp);
            else if (exp is PropertyNameExpression)
                VisitAttributeExpression(builder, (PropertyNameExpression)exp);
            else if (exp is CollectionExpression)
                VisitCollectionExpression(builder, (CollectionExpression)exp);
            else if (exp is OidExpression)
                VisitOidExpression(builder, (OidExpression)exp);
            else if (exp is SpatialAnalysisExpression)
                VisitSpatialAnalysisExpression(builder, (SpatialAnalysisExpression)exp);
            else
                throw new NotImplementedException(string.Format("Unknown Expression Type {0}", exp.GetType()));
        }

        private void VisitSortExpressionCollectionExpression(StringBuilder builder, SortExpressionCollectionExpression sortExpressionCollectionExpression)
        {
            int i = 0;
            foreach (SortExpression expr in sortExpressionCollectionExpression)
            {
                if (expr == null)
                    continue;

                if (i > 0)
                    builder.Append(", ");

                VisitExpression(builder, expr.Expression);

                builder.Append(expr.Direction == SortOrder.Ascending ? " ASC " : " DESC ");

                i++;
            }
        }


        protected abstract void VisitSpatialAnalysisExpressionInternal(StringBuilder builder,
                                                                       SpatialAnalysisExpression expression);


        private void VisitOidExpression(StringBuilder builder, OidExpression oidExpression)
        {
            builder.AppendFormat(" {0} ", Provider.OidColumn);
        }

        protected virtual void VisitProviderQueryExpression(StringBuilder builder, ProviderQueryExpression expression)
        {
            VisitExpression(builder, expression.ProviderProperties);
            VisitExpression(builder, expression.Projection);
            VisitExpression(builder, expression.Predicate);
        }

        protected virtual void VisitProviderPropertyExpression(StringBuilder builder, ProviderPropertyExpression exp)
        {
            _providerProperties.Add(exp);
        }

        protected virtual void VisitProviderPropertiesExpression(StringBuilder builder, ProviderPropertiesExpression exp)
        {
            foreach (ProviderPropertyExpression providerPropertyExpression in exp.ProviderProperties.Collection)
                VisitExpression(builder, providerPropertyExpression);
        }


        protected virtual void VisitQueryExpression(StringBuilder builder, QueryExpression exp)
        {
            if (exp == null)
                return;

            builder.Append("(");
            VisitExpression(builder, exp.Projection);
            VisitExpression(builder, exp.Predicate);
            builder.Append(")");
        }

        protected virtual void VisitProjectionExpression(ProjectionExpression exp)
        {
            if (exp == null)
                return;

            if (exp is AllAttributesExpression)
            {
                foreach (string s in Provider.SelectAllColumnNames())
                    InternalProjectedColumns.Add(s);
            }

            else if (exp is AttributesProjectionExpression)
                VisitAttributeProjectionExpression((AttributesProjectionExpression)exp);
        }

        protected virtual void VisitAttributeProjectionExpression(AttributesProjectionExpression exp)
        {
            if (exp == null)
                return;

            foreach (PropertyNameExpression pn in exp.Attributes.Collection)
            {
                InternalProjectedColumns.Add(pn.PropertyName);
                //InternalProjectedColumns.Add(
                //    string.Compare(pn.PropertyName, Provider.GeometryColumn, StringComparison.InvariantCultureIgnoreCase) ==
                //    0
                //        ? string.Format(Provider.GeometryColumnConversionFormatString,
                //                        Provider.QualifyColumnName(pn.PropertyName))
                //        : Provider.QualifyColumnName(pn.PropertyName));
            }
        }

        private IEnumerable<string> EnumerableToParameterConverter(IEnumerable enu)
        {
            foreach (object o in enu)
                yield return CreateParameter(o).ParameterName;
        }

        protected virtual void VisitCollectionExpression(StringBuilder builder, CollectionExpression exp)
        {
            if (exp == null)
                return;

            StringBuilder sb = new StringBuilder();

            sb.Append(string.Join(", ",
                                  Enumerable.ToArray(EnumerableToParameterConverter(exp.Collection))));

            if (sb.Length > 0)
                builder.AppendFormat("({0})", sb);
        }

        protected virtual void VisitCollectionBinaryExpression(StringBuilder builder, CollectionBinaryExpression exp)
        {
            if (exp == null)
                return;

            if (Enumerable.Count(Caster.Cast<object>(exp.Right.Collection)) == 0)
                return;

            StringBuilder sb = new StringBuilder();
            VisitExpression(sb, exp.Left);
            sb.Append(GetCollectionExpressionString(exp.Op));
            VisitExpression(sb, exp.Right);

            if (sb.Length > 0)
                builder.AppendFormat("({0})", sb);
        }

        protected virtual string GetCollectionExpressionString(CollectionOperator op)
        {
            switch (op)
            {
                case CollectionOperator.In:
                    return " IN ";
                case CollectionOperator.NotIn:
                    return " NOT IN ";
                case CollectionOperator.All:
                case CollectionOperator.Any:
                default:
                    throw new NotImplementedException();
            }
        }

        protected virtual void VisitAttributeExpression(StringBuilder builder, PropertyNameExpression exp)
        {
            if (exp == null)
                return;

            builder.Append(
                string.Compare(Provider.GeometryColumn, exp.PropertyName, StringComparison.InvariantCultureIgnoreCase) ==
                0
                    ? string.Format(Provider.GeometryColumnConversionFormatString,
                                    Provider.QualifyColumnName(exp.PropertyName))
                    : Provider.QualifyColumnName(exp.PropertyName));
        }


        protected virtual void VisitValueExpression(StringBuilder builder, LiteralExpression exp)
        {
            if (exp == null)
                return;

            if (Equals(exp.Value, null))
            {
                builder.Append(" NULL ");
                return;
            }


            builder.AppendFormat(CreateParameter(exp.Value).ParameterName);
        }


        protected virtual void VisitFeatureQueryExpression(StringBuilder builder, FeatureQueryExpression exp)
        {
            if (exp == null)
                return;

            if (exp.Projection != null)
                VisitExpression(builder, exp.Projection);

            StringBuilder sb = new StringBuilder();

            if (exp.IsMultiAttributePredicateNonEmpty)
                VisitExpression(sb, exp.MultiAttributePredicate);
            else if (exp.IsSingleAttributePredicateNonEmpty)
                VisitExpression(sb, exp.SingleAttributePredicate);

            if (exp.OidPredicate != null)
            {
                if (sb.Length > 0)
                    sb.Append(" AND ");
                VisitExpression(sb, exp.OidPredicate);
            }

            if (exp.SpatialPredicate != null)
            {
                if (sb.Length > 0)
                    sb.Append(" AND ");
                VisitExpression(sb, exp.SpatialPredicate);
            }
            if (sb.Length > 0)
                builder.AppendFormat("({0})", sb);
        }

        protected virtual void VisitSpatialBinaryExpression(StringBuilder builder, SpatialBinaryExpression exp)
        {
            if (exp == null)
                return;

            if (exp.SpatialExpression is ExtentsExpression)
                WriteSpatialExtentsExpressionSql(builder, exp.Op, (exp.SpatialExpression).Extents);
            else if (exp.SpatialExpression is GeometryExpression)
                WriteSpatialGeometryExpressionSql(builder, exp.Op, ((GeometryExpression)exp.SpatialExpression).Geometry);
            else
                throw new NotImplementedException(string.Format("{0} is not implemented", exp.GetType()));
        }


        protected virtual void VisitBinaryExpression(StringBuilder builder, BinaryExpression exp)
        {
            if (exp == null)
                return;

            StringBuilder sb = new StringBuilder();

            VisitExpression(sb, exp.Left);

            if (sb.Length > 0)
                sb.Append(GetBinaryExpressionString(exp.Op, exp.Right));

            VisitExpression(sb, exp.Right);

            if (sb.Length > 0)
                builder.AppendFormat("({0})", sb);
        }

        protected virtual void VisitBinaryStringExpression(StringBuilder builder, AttributeBinaryStringExpression exp)
        {
            if (exp == null)
                return;

            StringBuilder sb = new StringBuilder();
            VisitExpression(sb, exp.Left);

            if (sb.Length > 0)
                sb.Append(GetBinaryStringExpressionString(exp.Op, exp.Right));

            VisitStringLiteralExpression(sb, exp.Op, exp.Right);

            if (sb.Length > 0)
                builder.AppendFormat("({0})", sb);
        }

        protected virtual void VisitStringLiteralExpression(StringBuilder sb, BinaryStringOperator binaryStringOperator,
                                                            LiteralExpression<string> expression)
        {
            if (string.IsNullOrEmpty(expression.Value))
            {
                sb.Append(" NULL ");
                return;
            }

            string v;
            switch (binaryStringOperator)
            {
                case BinaryStringOperator.StartsWith:
                    v = expression.Value + "%";
                    break;
                case BinaryStringOperator.EndsWith:
                    v = "%" + expression.Value;
                    break;
                case BinaryStringOperator.Contains:
                    v = "%" + expression.Value + "%";
                    break;
                case BinaryStringOperator.Equals:
                case BinaryStringOperator.NotEquals:
                default:
                    v = expression.Value;
                    break;
            }

            sb.Append(CreateParameter(v).ParameterName);
        }

        protected virtual string GetBinaryStringExpressionString(BinaryStringOperator binaryStringOperator,
                                                                 Expression right)
        {
            if (right is LiteralExpression
                && (Equals(null, ((LiteralExpression)right).Value)
                    || Equals(string.Empty, ((LiteralExpression)right).Value)))
            {
                switch (binaryStringOperator)
                {
                    case BinaryStringOperator.Equals:
                        return " IS ";
                    case BinaryStringOperator.NotEquals:
                        return " IS NOT ";
                    case BinaryStringOperator.Contains:
                    case BinaryStringOperator.EndsWith:
                    case BinaryStringOperator.StartsWith:
                    default:
                        break;
                }
            }

            switch (binaryStringOperator)
            {
                case BinaryStringOperator.Equals:
                    return " = ";
                case BinaryStringOperator.NotEquals:
                    return " <> ";
                case BinaryStringOperator.Contains:
                case BinaryStringOperator.EndsWith:
                case BinaryStringOperator.StartsWith:
                default:
                    return " LIKE ";
            }
        }

        protected virtual string GetBinaryExpressionString(BinaryOperator binaryOperator, Expression right)
        {
            if (right is LiteralExpression && Equals(null, ((LiteralExpression)right).Value))
            {
                switch (binaryOperator)
                {
                    case BinaryOperator.Equals:
                        return " IS ";
                    case BinaryOperator.NotEquals:
                        return " IS NOT ";
                }
            }

            switch (binaryOperator)
            {
                case BinaryOperator.And:
                    return " AND ";
                case BinaryOperator.Equals:
                    return " = ";
                case BinaryOperator.GreaterThan:
                    return " > ";
                case BinaryOperator.GreaterThanOrEqualTo:
                    return " >= ";
                case BinaryOperator.LessThan:
                    return " < ";
                case BinaryOperator.LessThanOrEqualTo:
                    return " <= ";
                case BinaryOperator.Like:
                    return " LIKE ";
                case BinaryOperator.NotEquals:
                    return " <> ";
                case BinaryOperator.Or:
                    return " OR ";
                default:
                    throw new ArgumentException(string.Format("Unknown binary operator {0}", binaryOperator));
            }
        }


        protected void WriteSpatialExtentsExpressionSql(StringBuilder builder, SpatialOperation spatialOperation,
                                                        IExtents ext)
        {
            WriteSpatialExtentsExpressionSqlInternal(builder, spatialOperation, TransformExtents(ext));
        }

        private IExtents TransformExtents(IExtents ext)
        {
            if (!Provider.SpatialReference.EqualParams(ext.SpatialReference))
            {
                if (Provider.CoordinateTransformationFactory == null)
                    throw new MissingCoordinateTransformationFactoryException(
                        "Data requires transformation however no CoordinateTransformationFactory is set");

                ICoordinateTransformation ct =
                    Provider.CoordinateTransformationFactory.CreateFromCoordinateSystems(ext.SpatialReference,
                                                                                         Provider.
                                                                                             OriginalSpatialReference);
                return ct.Transform(ext, Provider.GeometryFactory);
            }
            return ext;
        }

        protected IGeometry TransformGeometry(IGeometry geom)
        {
            if (geom.SpatialReference == null && Provider.SpatialReference == null)
                return geom;

            if (!Provider.SpatialReference.EqualParams(geom.SpatialReference))
            {
                if (Provider.CoordinateTransformationFactory == null)
                    throw new MissingCoordinateTransformationFactoryException(
                        "Data requires transformation however no CoordinateTransformationFactory is set");

                ICoordinateTransformation ct =
                    Provider.CoordinateTransformationFactory.CreateFromCoordinateSystems(geom.SpatialReference,
                                                                                         Provider.
                                                                                             OriginalSpatialReference);

                return ct.Transform(geom, Provider.GeometryFactory);
            }
            return geom;
        }

        protected abstract void WriteSpatialExtentsExpressionSqlInternal(StringBuilder builder,
                                                                         SpatialOperation spatialOperation, IExtents ext);


        protected void WriteSpatialGeometryExpressionSql(StringBuilder builder, SpatialOperation op,
                                                         IGeometry geom)
        {
            WriteSpatialGeometryExpressionSqlInternal(builder, op, TransformGeometry(geom));
        }

        private void VisitSpatialAnalysisExpression(StringBuilder builder, SpatialAnalysisExpression expression)
        {
            VisitSpatialAnalysisExpressionInternal(builder, expression);
        }


        protected abstract void WriteSpatialGeometryExpressionSqlInternal(StringBuilder builder, SpatialOperation op,
                                                                          IGeometry geom);

        #region Nested type: NullValue

        protected internal struct NullValue<T>
        {
        }

        #endregion
    }

    public class MissingCoordinateTransformationFactoryException : Exception
    {
        public MissingCoordinateTransformationFactoryException(string s)
            : base(s)
        {
            throw new NotImplementedException();
        }
    }
}