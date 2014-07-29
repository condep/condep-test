using System;
using System.Globalization;
using System.Linq;
using ConDep.Test.Cli.Help;

namespace ConDep.Test.Cli
{
    public class CmdFactory
    {
        private string[] _args;

        public CmdFactory(string[] args)
        {
            _args = args;
        }

        public static IHandleConDepCommands Resolve(string[] args)
        {
            return new CmdFactory(args).Resolve();
        }

        private string CmdName
        {
            get
            {
                if (_args == null || _args.Length == 0)
                    throw new ConDepCmdParseException("No arguments in argument list.");

                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_args[0]);
            }
        }

        private string[] Args
        {
            get
            {
                if (_args == null || _args.Length < 2)
                {
                    return new string[] { };
                }
                return _args.Skip(1).ToArray();
            }
        }
        public IHandleConDepCommands Resolve()
        {
            try
            {
                if (_args.Length == 0)
                {
                    return new CmdHelpHandler(Args);
                }
                var cmdName = CmdName.ToLower();
                if (cmdName == "/?" || cmdName == "-?" || cmdName == "--help" || cmdName == "-help" || cmdName == "-h" ||
                    cmdName == "--h")
                    return new CmdHelpHandler(Args);

                var conventionType = GetType().Assembly.GetTypes().Single(type => type.Name == "Cmd" + CmdName + "Handler");
                return (IHandleConDepCommands)conventionType.GetConstructors().First().Invoke(new object[] { Args });
            }
            catch (Exception ex)
            {
                throw new ConDepCmdParseException(string.Format("The command [{0}] is not known to ConDep.", CmdName), ex);
            }
        }
    }
}