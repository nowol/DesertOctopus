using System;
using System.Linq;
using System.Linq.Expressions;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class to hold loop body information
    /// </summary>
    internal class EnumerableLoopBodyCargo
    {
        /// <summary>
        /// Gets or sets the expression representing the item as an object
        /// </summary>
        public ParameterExpression ItemAsObj { get; set; }

        /// <summary>
        /// Gets or sets the expression representing the enumerator
        /// </summary>
        public ParameterExpression Enumerator { get; set; }

        /// <summary>
        /// Gets or sets the type of enumerator
        /// </summary>
        public Type EnumeratorType { get; set; }

        /// <summary>
        /// Gets or sets the expression representing the type of object
        /// </summary>
        public ParameterExpression TypeExpr { get; set; }

        /// <summary>
        /// Gets or sets the expression representing the temporary serializer variable
        /// </summary>
        public ParameterExpression Serializer { get; set; }

        /// <summary>
        /// Gets or sets the type of the value
        /// </summary>
        public Type KvpType { get; set; }
    }
}
