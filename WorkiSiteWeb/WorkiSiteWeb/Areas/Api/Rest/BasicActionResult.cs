namespace Worki.Rest
{
    public class BasicActionResult : ObjectResult<BasicActionResult.Result>
    {
        public class Result
        {
            public bool success { get; set; }
            public string message { get; set; }
            public string data { get; set; }
        }

        public BasicActionResult()
            : base(new Result() { success = true })
        {            
        }

        public BasicActionResult(bool success)
            : base(new Result() { success = success })
        {
        }

        public BasicActionResult(bool success, string message) 
            : base( new Result() { message = message, success = success })
        {            
        }

        public BasicActionResult(bool success, string message, string data)
            : base(new Result() { message = message, success = success, data = data })
        {
        }
    }
}