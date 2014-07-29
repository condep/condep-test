using System.IO;
using NDesk.Options;

namespace ConDep.Test.Cli.Bootstrap
{
    public class CmdBootstrapHelpWriter : CmdHelpWriter
    {
        public CmdBootstrapHelpWriter(TextWriter writer) : base(writer)
        {
        }

        public override void WriteHelp(OptionSet optionSet)
        {
            PrintCopyrightMessage();

            var help = @"
Bootstrap servers for use with integration testing

Usage: ConDep bootstrap init <awsProfileName> <vpcId> <rsaPrivateKey> [options]

Where

  <awsProfileName>        The Amazon AWS profile to use

  <vpcId>                 The Virtual Private Cloud id for you VPC

  <rsaPrivateKeyPath>     The path to the private key (pem file). Used to
                          decrypt the administrator password for the server.
";
            _writer.Write(help);
            optionSet.WriteOptionDescriptions(_writer);
        }

    }
}