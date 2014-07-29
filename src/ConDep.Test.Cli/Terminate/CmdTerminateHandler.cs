using System;
using System.IO;
using ConDep.Test.Aws.Logging;
using ConDep.Test.Aws;
using ConDep.Test.Aws.Config;

namespace ConDep.Test.Cli.Terminate
{
    public class CmdTerminateHandler : IHandleConDepCommands
    {
        private CmdTerminateParser _parser;
        private CmdTerminateValidator _validator;
        private CmdTerminateHelpWriter _helpWriter;

        public CmdTerminateHandler(string[] args)
        {
            _parser = new CmdTerminateParser(args);
            _validator = new CmdTerminateValidator();
            _helpWriter = new CmdTerminateHelpWriter(Console.Out);
        }

        public void Execute(CmdHelpWriter helpWriter)
        {
            helpWriter.PrintCopyrightMessage();

            Logger.WithLogSection("Terminate", () =>
            {
                var options = _parser.Parse();
                _validator.Validate(options);

                var config = LoadConfig(options.BootstrapId);
                var terminator = new Ec2Terminator(config.AwsProfileName);
                terminator.Terminate(options.BootstrapId, config.VpcId);
            });
        }

        private Ec2BootstrapConfig LoadConfig(string bootstrapId)
        {
            var configHandler = new BootstrapConfigHandler(bootstrapId);
            return configHandler.GetTypedEnvConfig(Path.Combine(@"C:\temp\", bootstrapId + ".json"));
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