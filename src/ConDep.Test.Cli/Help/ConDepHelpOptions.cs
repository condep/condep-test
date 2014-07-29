namespace ConDep.Test.Cli.Help
{
    public class ConDepHelpOptions
    {
        public bool NoOptions()
        {
            return Command == ConDepCommand.NotFound;
        }

        public ConDepCommand Command { get; set; }
    }
}