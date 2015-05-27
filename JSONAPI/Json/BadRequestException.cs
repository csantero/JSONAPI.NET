using System;

namespace JSONAPI.Json
{
    /// <summary>
    /// This exception should be thrown from deserialization code in order to
    /// indicate that the request payload was formatted improperly.
    /// </summary>
    public class BadRequestException : Exception
    {
        /// <summary>
        /// Creates a new BadRequestException
        /// </summary>
        /// <param name="message">The message to be returned to the client</param>
        public BadRequestException(string message)
            : base(message)
        {
                
        }
    }
}