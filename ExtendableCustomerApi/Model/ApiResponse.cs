namespace ExtendableCustomerApi.Model
{
    public class ApiResponse
    {
        public ApiResponse() { }
        public ApiResponse(object result, string errorMessage)
        {
            Result = result;
            ErrorMessage = errorMessage;
        }

        public ApiResponse(object result, string errorMessage, int count)
        {
            Result = result;
            ErrorMessage = errorMessage;
            Count = count;
        }

        public object Result { get; set; }

        public int Count { get; set; }
        public string ErrorMessage { get; set; }
    }
}


