﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

using FluentAssertions.Common;
using FluentAssertions.Equivalency;
using FluentAssertions.Execution;

namespace FluentAssertions.Collections
{
    [DebuggerNonUserCode]
    public class GenericCollectionAssertions<T> :
        GenericCollectionAssertions<IEnumerable<T>, T, GenericCollectionAssertions<T>>
    {
        public GenericCollectionAssertions(IEnumerable<T> actualValue)
            : base(actualValue)
        {
        }
    }

    [DebuggerNonUserCode]
    public class GenericCollectionAssertions<TCollection, T> :
        GenericCollectionAssertions<TCollection, T, GenericCollectionAssertions<TCollection, T>>
        where TCollection : IEnumerable<T>
    {
        public GenericCollectionAssertions(TCollection actualValue)
            : base(actualValue)
        {
        }
    }

    [DebuggerNonUserCode]
    public class GenericCollectionAssertions<TCollection, T, TAssertions> :
        SelfReferencingCollectionAssertions<TCollection, T, TAssertions>
        where TCollection : IEnumerable<T>
        where TAssertions : GenericCollectionAssertions<TCollection, T, TAssertions>
    {
        public GenericCollectionAssertions(TCollection actualValue)
            : base(actualValue)
        {
        }

        /// <summary>
        /// Asserts that the collection does not contain any <c>null</c> items.
        /// </summary>
        /// <param name="predicate">The predicate when evaluated should not be null.</param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because" />.
        /// </param>
        public AndConstraint<TAssertions> NotContainNulls<TKey>(Expression<Func<T, TKey>> predicate, string because = "", params object[] becauseArgs)
            where TKey : class
        {
            Guard.ThrowIfArgumentIsNull(predicate, nameof(predicate));

            if (Subject is null)
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .FailWith("Expected {context:collection} not to contain <null>s{reason}, but collection is <null>.");
            }

            Func<T, TKey> compiledPredicate = predicate.Compile();

            T[] values = Subject
                .Where(e => compiledPredicate(e) is null)
                .ToArray();

            if (values.Length > 0)
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .FailWith("Expected {context:collection} not to contain <null>s on {0}{reason}, but found {1}.",
                        predicate.Body,
                        values);
            }

            return new AndConstraint<TAssertions>((TAssertions)this);
        }

        /// <summary>
        /// Asserts that the collection does not contain any duplicate items.
        /// </summary>
        /// <param name="predicate">The predicate to group the items by.</param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because" />.
        /// </param>
        public AndConstraint<TAssertions> OnlyHaveUniqueItems<TKey>(Expression<Func<T, TKey>> predicate, string because = "", params object[] becauseArgs)
        {
            Guard.ThrowIfArgumentIsNull(predicate, nameof(predicate));

            if (Subject is null)
            {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .FailWith("Expected {context:collection} to only have unique items{reason}, but found {0}.", Subject);
            }

            Func<T, TKey> compiledPredicate = predicate.Compile();

            IGrouping<TKey, T>[] groupWithMultipleItems = Subject
                .GroupBy(compiledPredicate)
                .Where(g => g.Count() > 1)
                .ToArray();

            if (groupWithMultipleItems.Length > 0)
            {
                if (groupWithMultipleItems.Length > 1)
                {
                    Execute.Assertion
                        .BecauseOf(because, becauseArgs)
                        .FailWith("Expected {context:collection} to only have unique items on {0}{reason}, but items {1} are not unique.",
                            predicate.Body,
                            groupWithMultipleItems.SelectMany(g => g));
                }
                else
                {
                    Execute.Assertion
                        .BecauseOf(because, becauseArgs)
                        .FailWith("Expected {context:collection} to only have unique items on {0}{reason}, but item {1} is not unique.",
                            predicate.Body,
                            groupWithMultipleItems[0].First());
                }
            }

            return new AndConstraint<TAssertions>((TAssertions)this);
        }

        /// <summary>
        /// Asserts that a collection is ordered in ascending order according to the value of the specified
        /// <paramref name="propertyExpression"/>.
        /// </summary>
        /// <param name="propertyExpression">
        /// A lambda expression that references the property that should be used to determine the expected ordering.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because" />.
        /// </param>
        /// <remarks>
        /// Empty and single element collections are considered to be ordered both in ascending and descending order at the same time.
        /// </remarks>
        public AndConstraint<TAssertions> BeInAscendingOrder<TSelector>(
            Expression<Func<T, TSelector>> propertyExpression, string because = "", params object[] becauseArgs)
        {
            return BeInAscendingOrder(propertyExpression, Comparer<TSelector>.Default, because, becauseArgs);
        }

        /// <summary>
        /// Asserts that a collection is not ordered in ascending order according to the value of the specified
        /// <paramref name="propertyExpression"/>.
        /// </summary>
        /// <param name="propertyExpression">
        /// A lambda expression that references the property that should be used to determine the expected ordering.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because" />.
        /// </param>
        /// <remarks>
        /// Empty and single element collections are considered to be ordered both in ascending and descending order at the same time.
        /// </remarks>
        public AndConstraint<TAssertions> NotBeInAscendingOrder<TSelector>(
            Expression<Func<T, TSelector>> propertyExpression, string because = "", params object[] becauseArgs)
        {
            return NotBeInAscendingOrder(propertyExpression, Comparer<TSelector>.Default, because, becauseArgs);
        }

        /// <summary>
        /// Asserts that a collection is ordered in ascending order according to the value of the specified
        /// <see cref="IComparer{T}"/> implementation.
        /// </summary>
        /// <param name="comparer">
        /// The object that should be used to determine the expected ordering.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because" />.
        /// </param>
        /// <remarks>
        /// Empty and single element collections are considered to be ordered both in ascending and descending order at the same time.
        /// </remarks>
        public AndConstraint<TAssertions> BeInAscendingOrder(
            IComparer<T> comparer, string because = "", params object[] becauseArgs)
        {
            return BeInAscendingOrder(item => item, comparer, because, becauseArgs);
        }

        /// <summary>
        /// Asserts that a collection is not ordered in ascending order according to the value of the specified
        /// <see cref="IComparer{T}"/> implementation.
        /// </summary>
        /// <param name="comparer">
        /// The object that should be used to determine the expected ordering.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because" />.
        /// </param>
        /// <remarks>
        /// Empty and single element collections are considered to be ordered both in ascending and descending order at the same time.
        /// </remarks>
        public AndConstraint<TAssertions> NotBeInAscendingOrder(
            IComparer<T> comparer, string because = "", params object[] becauseArgs)
        {
            return NotBeInAscendingOrder(item => item, comparer, because, becauseArgs);
        }

        /// <summary>
        /// Asserts that a collection is ordered in ascending order according to the value of the specified
        /// <paramref name="propertyExpression"/> and <see cref="IComparer{T}"/> implementation.
        /// </summary>
        /// <param name="propertyExpression">
        /// A lambda expression that references the property that should be used to determine the expected ordering.
        /// </param>
        /// <param name="comparer">
        /// The object that should be used to determine the expected ordering.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because" />.
        /// </param>
        /// <remarks>
        /// Empty and single element collections are considered to be ordered both in ascending and descending order at the same time.
        /// </remarks>
        public AndConstraint<TAssertions> BeInAscendingOrder<TSelector>(
            Expression<Func<T, TSelector>> propertyExpression, IComparer<TSelector> comparer, string because = "", params object[] becauseArgs)
        {
            return BeOrderedBy(propertyExpression, comparer, SortOrder.Ascending, because, becauseArgs);
        }

        /// <summary>
        /// Asserts that a collection is not ordered in ascending order according to the value of the specified
        /// <paramref name="propertyExpression"/> and <see cref="IComparer{T}"/> implementation.
        /// </summary>
        /// <param name="propertyExpression">
        /// A lambda expression that references the property that should be used to determine the expected ordering.
        /// </param>
        /// <param name="comparer">
        /// The object that should be used to determine the expected ordering.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because" />.
        /// </param>
        /// <remarks>
        /// Empty and single element collections are considered to be ordered both in ascending and descending order at the same time.
        /// </remarks>
        public AndConstraint<TAssertions> NotBeInAscendingOrder<TSelector>(
            Expression<Func<T, TSelector>> propertyExpression, IComparer<TSelector> comparer, string because = "", params object[] becauseArgs)
        {
            return NotBeOrderedBy(propertyExpression, comparer, SortOrder.Ascending, because, becauseArgs);
        }

        /// <summary>
        /// Asserts that a collection is ordered in descending order according to the value of the specified
        /// <paramref name="propertyExpression"/>.
        /// </summary>
        /// <param name="propertyExpression">
        /// A lambda expression that references the property that should be used to determine the expected ordering.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because" />.
        /// </param>
        /// <remarks>
        /// Empty and single element collections are considered to be ordered both in ascending and descending order at the same time.
        /// </remarks>
        public AndConstraint<TAssertions> BeInDescendingOrder<TSelector>(
            Expression<Func<T, TSelector>> propertyExpression, string because = "", params object[] becauseArgs)
        {
            return BeInDescendingOrder(propertyExpression, Comparer<TSelector>.Default, because, becauseArgs);
        }

        /// <summary>
        /// Asserts that a collection is not ordered in descending order according to the value of the specified
        /// <paramref name="propertyExpression"/>.
        /// </summary>
        /// <param name="propertyExpression">
        /// A lambda expression that references the property that should be used to determine the expected ordering.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because" />.
        /// </param>
        /// <remarks>
        /// Empty and single element collections are considered to be ordered both in ascending and descending order at the same time.
        /// </remarks>
        public AndConstraint<TAssertions> NotBeInDescendingOrder<TSelector>(
            Expression<Func<T, TSelector>> propertyExpression, string because = "", params object[] becauseArgs)
        {
            return NotBeInDescendingOrder(propertyExpression, Comparer<TSelector>.Default, because, becauseArgs);
        }

        /// <summary>
        /// Asserts that a collection is ordered in descending order according to the value of the specified
        /// <see cref="IComparer{T}"/> implementation.
        /// </summary>
        /// <param name="comparer">
        /// The object that should be used to determine the expected ordering.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because" />.
        /// </param>
        /// <remarks>
        /// Empty and single element collections are considered to be ordered both in ascending and descending order at the same time.
        /// </remarks>
        public AndConstraint<TAssertions> BeInDescendingOrder(
            IComparer<T> comparer, string because = "", params object[] becauseArgs)
        {
            return BeInDescendingOrder(item => item, comparer, because, becauseArgs);
        }

        /// <summary>
        /// Asserts that a collection is not ordered in descending order according to the value of the specified
        /// <see cref="IComparer{T}"/> implementation.
        /// </summary>
        /// <param name="comparer">
        /// The object that should be used to determine the expected ordering.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because" />.
        /// </param>
        /// <remarks>
        /// Empty and single element collections are considered to be ordered both in ascending and descending order at the same time.
        /// </remarks>
        public AndConstraint<TAssertions> NotBeInDescendingOrder(
            IComparer<T> comparer, string because = "", params object[] becauseArgs)
        {
            return NotBeInDescendingOrder(item => item, comparer, because, becauseArgs);
        }

        /// <summary>
        /// Asserts that a collection is ordered in descending order according to the value of the specified
        /// <paramref name="propertyExpression"/> and <see cref="IComparer{T}"/> implementation.
        /// </summary>
        /// <param name="propertyExpression">
        /// A lambda expression that references the property that should be used to determine the expected ordering.
        /// </param>
        /// <param name="comparer">
        /// The object that should be used to determine the expected ordering.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because" />.
        /// </param>
        /// <remarks>
        /// Empty and single element collections are considered to be ordered both in ascending and descending order at the same time.
        /// </remarks>
        public AndConstraint<TAssertions> BeInDescendingOrder<TSelector>(
            Expression<Func<T, TSelector>> propertyExpression, IComparer<TSelector> comparer, string because = "", params object[] becauseArgs)
        {
            return BeOrderedBy(propertyExpression, comparer, SortOrder.Descending, because, becauseArgs);
        }

        /// <summary>
        /// Asserts that a collection not is ordered in descending order according to the value of the specified
        /// <paramref name="propertyExpression"/> and <see cref="IComparer{T}"/> implementation.
        /// </summary>
        /// <param name="propertyExpression">
        /// A lambda expression that references the property that should be used to determine the expected ordering.
        /// </param>
        /// <param name="comparer">
        /// The object that should be used to determine the expected ordering.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])"/> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because" />.
        /// </param>
        /// <remarks>
        /// Empty and single element collections are considered to be ordered both in ascending and descending order at the same time.
        /// </remarks>
        public AndConstraint<TAssertions> NotBeInDescendingOrder<TSelector>(
            Expression<Func<T, TSelector>> propertyExpression, IComparer<TSelector> comparer, string because = "", params object[] becauseArgs)
        {
            return NotBeOrderedBy(propertyExpression, comparer, SortOrder.Descending, because, becauseArgs);
        }

        private AndConstraint<TAssertions> BeOrderedBy<TSelector>(
            Expression<Func<T, TSelector>> propertyExpression,
            IComparer<TSelector> comparer,
            SortOrder direction,
            string because,
            object[] becauseArgs)
        {
            Guard.ThrowIfArgumentIsNull(comparer, nameof(comparer), "Cannot assert collection ordering without specifying a comparer.");

            if (IsValidProperty(propertyExpression, because, becauseArgs))
            {
                ICollection<T> unordered = Subject.ConvertOrCastToCollection();

                IOrderedEnumerable<T> expectation = ConvertToOrderedEnumerable(
                    propertyExpression,
                    comparer,
                    direction,
                    unordered);

                string orderString = GetExpressionOrderString(propertyExpression);

                Execute.Assertion
                    .ForCondition(unordered.SequenceEqual(expectation))
                    .BecauseOf(because, becauseArgs)
                    .FailWith("Expected {context:collection} {0} to be ordered {1}{reason} and result in {2}.",
                        Subject, orderString, expectation);
            }

            return new AndConstraint<TAssertions>((TAssertions)this);
        }

        private AndConstraint<TAssertions> NotBeOrderedBy<TSelector>(
            Expression<Func<T, TSelector>> propertyExpression,
            IComparer<TSelector> comparer,
            SortOrder direction,
            string because,
            object[] becauseArgs)
        {
            Guard.ThrowIfArgumentIsNull(comparer, nameof(comparer), "Cannot assert collection ordering without specifying a comparer.");

            if (IsValidProperty(propertyExpression, because, becauseArgs))
            {
                ICollection<T> unordered = Subject.ConvertOrCastToCollection();

                IOrderedEnumerable<T> expectation = ConvertToOrderedEnumerable(
                    propertyExpression,
                    comparer,
                    direction,
                    unordered);

                string orderString = GetExpressionOrderString(propertyExpression);

                Execute.Assertion
                    .ForCondition(!unordered.SequenceEqual(expectation))
                    .BecauseOf(because, becauseArgs)
                    .FailWith("Expected {context:collection} {0} to not be ordered {1}{reason} and not result in {2}.",
                        Subject, orderString, expectation);
            }

            return new AndConstraint<TAssertions>((TAssertions)this);
        }

        private bool IsValidProperty<TSelector>(Expression<Func<T, TSelector>> propertyExpression, string because, object[] becauseArgs)
        {
            Guard.ThrowIfArgumentIsNull(propertyExpression, nameof(propertyExpression),
                "Cannot assert collection ordering without specifying a property.");

            return Execute.Assertion
                .ForCondition(!(Subject is null))
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected {context:collection} to be ordered by {0}{reason} but found <null>.",
                    propertyExpression.GetMemberPath());
        }

        private static IOrderedEnumerable<T> ConvertToOrderedEnumerable<TSelector>(
            Expression<Func<T, TSelector>> propertyExpression,
            IComparer<TSelector> comparer,
            SortOrder direction,
            ICollection<T> unordered)
        {
            Func<T, TSelector> keySelector = propertyExpression.Compile();

            IOrderedEnumerable<T> expectation = (direction == SortOrder.Ascending)
                ? unordered.OrderBy(keySelector, comparer)
                : unordered.OrderByDescending(keySelector, comparer);

            return expectation;
        }

        private static string GetExpressionOrderString<TSelector>(Expression<Func<T, TSelector>> propertyExpression)
        {
            string orderString = propertyExpression.GetMemberPath().ToString();

            orderString = orderString == "\"\"" ? string.Empty : "by " + orderString;

            return orderString;
        }

        /// <summary>
        /// Asserts that all elements in a collection of objects are equivalent to a given object.
        /// </summary>
        /// <remarks>
        /// Objects within the collection are equivalent to given object when both object graphs have equally named properties with the same
        /// value, irrespective of the type of those objects. Two properties are also equal if one type can be converted to another
        /// and the result is equal.
        /// The type of a collection property is ignored as long as the collection implements <see cref="IEnumerable"/> and all
        /// items in the collection are structurally equal.
        /// Notice that actual behavior is determined by the global defaults managed by <see cref="AssertionOptions"/>.
        /// </remarks>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because"/>.
        /// </param>
        public AndConstraint<TAssertions> AllBeEquivalentTo<TExpectation>(TExpectation expectation,
            string because = "", params object[] becauseArgs)
        {
            return AllBeEquivalentTo(expectation, options => options, because, becauseArgs);
        }

        /// <summary>
        /// Asserts that all elements in a collection of objects are equivalent to a given object.
        /// </summary>
        /// <remarks>
        /// Objects within the collection are equivalent to given object when both object graphs have equally named properties with the same
        /// value, irrespective of the type of those objects. Two properties are also equal if one type can be converted to another
        /// and the result is equal.
        /// The type of a collection property is ignored as long as the collection implements <see cref="IEnumerable"/> and all
        /// items in the collection are structurally equal.
        /// Notice that actual behavior is determined by the global defaults managed by <see cref="AssertionOptions"/>.
        /// </remarks>
        /// <param name="config">
        /// A reference to the <see cref="EquivalencyAssertionOptions{TExpectation}"/> configuration object that can be used
        /// to influence the way the object graphs are compared. You can also provide an alternative instance of the
        /// <see cref="EquivalencyAssertionOptions{TSubject}"/> class. The global defaults are determined by the
        /// <see cref="AssertionOptions"/> class.
        /// </param>
        /// <param name="because">
        /// A formatted phrase as is supported by <see cref="string.Format(string,object[])" /> explaining why the assertion
        /// is needed. If the phrase does not start with the word <i>because</i>, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">
        /// Zero or more objects to format using the placeholders in <paramref name="because"/>.
        /// </param>
        public AndConstraint<TAssertions> AllBeEquivalentTo<TExpectation>(TExpectation expectation,
            Func<EquivalencyAssertionOptions<TExpectation>, EquivalencyAssertionOptions<TExpectation>> config,
            string because = "",
            params object[] becauseArgs)
        {
            Guard.ThrowIfArgumentIsNull(config, nameof(config));

            TExpectation[] repeatedExpectation = RepeatAsManyAs(expectation, Subject).ToArray();

            // Because we have just manually created the collection based on single element
            // we are sure that we can force strict ordering, because ordering does not matter in terms
            // of correctness. On the other hand we do not want to change ordering rules for nested objects
            // in case user needs to use them. Strict ordering improves algorithmic complexity
            // from O(n^2) to O(n). For bigger tables it is necessary in order to achieve acceptable
            // execution times.
            Func<EquivalencyAssertionOptions<TExpectation>, EquivalencyAssertionOptions<TExpectation>> forceStringOrderingConfig =
                x => config(x).WithStrictOrderingFor(s => string.IsNullOrEmpty(s.SelectedMemberPath));

            return BeEquivalentTo(repeatedExpectation, forceStringOrderingConfig, because, becauseArgs);
        }
    }
}
