using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

public interface IManageEmailAsync
{
    Task<string> SendGridEmailAsync(ISendGridEmail sendGridEmail, CancellationToken cancellationToken = default);
}
