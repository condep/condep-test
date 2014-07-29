namespace ConDep.Test.Cli
{
    public abstract class CmdBaseValidator<T>
    {
        public abstract void Validate(T options);
    }
}