using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

public sealed class ManageEmailAsync : IManageEmailAsync
{
    private readonly IProductInternalSettingRepositoryAsync _productInternalSettingRepository;

    public ManageEmailAsync(IProductInternalSettingRepositoryAsync productInternalSettingRepository)
    {
        _productInternalSettingRepository = productInternalSettingRepository ?? throw new ArgumentNullException(nameof(productInternalSettingRepository));
    }

    public async Task<string> SendGridEmailAsync(ISendGridEmail sendGridEmail, CancellationToken cancellationToken = default)
    {
        if (sendGridEmail == null)
            throw new ArgumentNullException(nameof(sendGridEmail), "Null email object.");

        try
        {
            var productSettingList = await _productInternalSettingRepository
                .GetProductInternalSettingsAsync((int)ProductEnum.UnifiedPlatform, cancellationToken);

            if (productSettingList == null || productSettingList.Count == 0)
                return "Invalid product settings for Unified Platform.";

            var isSendGridEnabled = productSettingList
                .FirstOrDefault(s => s.Name.Equals("IsSendGridEnabled", StringComparison.OrdinalIgnoreCase))
                ?.Value?.Equals("1") ?? false;

            if (!isSendGridEnabled)
                return "SendGrid emails is disabled.";

            var apiEndPoint = productSettingList
                .FirstOrDefault(s => s.Name.Equals("SendGridApiEndPoint", StringComparison.OrdinalIgnoreCase))?.Value;
            var sendEmailEndPoint = productSettingList
                .FirstOrDefault(s => s.Name.Equals("SendGridSendEmailEndPoint", StringComparison.OrdinalIgnoreCase))?.Value;
            var sendGridUrl = string.Concat(apiEndPoint, sendEmailEndPoint);

            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, sendGridUrl)
            {
                Content = new StringContent(JsonConvert.SerializeObject(sendGridEmail), Encoding.UTF8, "application/json")
            };

            var response = await httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode
                ? "Email sent successfully."
                : "An error occured when sending the email.";
        }
        catch (Exception)
        {
            return "An error occured when sending the email.";
        }
    }
}
