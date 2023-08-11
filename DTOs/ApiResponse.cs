public class ApiResponse
{
    public string Status { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }
}

public class SuccessResponse : ApiResponse
{
    public SuccessResponse(string message, object data = null)
    {
        Status = "success";
        Message = message;
        Data = data;
    }
}

public class ErrorResponse : ApiResponse
{
    public string Code { get; set; }

    public ErrorResponse(string code, string message)
    {
        Status = "error";
        Code = code;
        Message = message;
    }
}
