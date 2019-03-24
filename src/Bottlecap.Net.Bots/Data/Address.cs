using System;
using System.Text;

namespace Bottlecap.Net.Bots.Data
{
    public class Address
    {
        public AddressGeometry geometry { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string AddressLine3 { get; set; }

        public string City { get; set; }

        public string Postcode { get; set; }

        public string CountryCode { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();
            if (String.IsNullOrEmpty(AddressLine1) == false)
            {
                builder.AppendFormat("{0},", AddressLine1);
            }

            if (String.IsNullOrEmpty(AddressLine2) == false)
            {
                builder.AppendFormat("{0},", AddressLine2);
            }

            if (String.IsNullOrEmpty(AddressLine3) == false)
            {
                builder.AppendFormat("{0},", AddressLine3);
            }

            if (String.IsNullOrEmpty(City) == false)
            {
                builder.AppendFormat("{0},", City);
            }

            if (String.IsNullOrEmpty(Postcode) == false)
            {
                builder.AppendFormat("{0},", Postcode);
            }

            if (String.IsNullOrEmpty(CountryCode) == false)
            {
                builder.AppendFormat("{0}", CountryCode);
            }

            return builder.ToString().TrimEnd(',');
        }
    }
}