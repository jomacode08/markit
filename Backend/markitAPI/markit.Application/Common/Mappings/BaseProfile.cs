using AutoMapper;

namespace markit.Application.Common.Mappings
{
    /// <summary>
    /// Abstract base profile that enforces a MaxDepth limit on all type maps to mitigate
    /// AutoMapper Denial of Service via uncontrolled recursion (GHSA-rvv3-g6hj-g44x).
    /// All profiles must use <see cref="CreateBoundedMap{TSource, TDest}"/> instead of
    /// the bare <see cref="Profile.CreateMap{TSource, TDestination}()"/> method.
    /// </summary>
    public abstract class BaseProfile : Profile
    {
        /// <summary>
        /// Default recursion depth limit applied to every map created via
        /// <see cref="CreateBoundedMap{TSource, TDest}"/>.
        /// Value is set to 64 (Standard): For general mapping and serializing. 
        /// Nested hierarchical collections don't represent a risk here since their navigation properties
        /// are already explicitly ignored in the mapping.
        /// </summary>
        public const int DefaultMaxDepth = 64;

        /// <summary>
        /// Creates a type map with an explicit recursion depth limit.
        /// Prefer this over <see cref="Profile.CreateMap{TSource, TDestination}()"/> in all profiles.
        /// </summary>
        /// <param name="depth">Maximum recursion depth. Defaults to <see cref="DefaultMaxDepth"/>.</param>
        protected IMappingExpression<TSource, TDest> CreateBoundedMap<TSource, TDest>(int depth = DefaultMaxDepth)
            => CreateMap<TSource, TDest>().MaxDepth(depth);
    }
}
