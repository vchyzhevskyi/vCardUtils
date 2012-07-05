using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace vCardUtils
{
	public class Name
	{
		public string FamilyName { get; set; }
		public string GivenName { get; set; }
		public string AdditionalNames { get; set; }
		public string HonorificPrefixes { get; set; }
		public string HonorificSuffixes { get; set; }
		public string FormatedName
		{
			get
			{
				return string.Format("{0} {1} {2} {3} {4}", HonorificPrefixes, GivenName, AdditionalNames, FamilyName, HonorificSuffixes);
			}
		}

		public Name()
		{
			FamilyName = string.Empty;
			GivenName = string.Empty;
			AdditionalNames = string.Empty;
			HonorificPrefixes = string.Empty;
			HonorificSuffixes = string.Empty;
		}
	}
}
