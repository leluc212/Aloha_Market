using Aloha.EventBus;
using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Kafka;
using Aloha.EventBus.Models;
using Aloha.ServiceDefaults.Storage;
using Aloha.ServiceDefaults.DependencyInjection;
using Aloha.ServiceDefaults.Hosting;
using Aloha.ServiceDefaults.Middlewares;
using Aloha.UserService.Data;
using Aloha.UserService.EventHandler;
using Aloha.UserService.Repositories;
using Aloha.UserService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace Aloha.UserService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        // Add CORS configuration
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
            });
        });

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.MapType<IFormFile>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "binary"
            });

            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Aloha User Service API",
                Version = "v1"
            });

            c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter your token:"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    new List<string>()
                }
            });
        });

        builder.Services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
        });

        // Read Kafka connection string and override bootstrap if needed
        var kafkaConnectionString = builder.Configuration.GetConnectionString("Kafka");
        if (!string.IsNullOrEmpty(kafkaConnectionString))
        {
            builder.Configuration["Kafka:BootstrapServers"] = kafkaConnectionString;
        }
        else
        {
            throw new InvalidOperationException("Kafka connection string is missing from configuration.");
        }

        // Register Kafka producer
        builder.AddKafkaProducer("Kafka");

        // Register Kafka event publisher
        var kafkaPublishTopic = builder.Configuration["Kafka:Topics:UserEvents:Publish"];
        if (!string.IsNullOrWhiteSpace(kafkaPublishTopic))
        {
            builder.AddKafkaEventPublisher(kafkaPublishTopic);
        }
        else
        {
            builder.Services.AddTransient<IEventPublisher, NullEventPublisher>();
        }

        var kafkaConsumeTopic = builder.Configuration["Kafka:Topics:UserEvents:Consume"];
        if (!string.IsNullOrWhiteSpace(kafkaConsumeTopic))
        {
            builder.AddKafkaEventConsumer(options =>
            {
                options.ServiceName = "UserService";
                options.KafkaGroupId = "aloha-user-service";
                options.Topics.AddRange(kafkaConsumeTopic.Split(','));
                options.IntegrationEventFactory = IntegrationEventFactory<TestSendEventModel>.Instance;
                options.AcceptEvent = e => e.IsEvent<TestSendEventModel>();  // Change to TestSendEventModel
            });
        }

        builder.Services.AddSharedServices<UserDbContext>(builder.Configuration);
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUserService, UserService.Services.UserService>();
        // Luu tru anh tren S3 + CloudFront (doc cau hinh tu section "S3", credential lay tu IAM role EC2).
        builder.Services.AddS3Storage(builder.Configuration);
        builder.Services.AddAutoMapper(typeof(Program).Assembly);

        // Replace the JWT auth configuration with the simplified extension
        builder.Services.AddKeycloakJwtAuthentication(builder.Configuration);

        var app = builder.Build();

        app.MapDefaultEndpoints();
        app.MigrateDatabase<UserDbContext>();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aloha User Service API V1");
                c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
            });
        }

        // Add CORS middleware - place it before other middleware
        app.UseCors("AllowAll");

        app.UseMiddleware<ApiExceptionHandlerMiddleware>();

        // Make sure Authentication is before Authorization
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
