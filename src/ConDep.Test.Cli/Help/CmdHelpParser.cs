using System.IO;
using NDesk.Options;

namespace ConDep.Test.Cli.Help
{
    public class CmdHelpParser : CmdBaseParser<ConDepHelpOptions>
    {
        private OptionSet _optionSet;

        public CmdHelpParser(string[] args)
            : base(args)
        {
            _optionSet = new OptionSet();
        }

        public override OptionSet OptionSet
        {
            get { return _optionSet; }
        }

        public override ConDepHelpOptions Parse()
        {
            var options = new ConDepHelpOptions();
            if (_args == null || _args.Length == 0)
            {
                return options;
            }

            var command = _args[0].Trim().ToLower();
            if (command == "bootstrap")
            {
                options.Command = ConDepCommand.Bootstrap;
            }
            else if (command == "test")
            {
                options.Command = ConDepCommand.Test;
            }
            else
            {
                throw new ConDepCmdParseException(string.Format("The command [{0}] is unknown to ConDep-Test and unable to show help for the command.", command));
            }
            return options;
        }

        public override void WriteOptionsHelp(TextWriter writer)
        {
            throw new System.NotImplementedException();
        }
    }
}