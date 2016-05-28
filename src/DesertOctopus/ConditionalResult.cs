#pragma warning disable SA1402 // File may only contain a single class


namespace DesertOctopus
{
    /// <summary>
    /// Helper class to create typed ConditionalResult
    /// </summary>
    public static class ConditionalResult
    {
        /// <summary>
        /// Creates a successful ConditionalResult
        /// </summary>
        /// <typeparam name="T">Any type</typeparam>
        /// <param name="value">Successful value</param>
        /// <returns>A successful ConditionalResult</returns>
        public static ConditionalResult<T> CreateSuccessful<T>(T value)
        {
            return new ConditionalResult<T>(value);
        }

        /// <summary>
        /// Creates a failed ConditionalResult
        /// </summary>
        /// <typeparam name="T">Any type</typeparam>
        /// <returns>A failed ConditionalResult</returns>
        public static ConditionalResult<T> CreateFailure<T>()
        {
            return new ConditionalResult<T>();
        }
    }

    /// <summary>
    /// Represents a conditional result
    /// </summary>
    /// <typeparam name="T">Any types</typeparam>
    public class ConditionalResult<T>
    {
        /// <summary>
        /// Gets the value of the conditional result
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Gets a value indicating whether or not the condition is successful
        /// </summary>
        public bool IsSuccessful { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalResult{T}"/> class with IsSuccessful = true
        /// </summary>
        /// <param name="value">Successful value</param>
        public ConditionalResult(T value)
        {
            Value = value;
            IsSuccessful = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalResult{T}"/> class.
        /// </summary>
        public ConditionalResult()
        {
            IsSuccessful = false;
        }
    }
}

#pragma warning restore SA1402 // File may only contain a single class
