﻿namespace Core5.Injection
{
    using System.Net.Mail;
    using System.Threading.Tasks;
    using NServiceBus;

    class Injection
    {
        void ConfigurePropertyInjectionForHandler(EndpointConfiguration endpointConfiguration)
        {
            #region ConfigurePropertyInjectionForHandler

            endpointConfiguration.RegisterComponents(
                registration: components =>
                {
                    components.ConfigureComponent(
                        builder =>
                        {
                            return new EmailHandler
                            {
                                SmtpPort = 25,
                                SmtpAddress = "10.0.1.233"
                            };
                        },
                        dependencyLifecycle: DependencyLifecycle.InstancePerUnitOfWork);
                });

            #endregion
        }

        #region PropertyInjectionWithHandler

        public class EmailHandler :
            IHandleMessages<EmailMessage>
        {
            public string SmtpAddress { get; set; }
            public int SmtpPort { get; set; }

            public Task Handle(EmailMessage message, IMessageHandlerContext context)
            {
                var client = new SmtpClient(SmtpAddress, SmtpPort);
                // ...
                return Task.FromResult(0);
            }
        }

        #endregion
    }

    public class EmailMessage
    {
    }
}