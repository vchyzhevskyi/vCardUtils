using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace vCardUtils
{
	public class Organization
	{
		public string OrganizationName { get; set; }
		public string OrganizationUnit1 { get; set; }
		public string OrganizationUnit2 { get; set; }

		public Organization()
		{
			OrganizationName = string.Empty;
			OrganizationUnit1 = string.Empty;
			OrganizationUnit2 = string.Empty;
		}
	}
}
