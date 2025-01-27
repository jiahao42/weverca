/*
Copyright (c) 2012-2014 David Skorvaga and David Hauzar

This file is part of WeVerca.

WeVerca is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version.

WeVerca is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with WeVerca.  If not, see <http://www.gnu.org/licenses/>.
*/


using System;
using System.Globalization;

using PHP.Core;

using Weverca.AnalysisFramework;
using Weverca.AnalysisFramework.Memory;

namespace Weverca.Analysis.ExpressionEvaluator
{
    /// <summary>
    /// Converts values of various PHP data types to another types.
    /// </summary>
    /// <remarks>
    /// The class <see cref="TypeConversion" /> with its static methods serves as converter between native
    /// and even user-defined (in case of objects) PHP types. The class is very similar to
    /// <c>System.Convert</c> class in .NET Framework. It is highly recommended to prefer this class to .NET
    /// one even if the conversions between equivalent PHP and .NET types do not differ, because it is more
    /// expressive and not so error prone. All these types are supported: Boolean, integer, floating point
    /// number, string, array, object, resource and NULL value (the only value of null type). However,
    /// conversion between every two types is not supported. There are particular cases that may occur:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// There is no conversion. This is the case of the conversion between the same types.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Conversion is not defined. There are conversions that return the right type, but does not make
    /// any sense (e.g. conversion of object to integer). The result of operation is implementation-defined
    /// and analysis should return an abstract interpretation of the given type.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Conversion can fail. Some conversion depends on particular value and in some cases can fail
    /// (e.g. conversion of too large floating point number to integer). For that reason, there are methods
    /// that try to perform conversion and indicate whether they succeed.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// A successful conversion. All other conversions will succeed even if new result value loses some data.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    public static class TypeConversion
    {
        /// <summary>
        /// Number style of PHP integer. It is used to string-to-number conversion.
        /// </summary>
        private const NumberStyles INTEGER_STYLE = NumberStyles.Integer
            | NumberStyles.AllowLeadingWhite | NumberStyles.AllowLeadingSign;

        /// <summary>
        /// Number style of PHP floating-point number. It is used to string-to-number conversion.
        /// </summary>
        private const NumberStyles FLOATING_POINT_NUMBER_STYLE = NumberStyles.Float
            | NumberStyles.AllowLeadingWhite | NumberStyles.AllowLeadingSign
            | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent;

        /// <summary>
        /// Name of standard generic empty class used for typecasting to object.
        /// </summary>
        private static readonly QualifiedName standardClass = new QualifiedName(new Name("stdClass"));

        #region ToBoolean

        /// <summary>
        /// Converts the numeric value to an equivalent boolean value.
        /// </summary>
        /// <typeparam name="T">Type of number representation.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Numeric value to convert.</param>
        /// <returns><c>true</c> if number is not zero, otherwise <c>false</c>.</returns>
        public static BooleanValue ToBoolean<T>(ISnapshotReadWrite snapshot, NumericValue<T> value)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return snapshot.CreateBool(ToBoolean(value));
        }

        /// <summary>
        /// Converts the native numeric value to an equivalent native boolean value.
        /// </summary>
        /// <typeparam name="T">Type of number representation.</typeparam>
        /// <param name="value">Native numeric value to convert.</param>
        /// <returns><c>true</c> if native number is not zero, otherwise <c>false</c>.</returns>
        public static bool ToBoolean<T>(NumericValue<T> value)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            return !value.Value.Equals(value.Zero);
        }

        /// <summary>
        /// Converts the value of integer to an equivalent boolean value.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Integer to convert.</param>
        /// <returns><c>true</c> if integer value is not zero, otherwise <c>false</c>.</returns>
        public static BooleanValue ToBoolean(ISnapshotReadWrite snapshot, ScalarValue<int> value)
        {
            return snapshot.CreateBool(ToBoolean(value.Value));
        }

        /// <summary>
        /// Converts the value of native integer to an equivalent native boolean value.
        /// </summary>
        /// <param name="value">Native integer to convert.</param>
        /// <returns><c>true</c> if integer value is not zero, otherwise <c>false</c>.</returns>
        public static bool ToBoolean(int value)
        {
            return value != 0;
        }

        /// <summary>
        /// Converts the value of long integer to an equivalent boolean value.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Long integer to convert.</param>
        /// <returns><c>true</c> if value is not zero, otherwise <c>false</c>.</returns>
        public static BooleanValue ToBoolean(ISnapshotReadWrite snapshot, ScalarValue<long> value)
        {
            return snapshot.CreateBool(ToBoolean(value.Value));
        }

        /// <summary>
        /// Converts the value of native long integer to an equivalent native boolean value.
        /// </summary>
        /// <param name="value">Native long integer to convert.</param>
        /// <returns><c>true</c> if value is not zero, otherwise <c>false</c>.</returns>
        public static bool ToBoolean(long value)
        {
            return value != 0;
        }

        /// <summary>
        /// Converts the value of floating-point number to an equivalent boolean value.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Floating-point number to convert.</param>
        /// <returns><c>true</c> if value is not zero, otherwise <c>false</c>.</returns>
        public static BooleanValue ToBoolean(ISnapshotReadWrite snapshot, ScalarValue<double> value)
        {
            return snapshot.CreateBool(ToBoolean(value.Value));
        }

        /// <summary>
        /// Converts the value of native floating-point number to an equivalent native boolean value.
        /// </summary>
        /// <param name="value">Native floating-point number to convert.</param>
        /// <returns><c>true</c> if value is not zero, otherwise <c>false</c>.</returns>
        public static bool ToBoolean(double value)
        {
            return value != 0.0;
        }

        /// <summary>
        /// Converts the string value to proper boolean value.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">String to convert.</param>
        /// <returns><c>true</c> if string is not empty or "0", otherwise <c>false</c>.</returns>
        public static BooleanValue ToBoolean(ISnapshotReadWrite snapshot, ScalarValue<string> value)
        {
            return snapshot.CreateBool(ToBoolean(value.Value));
        }

        /// <summary>
        /// Converts the native string value to proper native boolean value.
        /// </summary>
        /// <param name="value">Native string to convert.</param>
        /// <returns><c>true</c> if string is not empty or "0", otherwise <c>false</c>.</returns>
        public static bool ToBoolean(string value)
        {
            Debug.Assert(value != null, "String converted to boolean can never be null");

            return (value.Length != 0) && (!string.Equals(value, "0"));
        }

        /// <summary>
        /// Determines boolean value from the object reference value.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Object of any type to convert.</param>
        /// <returns>Always <c>true</c>.</returns>
        public static BooleanValue ToBoolean(ISnapshotReadWrite snapshot, ObjectValue value)
        {
            return snapshot.CreateBool(ToBoolean(value));
        }

        /// <summary>
        /// Determines native boolean value from the object reference value.
        /// </summary>
        /// <param name="value">Object of any type to convert.</param>
        /// <returns>Always <c>true</c>.</returns>
        public static bool ToBoolean(ObjectValue value)
        {
            // Notice that in PHP 4, an object evaluates as false if it has no properties.
            return true;
        }

        /// <summary>
        /// Determines boolean value from content of the array value.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Array to convert.</param>
        /// <returns><c>true</c> if array has at least one element, otherwise <c>false</c>.</returns>
        public static BooleanValue ToBoolean(SnapshotBase snapshot, AssociativeArray value)
        {
            return snapshot.CreateBool(ToNativeBoolean(snapshot, value));
        }

        /// <summary>
        /// Determines native boolean value from content of the array value.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Array to convert.</param>
        /// <returns><c>true</c> if array has at least one element, otherwise <c>false</c>.</returns>
        public static bool ToNativeBoolean(SnapshotBase snapshot, AssociativeArray value)
        {
            var entry = snapshot.CreateSnapshotEntry(new MemoryEntry(value));

            var indices = entry.IterateIndexes(snapshot);
            return indices.GetEnumerator ().MoveNext ();
        }

        /// <summary>
        /// Determines boolean value from the reference to external resource.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">External resource to convert.</param>
        /// <returns>Always <c>true</c>.</returns>
        public static BooleanValue ToBoolean(ISnapshotReadWrite snapshot, ResourceValue value)
        {
            return snapshot.CreateBool(ToBoolean(value));
        }

        /// <summary>
        /// Determines native boolean value from the reference to external resource.
        /// </summary>
        /// <param name="value">External resource to convert.</param>
        /// <returns>Always <c>true</c>.</returns>
        public static bool ToBoolean(ResourceValue value)
        {
            return true;
        }

        /// <summary>
        /// Determines boolean value from any object reference value.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Any object of any type to convert.</param>
        /// <returns>Always <c>true</c>.</returns>
        public static BooleanValue ToBoolean(ISnapshotReadWrite snapshot, AnyObjectValue value)
        {
            return snapshot.CreateBool(ToBoolean(value));
        }

        /// <summary>
        /// Determines native boolean value from any object reference value.
        /// </summary>
        /// <param name="value">Any object of any type to convert.</param>
        /// <returns>Always <c>true</c>.</returns>
        public static bool ToBoolean(AnyObjectValue value)
        {
            // Notice that in PHP 4, an object evaluates as false if it has no properties.
            return true;
        }

        /// <summary>
        /// Determines boolean value from any reference to external resource.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Any external resource to convert.</param>
        /// <returns>Always <c>true</c>.</returns>
        public static BooleanValue ToBoolean(ISnapshotReadWrite snapshot, AnyResourceValue value)
        {
            return snapshot.CreateBool(ToBoolean(value));
        }

        /// <summary>
        /// Determines native boolean value from any reference to external resource.
        /// </summary>
        /// <param name="value">Any external resource to convert.</param>
        /// <returns>Always <c>true</c>.</returns>
        public static bool ToBoolean(AnyResourceValue value)
        {
            return true;
        }

        /// <summary>
        /// Converts possible interval of numbers to an equivalent concrete or abstract boolean value.
        /// </summary>
        /// <typeparam name="T">Type of values represented by interval.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Value representing interval of numbers to convert.</param>
        /// <returns>Concrete boolean value if it is possible, otherwise abstract boolean value.</returns>
        public static Value ToBoolean<T>(ISnapshotReadWrite snapshot, IntervalValue<T> value)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if ((value.Start.CompareTo(value.Zero) <= 0) && (value.End.CompareTo(value.Zero) >= 0))
            {
                if (value.Start.Equals(value.Zero) && value.End.Equals(value.Zero))
                {
                    return snapshot.CreateBool(false);
                }
                else
                {
                    return snapshot.AnyBooleanValue;
                }
            }
            else
            {
                return snapshot.CreateBool(true);
            }
        }

        /// <summary>
        /// Tries to convert possible interval of numbers to an equivalent boolean value.
        /// </summary>
        /// <typeparam name="T">Type of values represented by interval.</typeparam>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Value representing interval of numbers to convert.</param>
        /// <param name="convertedValue">
        /// <c>true</c> if interval does not contain zero,
        /// <c>false</c> if interval consists only from zero value or <c>null</c> otherwise.
        /// </param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c>.</returns>
        public static bool TryConvertToBoolean<T>(ISnapshotReadWrite snapshot, IntervalValue<T> value,
            out BooleanValue convertedValue)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            bool casted;
            if (TryConvertToBoolean(value, out casted))
            {
                convertedValue = snapshot.CreateBool(casted);
                return true;
            }
            else
            {
                convertedValue = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to convert possible interval of numbers to an equivalent native boolean value.
        /// </summary>
        /// <typeparam name="T">Type of values represented by interval.</typeparam>
        /// <param name="value">Value representing interval of numbers to convert.</param>
        /// <param name="convertedValue">
        /// <c>true</c> if interval does not contain zero and
        /// <c>false</c> if interval consists only from zero value or other cases.
        /// </param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c>.</returns>
        public static bool TryConvertToBoolean<T>(IntervalValue<T> value, out bool convertedValue)
            where T : IComparable, IComparable<T>, IEquatable<T>
        {
            if ((value.Start.CompareTo(value.Zero) <= 0) && (value.End.CompareTo(value.Zero) >= 0))
            {
                if (value.Start.Equals(value.Zero) && value.End.Equals(value.Zero))
                {
                    convertedValue = false;
                    return true;
                }
                else
                {
                    convertedValue = false;
                    return false;
                }
            }
            else
            {
                convertedValue = true;
                return true;
            }
        }

        /// <summary>
        /// Converts an undefined value to an equivalent boolean value.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Undefined value.</param>
        /// <returns>Always <c>false</c>.</returns>
        public static BooleanValue ToBoolean(ISnapshotReadWrite snapshot, UndefinedValue value)
        {
            return snapshot.CreateBool(ToBoolean(value));
        }

        /// <summary>
        /// Converts an undefined value to an equivalent native boolean value.
        /// </summary>
        /// <param name="value">Undefined value.</param>
        /// <returns>Always <c>false</c>.</returns>
        public static bool ToBoolean(UndefinedValue value)
        {
            return false;
        }

        #endregion ToBoolean

        #region ToInteger

        /// <summary>
        /// Converts the boolean value to an equivalent value of integer.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Boolean value to convert.</param>
        /// <returns>The number 1 if value is <c>true</c>, otherwise 0.</returns>
        public static IntegerValue ToInteger(ISnapshotReadWrite snapshot, ScalarValue<bool> value)
        {
            return snapshot.CreateInt(ToInteger(value.Value));
        }

        /// <summary>
        /// Converts the native boolean value to an equivalent value of native integer.
        /// </summary>
        /// <param name="value">Native boolean value to convert.</param>
        /// <returns>The number 1 if value is <c>true</c>, otherwise 0.</returns>
        public static int ToInteger(bool value)
        {
            return System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Tries to convert the value of long integer to an equivalent integer value.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Long integer to convert.</param>
        /// <param name="convertedValue">New integer value if conversion is successful, otherwise 0.</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c>.</returns>
        public static bool TryConvertToInteger(ISnapshotReadWrite snapshot, ScalarValue<long> value,
            out IntegerValue convertedValue)
        {
            int casted;
            var isConverted = TryConvertToInteger(value.Value, out casted);
            convertedValue = snapshot.CreateInt(casted);
            return isConverted;
        }

        /// <summary>
        /// Tries to convert the value of native long integer to an equivalent native integer value.
        /// </summary>
        /// <param name="value">Native long integer to convert.</param>
        /// <param name="convertedValue">Integer type value if conversion is successful, otherwise 0.</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c>.</returns>
        public static bool TryConvertToInteger(long value, out int convertedValue)
        {
            // This condition suppresses <c>OverflowException</c> of <c>Convert.ToInt32</c> conversion.
            if (value < int.MinValue || value > int.MaxValue)
            {
                convertedValue = 0;
                return false;
            }

            convertedValue = System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>
        /// Tries to convert the value of floating-point number to an equivalent integer value.
        /// </summary>
        /// <remarks>
        /// <seealso cref="TypeConversion.TryConvertToInteger(double, out int)" />
        /// </remarks>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Floating-point number to convert.</param>
        /// <param name="convertedValue">New integer value if conversion is successful, otherwise 0.</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c>.</returns>
        public static bool TryConvertToInteger(ISnapshotReadWrite snapshot, ScalarValue<double> value,
            out IntegerValue convertedValue)
        {
            int casted;
            var isConverted = TryConvertToInteger(value.Value, out casted);
            convertedValue = snapshot.CreateInt(casted);
            return isConverted;
        }

        /// <summary>
        /// Tries to convert the value of native floating-point number to an equivalent native integer value.
        /// </summary>
        /// <remarks>
        /// In PHP 5, when converting from floating-point number to integer, the number is rounded
        /// towards zero. If the number is beyond the boundaries of integer, the result is undefined
        /// integer. No warning, not even a notice will be issued when this happens.
        /// </remarks>
        /// <param name="value">Native floating-point number to convert.</param>
        /// <param name="convertedValue">Integer type value if conversion is successful, otherwise 0.</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c>.</returns>
        public static bool TryConvertToInteger(double value, out int convertedValue)
        {
            var truncated = Math.Truncate(value);

            // This condition suppresses <c>OverflowException</c> of <c>Convert.ToInt32</c> conversion.
            if (double.IsNaN(truncated) || truncated < int.MinValue || truncated > int.MaxValue)
            {
                convertedValue = 0;
                return false;
            }

            convertedValue = System.Convert.ToInt32(truncated, CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>
        /// Converts the string value to corresponding integer value.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">String to convert.</param>
        /// <returns>Integer representation of string if it can be converted, otherwise 0.</returns>
        public static IntegerValue ToInteger(ISnapshotReadWrite snapshot, ScalarValue<string> value)
        {
            IntegerValue convertedValue;
            TryConvertToInteger(snapshot, value, out convertedValue);
            return convertedValue;
        }

        /// <summary>
        /// Converts the native string value to corresponding native integer value.
        /// </summary>
        /// <param name="value">Native string to convert.</param>
        /// <returns>Integer representation of string if it can be converted, otherwise 0.</returns>
        public static int ToInteger(string value)
        {
            int convertedValue;
            TryConvertToInteger(value, out convertedValue);
            return convertedValue;
        }

        /// <summary>
        /// Tries to convert the string value only if it represents a integer value.
        /// </summary>
        /// <remarks>
        /// <seealso cref="TypeConversion.TryIdentifyInteger(string, out int)" />
        /// </remarks>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">String to convert.</param>
        /// <param name="convertedValue">New integer value if conversion is successful, otherwise 0.</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c>.</returns>
        public static bool TryIdentifyInteger(ISnapshotReadWrite snapshot, ScalarValue<string> value,
            out IntegerValue convertedValue)
        {
            int integerValue;
            var isSuccessful = TryIdentifyInteger(value.Value, out integerValue);
            convertedValue = snapshot.CreateInt(integerValue);
            return isSuccessful;
        }

        /// <summary>
        /// Tries to convert the native string value only if it represents a integer value.
        /// </summary>
        /// <remarks>
        /// Conversion of string to integer value is always defined, but in certain cases, we want to know
        /// if the conversion is successful (e.g. explicit type-casting or when creating a new array using
        /// index of string) In these cases, hexadecimal numbers are not recognized.
        /// </remarks>
        /// <param name="value">Native string to convert.</param>
        /// <param name="convertedValue">New integer value if conversion is successful, otherwise 0.</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c>.</returns>
        public static bool TryIdentifyInteger(string value, out int convertedValue)
        {
            double floatValue;
            bool isInteger;
            var isSuccessful = TryConvertToNumber(value, false, out convertedValue,
                out floatValue, out isInteger);
            return isSuccessful && isInteger;
        }

        /// <summary>
        /// Tries to convert the string value to corresponding integer value.
        /// </summary>
        /// <remarks>
        /// <seealso cref="TypeConversion.TryConvertToInteger(string, out int)" />
        /// </remarks>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">String to convert.</param>
        /// <param name="convertedValue">New integer value if conversion is successful, otherwise 0.</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c>.</returns>
        public static bool TryConvertToInteger(ISnapshotReadWrite snapshot, ScalarValue<string> value,
            out IntegerValue convertedValue)
        {
            int integerValue;
            var isSuccessful = TryConvertToInteger(value.Value, out integerValue);
            convertedValue = snapshot.CreateInt(integerValue);
            return isSuccessful;
        }

        /// <summary>
        /// Tries to convert the native string value to corresponding native integer value.
        /// </summary>
        /// <remarks>
        /// The method succeeds when conversion gives a predictable result even if the conversion
        /// into integer itself fails. If the method fails, the result is any abstract integer.
        /// </remarks>
        /// <param name="value">Native string to convert.</param>
        /// <param name="convertedValue">New integer value if it can be predicted, otherwise 0.</param>
        /// <returns><c>true</c> if string is converted to concrete integer, otherwise <c>false</c></returns>
        public static bool TryConvertToInteger(string value, out int convertedValue)
        {
            double floatValue;
            bool isInteger;
            var isSuccessful = TryConvertToNumber(value, false, out convertedValue,
                out floatValue, out isInteger);

            return isInteger || (isSuccessful
                && TypeConversion.TryConvertToInteger(floatValue, out convertedValue));
        }

        /// <summary>
        /// Determines value of integer from content of the array value.
        /// </summary>
        /// <remarks>
        /// <seealso cref="ToNativeInteger(SnapshotBase, AssociativeArray)" />
        /// </remarks>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Array to convert.</param>
        /// <returns>1 if array has at least one element, otherwise 0.</returns>
        public static IntegerValue ToInteger(SnapshotBase snapshot, AssociativeArray value)
        {
            return snapshot.CreateInt(ToNativeInteger(snapshot, value));
        }

        /// <summary>
        /// Determines value of native integer from content of the array value.
        /// </summary>
        /// <remarks>
        /// Here the documentation is ambiguous. It says that the behavior of converting to integer
        /// is undefined for other than scalar types. However, it typically acts as predefined
        /// function <c>intval</c> which has conversion of to array defined clearly.
        /// </remarks>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Array to convert.</param>
        /// <returns>1 if array has at least one element, otherwise 0.</returns>
        public static int ToNativeInteger(SnapshotBase snapshot, AssociativeArray value)
        {
            return ToInteger(ToNativeBoolean(snapshot, value));
        }

        /// <summary>
        /// Converts an undefined value to an equivalent integer value.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Undefined value.</param>
        /// <returns>Always 0 value.</returns>
        public static IntegerValue ToInteger(ISnapshotReadWrite snapshot, UndefinedValue value)
        {
            return snapshot.CreateInt(ToInteger(value));
        }

        /// <summary>
        /// Converts an undefined value to an equivalent native integer value.
        /// </summary>
        /// <param name="value">Undefined value.</param>
        /// <returns>Always 0 value.</returns>
        public static int ToInteger(UndefinedValue value)
        {
            return 0;
        }

        #endregion ToInteger

        #region ToFloat

        /// <summary>
        /// Converts the boolean value to an equivalent floating-point number.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Boolean value to convert.</param>
        /// <returns>The number 1.0 if value is <c>true</c>, otherwise 0.0.</returns>
        public static FloatValue ToFloat(ISnapshotReadWrite snapshot, ScalarValue<bool> value)
        {
            return snapshot.CreateDouble(ToFloat(value.Value));
        }

        /// <summary>
        /// Converts the native boolean value to an equivalent native floating-point number.
        /// </summary>
        /// <param name="value">Native boolean value to convert.</param>
        /// <returns>The number 1.0 if value is <c>true</c>, otherwise 0.0.</returns>
        public static double ToFloat(bool value)
        {
            return System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts the value of integer value to an equivalent floating-point number.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Integer value to convert.</param>
        /// <returns>A floating-point number that is equivalent to integer value.</returns>
        public static FloatValue ToFloat(ISnapshotReadWrite snapshot, ScalarValue<int> value)
        {
            return snapshot.CreateDouble(ToFloat(value.Value));
        }

        /// <summary>
        /// Converts the value of native integer value to an equivalent native floating-point number.
        /// </summary>
        /// <param name="value">Native integer value to convert.</param>
        /// <returns>A floating-point number that is equivalent to native integer value.</returns>
        public static double ToFloat(int value)
        {
            return System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts the value of long integer value to an equivalent floating-point number.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Long integer value to convert.</param>
        /// <returns>A floating-point number that is equivalent to long integer value.</returns>
        public static FloatValue ToFloat(ISnapshotReadWrite snapshot, ScalarValue<long> value)
        {
            return snapshot.CreateDouble(ToFloat(value.Value));
        }

        /// <summary>
        /// Converts the value of native long integer value to an equivalent native floating-point number.
        /// </summary>
        /// <param name="value">Native long integer value to convert.</param>
        /// <returns>A floating-point number that is equivalent to native long integer value.</returns>
        public static double ToFloat(long value)
        {
            return System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts the string value to corresponding floating-point number.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">String to convert.</param>
        /// <returns>Number representation of string if it can be converted, otherwise 0.0.</returns>
        public static FloatValue ToFloat(ISnapshotReadWrite snapshot, ScalarValue<string> value)
        {
            FloatValue convertedValue;
            TryConvertToFloat(snapshot, value, out convertedValue);
            return convertedValue;
        }

        /// <summary>
        /// Tries to convert the string value to corresponding floating-point number.
        /// </summary>
        /// <remarks>
        /// <seealso cref="TypeConversion.TryConvertToInteger(string, out int)" />
        /// </remarks>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">String to convert.</param>
        /// <param name="convertedValue">Converted value if conversion is successful, otherwise 0.0.</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c>.</returns>
        public static bool TryConvertToFloat(ISnapshotReadWrite snapshot, ScalarValue<string> value,
            out FloatValue convertedValue)
        {
            double floatValue;
            var isSuccessful = TryConvertToFloat(value.Value, out floatValue);
            convertedValue = snapshot.CreateDouble(floatValue);
            return isSuccessful;
        }

        /// <summary>
        /// Tries to convert the native string value to corresponding native floating-point number.
        /// </summary>
        /// <remarks>
        /// Conversion of string to floating-point number is always defined, but in certain cases,
        /// we want to know if the conversion is successful (e.g. explicit type-casting).
        /// </remarks>
        /// <param name="value">Native string to convert.</param>
        /// <param name="convertedValue">Converted value if conversion is successful, otherwise 0.0.</param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c>.</returns>
        public static bool TryConvertToFloat(string value, out double convertedValue)
        {
            int integerValue;
            bool isInteger;
            var isSuccessful = TryConvertToNumber(value, false, out integerValue,
                out convertedValue, out isInteger);
            return isSuccessful || (!isInteger);
        }

        /// <summary>
        /// Determines floating-point number from content of the array value.
        /// </summary>
        /// <remarks>
        /// <seealso cref="TypeConversion.ToInteger(SnapshotBase, AssociativeArray)" />
        /// </remarks>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Array to convert.</param>
        /// <returns>1.0 if array has at least one element, otherwise 0.0.</returns>
        public static FloatValue ToFloat(SnapshotBase snapshot, AssociativeArray value)
        {
            return snapshot.CreateDouble(ToNativeFloat(snapshot, value));
        }

        /// <summary>
        /// Determines native floating-point number from content of the array value.
        /// </summary>
        /// <remarks>
        /// <seealso cref="TypeConversion.ToInteger(SnapshotBase, AssociativeArray)" />
        /// </remarks>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Array to convert.</param>
        /// <returns>1.0 if array has at least one element, otherwise 0.0.</returns>
        public static double ToNativeFloat(SnapshotBase snapshot, AssociativeArray value)
        {
            return ToFloat(ToNativeBoolean(snapshot, value));
        }

        /// <summary>
        /// Converts an undefined value to an equivalent floating-point number.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Undefined value.</param>
        /// <returns>Always 0.0 value.</returns>
        public static FloatValue ToFloat(ISnapshotReadWrite snapshot, UndefinedValue value)
        {
            return snapshot.CreateDouble(ToFloat(value));
        }

        /// <summary>
        /// Converts an undefined value to an equivalent native floating-point number.
        /// </summary>
        /// <param name="value">Undefined value.</param>
        /// <returns>Always 0.0 value.</returns>
        public static double ToFloat(UndefinedValue value)
        {
            return 0.0;
        }

        #endregion ToFloat

        #region ToString

        /// <summary>
        /// Converts the boolean value to an equivalent string representation.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Boolean value to convert.</param>
        /// <returns>String "1" if value is <c>true</c>, otherwise empty string.</returns>
        public static StringValue ToString(ISnapshotReadWrite snapshot, ScalarValue<bool> value)
        {
            return snapshot.CreateString(ToString(value.Value));
        }

        /// <summary>
        /// Converts the native boolean value to an equivalent native string representation.
        /// </summary>
        /// <param name="value">Native boolean value to convert.</param>
        /// <returns>String "1" if value is <c>true</c>, otherwise empty string.</returns>
        public static string ToString(bool value)
        {
            return value ? "1" : string.Empty;
        }

        /// <summary>
        /// Converts the integer value to an equivalent string representation.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Value of integer to convert.</param>
        /// <returns>The string representation of integer value.</returns>
        public static StringValue ToString(ISnapshotReadWrite snapshot, ScalarValue<int> value)
        {
            return snapshot.CreateString(ToString(value.Value));
        }

        /// <summary>
        /// Converts the native integer value to an equivalent native string representation.
        /// </summary>
        /// <param name="value">Value of native integer to convert.</param>
        /// <returns>The string representation of native integer value.</returns>
        public static string ToString(int value)
        {
            return System.Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Converts the long integer value to an equivalent string representation.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Value of long integer to convert.</param>
        /// <returns>The string representation of long integer value.</returns>
        public static StringValue ToString(ISnapshotReadWrite snapshot, ScalarValue<long> value)
        {
            return snapshot.CreateString(System.Convert.ToString(value.Value, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Converts the floating-point number to an equivalent string representation.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Floating-point number to convert.</param>
        /// <returns>The string representation of floating-point number.</returns>
        public static StringValue ToString(ISnapshotReadWrite snapshot, ScalarValue<double> value)
        {
            return snapshot.CreateString(ToString(value.Value));
        }

        /// <summary>
        /// Converts the native floating-point number to an equivalent native string representation.
        /// </summary>
        /// <param name="value">Native floating-point number to convert.</param>
        /// <returns>The string representation of native floating-point number.</returns>
        public static string ToString(double value)
        {
            return System.Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Determines string value from content of the array value.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Array to convert.</param>
        /// <returns>Always "Array" string.</returns>
        public static StringValue ToString(ISnapshotReadWrite snapshot, AssociativeArray value)
        {
            return snapshot.CreateString("Array");
        }

        /// <summary>
        /// Determines string value from the reference to external resource.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">External resource to convert.</param>
        /// <returns>
        /// Value "Resource id #X", where X is a unique number assigned to the resource by PHP at runtime.
        /// </returns>
        public static StringValue ToString(ISnapshotReadWrite snapshot, ResourceValue value)
        {
            return snapshot.CreateString(string.Concat("Resource id #",
                value.UID.ToString(CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// Determines string value from content of any array value.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Any array to convert.</param>
        /// <returns>Always "Array" string.</returns>
        public static StringValue ToString(ISnapshotReadWrite snapshot, AnyArrayValue value)
        {
            return snapshot.CreateString("Array");
        }

        /// <summary>
        /// Converts an undefined value to an equivalent string representation.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Undefined value.</param>
        /// <returns>Empty string.</returns>
        public static StringValue ToString(ISnapshotReadWrite snapshot, UndefinedValue value)
        {
            return snapshot.CreateString(string.Empty);
        }

        #endregion ToString

        #region ToObject

        /// <summary>
        /// Converts the array to corresponding new object.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Array to convert.</param>
        /// <returns>Object with fields named by indices of array and initialized by their values.</returns>
        public static ObjectValue ToObject(SnapshotBase snapshot, AssociativeArray value)
        {
            var objectValue = CreateStandardObject(snapshot);
            var objectEntry = GetSnapshotEntry(snapshot, objectValue);
            var arrayEntry = GetSnapshotEntry(snapshot, value);

            var indices = objectEntry.IterateFields(snapshot);

            foreach (var index in indices)
            {
                var fieldEntry = objectEntry.ReadField(snapshot, index);
                var identifier = new MemberIdentifier(index.DirectName.Value);
                var indexEntry = arrayEntry.ReadIndex(snapshot, identifier);

                var readValue = indexEntry.ReadMemory(snapshot);
                fieldEntry.WriteMemory(snapshot, readValue);
            }

            return objectValue;
        }

        /// <summary>
        /// Creates an object containing one field with the undefined value.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Undefined value.</param>
        /// <returns>Object with field named "scalar" which contains undefined value.</returns>
        public static ObjectValue ToObject(ISnapshotReadWrite snapshot, UndefinedValue value)
        {
            return CreateStandardObject(snapshot);
        }

        /// <summary>
        /// Creates an object containing one field with a value but an object, array or undefined value.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">A value to convert.</param>
        /// <returns>Object with field named "scalar" which contains the value.</returns>
        public static ObjectValue ToObject(SnapshotBase snapshot, Value value)
        {
            var objectValue = CreateStandardObject(snapshot);
            var objectEntry = GetSnapshotEntry(snapshot, objectValue);

            var outSnapshot = snapshot;
            var fieldEntry = GetFieldEntry(snapshot, objectEntry, "scalar");

            var valueEntry = new MemoryEntry(value);
            fieldEntry.WriteMemory(outSnapshot, valueEntry);

            return objectValue;
        }

        #endregion ToObject

        #region ToArray

        /// <summary>
        /// Converts the object to corresponding array structure.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Object of any type to convert.</param>
        /// <returns>Array with keys named by fields of object and initialized by their values.</returns>
        public static AssociativeArray ToArray(SnapshotBase snapshot, ObjectValue value)
        {
            // TODO: This conversion is quite difficult. It does not convert integer properties, needs to
            // know visibility of every property and needs access to private properties of base classes.

            var arrayValue = snapshot.CreateArray();
            var arrayEntry = GetSnapshotEntry(snapshot, arrayValue);
            var objectEntry = GetSnapshotEntry(snapshot, value);

            var fields = objectEntry.IterateFields(snapshot);

            foreach (var field in fields)
            {
                var fieldEntry = objectEntry.ReadField(snapshot, field);
                var identifier = new MemberIdentifier(field.DirectName.Value);
                var indexEntry = arrayEntry.ReadIndex(snapshot, identifier);

                var readValue = fieldEntry.ReadMemory(snapshot);
                indexEntry.WriteMemory(snapshot, readValue);
            }

            return arrayValue;
        }

        /// <summary>
        /// Converts an undefined value to corresponding array structure.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Undefined value.</param>
        /// <returns>Empty with no elements.</returns>
        public static AssociativeArray ToArray(ISnapshotReadWrite snapshot, UndefinedValue value)
        {
            return snapshot.CreateArray();
        }

        /// <summary>
        /// Converts a value but an object, array or undefined value to corresponding array structure.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">A value to convert.</param>
        /// <returns>Array with a single element with the value on position of 0 index.</returns>
        public static AssociativeArray ToArray(SnapshotBase snapshot, Value value)
        {
            var arrayValue = snapshot.CreateArray();
            var arrayEntry = GetSnapshotEntry(snapshot, arrayValue);

            var indexEntry = GetIndexEntry(snapshot, arrayEntry, "0");

            var valueEntry = new MemoryEntry(value);
            indexEntry.WriteMemory(snapshot, valueEntry);

            return arrayValue;
        }

        #endregion ToArray

        #region ToIntegerInterval

        /// <summary>
        /// Tries to convert the interval of long integer to an equivalent integer interval.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Long integer to convert.</param>
        /// <param name="convertedValue">
        /// Integer interval in the same range as input if conversion is successful, otherwise (0;0).
        /// </param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c>.</returns>
        public static bool TryConvertToIntegerInterval(ISnapshotReadWrite snapshot, IntervalValue<long> value,
            out IntervalValue<int> convertedValue)
        {
            int castedStart, castedEnd = 0;
            var isConverted = TryConvertToInteger(value.Start, out castedStart)
                && TryConvertToInteger(value.End, out castedEnd);
            convertedValue = snapshot.CreateIntegerInterval(castedStart, castedEnd);
            return isConverted;
        }

        /// <summary>
        /// Tries to convert the interval of floating-point numbers to an equivalent integer interval.
        /// </summary>
        /// <remarks>
        /// <seealso cref="TypeConversion.TryConvertToInteger(double, out int)" />
        /// </remarks>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Floating-point number to convert.</param>
        /// <param name="convertedValue">
        /// Integer interval in the same range as input if conversion is successful, otherwise (0;0).
        /// </param>
        /// <returns><c>true</c> if value is converted successfully, otherwise <c>false</c>.</returns>
        public static bool TryConvertToIntegerInterval(ISnapshotReadWrite snapshot,
            IntervalValue<double> value, out IntervalValue<int> convertedValue)
        {
            int castedStart, castedEnd = 0;
            var isConverted = TryConvertToInteger(value.Start, out castedStart)
                && TryConvertToInteger(value.End, out castedEnd);
            convertedValue = snapshot.CreateIntegerInterval(castedStart, castedEnd);
            return isConverted;
        }

        /// <summary>
        /// Create integer interval representing all boolean values converted into integers.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <returns>Integer interval with two values, integer <c>true</c> and <c>false</c>.</returns>
        public static IntervalValue<int> AnyBooleanToIntegerInterval(ISnapshotReadWrite snapshot)
        {
            return snapshot.CreateIntegerInterval(ToInteger(false), ToInteger(true));
        }

        /// <summary>
        /// Create entire integer interval.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <returns>The entire integer interval from minimal to maximal integer value.</returns>
        public static IntervalValue<int> AnyIntegerToIntegerInterval(ISnapshotReadWrite snapshot)
        {
            return snapshot.CreateIntegerInterval(int.MinValue, int.MaxValue);
        }

        /// <summary>
        /// Create integer interval of all possible values of an array converted to integer.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <returns>Integer interval with 0 and 1 values.</returns>
        public static IntervalValue<int> AnyArrayToIntegerInterval(ISnapshotReadWrite snapshot)
        {
            // Interval has only two values meaning that array can be empty or not
            return snapshot.CreateIntegerInterval(0, 1);
        }

        /// <summary>
        /// Casts the provided interval to <see cref="IntegerIntervalValue"/>, if possible.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">The interval value to process.</param>
        /// <returns>
        /// Corresponding <see cref="IntegerIntervalValue"/> if cast is possible, <c>null</c> otherwise.
        /// </returns>
        public static IntegerIntervalValue ToIntegerInterval(ISnapshotReadWrite snapshot, IntervalValue value)
        {
            var visitor = new ToIntegerIntervalConversionVisitor(snapshot);
            value.Accept(visitor);
            return visitor.Result;
        }

        #endregion ToIntegerInterval

        #region ToLongInterval

        /// <summary>
        /// Casts the provided interval to <see cref="LongintIntervalValue"/>, if possible.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">The interval value to process.</param>
        /// <returns>
        /// Corresponding <see cref="LongintIntervalValue"/> if cast is possible, <c>null</c> otherwise.
        /// </returns>
        public static LongintIntervalValue ToLongInterval(ISnapshotReadWrite snapshot, IntervalValue value)
        {
            var visitor = new ToLongIntervalConversionVisitor(snapshot);
            value.Accept(visitor);
            return visitor.Result;
        }

        #endregion ToLongInterval

        #region ToFloatInterval

        /// <summary>
        /// Extends integer interval into floating-point interval of the same range.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Integer interval to extend.</param>
        /// <returns>Floating-point interval extended from integer interval.</returns>
        public static IntervalValue<double> ToFloatInterval(ISnapshotReadWrite snapshot,
            IntervalValue<int> value)
        {
            return snapshot.CreateFloatInterval(value.Start, value.End);
        }

        /// <summary>
        /// Extends integer interval into floating-point interval of the same range.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">An interval value.</param>
        /// <returns>
        /// Floating-point interval extended from integer interval,
        /// if the value can be casted, <c>null</c> otherwise.
        /// </returns>
        public static FloatIntervalValue ToFloatInterval(ISnapshotReadWrite snapshot, IntervalValue value)
        {
            var visitor = new ToFloatIntervalConversionVisitor(snapshot);
            value.Accept(visitor);
            return visitor.Result;
        }

        #endregion ToFloatInterval

        #region ToNumber

        /// <summary>
        /// Tries to convert the string value to integer and if it fails, then to floating-point number.
        /// </summary>
        /// <remarks>
        /// Conversion to number distinguishes if value is integer or floating-point constant and creates
        /// integer whenever it is possible. A valid number constant is represented by regular expression
        /// "[:space:]*(0[xX][0-9a-fA-F]+|[+-]?[0-9]*([0-9]([\.][0-9]*)?|[\.][0-9]+)([eE][+-]?[0-9]+)?)".
        /// Conversion does not work the same as PHP scanner. In the first place, it absolutely ignores
        /// binary and octane numbers. On the contrary, it permits whitespaces at the beginning and tolerates
        /// characters after valid number format. In other words, it tries to parse everything what is
        /// possible. In this respect it behaves identically to C function <c>strtod</c>. Finally, it is
        /// very odd how numbers are converted during different operations. If string is converted by
        /// an arithmetic or comparison operation, conversion is proper as described. However, if we convert
        /// explicitly by type-casting or with bitwise operation, hexadecimal numbers are not recognized.
        /// </remarks>
        /// <param name="value">String value to convert.</param>
        /// <param name="canBeHexadecimal">Determines whether to parse hexadecimal format too.</param>
        /// <param name="integerValue">New integer value if conversion is successful, otherwise 0.</param>
        /// <param name="floatValue">
        /// New floating-point number if conversion is successful or integer is too large, otherwise 0.0.
        /// </param>
        /// <param name="isInteger">
        /// <c>true</c> if value is not converted to floating-point number, otherwise <c>false</c>.
        /// </param>
        /// <param name="isHexadecimal">
        /// <c>true</c> if number is converted from string in hexadecimal format, otherwise <c>false</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if value is converted successfully to integer or floating-point number, otherwise
        /// <c>false</c>, even if conversion to integer fails and result is stored as floating-point value.
        /// </returns>
        public static bool TryConvertToNumber(string value, bool canBeHexadecimal, out int integerValue,
            out double floatValue, out bool isInteger, out bool isHexadecimal)
        {
            // Skip whitespaces at the beginning of the string
            var index = SkipWhiteSpace(value);

            if (canBeHexadecimal && (value.Length > index + 2) && (value[index] == '0')
                && ((value[index + 1] == 'x') || (value[index + 1] == 'X')))
            {
                index += 2;
                if (TryConvertHexadecialToInteger(value[index], out integerValue))
                {
                    // The hexadecimal format is converted to integer or float
                    isInteger = TryParseHexadecimal(value, index + 1, ref integerValue, out floatValue);
                    isHexadecimal = true;
                    return isInteger;
                }
                else
                {
                    // Conversion is valid because of the first zero
                    integerValue = 0;
                    floatValue = 0.0;
                    isInteger = true;
                    isHexadecimal = false;
                    return true;
                }
            }

            isHexadecimal = false;
            var start = index;
            index = SkipSign(value, index);

            // Skip digits in integer part
            var startDigits = index;
            index = SkipDigits(value, index);
            var isIntegerPart = index > startDigits;

            bool isFractionalPart;
            if (index >= value.Length)
            {
                if (isIntegerPart)
                {
                    // There is only integer part
                    isInteger = TryParseToInteger(value, start, value.Length - start,
                        out integerValue, out floatValue);
                    return isInteger;
                }
                else
                {
                    // There is the end before begin of a number
                    return SetParsingFailure(out integerValue, out floatValue, out isInteger);
                }
            }
            else if (value[index] == '.')
            {
                ++index;
                startDigits = index;
                index = SkipDigits(value, index);

                if ((index > startDigits) || isIntegerPart)
                {
                    // It is floating-point number, becasue there is decimal point
                    isFractionalPart = true;
                }
                else
                {
                    // Before and after decimal point is not any digit
                    return SetParsingFailure(out integerValue, out floatValue, out isInteger);
                }
            }
            else
            {
                if (isIntegerPart)
                {
                    // It is valid number without decimal point (i.e. still integer)
                    isFractionalPart = false;
                }
                else
                {
                    // Invalid character at the begin of the string
                    return SetParsingFailure(out integerValue, out floatValue, out isInteger);
                }
            }

            var end = SkipExponent(value, index);
            if (isFractionalPart || (end > index))
            {
                integerValue = 0;
                bool isSuccessful;

                // We identify a correct floating-point number format
                if ((start == 0) && (end == value.Length))
                {
                    isSuccessful = double.TryParse(value, FLOATING_POINT_NUMBER_STYLE,
                        CultureInfo.InvariantCulture, out floatValue);
                }
                else
                {
                    isSuccessful = double.TryParse(value.Substring(start, end - start),
                        FLOATING_POINT_NUMBER_STYLE, CultureInfo.InvariantCulture, out floatValue);
                }

                Debug.Assert(isSuccessful, "The string is definitely in floating-point number format");
                isInteger = false;
                return true;
            }
            else
            {
                // There is only integer part
                isInteger = TryParseToInteger(value, start, end - start, out integerValue, out floatValue);
                return isInteger;
            }
        }

        /// <summary>
        /// Tries to convert the string value to integer and if it fails, then to floating-point number.
        /// </summary>
        /// <param name="value">String value to convert.</param>
        /// <param name="canBeHexadecimal">Determines whether to parse hexadecimal format too.</param>
        /// <param name="integerValue">New integer value if conversion is successful, otherwise 0.</param>
        /// <param name="floatValue">
        /// New floating-point number if conversion is successful or integer is too large, otherwise 0.0.
        /// </param>
        /// <param name="isInteger">
        /// <c>true</c> if value is not converted to floating-point number, otherwise <c>false</c>.
        /// </param>
        /// <returns>
        /// <c>true</c> if value is converted successfully to integer or floating-point number, otherwise
        /// <c>false</c>, even if conversion to integer fails and result is stored as floating-point value.
        /// </returns>
        public static bool TryConvertToNumber(string value, bool canBeHexadecimal, out int integerValue,
            out double floatValue, out bool isInteger)
        {
            bool isHexadecimal;
            return TryConvertToNumber(value, canBeHexadecimal, out integerValue, out floatValue,
                out isInteger, out isHexadecimal);
        }

        /// <summary>
        /// Tries to convert the string value to integer and if it fails, then to floating-point number.
        /// </summary>
        /// <param name="value">String in integer format value to parse.</param>
        /// <param name="integerValue">New integer value if conversion is successful, otherwise 0.</param>
        /// <param name="floatValue">New floating-point number if conversion fails, otherwise 0.0.</param>
        /// <returns><c>true</c> if integer is parsed successfully, otherwise <c>false</c>.</returns>
        private static bool TryParseToInteger(string value, out int integerValue, out double floatValue)
        {
            Debug.Assert(value.Length > 0, "The string with number must not be empty");

            if (int.TryParse(value, INTEGER_STYLE, CultureInfo.InvariantCulture, out integerValue))
            {
                floatValue = integerValue;
                return true;
            }
            else
            {
                integerValue = 0;
                var isSuccessful = double.TryParse(value, FLOATING_POINT_NUMBER_STYLE,
                    CultureInfo.InvariantCulture, out floatValue);
                Debug.Assert(isSuccessful, "The string is definitely in floating-point number format");
                return false;
            }
        }

        /// <summary>
        /// Tries to convert the substring to integer value and if it fails, then to floating-point number.
        /// </summary>
        /// <param name="value">String value to parse.</param>
        /// <param name="start">Start of the substring to parse.</param>
        /// <param name="length">Length of the substring to parse.</param>
        /// <param name="integerValue">New integer value if conversion is successful, otherwise 0.</param>
        /// <param name="floatValue">New floating-point number if conversion fails, otherwise 0.0.</param>
        /// <returns><c>true</c> if integer is parsed successfully, otherwise <c>false</c>.</returns>
        private static bool TryParseToInteger(string value, int start, int length,
            out int integerValue, out double floatValue)
        {
            Debug.Assert((start >= 0) && (length >= 0) && (length <= value.Length),
                "Start and length must indicate correct substring");

            if ((start == 0) && (length == value.Length))
            {
                return TryParseToInteger(value, out integerValue, out floatValue);
            }
            else
            {
                return TryParseToInteger(value.Substring(start, length), out integerValue, out floatValue);
            }
        }

        /// <summary>
        /// Tries to convert hexadecimal string to integer and if it fails, then to floating-point number.
        /// </summary>
        /// <param name="value">String to convert, it must be in format "0x[0-9a-fA-F]+".</param>
        /// <param name="index">Position of the second digit of hexadecimal number within string.</param>
        /// <param name="integerValue">New integer value if conversion is successful, otherwise 0.</param>
        /// <param name="floatValue">New floating-point number if conversion fails, otherwise 0.0.</param>
        /// <returns><c>true</c> if value is parsed to integer successfully, otherwise <c>false</c>.</returns>
        private static bool TryParseHexadecimal(string value, int index, ref int integerValue,
            out double floatValue)
        {
            Debug.Assert((index > 2) && (value.Length >= index),
                "Index is the position of the second digit of hexadecimal number within string");

            long convertedLong = integerValue;
            for (; index < value.Length; ++index)
            {
                int hexaValue;
                if (TryConvertHexadecialToInteger(value[index], out hexaValue))
                {
                    convertedLong <<= 4;
                    convertedLong += hexaValue;
                }
                else
                {
                    break;
                }

                if (convertedLong > int.MaxValue)
                {
                    integerValue = 0;
                    floatValue = System.Convert.ToDouble(convertedLong, CultureInfo.InvariantCulture);

                    ++index;
                    for (; index < value.Length; ++index)
                    {
                        if (TryConvertHexadecialToInteger(value[index], out hexaValue))
                        {
                            floatValue *= 16;
                            floatValue += hexaValue;
                        }
                        else
                        {
                            break;
                        }
                    }

                    return false;
                }
            }

            integerValue = System.Convert.ToInt32(convertedLong, CultureInfo.InvariantCulture);
            floatValue = integerValue;
            return true;
        }

        /// <summary>
        /// Tries to convert character representing hexadecimal digit to integer value.
        /// </summary>
        /// <param name="character">Character to convert.</param>
        /// <param name="value">New integer value if conversion is successful, otherwise 0.</param>
        /// <returns><c>true</c> if character is hexadecimal digit, otherwise <c>false</c>.</returns>
        private static bool TryConvertHexadecialToInteger(char character, out int value)
        {
            if (char.IsDigit(character))
            {
                value = character - '0';
                return true;
            }
            else if ((character >= 'a') && (character <= 'f'))
            {
                value = 10 + (character - 'a');
                return true;
            }
            else if ((character >= 'A') && (character <= 'F'))
            {
                value = 10 + (character - 'A');
                return true;
            }
            else
            {
                value = 0;
                return false;
            }
        }

        /// <summary>
        /// Finds out if the exponent part is at the beginning of string.
        /// </summary>
        /// <param name="value">String value to search for.</param>
        /// <param name="index">The starting character position to search for.</param>
        /// <returns>
        /// The first position after exponent part if it is valid, otherwise <paramref name="index" />.
        /// </returns>
        private static int SkipExponent(string value, int index)
        {
            if (index < value.Length)
            {
                var character = value[index];
                if ((character != 'e') && (character != 'E'))
                {
                    return index;
                }
            }
            else
            {
                return index;
            }

            var start = index;
            ++index;
            index = SkipSign(value, index);

            var end = SkipDigits(value, index);
            return (end > index) ? end : start;
        }

        /// <summary>
        /// Search the first non-digit character and returns its position.
        /// </summary>
        /// <param name="value">String value to search for.</param>
        /// <param name="index">The starting character position to search for.</param>
        /// <returns>The first position of non-digit character or length of string.</returns>
        private static int SkipDigits(string value, int index)
        {
            for (; index < value.Length; ++index)
            {
                if (!char.IsDigit(value[index]))
                {
                    break;
                }
            }

            return index;
        }

        /// <summary>
        /// Determines if the string at given position is character + or - and if so, skips it.
        /// </summary>
        /// <param name="value">String value to search for.</param>
        /// <param name="index">The starting character position to search for.</param>
        /// <returns>
        /// <paramref name="index" /> if character at position is not + or -, otherwise the next position.
        /// </returns>
        private static int SkipSign(string value, int index)
        {
            if (index < value.Length)
            {
                var character = value[index];
                if ((character == '+') || (character == '-'))
                {
                    return index + 1;
                }
            }

            return index;
        }

        /// <summary>
        /// Search the first non-whitespace character and returns its position.
        /// </summary>
        /// <param name="value">String value to search for.</param>
        /// <returns>The first position of non-whitespace character or length of string.</returns>
        private static int SkipWhiteSpace(string value)
        {
            int index;
            for (index = 0; index < value.Length; ++index)
            {
                if (!char.IsWhiteSpace(value[index]))
                {
                    break;
                }
            }

            return index;
        }

        /// <summary>
        /// Set all return values of <see cref="TypeConversion.TryConvertToNumber(
        /// string, bool, out int, out double, out bool, out bool)" /> to indicate failure.
        /// </summary>
        /// <param name="integerValue">Converted integer value, always 0.</param>
        /// <param name="floatValue">Converted floating-point value, always 0.0.</param>
        /// <param name="isInteger">Always <c>true</c>.</param>
        /// <returns>Always <c>false</c>.</returns>
        private static bool SetParsingFailure(out int integerValue, out double floatValue, out bool isInteger)
        {
            integerValue = 0;
            floatValue = 0.0;
            isInteger = true;
            return false;
        }

        #endregion ToNumber

        #region Helper methods

        /// <summary>
        /// Creates a new object of build-in type <c>stdClass</c>
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <returns>Object of <c>stdClass</c> type with no fields nor methods.</returns>
        private static ObjectValue CreateStandardObject(ISnapshotReadWrite snapshot)
        {
            var standardClassType = snapshot.ResolveType(standardClass);
            var enumerator = standardClassType.GetEnumerator();
            enumerator.MoveNext();
            return snapshot.CreateObject(enumerator.Current as TypeValue);
        }

        /// <summary>
        /// Creates snapshot entry containing a given value.
        /// </summary>
        /// <param name="snapshot">Read-write memory snapshot used for fix-point analysis.</param>
        /// <param name="value">Value wrapped into snapshot entry.</param>
        /// <returns>New value snapshot entry.</returns>
        private static ReadSnapshotEntryBase GetSnapshotEntry(ISnapshotReadWrite snapshot, Value value)
        {
            var entry = new MemoryEntry(value);
            return snapshot.CreateSnapshotEntry(entry);
        }

        /// <summary>
        /// Read memory represented by a given field identifier resolved in current snapshot entry.
        /// </summary>
        /// <param name="snapshot">Context snapshot where operation is proceeded.</param>
        /// <param name="objectEntry">Snapshot entry of the object value.</param>
        /// <param name="index">Name of the field identifier.</param>
        /// <returns>Snapshot entry representing field resolving in current entry.</returns>
        private static ReadWriteSnapshotEntryBase GetFieldEntry(SnapshotBase snapshot,
            ReadSnapshotEntryBase objectEntry, string index)
        {
            var fieldIdentifier = new VariableIdentifier(index);
            return objectEntry.ReadField(snapshot, fieldIdentifier);
        }

        /// <summary>
        /// Read memory represented by a given array index resolved in current snapshot entry.
        /// </summary>
        /// <param name="snapshot">Context snapshot where operation is proceeded.</param>
        /// <param name="arrayEntry">Snapshot entry of the array value.</param>
        /// <param name="index">Value of the index.</param>
        /// <returns>Snapshot entry representing index resolving in current entry.</returns>
        private static ReadWriteSnapshotEntryBase GetIndexEntry(SnapshotBase snapshot,
            ReadSnapshotEntryBase arrayEntry, string index)
        {
            var indexIdentifier = new MemberIdentifier(index);
            return arrayEntry.ReadIndex(snapshot, indexIdentifier);
        }

        #endregion Helper methods
    }

    /// <summary>
    /// Static class that provides functionality for comparing of value types.
    /// </summary>
    public static class ValueTypeResolver
    {
        /// <summary>
        /// Indicates if value is boolean.
        /// </summary>
        /// <param name="value">Value to be checked.</param>
        /// <returns>True if value is boolean.</returns>
        public static bool IsBool(Value value)
        {
            return value is ScalarValue<bool> || value is AnyBooleanValue;
        }

        /// <summary>
        /// Indicates if Value is integer.
        /// </summary>
        /// <param name="value">Value to be checked.</param>
        /// <returns>True if value is integer.</returns>
        public static bool IsInt(Value value)
        {
            return value is NumericValue<int> || value is IntervalValue<int> || value is AnyIntegerValue;
        }

        /// <summary>
        /// Indicates if value is long integer.
        /// </summary>
        /// <param name="value">Value to be checked.</param>
        /// <returns>True if value is long integer.</returns>
        public static bool IsLong(Value value)
        {
            return value is NumericValue<long> || value is IntervalValue<long> || value is AnyLongintValue;
        }

        /// <summary>
        /// Indicates if value is floating-point number.
        /// </summary>
        /// <param name="value">Value to be checked.</param>
        /// <returns>True if value is floating-point number.</returns>
        public static bool IsFloat(Value value)
        {
            return value is NumericValue<double> || value is IntervalValue<double> || value is AnyFloatValue;
        }

        /// <summary>
        /// Indicates if value is string.
        /// </summary>
        /// <param name="value">Value to be checked.</param>
        /// <returns>True if value is string.</returns>
        public static bool IsString(Value value)
        {
            return value is ScalarValue<string> || value is AnyStringValue;
        }

        /// <summary>
        /// Indicates if value is compound type.
        /// </summary>
        /// <param name="value">Value to be checked.</param>
        /// <returns>True if value is compound type.</returns>
        public static bool IsCompound(Value value)
        {
            return value is CompoundValue || value is AnyCompoundValue;
        }

        /// <summary>
        /// Indicates if value is object.
        /// </summary>
        /// <param name="value">Value to be checked.</param>
        /// <returns>True if value is object.</returns>
        public static bool IsObject(Value value)
        {
            return value is ObjectValue || value is AnyObjectValue;
        }

        /// <summary>
        /// Indicates if value is array.
        /// </summary>
        /// <param name="value">Value to be checked.</param>
        /// <returns>True if value is array.</returns>
        public static bool IsArray(Value value)
        {
            return value is AssociativeArray || value is AnyArrayValue;
        }

        /// <summary>
        /// Indicates if value is resource.
        /// </summary>
        /// <param name="value">Value to be checked.</param>
        /// <returns>True if value is resource.</returns>
        public static bool IsResource(Value value)
        {
            return value is ResourceValue || value is AnyResourceValue;
        }

        /// <summary>
        /// Indicates if value can be dirty.
        /// </summary>
        /// <param name="value">Value to be checked.</param>
        /// <returns>True if value is dirty.</returns>
        public static bool CanBeDirty(Value value)
        {
            return !(ValueTypeResolver.IsBool(value)
                || ValueTypeResolver.IsInt(value)
                || ValueTypeResolver.IsLong(value)
                || ValueTypeResolver.IsFloat(value)
                || ValueTypeResolver.IsObject(value)
                || ValueTypeResolver.IsArray(value));
        }

        /// <summary>
        /// Indicates if value is unknown.
        /// </summary>
        /// <param name="value">Value to be checked.</param>
        /// <returns>True if value is unknown.</returns>
        public static bool IsUnknown(Value value)
        {
            return value is UndefinedValue
                || value is AnyValue
                || value is IntervalValue<int>
                || value is IntervalValue<long>
                || value is IntervalValue<double>;
        }
    }
}