using System;

namespace Solution.Common.Facade { 
    public class SubServiceException : Exception {
        
        public SubServiceException(string message) : base(message) {
        }
        
        public SubServiceException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}