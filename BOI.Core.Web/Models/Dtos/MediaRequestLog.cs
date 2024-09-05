using NPoco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace BOI.Core.Web.Models.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("Id", AutoIncrement = true)]
    [ExplicitColumns]
    public class MediaRequestLog
    {
        public const string TableName = "MediaRequestLog";
        public const string MediaUrlColumnName = "MediaUrl";
        public const string DateViewedColumnName = "DateViewed";
        public const string MediaItemIdColumnName = "MediaItemId";

        [Column("id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column(MediaUrlColumnName)]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        public string MediaUrl { get; set; }

        [Column(DateViewedColumnName)]
        public DateTime DateViewed { get; set; }

        [Column(MediaItemIdColumnName)]
        public Guid MediaItemId { get; set; }


    }
}
