﻿// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="EnumerableFluentAssert.cs" company="">
// //   Copyright 2013 Thomas PIERRAIN
// //   Licensed under the Apache License, Version 2.0 (the "License");
// //   you may not use this file except in compliance with the License.
// //   You may obtain a copy of the License at
// //       http://www.apache.org/licenses/LICENSE-2.0
// //   Unless required by applicable law or agreed to in writing, software
// //   distributed under the License is distributed on an "AS IS" BASIS,
// //   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// //   See the License for the specific language governing permissions and
// //   limitations under the License.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------
namespace NFluent
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class EnumerableFluentAssert : IEnumerableFluentAssert
    {
        private readonly IEnumerable sutEnumerable;

        public EnumerableFluentAssert(IEnumerable sutEnumerable)
        {
            this.sutEnumerable = sutEnumerable;
        }

        #region IEqualityFluentAssert members

        public void IsEqualTo(object expected)
        {
            EqualityHelper.IsEqualTo(this.sutEnumerable, expected);
        }

        public void IsNotEqualTo(object expected)
        {
            EqualityHelper.IsNotEqualTo(this.sutEnumerable, expected);
        }

        #endregion

        #region IFluentAssert members

        public void IsInstanceOf(Type expectedType)
        {
            IsInstanceHelper.IsInstanceOf(this.sutEnumerable, expectedType);
        }

        public void IsNotInstanceOf(Type expectedType)
        {
            IsInstanceHelper.IsNotInstanceOf(this.sutEnumerable, expectedType);
        }

        #endregion


        /// <summary>
        /// Verifies that the specified enumerable contains the given expected values, in any order.
        /// </summary>
        /// <typeparam name="T">Type of the elements contained in the enumerable.</typeparam>
        /// <param name="enumerable">The enumerable that should hold the expected values.</param>
        /// <param name="expectedValues">The expected values.</param>
        /// <returns>
        ///   <c>true</c> if the enumerable contains all the specified expected values, in any order; throws a <see cref="FluentAssertionException"/> otherwise.
        /// </returns>
        /// <exception cref="NFluent.FluentAssertionException">The enumerable does not contains all the expected values.</exception>
        public void Contains<T>(params T[] expectedValues)
        {
            IEnumerable properExpectedValues;
            if (IsAOneValueArrayWithOneCollectionInside(expectedValues))
            {
                properExpectedValues = expectedValues[0] as IEnumerable;
            }
            else
            {
                properExpectedValues = expectedValues as IEnumerable;
            }

            this.Contains(properExpectedValues);
        }

        public void Contains(IEnumerable otherEnumerable)
        {
            var notFoundValues = ExtractNotFoundValues(this.sutEnumerable, otherEnumerable);

            if (notFoundValues.Count == 0)
            {
                return;
            }

            throw new FluentAssertionException(String.Format("The enumerable [{0}] does not contain the expected value(s): [{1}].", EnumerableExtensions.ToEnumeratedString(this.sutEnumerable),  EnumerableExtensions.ToEnumeratedString(notFoundValues)));
        }

        // TODO: make all EnumerableFluentAssert failure messages the same. I.e. "the enumerable [sut] does not contain ... [...]."

        /// <summary>
        /// Verifies that the actual enumerable contains only the given expected values and nothing else, in order.
        /// This assertion should only be used with IEnumerable that have a consistent iteration order 
        /// (i.e. don't use it with <see cref="Hashtable"/>, prefer <see cref="ContainsOnly{T}"/> in that case).
        /// </summary>
        /// <typeparam name="T">Type of the elements contained in the <see cref="expectedValues"/> array.</typeparam>
        /// <param name="enumerable">The enumerable to verify.</param>
        /// <param name="expectedValues">The expected values to be searched.</param>
        /// <returns>
        ///   <c>true</c> if the enumerable contains exactly the specified expected values; throws a <see cref="FluentAssertionException"/> otherwise.
        /// </returns>
        /// <exception cref="NFluent.FluentAssertionException">The specified enumerable does not contains exactly the specified expected values.</exception>
        public void ContainsExactly<T>(params T[] expectedValues)
        {
            IEnumerable properExpectedValues;
            if (IsAOneValueArrayWithOneCollectionInside(expectedValues))
            {
                properExpectedValues = expectedValues[0] as IEnumerable;
            }
            else
            {
                properExpectedValues = expectedValues as IEnumerable;
            }

            this.ContainsExactly(properExpectedValues);
        }

        /// <summary>
        /// Determines whether the enumerable contains exactly some expected values present in another given enumerable. 
        /// </summary>
        /// <param name="otherEnumerable">The other enumerable.</param>
        /// <exception cref="FluentAssertionException"></exception>
        /// <exception cref="NFluent.FluentAssertionException">The specified enumerable does not contains exactly the specified expected values.</exception>
        public void ContainsExactly(IEnumerable otherEnumerable)
        {
            // TODO: Refactor this implementation
            if (otherEnumerable == null)
            {
                long foundCount;
                var foundItems = this.sutEnumerable.ToEnumeratedString(out foundCount);
                var foundItemsCount = FormatItemCount(foundCount);
                throw new FluentAssertionException(String.Format("Found: [{0}] ({1}) instead of the expected [null] (0 item).", foundItems, foundItemsCount));
            }

            var first = this.sutEnumerable.GetEnumerator();
            var second = otherEnumerable.GetEnumerator();

            while (first.MoveNext())
            {
                if (!second.MoveNext() || !Equals(first.Current, second.Current))
                {
                    long foundCount;
                    var foundItems = this.sutEnumerable.ToEnumeratedString(out foundCount);
                    var formatedFoundCount = FormatItemCount(foundCount);

                    long expectedCount;
                    object expectedItems = otherEnumerable.ToEnumeratedString(out expectedCount);
                    var formatedExpectedCount = FormatItemCount(expectedCount);

                    throw new FluentAssertionException(String.Format("Found: [{0}] ({1}) instead of the expected [{2}] ({3}).", foundItems, formatedFoundCount, expectedItems, formatedExpectedCount));
                }
            }
        }

        // TODO: Move the FormatItemCount() method from ContainsExtensions to EnumerableFluentAssert. 

        ///// <summary>
        ///// Verifies that the actual enumerable contains only the given expected values and nothing else, in order.
        ///// This assertion should only be used with IEnumerable that have a consistent iteration order 
        ///// (i.e. don't use it with <see cref="Hashtable"/>, prefer <see cref="ContainsOnly"/> in that case).
        ///// </summary>
        ///// <typeparam name="T">Type of the elements contained in the <see cref="expectedValues"/> array.</typeparam>
        ///// <param name="enumerable">The enumerable to verify.</param>
        ///// <param name="expectedValues">The expected values to be searched.</param>
        ///// <returns>
        /////   <c>true</c> if the enumerable contains exactly the specified expected values; throws a <see cref="FluentAssertionException"/> otherwise.
        ///// </returns>
        ///// <exception cref="NFluent.FluentAssertionException">The specified enumerable does not contains exactly the specified expected values.</exception>
        //public bool ContainsExactly<T>(params T[] expectedValues)
        //{
        //    long i = 0;
        //    foreach (var obj in this.enumerable)
        //    {
        //        if (!object.Equals(obj, expectedValues[i]))
        //        {
        //            var expectedNumberOfItemsDescription = ContainsExtensions.FormatItemCount(expectedValues.LongLength);

        //            var enumerableCount = 0;
        //            foreach (var item in this.enumerable)
        //            {
        //                enumerableCount++;
        //            }

        //            var foundNumberOfItemsDescription = string.Format(enumerableCount <= 1 ? "{0} item" : "{0} items", enumerableCount);

        //            throw new FluentAssertionException(string.Format("Found: [{0}] ({1}) instead of the expected [{2}] ({3}).", this.enumerable.ToEnumeratedString(), foundNumberOfItemsDescription, expectedValues.ToEnumeratedString(), expectedNumberOfItemsDescription));
        //        }

        //        i++;
        //    }
        //    return true;
        //}

        /// <summary>
        /// Extract all the values of a given property given its name, from an enumerable collection of objects holding that property.
        /// </summary>
        /// <typeparam name="T">Type of the objects belonging to the initial enumerable collection.</typeparam>
        /// <param name="enumerable">The enumerable collection of objects.</param>
        /// <param name="propertyName">Name of the property to extract value from for every object of the collection.</param>
        /// <returns>
        /// An enumerable of all the property values for every <see cref="T"/> objects in the <see cref="sutEnumerable"/>.
        /// </returns>
        //public IEnumerableFluentAssert<R> Properties<T, R>(string propertyName)
        //{
        //    IEnumerable properties = this.enumerable.Properties(propertyName);

        //    return new EnumerableFluentAssert<R>(properties as IEnumerable<R>);
        //}

        

        /// <summary>
        /// Verifies that the specified array contains the given expected values, in any order.
        /// </summary>
        /// <typeparam name="T">Type of the elements contained in the arrays.</typeparam>
        /// <param name="array">The array that should hold the expected values.</param>
        /// <param name="expectedValues">The expected values.</param>
        /// <returns>
        ///   <c>true</c> if the array contains all the specified expected values, in any order; throws a <see cref="FluentAssertionException"/> otherwise.
        /// </returns>
        /// <exception cref="NFluent.FluentAssertionException">The array does not contains all the expected values.</exception>
        //public static bool Contains<T>(this T[] array, params T[] expectedValues)
        //{
        //    var notFoundValues = ContainsExtensions.ExtractNotFoundValues(array, expectedValues);

        //    if (notFoundValues.Count == 0)
        //    {
        //        return true;
        //    }

        //    throw new FluentAssertionException(String.Format("The array does not contain the expected value(s): [{0}].", notFoundValues.ToEnumeratedString()));
        //}
        /// <summary>
        /// Verifies that the actual array contains only the given values and nothing else, in any order.
        /// </summary>
        /// <typeparam name="T">Type of the expected elements to search within.</typeparam>
        /// <param name="array">The array to verify.</param>
        /// <param name="expectedValues">The expected values to be searched.</param>
        /// <returns>
        ///   <c>true</c> if the specified array contains only the given values and nothing else, in any order; otherwise, throws a <see cref="FluentAssertionException"/>.
        /// </returns>
        //public static bool ContainsOnly<T>(this T[] array, params T[] expectedValues)
        //{
        //    var unexpectedValuesFound = ContainsExtensions.ExtractUnexpectedValues(array, expectedValues);

        //    if (unexpectedValuesFound.Count > 0)
        //    {
        //        throw new FluentAssertionException(String.Format("The array does not contain only the expected value(s). It contains also other values: [{0}].", unexpectedValuesFound.ToEnumeratedString()));
        //    }

        //    return true;
        //}

        /// <summary>
        /// Verifies that the actual enumerable contains only the given values and nothing else, in any order.
        /// </summary>
        /// <typeparam name="T">Type of the expected elements to search within.</typeparam>
        /// <param name="enumerable">The array to verify.</param>
        /// <param name="expectedValues">The expected values to be searched.</param>
        /// <returns>
        ///   <c>true</c> if the specified enumerable contains only the given values and nothing else, in any order; otherwise, throws a <see cref="FluentAssertionException"/>.
        /// </returns>
        public void ContainsOnly<T>(params T[] expectedValues)
        {
            IEnumerable properExpectedValues;
            if (IsAOneValueArrayWithOneCollectionInside(expectedValues))
            {
                properExpectedValues = expectedValues[0] as IEnumerable;
            }
            else
            {
                properExpectedValues = expectedValues as IEnumerable;
            }

            this.ContainsOnly(properExpectedValues);
        }

        private static bool IsAOneValueArrayWithOneCollectionInside<T>(T[] expectedValues)
        {
            // For every collections like ArrayList, List<T>, IEnumerable<T>, StringCollection, etc.
            return expectedValues != null && (expectedValues.LongLength == 1) && (IsAnEnumerableButNotAnEnumerableOfChars(expectedValues[0]));
        }

        private static bool IsAnEnumerableButNotAnEnumerableOfChars<T>(T element)
        {
            return (element is IEnumerable) && !(element is IEnumerable<char>);
        }

        public void ContainsOnly(IEnumerable expectedValues)
        {
            var unexpectedValuesFound = ExtractUnexpectedValues(this.sutEnumerable, expectedValues);

            if (unexpectedValuesFound.Count > 0)
            {
                throw new FluentAssertionException(String.Format("The enumerable [{0}] does not contain only the expected value(s). It contains also other values: [{1}].", EnumerableExtensions.ToEnumeratedString(this.sutEnumerable), EnumerableExtensions.ToEnumeratedString(unexpectedValuesFound)));
            }
        }

        /// <summary>
        /// Determines whether the SUT enumerable has the proper size (i.e. number of elements).
        /// </summary>
        /// <param name="expectedSize">The expected size.</param>
        /// <exception cref="FluentAssertionException">The SUT enumerable has not the expected size.</exception>
        public void HasSize(long expectedSize)
        {
            long itemsCount = this.sutEnumerable.Cast<object>().LongCount();

            if (expectedSize != itemsCount)
            {
                throw new FluentAssertionException(String.Format("Has [{0}] items instead of the expected value [{1}].", itemsCount, expectedSize));
            }
        }

        /// <summary>
        /// Verifies whether the enumerable is empty, and throws a <see cref="FluentAssertionException" /> if not empty.
        /// </summary>
        /// <param name="enumerable">The enumerable to check.</param>
        /// <exception cref="FluentAssertionException">The actual enumeration is not empty.</exception>
        public void IsEmpty()
        {
            if (this.sutEnumerable.Cast<object>().Any())
            {
                throw new FluentAssertionException(String.Format("Enumerable not empty. Contains the element(s): [{0}].", this.sutEnumerable.ToEnumeratedString()));
            }
        }

        /// <summary>
        /// Returns all expected values that aren't present in the enumerable.
        /// </summary>
        /// <typeparam name="T">Type of data to enumerate and find.</typeparam>
        /// <param name="enumerable">The enumerable to inspect.</param>
        /// <param name="expectedValues">The expected values to search within the enumerable.</param>
        /// <returns>A list containing all the expected values that aren't present in the enumerable.</returns>
        internal static IList ExtractNotFoundValues(IEnumerable enumerable, IEnumerable expectedValues)
        {
            // Prepares the list to return
            var notFoundValues = new List<object>();
            foreach (var expectedValue in expectedValues)
            {
                notFoundValues.Add(expectedValue);
            }

            foreach (var element in enumerable)
            {
                foreach (var expectedValue in expectedValues)
                {
                    if (Equals(element, expectedValue))
                    {
                        notFoundValues.RemoveAll((one) => one.Equals(expectedValue));
                        break;
                    }
                }
            }

            return notFoundValues;
        }

        /// <summary>
        /// Returns all the values of the enumerable that don't belong to the expected ones. 
        /// </summary>
        /// <typeparam name="T">Type of enumerable and expected values.</typeparam>
        /// <param name="enumerable">The enumerable to inspect.</param>
        /// <param name="expectedValues">The allowed values to be part of the enumerable.</param>
        /// <returns>A list with all the values found in the enumerable that don't belong to the expected ones.</returns>
        internal static IList ExtractUnexpectedValues(IEnumerable enumerable, IEnumerable expectedValues)
        {
            var unexpectedValuesFound = new List<object>();
            foreach (var element in enumerable)
            {
                var isExpectedValue = false;
                foreach (var expectedValue in expectedValues)
                {
                    if (Equals(element, expectedValue))
                    {
                        isExpectedValue = true;
                        break;
                    }
                }

                if (!isExpectedValue)
                {
                    unexpectedValuesFound.Add(element);
                }
            }

            return unexpectedValuesFound;
        }

        /// <summary>
        /// Generates the proper description for the items count, based on their numbers.
        /// </summary>
        /// <param name="itemsCount">The number of items.</param>
        /// <returns>The proper description for the items count.</returns>
        internal static string FormatItemCount(long itemsCount)
        {
            return String.Format(itemsCount <= 1 ? "{0} item" : "{0} items", itemsCount);
        }
    }
}