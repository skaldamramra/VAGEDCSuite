using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Net.Cache;
using System.Web;

namespace VAGSuite
{
    class msiupdater
    {
        private Version m_currentversion;
        private string m_customer = "Global";
        private string m_server = "http://trionic.mobixs.eu/vagedcsuite/";
        private string m_username = "";
        private string m_password = "";
        private Version m_NewVersion;
        private string m_apppath = "";
        private bool m_fromFileLocation = false;
        private bool m_blockauto_updates = false;

        public bool Blockauto_updates
        {
            get { return m_blockauto_updates; }
            set { m_blockauto_updates = value; }
        }
        public string Apppath
        {
            get { return m_apppath; }
            set { m_apppath = value; }
        }

        public Version NewVersion
        {
            get { return m_NewVersion; }
            set { m_NewVersion = value; }
        }
        public delegate void DataPump(MSIUpdaterEventArgs e);
        public event msiupdater.DataPump onDataPump;

        public delegate void UpdateProgressChanged(MSIUpdateProgressEventArgs e);
        public event msiupdater.UpdateProgressChanged onUpdateProgressChanged;
        public class MSIUpdateProgressEventArgs : System.EventArgs
        {
            private Int32 _NoFiles;
            private Int32 _NoFilesDone;
            private Int32 _PercentageDone;
            private Int32 _NoBytes;
            private Int32 _NoBytesDone;

            public Int32 NoBytesDone
            {
                get
                {
                    return _NoBytesDone;
                }
            }

            public Int32 NoBytes
            {
                get
                {
                    return _NoBytes;
                }
            }


            public Int32 NoFiles
            {
                get
                {
                    return _NoFiles;
                }
            }
            public Int32 NoFilesDone
            {
                get
                {
                    return _NoFilesDone;
                }
            }
            public Int32 PercentageDone
            {
                get
                {
                    return _PercentageDone;
                }
            }


            public MSIUpdateProgressEventArgs(Int32 NoFiles, Int32 NoFilesDone, Int32 PercentageDone, Int32 NoBytes, Int32 NoBytesDone)
            {
                this._NoFiles = NoFiles;
                this._NoFilesDone = NoFilesDone;
                this._PercentageDone = PercentageDone;
                this._NoBytes = NoBytes;
                this._NoBytesDone = NoBytesDone;
            }
        }


        public class MSIUpdaterEventArgs : System.EventArgs
        {
            private string _Data;
            private bool _UpdateAvailable;
            private bool _Version2High;
            private bool _info;
            private string _xmlFile;

            public string XMLFile
            {
                get
                {
                    return _xmlFile;
                }
            }

            public bool Info
            {
                get
                {
                    return _info;
                }
            }

            private Version _Version;
            public string Data
            {
                get
                {
                    return _Data;
                }
            }
            public bool UpdateAvailable
            {
                get
                {
                    return _UpdateAvailable;
                }
            }
            public bool Version2High
            {
                get
                {
                    return _Version2High;
                }
            }
            public Version Version
            {
                get
                {
                    return _Version;
                }
            }
            public MSIUpdaterEventArgs(string Data, bool Update, bool mVersion2High, Version NewVersion, bool info, string xmlfile)
            {
                this._Data = Data;
                this._info = info;
                this._UpdateAvailable = Update;
                this._Version2High = mVersion2High;
                this._Version = NewVersion;
                this._xmlFile = xmlfile;
            }
        }

        public msiupdater(Version CurrentVersion)
        {
            m_currentversion = CurrentVersion;
            m_NewVersion = new Version("1.0.0.0");
        }

        public void CheckForUpdates(string customer, string server, string username, string password, bool FromFile)
        {
            m_server = server;
            m_customer = customer;
            m_username = username;
            m_password = password;
            m_fromFileLocation = FromFile;
            if (!m_blockauto_updates)
            {
                System.Threading.Thread t = new System.Threading.Thread(updatecheck);
                t.Start();
            }
        }

        public void ExecuteUpdate(Version ver)
        {
            string command = "http://trionic.mobixs.eu/vagedcsuite/" + ver.ToString() + "/VAGEDCSuite.msi";
            try
            {
                System.Diagnostics.Process.Start(command);
            }
            catch (Exception E)
            {
                PumpString("Exception when checking new update(s): " + E.Message, false, false, new Version(), false, "");
            }
        }


        private void PumpString(string text, bool updateavailable, bool version2high, Version newver, bool info, string xmlfile)
        {
            onDataPump(new MSIUpdaterEventArgs(text, updateavailable, version2high, newver, info, xmlfile));
        }

        private void NotifyProgress(Int32 NoFiles, Int32 NoFilesDone, Int32 PercentageDone, Int32 NoBytes, Int32 NoBytesDone)
        {
            onUpdateProgressChanged(new MSIUpdateProgressEventArgs(NoFiles, NoFilesDone, PercentageDone, NoBytes, NoBytesDone));
        }

        public string GetPageHTML(string pageUrl, int timeoutSeconds)
        {
            System.Net.WebResponse response = null;

            try
            {
                // Enable TLS 1.2 for GitHub API
                try { System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072; } catch { }

                // Setup our Web request
                System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(pageUrl);
                request.UserAgent = "VAGEDCSuite-Updater";
                HttpRequestCachePolicy noCachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                request.CachePolicy = noCachePolicy;
                try
                {
                    request.Proxy.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                }
                catch (Exception proxyE)
                {
                    PumpString("Error setting proxy server: " + proxyE.Message, false, false, new Version(), false, "");
                }

                request.Timeout = timeoutSeconds * 1000;

                // Retrieve data from request
                response = request.GetResponse();

                System.IO.Stream streamReceive = response.GetResponseStream();
                System.Text.Encoding encoding = System.Text.Encoding.GetEncoding("utf-8");
                System.IO.StreamReader streamRead = new System.IO.StreamReader(streamReceive, encoding);

                // return the retrieved HTML
                return streamRead.ReadToEnd();
            }
            catch (Exception ex)
            {
                // Error occured grabbing data, return empty string.
                Console.WriteLine("GetPageHTML Error [" + pageUrl + "]: " + ex.Message);
                PumpString("An error occurred while retrieving the HTML content. " + ex.Message, false, false, new Version(), false, "");
                return "";
            }
            finally
            {
                // Check if exists, then close the response.
                if (response != null)
                {
                    response.Close();
                }
            }
        }
        private void ExtractNameValue(string input, out string Name, out string Value)
		{
			Name = "";
			Value = "";
			// input : <Element name="I2l" value="00" />
			int id1,id2,id3,id4;
			id1=input.IndexOf("\"",0,input.Length);
			if(id1>0) // eerste " gevonden
			{
				id2=input.IndexOf("\"",id1+1,input.Length - id1 - 1);
				if(id2 > 0) // tweede " gevonden
				{
					id3=input.IndexOf("\"",id2+1,input.Length - id2 - 1);
					if(id3>0)
					{
						id4=input.IndexOf("\"",id3+1,input.Length - id3 - 1);
						if(id4>0)
						{
							Name = input.Substring(id1+1,id2-id1-1);
							Value = input.Substring(id3+1,id4-id3-1);
							// alles gevonden
						}
					}

				}
				
			}
		}

        private string FileToString(string infile)
        {
            StreamReader stream = System.IO.File.OpenText(infile);
            string returnvalues;
            returnvalues = stream.ReadToEnd();
            stream.Close();
            return returnvalues;
        }

        private void updatecheck()
        {
            string URLString="";
            string XMLResult="";
            //string VehicleString;
            bool m_updateavailable = false;
            bool m_version_toohigh = false;
            bool _info = false;
            Version maxversion = new Version("1.0.0.0");
            File.Delete(Apppath + "\\input.xml");
            File.Delete(Apppath + "\\Notes.xml");

            try
            {
                if (m_customer.Length > 0)
                {
                    URLString = "http://trionic.mobixs.eu/vagedcsuite/version.xml";
                    XMLResult = GetPageHTML(URLString, 10);
                    using (StreamWriter xmlfile = new StreamWriter(Apppath + "\\input.xml", false, System.Text.Encoding.ASCII, 2048))
                    {
                        xmlfile.Write(XMLResult);
                        xmlfile.Close();
                    }
                    URLString = "http://trionic.mobixs.eu/vagedcsuite/Notes.xml";
                    XMLResult = GetPageHTML(URLString, 10);
                    using (StreamWriter xmlfile = new StreamWriter(Apppath + "\\Notes.xml", false, System.Text.Encoding.ASCII, 2048))
                    {
                        xmlfile.Write(XMLResult);
                        xmlfile.Close();
                    }
                }

                XmlDocument doc;
                try
                {
                    doc = new XmlDocument();
                    doc.LoadXml(FileToString(Apppath + "\\input.xml"));

                    // Add any other properties that would be useful to store
                    //foreach (
                    System.Xml.XmlNodeList Nodes;
                    Nodes = doc.GetElementsByTagName("vagedcsuite");
                    foreach (System.Xml.XmlNode Item in Nodes)
                    {
                        System.Xml.XmlAttributeCollection XMLColl;
                        XMLColl = Item.Attributes;
                        foreach (System.Xml.XmlAttribute myAttr in XMLColl)
                        {
                            if (myAttr.Name == "version")
                            {
                                Version v = new Version(myAttr.Value);
                                if (v > m_currentversion) 
                                {
                                    if (v > maxversion) maxversion = v;
                                    m_updateavailable = true;
                                    PumpString("Available version: " + myAttr.Value, false, false, new Version(), false, Apppath + "\\Notes.xml");
                                }
                                else if (v.Major < m_currentversion.Major || (v.Major == m_currentversion.Major && v.Minor < m_currentversion.Minor) || (v.Major == m_currentversion.Major && v.Minor == m_currentversion.Minor && v.Build < m_currentversion.Build))
                                {

                                    // mmm .. gebruiker draait een versie die hoger is dan dat is vrijgegeven... 
                                    if (v > maxversion) maxversion = v;
                                    m_updateavailable = false;
                                    m_version_toohigh = true;
                                }
                            }
                            else if (myAttr.Name == "info")
                            {
                                try
                                {
                                    _info = Convert.ToBoolean(myAttr.Value);
                                }
                                catch (Exception sendIE)
                                {
                                    Console.WriteLine(sendIE.Message);
                                }
                            }
                        }

                    }
                }
                catch (Exception E)
                {
                    PumpString(E.Message, false, false, new Version(), false, "");
                }
                if (m_updateavailable)
                {

                    //Console.WriteLine("An update is available: " + maxversion.ToString());
                    PumpString("A newer version is available: " + maxversion.ToString(), m_updateavailable, m_version_toohigh, maxversion, _info, Apppath + "\\Notes.xml");
                    m_NewVersion = maxversion;

                }
                else if (m_version_toohigh)
                {
                    PumpString("Versionnumber is too high: " + maxversion.ToString(), m_updateavailable, m_version_toohigh, maxversion, _info, Apppath + "\\Notes.xml");
                    m_NewVersion = maxversion;
                }
                else
                {
                    PumpString("No new version(s) found...", false, false, new Version(), _info, Apppath + "\\Notes.xml");
                }
            }
            catch (Exception tuE)
            {
                PumpString(tuE.Message, false, false, new Version(), _info, "");
            }
            
        }



        internal string GetReleaseNotes()
        {
            // Try GitHub Releases API first
            string githubUrl = "https://api.github.com/repos/skaldamramra/VAGEDCSuite/releases";
            string jsonResult = GetPageHTML(githubUrl, 15);
            Console.WriteLine("GitHub JSON Result Length: " + (jsonResult != null ? jsonResult.Length.ToString() : "null"));
            
            if (!string.IsNullOrEmpty(jsonResult) && jsonResult.StartsWith("["))
            {
                // Convert GitHub JSON to RSS-like XML format
                string xmlResult = ConvertGitHubReleasesToXML(jsonResult);
                Console.WriteLine("Generated XML (first 500 chars): " + (xmlResult.Length > 500 ? xmlResult.Substring(0, 500) : xmlResult));
                WriteNotesXML(xmlResult);
                return Apppath + "\\Notes.xml";
            }
            else
            {
                Console.WriteLine("GitHub JSON Result is invalid or empty. Starts with: " + (jsonResult != null && jsonResult.Length > 0 ? jsonResult[0].ToString() : "N/A"));
            }
            
            // Fallback to old server if GitHub fails
            string URLString = "http://trionic.mobixs.eu/vagedcsuite/Notes.xml";
            string XMLResult = GetPageHTML(URLString, 10);
            WriteNotesXML(XMLResult);
            return Apppath + "\\Notes.xml";
        }

        private void WriteNotesXML(string content)
        {
            using (StreamWriter xmlfile = new StreamWriter(Apppath + "\\Notes.xml", false, System.Text.Encoding.UTF8, 2048))
            {
                xmlfile.Write(content);
                xmlfile.Close();
            }
        }

        private string ConvertGitHubReleasesToXML(string json)
        {
            try
            {
                List<GitHubRelease> releases = ParseGitHubReleases(json);
                
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sb.AppendLine("<rss version=\"2.0\">");
                sb.AppendLine("  <channel>");
                sb.AppendLine("    <title>VAGEDCSuite Releases</title>");
                sb.AppendLine("    <link>https://github.com/skaldamramra/VAGEDCSuite/releases</link>");
                
                foreach (var release in releases)
                {
                    string pubDate = release.published_at;
                    if (!string.IsNullOrEmpty(pubDate))
                    {
                        try
                        {
                            DateTime dt = DateTime.ParseExact(pubDate, "yyyy-MM-dd'T'HH:mm:ss'Z'", System.Globalization.CultureInfo.InvariantCulture);
                            pubDate = dt.ToString("ddd, dd MMM yyyy HH:mm:ss GMT", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                        }
                        catch
                        {
                            pubDate = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss GMT", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
                        }
                    }
                    
                    sb.AppendLine("    <item>");
                    sb.AppendLine("      <title>" + EscapeXml(release.name ?? release.tag_name) + "</title>");
                    sb.AppendLine("      <description>" + EscapeXml(release.body ?? "") + "</description>");
                    sb.AppendLine("      <link>" + EscapeXml(release.html_url) + "</link>");
                    sb.AppendLine("      <pubDate>" + pubDate + "</pubDate>");
                    sb.AppendLine("      <version>" + EscapeXml(release.tag_name) + "</version>");
                    sb.AppendLine("    </item>");
                }
                
                sb.AppendLine("  </channel>");
                sb.AppendLine("</rss>");
                
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error converting GitHub releases: " + ex.Message);
                // Return minimal valid XML
                return "<?xml version=\"1.0\"?><rss><channel><item><title>Error loading releases</title></item></channel></rss>";
            }
        }

        private List<GitHubRelease> ParseGitHubReleases(string json)
        {
            List<GitHubRelease> releases = new List<GitHubRelease>();
            
            // Simple JSON parsing without external libraries
            int pos = 0;
            while (pos < json.Length)
            {
                // Find start of object
                int objStart = json.IndexOf("{", pos);
                if (objStart < 0) break;
                
                // Find matching closing brace
                int braceCount = 1;
                int objEnd = objStart + 1;
                while (objEnd < json.Length && braceCount > 0)
                {
                    if (json[objEnd] == '{') braceCount++;
                    else if (json[objEnd] == '}') braceCount--;
                    objEnd++;
                }
                
                if (braceCount == 0)
                {
                    string objJson = json.Substring(objStart, objEnd - objStart);
                    GitHubRelease release = ParseGitHubReleaseObject(objJson);
                    if (release != null)
                    {
                        releases.Add(release);
                    }
                    pos = objEnd;
                }
                else
                {
                    break;
                }
            }
            
            return releases;
        }

        private GitHubRelease ParseGitHubReleaseObject(string json)
        {
            GitHubRelease release = new GitHubRelease();
            
            // Parse tag_name
            release.tag_name = ExtractJsonString(json, "tag_name");
            
            // Parse name (fallback to tag_name if empty)
            release.name = ExtractJsonString(json, "name");
            if (string.IsNullOrEmpty(release.name))
            {
                release.name = release.tag_name;
            }
            
            // Parse body
            release.body = ExtractJsonString(json, "body");
            
            // Parse html_url
            release.html_url = ExtractJsonString(json, "html_url");
            
            // Parse published_at
            release.published_at = ExtractJsonString(json, "published_at");
            
            return release;
        }

        private string ExtractJsonString(string json, string key)
        {
            string searchKey = "\"" + key + "\"";
            int keyPos = json.IndexOf(searchKey);
            if (keyPos < 0) return "";

            int colonPos = json.IndexOf(":", keyPos + searchKey.Length);
            if (colonPos < 0) return "";

            int valueStart = colonPos + 1;
            while (valueStart < json.Length && (char.IsWhiteSpace(json[valueStart]) || json[valueStart] == '\r' || json[valueStart] == '\n')) valueStart++;

            if (valueStart >= json.Length) return "";

            if (json[valueStart] == '"')
            {
                int valueEnd = valueStart + 1;
                StringBuilder sb = new StringBuilder();
                while (valueEnd < json.Length)
                {
                    if (json[valueEnd] == '\\' && valueEnd + 1 < json.Length)
                    {
                        char escaped = json[valueEnd + 1];
                        if (escaped == 'n') sb.Append('\n');
                        else if (escaped == 'r') sb.Append('\r');
                        else if (escaped == 't') sb.Append('\t');
                        else sb.Append(escaped);
                        valueEnd += 2;
                    }
                    else if (json[valueEnd] == '"') break;
                    else
                    {
                        sb.Append(json[valueEnd]);
                        valueEnd++;
                    }
                }
                return sb.ToString();
            }
            else
            {
                int valueEnd = valueStart;
                while (valueEnd < json.Length && json[valueEnd] != ',' && json[valueEnd] != '}' && json[valueEnd] != ']' && !char.IsWhiteSpace(json[valueEnd])) valueEnd++;
                return json.Substring(valueStart, valueEnd - valueStart).Trim();
            }
        }

        private string EscapeXml(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            StringBuilder sb = new StringBuilder(text);
            sb.Replace('&', '\0').Replace("&", "&"); // Temporary placeholder
            // Properly escape XML characters
            return System.Security.SecurityElement.Escape(text);
        }
    }

    public class GitHubRelease
    {
        public string id { get; set; }
        public string tag_name { get; set; }
        public string name { get; set; }
        public string body { get; set; }
        public string html_url { get; set; }
        public string published_at { get; set; }
        public bool prerelease { get; set; }
    }
}
