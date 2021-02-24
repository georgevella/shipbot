using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using SemVersion;

namespace Shipbot.Applications.Models
{
    using ComparerFunc = Func<SemanticVersion, SemanticVersion, bool>;
    
    public class SemverImageUpdatePolicy : ImageUpdatePolicy
    {
        delegate bool EqualityFunc(SemanticVersion target);
        delegate bool StandardEqualityTemplate(SemanticVersion x, SemanticVersion y);
        //
        private static readonly Expression<StandardEqualityTemplate> Match = (x, y) => x == y;
        private static readonly Expression<StandardEqualityTemplate> LessThen = (x, y) => x < y;
        private static readonly Expression<StandardEqualityTemplate> LessThenOrEqual = (x, y) => x <= y;
        private static readonly Expression<StandardEqualityTemplate> GreaterThan = (x, y) => x > y;
        private static readonly Expression<StandardEqualityTemplate> GreaterThanOrEqual = (x, y) => x >= y;
        
        // private readonly SemanticVersion _baseSemanticVersion;
        private readonly EqualityFunc _equalityFunc;

        public SemverImageUpdatePolicy(string semver)
        {
            var first = semver[0];
            var second = semver[1];

            _equalityFunc = first switch
            {
                '<' => second == '=' 
                    ? BuildStandardEqualityFunc(semver.Substring(2), LessThenOrEqual) 
                    : BuildStandardEqualityFunc(semver.Substring(1), LessThen),
                '>' => second == '=' 
                    ? BuildStandardEqualityFunc(semver.Substring(2), GreaterThanOrEqual) 
                    : BuildStandardEqualityFunc(semver.Substring(1), GreaterThan),
                '=' => BuildStandardEqualityFunc(semver.Substring(1), Match),
                '~' => BuildTildeEqualityFunc(semver.Substring(1)),
                '^' => BuildCaretEqualityFunc(semver.Substring(1)),
                _ => char.IsDigit(first) 
                    ? BuildStandardEqualityFunc(semver, Match) 
                    : throw new InvalidOperationException()
            };
            
            //
            // var actualVersionBuffer = new StringBuilder(
            //     first switch
            //     {
            //         '<' => second == '=' ? semver.Substring(2) : semver.Substring(1),
            //         '>' => second == '=' ? semver.Substring(2) : semver.Substring(1),
            //         '=' => semver.Substring(1),
            //         _ => semver
            //     }
            // );
            //
            // if (!SemanticVersion.TryParse(actualVersionBuffer.ToString(), out _baseSemanticVersion))
            // {
            //     throw new InvalidOperationException("Semantic version is not valid.");
            // }
        }

        private EqualityFunc BuildStandardEqualityFunc(string semverString,
            Expression<StandardEqualityTemplate> expression)
        {
            var semver = GetSourceSemVer(semverString);

            return expression.Body.NodeType switch
            {
                ExpressionType.LessThan => (target => target < semver),
                ExpressionType.LessThanOrEqual => (target => target <= semver),
                ExpressionType.GreaterThan => (target => target > semver),
                ExpressionType.GreaterThanOrEqual => (target => target >= semver),
                ExpressionType.Equal => (target => target.Equals(semver)),
                _ => throw new NotSupportedException()
            };
        }

        private static SemanticVersion GetSourceSemVer(string semverString)
        {
            // validation
            if (!SemanticVersion.TryParse(semverString, out var semver))
            {
                throw new InvalidOperationException();
            }
            return semver;
        }

        private EqualityFunc BuildCaretEqualityFunc(string semverString)
        {
            var semver = GetSourceSemVer(semverString);

            // TODO: think about this logic down here: when we include a pre-relase tag, do we need to have conditions
            // or just include any release with a tag ?
            if (semver.Major > 0)
            {
                // ^1.2.3 := >=1.2.3 <2.0.0-0
                if (string.IsNullOrEmpty(semver.Prerelease))
                {
                    return (version) => version.Major == semver.Major &&
                                     string.IsNullOrEmpty(version.Prerelease) &&
                                     version >= semver;
                }
                else
                {
                    // if caret version specifies a pre-release filter, we should accept
                    // prereleases only from matching minor versions.
                    return (version) => version.Major == semver.Major &&
                                        (version.Minor == semver.Minor || (version.Minor > semver.Minor && string.IsNullOrEmpty(version.Prerelease))) &&
                                        version >= semver;
                }
            }
            else if (semver.Major == 0 && semver.Minor > 0)
            {
                // ^0.2.3 := >=0.2.3 <0.3.0-0
                if (string.IsNullOrEmpty(semver.Prerelease))
                {
                    return (version) => version.Major == 0 &&
                                        version.Minor == semver.Minor &&
                                        string.IsNullOrEmpty(version.Prerelease) &&
                                        version >= semver;
                }
                else
                {
                    return (version) => version.Major == 0 &&
                                        (version.Minor == semver.Minor || (version.Minor > semver.Minor && string.IsNullOrEmpty(version.Prerelease))) &&
                                        version >= semver;
                }
            }
            else if (semver.Major == semver.Minor && semver.Major == 0)
            {
                return (version) => version.Major == 0 && 
                                    version.Minor == 0 && 
                                    version.Patch == semver.Patch &&  
                                    version >= semver;
            }

            throw new InvalidOperationException();
        }

        private EqualityFunc BuildTildeEqualityFunc(string semver)
        {
            throw new NotImplementedException();
        }

        public override bool IsMatch(string value)
        {
            return SemanticVersion.TryParse(value, out var targetSemver) 
                   && _equalityFunc(targetSemver);
        }
    }
}