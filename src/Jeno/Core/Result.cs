namespace Jeno.Core
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }

        private Result(bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static Result Ok()
        {
            return new Result(true, string.Empty);
        }

        public static Result Invalid(string message)
        {
            return new Result(false, message);
        }
    }
}