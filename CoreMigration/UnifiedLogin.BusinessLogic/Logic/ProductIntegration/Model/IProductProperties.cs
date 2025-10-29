namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model
{
    public interface IProductProperties
    {
        string GetPropertyId { get; }

        string SetPropertyId { set; }

        string GetName { get; }

        string SetName { set; }

        bool IsAssigned { get; set; }

        string GroupId { get; set; }

        string PropertyType { get; set; }

        string State { get; set; }
    }
}
