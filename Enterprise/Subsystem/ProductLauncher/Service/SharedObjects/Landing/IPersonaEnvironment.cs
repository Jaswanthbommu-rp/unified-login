using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    /// <summary>
    /// Persona Environment Type Object
    /// </summary>
    public interface IPersonaEnvironment
    {
        /// <summary>
        /// Persona Environment Type Unique Id
        /// </summary>
        long PersonaEnvironmentTypeId { get; set; }

        /// <summary>
        /// Persona Environment Type Name
        /// </summary>
        string Name { get; set; }

        
    }
}
