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
		public string Title { get; private set; }
		public string Role { get; private set; }
		public Bitmap Logo { get; private set; }
		public object Agent { get; private set; }
		public Organization Organization { get; private set; }

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
			Title = string.Empty;
			Role = string.Empty;
			Logo = null;
			Agent = null;
			Organization = new Organization();
		}

		private void ProcessLines(string[] lines)
		{
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
							case "title":
								{
									Title = parsedLine[1] as string;
									break;
								}
							case "role":
								{
									Role = parsedLine[1] as string;
									break;
								}
							case "logo":
								{
									Logo = parsedLine[1] as Bitmap;
									break;
								}
							case "agent":
								{
									if (parsedLine[1] is string)
										Agent = parsedLine[1] as string;
									else
										Agent = parsedLine[1] as vCard;
									break;
								}
							case "org":
								{
									Organization = parsedLine[1] as Organization;
									break;
								}
							default:
								break;
						}
					}
					catch (Exception)
					{
					}
				}
			else
				return;
		}

		public void Load(string path)
		{
			string[] lines = File.ReadAllLines(path);
			ProcessLines(lines);
		}

		public void Load(string[] lines)
		{
			ProcessLines(lines);
		}

		private object[] ParseLine(string line)
		{
			object[] res = new object[2];
			Match m = Regex.Match(line, "^(?<tag>[A-Za-z]*)[:;]");
			if (m.Success)
			{
				string tag = m.Groups["tag"].Value;
				line = line.Replace(string.Format("{0}:", tag), "");
				switch (tag.ToLower())
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
							Match ma = Regex.Match(line, @":(?<familyname>[\w\W\s]*);(?<givenname>[\w\W\s]*);(?<additionalnames>[\w\W\s]*);(?<honorificprefixes>[\w\W\s]*);(?<honorificsuffixes>[\w\W\s]*)");
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
								Match mb = Regex.Match(line, @"uri:(?<value>[\w\W]+)$");
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
							Match ma = Regex.Match(line, @";(?<types>[A-Za-z,]*):(?<number>[-+\d]+)");
							res[0] = "tel";
							if (ma.Captures.Count > 0)
							{
								MatchCollection mb = Regex.Matches(ma.Groups["types"].Value, "(?<type>[A-Za-z]+)");
								res[1] = new Telephone()
								{
									Number = ma.Groups["number"].Value,
									Type = new TelephoneType[mb.Count]
								};
								for (int i = 0; i < mb.Count; i++)
									(res[1] as Telephone).Type[i] = (TelephoneType)Enum.Parse(typeof(TelephoneType), mb[i].Value, true);
							}
							else
							{
								ma = Regex.Match(line, @"(?<number>[-+\d]+)");
								res[1] = new Telephone()
								{
									Number = ma.Groups["number"].Value,
									Type = new TelephoneType[1] { TelephoneType.home }
								};
							}
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
					case "title":
						{
							Match ma = Regex.Match(line, @":(?<title>[\w\W\s]*)$");
							res[0] = "title";
							res[1] = ma.Groups["title"].Value.Replace(@"\", "");
							return res;
						}
					case "role":
						{
							Match ma = Regex.Match(line, @":(?<role>[\w\W\s]*)$");
							res[0] = "role";
							res[1] = ma.Groups["role"].Value.Replace(@"\", "");
							return res;
						}
					case "logo":
						{
							Match ma = Regex.Match(line, "=(?<encoding>.);");
							res[0] = "logo";
							res[1] = null;
							if (ma.Success)
							{
								Match mb = Regex.Match(line, @"(?<binarydata>[\w]+)$");
								res[1] = new Bitmap(new MemoryStream(Encoding.ASCII.GetBytes(mb.Groups["binarydata"].Value)));
							}
							else
							{
								Match mb = Regex.Match(line, @"uri:(?<value>[\w\W]+)$");
								byte[] data = new WebClient().DownloadData(new Uri(mb.Groups["value"].Value));
								res[1] = new Bitmap(new MemoryStream(data));
							}
							return res;
						}
					case "agent":
						{
							res[0] = "agent";
							if (line.ToLower().Contains("begin:vcard") && line.ToLower().Contains("end:vcard"))
							{
								string[] splitedLines = line.Replace("\\;", ";").Split(new string[1] { "\\n" }, StringSplitOptions.RemoveEmptyEntries);
								vCard agent = new vCard();
								agent.Load(splitedLines);
								res[1] = agent;
							}
							else
							{
								Match ma = Regex.Match(line, @"uri:(?<value>[\w\W]+)$");
								res[1] = ma.Groups["value"].Value;
							}
							return res;
						}
					case "org":
						{
							Match ma = Regex.Match(line, @":(?<organization>[\w\W\s]*)$");
							res[0] = "org";
							Match mb = Regex.Match(ma.Groups["organization"].Value.Replace(@"\", ""), @"(?<organizationname>[\w\W\s]*);(?<organizationunit1>[\w\W\s]*);(?<organizationunit2>[\w\W\s]*)");
							res[1] = new Organization()
							{
								OrganizationName = mb.Groups["organizationname"].Value,
								OrganizationUnit1 = mb.Groups["organizationunit1"].Value,
								OrganizationUnit2 = mb.Groups["organizationunit2"].Value
							};
							return res;
						}
					default:
						{
							return null;
						}
				}
			}
			return null;
		}
	}
}
