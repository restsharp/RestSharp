using FluentAssertions.Collections;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
// Since the generic types are so long, lets simplify them:
using CookieCollection = FluentAssertions.Collections.GenericCollectionAssertions<System.Net.Cookie>;
using AndWhich = FluentAssertions.AndWhichConstraint<
    FluentAssertions.Collections.GenericCollectionAssertions<System.Net.Cookie>, System.Net.Cookie>;

namespace RestSharp.Tests.Shared.Extensions;

/// <summary>
/// Some Fluent Assertion helper extensions for verifying CookieCollection contents.
/// </summary>
public static class FluentAssertionCookieExtensions {
    /// <summary>
    /// Allow FluentAssertions to be able to easily verify name/value cookies exist.
    /// </summary>
    /// <param name="genericCollection"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    static public AndWhich ContainCookieWithNameAndValue(this CookieCollection genericCollection, string name, string value, string because = "", params object[] becauseArgs) {
        bool success = Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(genericCollection.Subject is not null)
            .FailWith("Expected Cookie {context:collection} to contain Name: '{0}', Value: '{1}'{reason}, but found <null>.", name, value);

        IEnumerable<Cookie> matches = Enumerable.Empty<Cookie>();

        if (success) {
            IEnumerable<Cookie> collection = genericCollection.Subject;

            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(ContainsCookieWithNameAndValue(collection, name, value))
                .FailWith("Expected Cookie {context:collection} {0} to contain cookie with Name: '{1}' Value: '{2}'{reason}.", collection, name, value);

            matches = collection.Where(item => ContainsCookieWithNameAndValue(collection, name, value));
        }

        return new AndWhich(genericCollection, matches);
    }

    /// <summary>
    /// Allow FluentAssertions to be able to easily verify that the supplied name/value cookie does NOT exist.
    /// </summary>
    /// <param name="genericCollection"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    static public AndWhich NotContainCookieWithNameAndValue(this CookieCollection genericCollection, string name, string value, string because = "", params object[] becauseArgs) {
        bool success = Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(genericCollection.Subject is not null)
            .FailWith("Expected {context:collection} to not contain Name: '{0}', Value: '{1}'{reason}, but found <null>.", name, value);

        IEnumerable<Cookie> matched = Enumerable.Empty<Cookie>();

        if (success) {
            IEnumerable<Cookie> collection = genericCollection.Subject;

            if (ContainsCookieWithNameAndValue(collection, name, value)) {
                Execute.Assertion
                    .BecauseOf(because, becauseArgs)
                    .FailWith("Expected {context:collection} {0} to not contain cookie with Name: '{1}' Value: '{2}'{reason}.", collection, name, value);
            }

            matched = collection.Where(item => ContainsCookieWithNameAndValue(collection, name, value));
        }

        return new AndWhich(genericCollection, matched);
    }

    /// <summary>
    /// Determine if the collection contains a name/value matching cookie.
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <remarks>
    /// NOTE: There are other important criteria in Cookies like domain, path, etc...
    /// If you want to check everything, don't use these extensions..
    /// </remarks>
    private static bool ContainsCookieWithNameAndValue(IEnumerable<Cookie> collection, string name, string value) {
        foreach (Cookie cookie in collection) {
            if (string.Compare(cookie.Name, name, StringComparison.OrdinalIgnoreCase) == 0
                && string.Compare(cookie.Value, value, StringComparison.Ordinal) == 0) {
                return true;
            }
        }
        return false;
    }

}
