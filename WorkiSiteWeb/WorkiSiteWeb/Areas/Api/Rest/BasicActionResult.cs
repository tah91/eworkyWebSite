namespace Worki.Rest
{
    public class BasicActionResult : ObjectResult<BasicActionResult.Result>
    {
        public class Result
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string Data { get; set; }
        }

        public BasicActionResult()
            : base(new Result() { Success = true })
        {            
        }

        public BasicActionResult(bool success)
            : base(new Result() { Success = success })
        {
        }

        public BasicActionResult(bool success, string message) 
            : base( new Result() { Message = message, Success = success })
        {            
        }

        public BasicActionResult(bool success, string message, string data)
            : base(new Result() { Message = message, Success = success, Data = data })
        {
        }
    }
}