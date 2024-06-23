using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net.Mime;

namespace identity_provider.api
{
    public class FormDataOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.ApiDescription.HttpMethod == HttpMethod.Post.Method &&
                context.ApiDescription.RelativePath != null)
            {
                var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata
                                .OfType<IRouteDiagnosticsMetadata>()
                                .FirstOrDefault();

                if (metadata is null)
                    return;

                var (properties, requiredFields) = metadata.Route switch
                {
                     Constants.Endpoints.TOKEN => (
                        new Dictionary<string, OpenApiSchema>
                        {
                            [Constants.FormIdentifier.TOKEN_USERNAME] = new OpenApiSchema { Type = typeof(string).Name.ToLower() },
                            [Constants.FormIdentifier.TOKEN_PASSWORD] = new OpenApiSchema { Type = typeof(string).Name.ToLower() }
                        },
                        new HashSet<string>
                        {
                            Constants.FormIdentifier.TOKEN_USERNAME,
                            Constants.FormIdentifier.TOKEN_PASSWORD
                        }),
                    Constants.Endpoints.NEW_PASSWORD => (
                        new Dictionary<string, OpenApiSchema>
                        {
                            [Constants.FormIdentifier.NEW_PASSWORD_USERNAME] = new OpenApiSchema { Type = typeof(string).Name.ToLower() },
                            [Constants.FormIdentifier.NEW_PASSWORD_NEW_PASSWORD] = new OpenApiSchema { Type = typeof(string).Name.ToLower() },
                            [Constants.FormIdentifier.NEW_PASSWORD_SESSION] = new OpenApiSchema { Type = typeof(string).Name.ToLower() }
                        },
                        new HashSet<string>
                        {
                            Constants.FormIdentifier.NEW_PASSWORD_USERNAME,
                            Constants.FormIdentifier.NEW_PASSWORD_NEW_PASSWORD,
                            Constants.FormIdentifier.NEW_PASSWORD_SESSION
                        }),
                    _ => (null, null)
                };


                if (properties != null && requiredFields != null)
                {
                    operation.RequestBody = new OpenApiRequestBody
                    {
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            [MediaTypeNames.Application.FormUrlEncoded] = new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = typeof(object).Name.ToLower(),
                                    Properties = properties,
                                    Required = requiredFields
                                }
                            }
                        }
                    };
                }
            }
        }
    }
}
