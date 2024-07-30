using Nest;
using BOI.Core.Extensions;
namespace BOI.Core.Search.Models
{
	public class Solicitor
	{
		public string SolicitorName { get; set; }

		public string Address1 { get; set; }

		public string Address2 { get; set; }

		public string Address3 { get; set; }

		public string Address4 { get; set; }

		public string Address5 { get; set; }

		[CsvHelper.Configuration.Attributes.Ignore]
		public string Address
		{
			get
			{
				var addressParts = new[] { Address1, Address2, Address3, Address4, Address5, PostCode }.Where(x => x.HasValue());
				return string.Join(", ", addressParts);
			}
		}

		public string PostCode { get; set; }

		public string Telephone { get; set; }

		[CsvHelper.Configuration.Attributes.Ignore]
		public string TelephoneFormatted { get { return Telephone.Replace(" ", ""); } }
		[CsvHelper.Configuration.Attributes.Ignore]
		public float Lat { get; set; }
		[CsvHelper.Configuration.Attributes.Ignore]
		public float Lon { get; set; }

		[CsvHelper.Configuration.Attributes.Ignore]
		[GeoPoint]
		public string Location { get { return string.Concat(Lat, ",", Lon); } }

		[CsvHelper.Configuration.Attributes.Ignore]
		public string Distance { get; set; }
	}
}
