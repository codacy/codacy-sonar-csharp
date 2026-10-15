using System.Net;
using System.Text;
using System.Collections.Generic;

public class Example
{
    public HttpStatusCode Status { get; set; }

    public string Build()
    {
        return new StringBuilder().Append(Status).ToString();
    }
}
