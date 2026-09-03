namespace Clinic_System.API.Extensions
{
    public static class MessageBrokerServiceExtensions
    {
        public static IServiceCollection AddMessageBrokerServices(this IServiceCollection services, IConfiguration configuration)
        {
            var rabbitMqUrl = configuration["RabbitMQ:Url"];

            if (string.IsNullOrEmpty(rabbitMqUrl))
            {
                services.AddScoped<IMessagePublisher, NullMessagePublisher>();
                return services;
            }

            services.AddScoped<IMessagePublisher, MessagePublisher>();

            services.AddMassTransit(x =>
            {
                x.AddConsumer<AppointmentBookedEventConsumer>();
                x.AddConsumer<AppointmentCancelledEventConsumer>();
                x.AddConsumer<AppointmentRescheduledEventConsumer>();
                x.AddConsumer<AppointmentNoShowEventConsumer>();
                x.AddConsumer<AppointmentConfirmedEventConsumer>();
                x.AddConsumer<MedicalReportGeneratedEventConsumer>();
                x.AddConsumer<AppointmentAutoCancelledEventConsumer>();
                x.AddConsumer<UserRegisteredEventConsumer>();
                x.AddConsumer<PasswordResetRequestedEventConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri(rabbitMqUrl));

                    cfg.UseMessageRetry(r =>
                    {
                        r.Handle<HttpRequestException>();
                        r.Handle<TimeoutException>();
                        r.Handle<TaskCanceledException>();
                        r.Ignore<ArgumentNullException>();
                        r.Ignore<InvalidOperationException>();
                        r.Incremental(3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}
