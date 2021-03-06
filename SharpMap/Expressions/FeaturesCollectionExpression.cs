﻿
using System.Collections.Generic;
using SharpMap.Data;

namespace SharpMap.Expressions
{
    public class FeaturesCollectionExpression : CollectionExpression
    {
        public FeaturesCollectionExpression(IEnumerable<IFeatureDataRecord> collection)
            : base(collection) { }

        public new IEnumerable<IFeatureDataRecord> Collection
        {
            get { return base.Collection as IEnumerable<IFeatureDataRecord>; }
        }
    }
}
