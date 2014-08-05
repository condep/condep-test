using System;
using System.IO;
using NDesk.Options;

namespace ConDep.Test.Cli.Bootstrap
{
    public class CmdBootstrapParser : CmdBaseParser<ConDepBootstrapOptions>
    {
        private const int MIN_ARGS_REQUIRED = 3;
        private readonly OptionSet _optionSet;
        private ConDepBootstrapOptions _options;

        public CmdBootstrapParser(string[] args) : base(args)
        {
            _options = new ConDepBootstrapOptions();
            _optionSet = new OptionSet
            {
                {"n=|numOfInstances", "Number of instances.", v => _options.NumOfInstances = Convert.ToInt32(v)},
                {"a=|amiId", "Id for the AMI to use", v => _options.AmiId = v},
                {"i=|instanceType", "Type of instance (default t2.micro)", v => _options.InstanceType = v},
                {"s=|securityGroup", "The security group to use (by default a new security group will be created)", v => _options.SecurityGroup = v}
            };
        }

        public override OptionSet OptionSet
        {
            get { return _optionSet; }
        }

        public override ConDepBootstrapOptions Parse()
        {
            if (_args.Length < MIN_ARGS_REQUIRED)
                throw new ConDepCmdParseException(string.Format("The bootstrap init command requires at least {0} arguments.", MIN_ARGS_REQUIRED));

            _options.AwsProfileName = _args[0];
            _options.VpcId = _args[1];
            _options.RsaPrivateKeyPath = _args[2];

            try
            {
                OptionSet.Parse(_args);
            }
            catch (OptionException oe)
            {
                throw new ConDepCmdParseException("Unable to successfully parse arguments.", oe);
            }
            return _options;
        }

        public override void WriteOptionsHelp(TextWriter writer)
        {
            OptionSet.WriteOptionDescriptions(writer);
        }
    }
}