namespace ConDep.Test.Cli.Reset
{
    public class CmdResetHandler : IHandleConDepCommands
    {
        /// <summary>
        /// 1) Stop instance
        /// 2) Detach Volumes
        /// 3) Create new Volumes from snapshots
        /// 4) Delete old Volumes
        /// 5) Attach new volumes with correct device mappings
        /// 6) Start instance
        /// 7) Wait for instance to respond
        /// </summary>
        /// <param name="helpWriter"></param>
        public void Execute(CmdHelpWriter helpWriter)
        {
            
        }

        public void WriteHelp()
        {
            throw new System.NotImplementedException();
        }

        public void Cancel()
        {
            throw new System.NotImplementedException();
        }
    }
}