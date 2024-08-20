using System.Collections.Generic;

namespace BOI.Core.Web.Models.Dtos.EdAdmin
{
    public class ExportResponse
    {
        public byte[] Data { get; set; }
        public List<string> Errors { get; set; }
    }
}
