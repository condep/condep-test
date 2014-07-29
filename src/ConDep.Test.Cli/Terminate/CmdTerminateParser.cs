using System.IO;
using NDesk.Options;

namespace ConDep.Test.Cli.Terminate
{
    public class CmdTerminateParser : CmdBaseParser<ConDepTerminateOptions>
    {
        private ConDepTerminateOptions _options;
        private OptionSet _optionSet;
        private const int MIN_ARGS_REQUIRED = 1;

        public CmdTerminateParser(string[] args) : base(args)
        {
            _options = new ConDepTerminateOptions();
            _optionSet = new OptionSet
            {
            };
        }

        public override OptionSet OptionSet
        {
            get { return _optionSet; }
        }

        public override ConDepTerminateOptions Parse()
        {
            if (_args.Length < MIN_ARGS_REQUIRED)
                throw new ConDepCmdParseException(string.Format("The terminate command requires at least {0} arguments.", MIN_ARGS_REQUIRED));

            _options.BootstrapId = _args[0];

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

    public class ConDepTerminateOptions
    {
        public string BootstrapId { get; set; }
    }
}