namespace DapperWrapper.Contracts
{
    using System.Collections.Generic;

    public class DapperRequest
    {
        public IList<(string Name, object Value)> Properties { get; set; }
    }
}
