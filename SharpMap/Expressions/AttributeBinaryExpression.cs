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
using GeoAPI.Diagnostics;

namespace SharpMap.Expressions
{
    public class AttributeBinaryExpression : BinaryExpression
    {
        private TypeCode _valueTypeCode = TypeCode.Empty;

        protected AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, Expression right)
            : base(left, op, right) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, String value)
            : base(left, op, new StringExpression(value)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, Int16 value)
            : base(left, op, new ValueExpression<Int16>(value)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, Int32 value)
            : base(left, op, new ValueExpression<Int32>(value)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, Int64 value)
            : base(left, op, new ValueExpression<Int64>(value)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, Single value)
            : base(left, op, new ValueExpression<Single>(value)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, Double value)
            : base(left, op, new ValueExpression<Double>(value)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, Decimal value)
            : base(left, op, new ValueExpression<Decimal>(value)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, Guid value)
            : base(left, op, new ValueExpression<Guid>(value)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, UInt16 value)
            : base(left, op, new ValueExpression<UInt16>(value)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, UInt32 value)
            : base(left, op, new ValueExpression<UInt32>(value)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, UInt64 value)
            : base(left, op, new ValueExpression<UInt64>(value)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, Char value)
            : base(left, op, new ValueExpression<Char>(value)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, Byte value)
            : base(left, op, new ValueExpression<Byte>(value)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, SByte value)
            : base(left, op, new ValueExpression<SByte>(value)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, DateTime value)
            : base(left, op, new ValueExpression<DateTime>(value)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, IEnumerable<String> value)
            : base(left, op, new StringCollectionExpression(value)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, IEnumerable<Int16> values)
            : base(left, op, new CollectionExpression<Int16>(values)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, IEnumerable<Int32> values)
            : base(left, op, new CollectionExpression<Int32>(values)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, IEnumerable<Int64> values)
            : base(left, op, new CollectionExpression<Int64>(values)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, IEnumerable<Single> values)
            : base(left, op, new CollectionExpression<Single>(values)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, IEnumerable<Double> values)
            : base(left, op, new CollectionExpression<Double>(values)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, IEnumerable<Decimal> values)
            : base(left, op, new CollectionExpression<Decimal>(values)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, IEnumerable<Guid> values)
            : base(left, op, new CollectionExpression<Guid>(values)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, IEnumerable<UInt16> values)
            : base(left, op, new CollectionExpression<UInt16>(values)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, IEnumerable<UInt32> values)
            : base(left, op, new CollectionExpression<UInt32>(values)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, IEnumerable<UInt64> values)
            : base(left, op, new CollectionExpression<UInt64>(values)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, IEnumerable<Char> values)
            : base(left, op, new CollectionExpression<Char>(values)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, IEnumerable<Byte> values)
            : base(left, op, new CollectionExpression<Byte>(values)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, IEnumerable<SByte> values)
            : base(left, op, new CollectionExpression<SByte>(values)) { }

        public AttributeBinaryExpression(AttributeExpression left, BinaryOperator op, IEnumerable<DateTime> values)
            : base(left, op, new CollectionExpression<DateTime>(values)) { }
        
        public Boolean HasSingleValueExpression
        {
            get { return Right is ValueExpression; }
        }

        public Boolean HasCollectionValueExpression
        {
            get { return Right is CollectionExpression; }
        }

        public TypeCode ValueExpressionType
        {
            get
            {
                if (_valueTypeCode != TypeCode.Empty)
                {
                    return _valueTypeCode;
                }

                ValueExpression valueExpression = Right as ValueExpression;

                if (valueExpression != null)
                {
                    _valueTypeCode = Type.GetTypeCode(valueExpression.Value.GetType());
                }

                CollectionExpression collectionExpression = Right as CollectionExpression;

                if (collectionExpression != null)
                {
                    _valueTypeCode = Type.GetTypeCode(collectionExpression.GetType().GetGenericArguments()[0]);
                }

                Assert.IsNotEquals(_valueTypeCode, TypeCode.Empty);

                return _valueTypeCode;
            }
        }

        public AttributeExpression Attribute
        {
            get { return Left as AttributeExpression; }
        }

        public ValueExpression Value
        {
            get { return Right as ValueExpression; }
        }

        public CollectionExpression Values
        {
            get { return Right as CollectionExpression; }
        }

        public ValueExpression<TValue> GetValue<TValue>()
        {
            return Right as ValueExpression<TValue>;
        }

        public CollectionExpression<TValue> GetValues<TValue>()
        {
            return Right as CollectionExpression<TValue>;
        }
    }
}