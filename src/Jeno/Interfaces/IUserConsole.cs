namespace Jeno.Interfaces
{
    public interface IUserConsole
    {
        string GetInput(string parameterName, bool hideInput = false);
    }
}