using System.IO;
using NDesk.Options;

namespace ConDep.Test.Cli.Terminate
{
    public class CmdTerminateHelpWriter : CmdHelpWriter
    {
        public CmdTerminateHelpWriter(TextWriter writer) : base(writer)
        {
        }

        public override void WriteHelp(OptionSet optionSet)
        {
            PrintCopyrightMessage();

            var help = @"
Terminate bootstrapped servers

Usage: ConDep terminate <bootstrapId>

Where

  <bootstrapId>        The id of of a previously bootstrapped set of servers
";
            _writer.Write(help);
            optionSet.WriteOptionDescriptions(_writer);
        }
    }
}