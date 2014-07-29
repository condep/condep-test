namespace ConDep.Test.Cli
{
    public interface IHandleConDepCommands {
        void Execute(CmdHelpWriter helpWriter);
        //TParser Parser { get; }
        //TValidator Validator { get; }
        void WriteHelp();
        void Cancel();
    }
}