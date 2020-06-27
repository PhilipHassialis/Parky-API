using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ParkyAPI
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

        public void Configure(SwaggerGenOptions options)
        {
            foreach(var desc in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(desc.GroupName, new Microsoft.OpenApi.Models.OpenApiInfo()
                {
                    Title = $"Parky API {desc.ApiVersion}",
                    Version = desc.ApiVersion.ToString()
                });
            }

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { 
                Description = "JWT authentication header using Bearer scheme.\r\n Enter 'Bearer' [space] and then your token e.g. Bearer abc12345678",
                Name="Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme="Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference= new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id="Bearer"
                        },
                        Scheme="oauth2",
                        Name="Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });

            var xmlCommentFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentFile);
            options.IncludeXmlComments(xmlCommentsFullPath);
        }
    }
}
