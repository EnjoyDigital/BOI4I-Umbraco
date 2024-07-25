using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankOfIreland.Intermediaries.Core.Web.Extensions;

namespace BankOfIreland.Intermediaries.Core.Web.Models.CmsModels
{
    public partial class BDmcontact
    {

        public string Jobtitle
        {
            get
            {
                switch (BDmtype.ToLower())
                {
                    case "bmd":
                        return "Business Development Manager";

                    case "tbdm":                       
                    case "iel":
                        return "Telephone Business Development Manager";
                    default:
                        return "";

                }
            }
        }

        public BDMType BDMType
        {

            get
            {
                if (this.BDmtype.HasValue())
                {
                    try
                    {
                        return (BDMType)Enum.Parse(typeof(BDMType), BDmtype);
                    }
                    catch(Exception ex)
                    {
                        if(string.Equals(BDmtype, "sbdm", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return BDMType.BDM;
                        }
                        else
                        {
                            return BDMType.None;
                        }
                    }
                }
                else
                {

                    switch (this.JobTitle.Trim().ToLowerInvariant())
                    {
                        case "business development manager":
                        case "senior business development manager":
                        case "bdm":
                        case "sbdm":

                            return BDMType.BDM;

                        case "tbdm":
                        case "telephone business development manager":
                        case "telephone business development manager (monday - wednesday)":
                            return BDMType.TBDM;

                        case "iel":
                            return BDMType.IEL;

                        default:
                            return BDMType.None;
                    }

                }

            }
        }

        public string FullName { get { return $"{Firstname} {Surname}"; } }
        public string ShortBio(int length =235)
        {
            
                return Bio.Length >= length ? Bio?.Substring(0, length) : Bio;
            
        }

        public string[] PostCodeOutcodes { get { return Regions.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToUpperInvariant().Trim()).ToArray(); } }
        public string[] FcaCodes { get {  return FCanumber.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToUpperInvariant().Trim()).ToArray(); } }

    }

    public enum BDMType
    {
        None = 0, BDM=1, TBDM=2, IEL=3
    }
}
