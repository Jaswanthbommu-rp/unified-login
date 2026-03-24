using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

public interface IEmailRepositoryAsync
{
    Task<CommunicationEmail> GetEmailTemplateAsync(int communicationEventAudienceTypeId, int communicationEventPurposeTypeId, CancellationToken cancellationToken = default);
    Task<IList<CommunicationEmail>> ListEmailTemplatesAsync(CancellationToken cancellationToken = default);
}