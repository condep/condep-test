using System;
using System.Diagnostics;
using System.Text;
using ConDep.Test.Aws.Logging;

namespace ConDep.Test.Cli
{
    /// <summary>
    /// Boostrap AWS images to use for integration testing.
    /// 
    /// Following should be supported:
    ///     * Creating x number of images given an AMI image id
    ///     * Creating x number of images given 2012R2, 2012, 2008R2 or 
    ///       a AMI name
    ///     * Optionally take a snapshot of the configured image, so it
    ///       can be utilized multiple times without bootstrapping
    ///     * 
    /// When images exist:
    ///     * 
    /// 
    /// 
    /// condep-int bootstrap init <awsProfileName> <vpcId> <rsaPrivateKey> /numOfInstances /instanceType /securityGroup /preventSnapshots
    /// condep-int bootstrap reset <bootstrapId>
    /// condep-int bootstrap terminate <bootstrapId>
    /// condep-int test <assembly> <bootstrapId>
    /// </summary>
    class Program
    {
        private static IHandleConDepCommands _handler;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.GetEncoding(1252);
            Console.CancelKeyPress += Console_CancelKeyPress;
            var exitCode = 0;

            try
            {
                ConfigureLogger();
                ExecuteCommand(args);
            }
            catch (Exception ex)
            {
                exitCode = 1;
                Console.WriteLine(ex);
            }
            Environment.ExitCode = exitCode;
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("I'm exiting now because you forced my hand!");
        }

        private static void ExecuteCommand(string[] args)
        {
            var helpWriter = new CmdHelpWriter(Console.Out);

            try
            {
                _handler = CmdFactory.Resolve(args);
                _handler.Execute(helpWriter);
            }
            catch (AggregateException aggEx)
            {
                foreach (var ex in aggEx.InnerExceptions)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    helpWriter.WriteException(ex);
                    Console.ResetColor();
                    Console.WriteLine("For help type ConDep Help <command>");
                }
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                helpWriter.WriteException(ex);
                Console.ResetColor();
                Console.WriteLine("For help type ConDep Help <command>");
                Environment.Exit(1);
            }
        }

        private static void ConfigureLogger()
        {
            new LogConfigLoader().Load();
            new Logger().AutoResolveLogger();
            Logger.TraceLevel = TraceLevel.Info;
        }

    }
}
