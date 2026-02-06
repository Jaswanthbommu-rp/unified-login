# Copilot Instructions for Unified Login Main Solution

## General Guidelines

### Technology Stack
- Use C# 13.0 features where compatible with .NET Framework 4.8 projects  
- Target .NET Framework 4.8 across web and class library projects  
- Follow existing style: single namespace per file, use `var` for obvious types, async/await for I/O  
- Prefer dependency injection for controllers, logic, and repositories  
- Add XML documentation to public APIs and business logic  
- Use Serilog for structured logging with correlation IDs and contextual properties  

### Operational Services
- Authentication: Identity Server 7  
- Feature flags: LaunchDarkly  
- Messaging: Kafka  
- Observability: Kibana logs and APM (links in README)  
- Email: Unified Email or SendGrid via ProductInternalSetting toggles; signed SendGrid Event Webhook supported  

---

## Project Structure & Organization

This solution is composed of services, components, shared libraries, and tests.

### Services
- **Service/LandingAPI**
  - ASP.NET Web API controllers for organization, product, user, credentials, SAML, webhooks  
  - Swagger helpers and startup  
  - Web.config  

- **Service/LandingAPIEnterprise**
  - Enterprise-facing Web API controllers (product, property, user)  
  - Swagger helpers and startup  
  - Web.config  

- **Web/Landing**
  - MVC site for landing portal  
  - Controllers and Web.config  

### Components
- **Component/Landing**
  - Logic: ManageOrganization, ManageUser, ManageUserLogin, ManageUserRegistrationEmail, ManageUnifiedLogin, ManageUnifiedSettings  
  - Product logic: ManageProduct* per-product implementations (OneSite, AssetOptimization, MarketingCenter, ResidentPortal, RealConnect, Ops, Panel, OneSiteAccounting, Rum, Intelligent Building, etc.)  
  - Product integration: Types (Legacy, StandardV1, UPFM), helpers, models, implementations (PortfolioManagement, RenovationManager)  
  - Batch processing: BatchProcessor logic and processes  
  - Repositories: Organization, User, Property, Persona, Product, SAML, UnifiedSettings  
  - Cache: RedisCacheService  

### Shared/Foundation
- **Service/SharedObjects**
  - DTOs, Enums, Constants (StoredProcNameConstants, BlueBookProductConstants)  
  - Helpers (EmailFormatValidation), Extensions (LogActivity), Landing models, Batch and Export DTOs  

- **Foundation/DataAccess**
  - Data access base library  

- **Foundation/Activity/Service/Command**
  - Activity service logging/command library  

### Tests
- **Service/LandingAPI/LandingAPI.Test**
  - xUnit tests for controllers and logic  
  - Moq-based mocking patterns for repositories, HTTP handlers, feature flags, and product services  

---

## Domain Architecture Pattern

### Layered Architecture
- **API**: controllers, routing, Swagger, HTTP concerns  
- **Logic**: orchestration and business rules in `Component/Landing/Logic`  
- **Repository**: Dapper/SP-based data access and external HTTP integrations in `Component/Landing/Repository`  
- **Shared**: DTOs, enums, constants, helpers in `Service/SharedObjects`  

### Typical Layout
- `Service//Controllers/` — Web API and MVC endpoints  
- `Component/Landing/Logic/*` — domain orchestration and product integrations  
- `Component/Landing/Repository/*` — stored procedure calls and external API clients  
- `Service/SharedObjects/*` — contracts and utilities  

---

## Naming Conventions

### Projects
- Service.LandingAPI, Service.LandingAPIEnterprise, Web.Landing  
- Component.Landing  
- Service.SharedObjects  
- Foundation.DataAccess, Foundation.Activity.Service.Command  
- LandingAPI.Test  

### Files & Classes
- Controllers: `{Entity}Controller` (e.g., `OrganizationController`)  
- Logic: `Manage{Entity}` with interface `IManage{Entity}`  
- Repositories: `{Entity}Repository` with interface `I{Entity}Repository`  
- DTOs: `{Entity}` or `{Entity}Dto` aligned with SharedObjects  
- Product integrations: `ManageProduct{ProductName}`, `ProductIntegration/*`  

---

## Patterns & Practices

### Dependency Injection
- Constructor injection for controllers and logic  
- Controllers expose test constructors for injecting mocks  
- Logic accepts `IRepository`, `HttpMessageHandler`, `DefaultUserClaim` where applicable  

### API Development
- Consistent routes under `/api`  
- Proper HTTP status codes (OK, Created, NoContent, BadRequest, NotFound)  
- Swagger/OpenAPI attributes and examples for responses  
- Input validation (GUIDs, IDs, names, domain)  

### Data Access
- `StoredProcNameConstants` with GetOne/GetMany/Execute patterns  
- Table-valued parameters for bulk operations  
- External HTTP via typed helpers over `HttpMessageHandler`  

### Testing
- xUnit with Moq  
- AAA pattern (Arrange, Act, Assert)  
- Route verification using WebApiConfig and selector  
- Cache busting in tests via `RPObjectCache.BustCache()`  

### Configuration
- Web.config per web project  
- ProductInternalSetting toggles (BooksUseDomains, BooksUseUPFMId, SettingsApiEndPoint, IsSendGridEnabled, IsUnifiedEmailEnabled)  
- Secrets handled outside source (per central configuration practices)  

---

## Project-Specific Context

### Landing API
- Organization lifecycle: insert/update/get/list, types/domains, removal queue and cleanup  
- Property lifecycle: list/search/add/update/delete, audit to UPFM, export  
- Product endpoints: OneSite, Resident Portal, Marketing Center, Admin Support Portal, Panel, Ops, EasyLMS  
- Identity: SAML, credentials, provider type lookup  
- Webhooks: general and SendGrid Event Webhook  

### Enterprise API
- Enterprise-oriented organization/product/property endpoints  
- Swagger filters and OAuth settings  

---

## Component Logic Highlights
- **ManageUserRegistrationEmail**: Unified Email vs SendGrid, logs and CreateCommunicationEvent results  
- **ManageOrganization**: create/update org, product linkage, property translation/mapping, provisioning calls  
- **Product integrations**: user and property flows per product, operator filtering for Asset Optimization  
- **ManageUnifiedSettings**: client token and settings API calls  

---

## SharedObjects Highlights
- ProductEnum and BlueBookProductConstants mapping must stay consistent (tests verify coverage)  
- EmailFormatValidation uses `EmailAddressAttribute`; typical emails like `Temp08@myhslhome.team` are valid  
- RequestParameter for pagination and filtering  

---

## HTTP Client Configuration

### API Communication
- HttpMessageHandler-based clients for Books API, Translate API, Settings API  
- Mock handlers in tests with deterministic responses  

### Authentication
- Controllers decorated with Authorize/AuthorizeScope  
- Claims populated into DefaultUserClaim (UserId, CorrelationId, OrganizationRealPageGuid, CustomerMasterId)  

---

## Documentation Standards

### API Documentation
- Swagger annotations: `SwaggerResponse`, `SwaggerOperation`, examples providers  
- Document authentication, request/response models, and error cases  

### Code Documentation
- XML docs for public controller and logic methods  
- Repository README links for Kibana/APM and tooling  

---

## Setup Classes & Patterns

### Service Initialization
- BaseApiController Initialize populates claims and infra  
- Feature flags via `ILdClient` injected in controllers where needed  

### Mock Setup Patterns
- Verify repository SP parameters via `It.Is<object>(predicate)`  
- SetupSequence for ordered mock responses  
- Reflection used in tests to inject private fields where needed  

---

## Common Commands Context
- Landing API — `Service/LandingAPI/*`  
- Enterprise API — `Service/LandingAPIEnterprise/*`  
- Landing component — `Component/Landing/*`  
- Shared objects — `Service/SharedObjects/*`  
- Data access — `Foundation/DataAccess/*`  
- Activity command — `Foundation/Activity/Service/Command/*`  
- Landing tests — `Service/LandingAPI/LandingAPI.Test/*`  

---

## Miscellaneous

### Performance Considerations
- Use async/await for external HTTP  
- Paginate large datasets via RequestParameter  
- In tests, bust caches when asserting cached SP calls  

### Security
- Validate inputs and guard against default/empty GUIDs  
- Apply AuthorizeScope to sensitive operations (provisioning, exports)  
- Configure CORS using AllowCors attributes  

### Logging
- Serilog with contextual properties:
  - CorrelationId  
  - ProductModule (type names)  
  - AdditionalInfo payloads  
- Communication events persisted via `IManageCommunicationEvents`  

### Email Delivery Observability
- SendGrid Event Webhook endpoint: `/api/sendgrid/events`  
- Verify signatures using `X-Twilio-Email-Event-Webhook-Signature` and Timestamp headers with configured public key  
- Log and persist event details: event, sg_message_id, reason, status, category, transId  
- Use SendGrid dashboard for suppressions/bounces/drops when sends return “success” but delivery fails  
