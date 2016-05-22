using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IO;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Represents the memory stream management
    /// </summary>
    internal static class MemoryPool
    {
        private static RecyclableMemoryStreamManager _manager;

        static MemoryPool()
        {
            _manager = new RecyclableMemoryStreamManager();
        }

        /// <summary>
        /// Get a new memory stream
        /// </summary>
        /// <returns>A new memory stream</returns>
        public static MemoryStream GetStream()
        {
            return _manager.GetStream();
        }
    }
}
