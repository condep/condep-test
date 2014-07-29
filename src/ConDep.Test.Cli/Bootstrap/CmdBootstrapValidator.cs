using System.IO;

namespace ConDep.Test.Cli.Bootstrap
{
    public class CmdBootstrapValidator : CmdBaseValidator<ConDepBootstrapOptions>
    {
        public override void Validate(ConDepBootstrapOptions options)
        {
            if(!options.VpcId.ToLower().StartsWith("vpc-"))
                throw new ConDepCmdParseException("VPC id not valid. VPC id's starts with 'vpc-'");

            if(!File.Exists(options.RsaPrivateKeyPath))
                throw new ConDepCmdParseException(string.Format("Private RSA key file ({0}) does not exist", options.RsaPrivateKeyPath));

            if (options.SecurityGroup != null)
            {
                if(!options.SecurityGroup.ToLower().StartsWith("sg-"))
                    throw new ConDepCmdParseException("Security Group id not valid. Security Group id's starts with 'sg-'");
            }
        }
    }
}