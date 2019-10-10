namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
    /// <summary>
    /// Used to define what the object will look like when it contains data for Swagger
    /// </summary>
    public interface IProvideExamples
    {
        /// <summary>
        /// Used to get examples for swagger
        /// </summary>
        /// <returns></returns>
        object GetExamples();
    }
}
