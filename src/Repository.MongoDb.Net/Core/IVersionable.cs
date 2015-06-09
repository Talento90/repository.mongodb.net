using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core
{

    /// <summary>
    /// Versionable Entity (For Entity Consistency)
    /// </summary>
    public interface IVersionable
    {
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        long Version { get; set; }

        /// <summary>
        /// Ignores the version.
        /// </summary>
        /// <returns></returns>
        bool IgnoreVersion();
    }
}
