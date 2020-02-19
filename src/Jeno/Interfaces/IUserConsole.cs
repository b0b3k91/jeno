namespace Jeno.Interfaces
{
    public interface IUserConsole
    {
        string ReadInput(string parameterName, bool hideInput = false);

        void WriteLine(string text);
    }
}