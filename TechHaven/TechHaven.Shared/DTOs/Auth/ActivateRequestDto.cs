using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechHaven.Shared.DTOs.Auth
{
    public class ActivateRequestDto
    {
        /// <summary>
        /// Indicates the activation key for the user account.
        /// </summary>
        public string Key { get; set; } = string.Empty;
    }
}
