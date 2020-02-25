using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using Jeno.Core;
using System.Linq;

namespace Jeno.Validators
{
    class ConfigurationValidator : AbstractValidator<JenoConfiguration>
    {
        private const string DefaultJobKey = "default"; 
        public ConfigurationValidator()
        {
            var uriMessage = new StringBuilder()
                .AppendLine(Messages.IncorrectJenkinsAddress)
                .AppendLine(Messages.ConfigureJenkinsAddressTip)
                .ToString();

            var repoMessage = new StringBuilder()
                .AppendLine(Messages.MissingDefaultJob)
                .AppendLine(Messages.ConfigureDefaultJobTip)
                .ToString();

            var userNameMessage = new StringBuilder()
                .AppendLine(Messages.UndefinedUserName)
                .AppendLine(Messages.ConfigureUserNameTip)
                .ToString();

            var tokenMessage = new StringBuilder()
                .AppendLine(Messages.UndefinedToken)
                .AppendLine(Messages.ConfigureTokenTip)
                .ToString();

            RuleFor(c => c.JenkinsUrl).Must(u => Uri.IsWellFormedUriString(u, UriKind.Absolute)).WithMessage(uriMessage);
            RuleFor(c => c.Repository).Must(r => r.Keys.Contains(DefaultJobKey)).WithMessage(repoMessage);
            RuleFor(c => c.UserName).NotEmpty().WithMessage(userNameMessage);
            RuleFor(c => c.Token).NotEmpty().WithMessage(tokenMessage);
        }
    }
}
