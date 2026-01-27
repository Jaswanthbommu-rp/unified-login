namespace UnifiedLogin.BatchProcessor.Models;

public class ProductInternalSetting
{
    public string ProductConfigurationId { get; set; } = string.Empty;
    public string ConfigurationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool SensitiveData { get; set; }
}
