﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weverca.Analysis.Memory
{
    /// <summary>
    /// Value representing interval of values
    /// </summary>
    /// <typeparam name="T">Type of stored value - NOTE: Has to provide immutability</typeparam>
    public abstract class IntervalValue<T> : Value
    {
        /// <summary>
        /// Start, End of Interval
        /// </summary>
        public readonly T Start, End;

        /// <summary>
        /// Create Interval value
        /// </summary>
        /// <param name="start">Start of interval</param>
        /// <param name="end">End of interval</param>
        internal IntervalValue(T start, T end)
        {
            Start = start;
            End = end;
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitGenericIntervalValue(this);
        }

        public override bool Equals(object obj)
        {
            var o = obj as IntervalValue<T>;
            if (o == null)
            {
                return false;
            }
            return (Start.Equals(o.Start) && End.Equals(o.End));
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode() + End.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("'({0},{1})', Type: {2}", Start, End, typeof(T).Name);
        }


    }

    public class IntegerIntervalValue : IntervalValue<int>
    {
        internal IntegerIntervalValue(int start, int end) :base(start,end){}

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitIntervalIntegerValue(this);
        }
    }

    public class LongintIntervalValue : IntervalValue<long>
    {
        internal LongintIntervalValue(long start, long end) : base(start, end) { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitIntervalLongintValue(this);
        }
    }

    public class FloatIntervalValue : IntervalValue<double>
    {
        internal FloatIntervalValue(double start, double end) : base(start, end) { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitIntervalFloatValue(this);
        }
    }
}
