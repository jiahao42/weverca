﻿namespace Weverca.Analysis.Memory
{
    /// <summary>
    /// Represent special kind of value. For example these values can express some non-determinism.
    /// </summary>
    public class SpecialValue : Value
    {
        public override int GetHashCode()
        {
            return this.GetType().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.GetType() == obj.GetType();
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitSpecialValue(this);
        }
    }

    public abstract class AliasValue : SpecialValue
    {
        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAliasValue(this);
        }
    }

    public class AnyValue : SpecialValue
    {
        internal AnyValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyValue(this);
        }
    }

    public class UndefinedValue : SpecialValue
    {
        internal UndefinedValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitUndefinedValue(this);
        }
    }

    public class ResourceValue : SpecialValue
    {
        internal ResourceValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitResourceValue(this);
        }
    }

    public abstract class AnyPrimitiveValue : AnyValue
    {
        internal AnyPrimitiveValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyPrimitiveValue(this);
        }
    }

    public class AnyStringValue : AnyPrimitiveValue
    {
        internal AnyStringValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyStringValue(this);
        }
    }

    public class AnyIntegerValue : AnyPrimitiveValue
    {
        internal AnyIntegerValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyIntegerValue(this);
        }
    }

    public class AnyLongintValue : AnyPrimitiveValue
    {
        internal AnyLongintValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyLongintValue(this);
        }
    }

    public class AnyFloatValue : AnyPrimitiveValue
    {
        internal AnyFloatValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyFloatValue(this);
        }
    }

    public class AnyBooleanValue : AnyPrimitiveValue
    {
        internal AnyBooleanValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyBooleanValue(this);
        }
    }

    public class AnyObjectValue : AnyValue
    {
        internal AnyObjectValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyObjectValue(this);
        }
    }

    public class AnyArrayValue : AnyValue
    {
        internal AnyArrayValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyArrayValue(this);
        }
    }

    public class AnyResourceValue : AnyValue
    {
        internal AnyResourceValue() { }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitAnyResourceValue(this);
        }
    }

    public abstract class InfoValue : SpecialValue
    {
        public readonly object RawData;

        internal InfoValue(object rawData)
        {
            RawData = rawData;
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitInfoValue(this);
        }

        public override int GetHashCode()
        {
            return RawData.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var o = obj as InfoValue;
            if (o == null)
            {
                return false;
            }

            return o.RawData.Equals(RawData);
        }
    }

    /// <summary>
    /// Stores meta information for variables and values
    /// WARNING:
    ///     Has to be immutable - also generic type T
    /// </summary>
    /// <typeparam name="T">Type of meta information</typeparam>
    public class InfoValue<T> : InfoValue
    {
        public readonly T Data;

        internal InfoValue(T data)
            : base(data)
        {
            Data = data;
        }

        public override void Accept(IValueVisitor visitor)
        {
            visitor.VisitInfoValue<T>(this);
        }

        public override string ToString()
        {
            return Data.ToString();
        }
    }
}
