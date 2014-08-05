using System;
using ConDep.Test.Aws.Logging;
using ConDep.Test.Aws;

namespace ConDep.Test.Cli.Bootstrap
{
    public class CmdBootstrapHandler : IHandleConDepCommands
    {
        private readonly CmdBootstrapParser _parser;
        private CmdBootstrapValidator _validator;
        private CmdBootstrapHelpWriter _helpWriter;

        public CmdBootstrapHandler(string[] args)
        {
            _parser = new CmdBootstrapParser(args);
            _validator = new CmdBootstrapValidator();
            _helpWriter = new CmdBootstrapHelpWriter(Console.Out);
        }

        public void Execute(CmdHelpWriter helpWriter)
        {
            helpWriter.PrintCopyrightMessage();

            Logger.WithLogSection("Bootstrap", () =>
            {
                var options = _parser.Parse();
                _validator.Validate(options);

                string ami;
                if (string.IsNullOrWhiteSpace(options.AmiId))
                {
                    var amiLocator = new Ec2AmiLocator(options.AwsProfileName);
                    ami = amiLocator.Find2012R2Core();
                }
                else
                {
                    ami = options.AmiId;
                }

                var bootstrapper = new Ec2Bootstrapper(options.AwsProfileName);
                var bootstrapId = bootstrapper.Boostrap(options.VpcId, ami, options.NumOfInstances == 0 ? 1 : options.NumOfInstances, options.RsaPrivateKeyPath);

                Logger.WithLogSection("Bootstrap finished", () =>
                {
                    Logger.Info("Bootstrap settings stored in: {0}", @"C:\temp\" + bootstrapId + ".json");
                    Logger.Info("To execute tests using the new server(s) do :");
                    Logger.Info("\tConDepTest test <your assembly> " + bootstrapId);
                    Logger.Info("To reset server(s) back into initial state do :");
                    Logger.Info("\tConDepTest reset " + bootstrapId);
                    Logger.Info("To terminate server(s) do :");
                    Logger.Info("\tConDepTest terminate " + bootstrapId);
                });
            });
        }

        public void WriteHelp()
        {
            _helpWriter.WriteHelp(_parser.OptionSet);
        }

        public void Cancel()
        {
        }
    }
}