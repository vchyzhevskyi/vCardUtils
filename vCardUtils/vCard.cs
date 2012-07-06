using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace vCardUtils
{
	public class vCard
	{
		public float Version { get; private set; }
		public Name Name { get; private set; }
		public string Nickname { get; private set; }
		public Bitmap Photo { get; private set; }
		public DateTime Birthday { get; private set; }
		public Telephone[] Telephone { get; private set; }
		public Email[] Email { get; private set; }
		public string Mailer { get; private set; }

		public vCard()
		{
			Init();
		}

		public vCard(string path)
		{
			Init();
			Load(path);
		}

		private void Init()
		{
			Version = 0;
			Name = new Name();
			Nickname = string.Empty;
			Photo = null;
			Birthday = new DateTime();
			Telephone = new Telephone[0];
			Email = new Email[0];
			Mailer = string.Empty;
		}

		public void Load(string path)
		{
			string[] lines = File.ReadAllLines(path);
			if (lines.Contains("begin:vcard".ToUpper()) || lines.Contains("begin:vcard") || lines.Contains("end:vcard".ToUpper()) || lines.Contains("begin:vcard"))
				foreach (string line in lines)
				{
					try
					{
						object[] parsedLine = ParseLine(line);
						switch ((parsedLine[0] as string))
						{
							case "version":
								{
									Version = (float)parsedLine[1];
									break;
								}
							case "n":
								{
									Name = parsedLine[1] as Name;
									break;
								}
							case "nickname":
								{
									Nickname = parsedLine[1] as string;
									break;
								}
							case "photo":
								{
									Photo = parsedLine[1] as Bitmap;
									break;
								}
							case "bday":
								{
									Birthday = (DateTime)parsedLine[1];
									break;
								}
							case "tel":
								{
									Telephone[] tmp = Telephone;
									Telephone = new Telephone[tmp.Length + 1];
									tmp.CopyTo(Telephone, 0);
									Telephone[tmp.Length] = parsedLine[1] as Telephone;
									break;
								}
							case "email":
								{
									Email[] tmp = Email;
									Email = new Email[tmp.Length + 1];
									tmp.CopyTo(Email, 0);
									Email[tmp.Length] = parsedLine[1] as Email;
									break;
								}
							case "mailer":
								{
									Mailer = parsedLine[1] as string;
									break;
								}
							default:
								break;
						}
					}
					catch (Exception ex)
					{
					}
				}
			else
				return;
		}

		private object[] ParseLine(string line)
		{
			object[] res = new object[2];
			Match m = Regex.Match(line, "^(?<tag>[A-Za-z]*)[:;]");
			if (m.Success)
				switch (m.Groups["tag"].Value.ToLower())
				{
					case "version":
						{
							Match ma = Regex.Match(line, @"(?<version>[\d.]+)");
							res[0] = "version";
							res[1] = float.Parse(ma.Groups["version"].Value);
							return res;
						}
					case "fn":
						{
							return null;
						}
					case "n":
						{
							Match ma = Regex.Match(line, "(?<familyname>[A-Za-z]*);(?<givenname>[A-Za-z]*);(?<additionalnames>[A-Za-z]*);(?<honorificprefixes>[A-Za-z]*);(?<honorificsuffixes>[A-Za-z]*)");
							res[0] = "n";
							res[1] = new Name()
							{
								FamilyName = ma.Groups["familyname"].Value,
								GivenName = ma.Groups["givenname"].Value,
								AdditionalNames = ma.Groups["additionalnames"].Value,
								HonorificPrefixes = ma.Groups["honorificprefixes"].Value,
								HonorificSuffixes = ma.Groups["honorificsuffixes"].Value
							};
							return res;
						}
					case "nickname":
						{
							Match ma = Regex.Match(line, "(?<nickname>[A-Za-z,]*)$");
							res[0] = "nickname";
							res[1] = ma.Groups["nickname"].Value;
							return res;
						}
					case "photo":
						{
							Match ma = Regex.Match(line, "=(?<encoding>.);");
							res[0] = "photo";
							res[1] = null;
							if (ma.Success)
							{
								Match mb = Regex.Match(line, @"(?<binarydata>[\w]+)$");
								res[1] = new Bitmap(new MemoryStream(Encoding.ASCII.GetBytes(mb.Groups["binarydata"].Value)));
							}
							else
							{
								Match mb = Regex.Match(line, @":(?<value>[\w\W]+)$");
								byte[] data = new WebClient().DownloadData(new Uri(mb.Groups["value"].Value));
								res[1] = new Bitmap(new MemoryStream(data));
							}
							return res;
						}
					case "bday":
						{
							Match ma = Regex.Match(line, ":(?<date>[0-9-:TtZz,]*)$");
							res[0] = "bday";
							Match mb = Regex.Match(ma.Groups["date"].Value, "(?<year>[0-9]+)[-/.](?<month>[0-9]+)[-/.](?<day>[0-9]+)");
							res[1] = new DateTime(int.Parse(mb.Groups["year"].Value), int.Parse(mb.Groups["month"].Value), int.Parse(mb.Groups["day"].Value));
							return res;
						}
					case "tel":
						{
							Match ma = Regex.Match(line, @"(?<types>[A-Za-z,]*):(?<number>[-+\d]+)");
							res[0] = "tel";
							MatchCollection mb = Regex.Matches(ma.Groups["types"].Value, "(?<type>[A-Za-z]+)");
							res[1] = new Telephone()
							{
								Number = ma.Groups["number"].Value,
								Type = new TelephoneType[mb.Count]
							};
							for (int i = 0; i < mb.Count; i++)
								(res[1] as Telephone).Type[i] = (TelephoneType)Enum.Parse(typeof(TelephoneType), mb[i].Value, true);
							return res;
						}
					case "email":
						{
							Match ma = Regex.Match(line, @"(?<types>[A-Za-z,]*):(?<email>[\w\W]+)");
							res[0] = "email";
							MatchCollection mb = Regex.Matches(ma.Groups["types"].Value, "(?<type>[A-Za-z]+)");
							res[1] = new Email()
							{
								Address = ma.Groups["email"].Value,
								Type = new EmailType[mb.Count]
							};
							for (int i = 0; i < mb.Count; i++)
								(res[1] as Email).Type[i] = (EmailType)Enum.Parse(typeof(EmailType), mb[i].Value, true);
							return res;
						}
					case "mailer":
						{
							Match ma = Regex.Match(line, @":(?<mailer>[\w\d\s\W]+)$");
							res[0] = "mailer";
							res[1] = ma.Groups["mailer"].Value;
							return res;
						}
					default:
						{
							return null;
						}
				}
			return null;
		}
	}
}
