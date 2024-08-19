using ReadingList.Models;

namespace ReadingList.Ui.Exceptions
{
    public class ApiResponseException(ApiErrorResponse errorDetails) : Exception(errorDetails.Message)
    {
        public ApiErrorResponse ErrorDetails { get; set; } = errorDetails;
    }
}
