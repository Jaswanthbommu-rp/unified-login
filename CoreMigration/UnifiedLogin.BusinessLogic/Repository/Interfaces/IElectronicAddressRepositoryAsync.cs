using System;
using System.Collections.Generic;
using System.Threading;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

/// <summary>
/// Async interface for ElectronicAddressRepository.
/// </summary>
public interface IElectronicAddressRepositoryAsync
{
    /// <summary>Links an electronic address to a contact mechanism.</summary>
    Task<RepositoryResponse> CreateElectronicAddressAsync(IElectronicAddress electronicAddress, CancellationToken cancellationToken = default);

    /// <summary>Lists electronic addresses for a person by RealPage GUID.</summary>
    Task<IList<ElectronicAddress>> ListElectronicAddressForPersonAsync(Guid realPageId, string contactMechanismUsageTypeName = "", CancellationToken cancellationToken = default);

    /// <summary>Lists electronic addresses for a person by login name and organisation.</summary>
    Task<IList<ElectronicAddress>> ListElectronicAddressForPersonAsync(string loginName, long orgPartyId, string contactMechanismUsageTypeName = "", CancellationToken cancellationToken = default);
}