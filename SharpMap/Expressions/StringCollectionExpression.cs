﻿// Copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
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
using System.Collections.Generic;
#if DOTNET35
using Enumerable = System.Linq.Enumerable;
#else
using Enumerable = GeoAPI.DataStructures.Enumerable;
#endif

namespace SharpMap.Expressions
{
    public class StringCollectionExpression : CollectionExpression<String>
    {
        private readonly StringComparison _comparison;

        public StringCollectionExpression(IEnumerable<String> collection)
            : this(collection, StringComparison.CurrentCultureIgnoreCase) { }

        public StringCollectionExpression(IEnumerable<String> collection, StringComparison comparison)
            : base(collection)
        {
            _comparison = comparison;
        }

        private StringCollectionExpression(IEnumerable<String> collection, 
                                           StringComparison comparison, 
                                           IEqualityComparer<String> comparer)
            : base(collection, comparer)
        {
            _comparison = comparison;
        }

        public StringComparison Comparison
        {
            get { return _comparison; }
        }

        public override Boolean Contains(Expression other)
        {
            return Equals(other);
        }

        public override Expression Clone()
        {
            IEnumerable<String> values = Collection;
            return new StringCollectionExpression(Enumerable.ToArray(values), _comparison, Comparer);
        }
    }
}
