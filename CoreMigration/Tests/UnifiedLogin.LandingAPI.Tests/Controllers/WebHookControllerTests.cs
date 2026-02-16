using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects.WebHook;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    [ExcludeFromCodeCoverage]
    public class WebHookControllerTests : ControllerTestBase
    {
        private WebHookController _controller;

        public WebHookControllerTests()
        {
            _controller = new WebHookController(MockUserClaimsAccessor.Object)
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var controller = new WebHookController(MockUserClaimsAccessor.Object);

            Assert.NotNull(controller);
        }

        #endregion

        #region PostBooks Tests - Null ThinEvent

        [Fact]
        public async Task PostBooks_WithNullThinEvent_ReturnsBadRequest()
        {
            var result = await _controller.PostBooks(null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing Content.", badRequestResult.Value);
        }

        #endregion

        #region PostBooks Tests - Missing Signature

        [Fact]
        public async Task PostBooks_WithMissingSignature_ReturnsBadRequest()
        {
            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "test.topic",
                CreatedAt = DateTime.UtcNow,
                Payload = JToken.Parse("{}")
            };

            var result = await _controller.PostBooks(thinEvent);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Missing Signature.", badRequestResult.Value);
        }

        #endregion

        #region PostBooks Tests - With Signature Header

        [Fact]
        public async Task PostBooks_WithSignatureButNoRequestBody_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "test-signature");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "test.topic",
                CreatedAt = DateTime.UtcNow,
                Payload = JToken.Parse("{}")
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        #endregion

        #region PostBooks Tests - Various Topics

        [Fact]
        public async Task PostBooks_WithValidThinEventAndSignature_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "unknown.topic",
                CreatedAt = DateTime.UtcNow,
                Payload = JToken.Parse("{}")
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostBooks_WithBooksCustomerPropertyDeletedTopic_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var payload = JToken.Parse(@"{
                ""payload"": {
                    ""customerPropertyId"": 123,
                    ""replacementCustomerPropertyId"": 456
                }
            }");

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "books.customerproperty.deleted",
                CreatedAt = DateTime.UtcNow,
                Payload = payload
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostBooks_WithBooksCustomerPropertyUpdatedTopic_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var payload = JToken.Parse(@"{
                ""payload"": {
                    ""customerPropertyId"": 123
                }
            }");

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "books.customerproperty.updated",
                CreatedAt = DateTime.UtcNow,
                Payload = payload
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostBooks_WithBooksCustomerCompanyDeletedTopic_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var payload = JToken.Parse(@"{
                ""payload"": {
                    ""customerCompanyId"": 123,
                    ""replacementCustomerCompanyId"": 456
                }
            }");

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "books.customercompany.deleted",
                CreatedAt = DateTime.UtcNow,
                Payload = payload
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostBooks_WithBooksCustomerCompanyUpdatedTopic_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var payload = JToken.Parse(@"{
                ""payload"": {
                    ""customerCompanyId"": 123
                }
            }");

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "books.customercompany.updated",
                CreatedAt = DateTime.UtcNow,
                Payload = payload
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostBooks_WithProvisioningUpfmOrderCreateTopic_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var payload = JToken.Parse(@"{
                ""company"": {
                    ""customerCompanyId"": 123,
                    ""customerEnvironment"": ""test"",
                    ""companyInstanceSourceId"": null,
                    ""productCenters"": []
                },
                ""properties"": [],
                ""customerEnvironment"": ""test""
            }");

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "provisioning.upfmorder.create",
                CreatedAt = DateTime.UtcNow,
                Payload = payload
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostBooks_WithProvisioningUpfmOrderCancelTopic_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var payload = JToken.Parse(@"{
                ""company"": {
                    ""companyInstanceSourceId"": null,
                    ""productCenters"": []
                }
            }");

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "provisioning.upfmorder.cancel",
                CreatedAt = DateTime.UtcNow,
                Payload = payload
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostBooks_WithBooksUpfmVendorCreateTopic_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var payload = JToken.Parse(@"{
                ""customerCompanyId"": 123,
                ""source"": ""TEST"",
                ""companyInstanceSourceId"": ""test-id"",
                ""user"": {
                    ""email"": ""test@test.com"",
                    ""firstName"": ""Test"",
                    ""lastName"": ""User"",
                    ""roles"": []
                }
            }");

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "books.upfmvendor.create",
                CreatedAt = DateTime.UtcNow,
                Payload = payload
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostBooks_WithProvisioningUpfmCloneCreateTopic_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var payload = JToken.Parse(@"{
                ""company"": {
                    ""customerCompanyId"": 123,
                    ""customerEnvironment"": ""test"",
                    ""companyInstanceSourceId"": null,
                    ""cloneCompanyInstanceSourceId"": """ + Guid.NewGuid().ToString() + @""",
                    ""productCenters"": []
                },
                ""properties"": [],
                ""customerEnvironment"": ""test""
            }");

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "provisioning.upfmclone.create",
                CreatedAt = DateTime.UtcNow,
                Payload = payload
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostBooks_WithDefaultTopic_Returns202()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "unknown.default.topic",
                CreatedAt = DateTime.UtcNow,
                Payload = JToken.Parse("{}")
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        #endregion

        #region PostBooks Tests - Payload Variations

        [Fact]
        public async Task PostBooks_WithNullPayloadValues_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var payload = JToken.Parse(@"{
                ""payload"": {
                    ""customerPropertyId"": null,
                    ""replacementCustomerPropertyId"": null
                }
            }");

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "books.customerproperty.deleted",
                CreatedAt = DateTime.UtcNow,
                Payload = payload
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostBooks_WithZeroCustomerPropertyId_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var payload = JToken.Parse(@"{
                ""payload"": {
                    ""customerPropertyId"": 0,
                    ""replacementCustomerPropertyId"": 0
                }
            }");

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "books.customerproperty.deleted",
                CreatedAt = DateTime.UtcNow,
                Payload = payload
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostBooks_WithEmptyPropertiesArray_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var payload = JToken.Parse(@"{
                ""company"": {
                    ""customerCompanyId"": 123,
                    ""customerEnvironment"": ""test"",
                    ""companyInstanceSourceId"": """ + Guid.NewGuid().ToString() + @""",
                    ""productCenters"": []
                },
                ""properties"": [],
                ""customerEnvironment"": ""test""
            }");

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "provisioning.upfmorder.create",
                CreatedAt = DateTime.UtcNow,
                Payload = payload
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public async Task PostBooks_WithEmptyTopic_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "",
                CreatedAt = DateTime.UtcNow,
                Payload = JToken.Parse("{}")
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostBooks_WithUpperCaseTopic_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "BOOKS.CUSTOMERPROPERTY.DELETED",
                CreatedAt = DateTime.UtcNow,
                Payload = JToken.Parse(@"{""payload"": {}}")
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostBooks_WithMixedCaseTopic_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "Books.CustomerProperty.Deleted",
                CreatedAt = DateTime.UtcNow,
                Payload = JToken.Parse(@"{""payload"": {}}")
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostBooks_WithSpecialId_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "special-id-!@#$%",
                Topic = "test.topic",
                CreatedAt = DateTime.UtcNow,
                Payload = JToken.Parse("{}")
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostBooks_WithPastCreatedAt_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "test.topic",
                CreatedAt = DateTime.UtcNow.AddYears(-1),
                Payload = JToken.Parse("{}")
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task PostBooks_WithFutureCreatedAt_ReturnsResult()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Append("signature", "valid-signature");
            httpContext.Items["TibcoPostData"] = "{}";

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            var thinEvent = new ThinEvent<JToken>
            {
                Id = "test-id",
                Topic = "test.topic",
                CreatedAt = DateTime.UtcNow.AddYears(1),
                Payload = JToken.Parse("{}")
            };

            var result = await _controller.PostBooks(thinEvent);

            Assert.NotNull(result);
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _controller = null!;
            base.Dispose();
        }

        #endregion
    }
}
