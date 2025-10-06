using System.Text.RegularExpressions;

namespace OracleClient.Models
{
    public class TnsNamesParser
    {
        public class TnsEntry
        {
            public string Name { get; set; } = string.Empty;
            public string Host { get; set; } = string.Empty;
            public int Port { get; set; } = 1521;
            public string ServiceName { get; set; } = string.Empty;
            public string Sid { get; set; } = string.Empty;
            public string ConnectionString => $"Data Source={Host}:{Port}/{ServiceName}{(string.IsNullOrEmpty(Sid) ? "" : $";SID={Sid}")}";
        }

        public static List<TnsEntry> ParseTnsNamesFile(string filePath)
        {
            var entries = new List<TnsEntry>();
            
            if (!File.Exists(filePath))
                return entries;

            try
            {
                var content = File.ReadAllText(filePath);
                var lines = content.Split('\n');
                TnsEntry? currentEntry = null;
                bool inEntry = false;
                string entryContent = string.Empty;

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    
                    // Skip empty lines and comments
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                        continue;

                    // Check if this is a new entry (starts with a name followed by =)
                    var entryMatch = Regex.Match(trimmedLine, @"^([A-Za-z0-9_]+)\s*=");
                    if (entryMatch.Success)
                    {
                        // Save previous entry if exists
                        if (currentEntry != null)
                        {
                            ParseEntryContent(currentEntry, entryContent);
                            entries.Add(currentEntry);
                        }

                        // Start new entry
                        currentEntry = new TnsEntry { Name = entryMatch.Groups[1].Value };
                        inEntry = true;
                        entryContent = trimmedLine.Substring(entryMatch.Length).Trim();
                    }
                    else if (inEntry && currentEntry != null)
                    {
                        // Continue building entry content
                        entryContent += " " + trimmedLine;
                    }
                }

                // Don't forget the last entry
                if (currentEntry != null)
                {
                    ParseEntryContent(currentEntry, entryContent);
                    entries.Add(currentEntry);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing TNSNAMES.ORA file: {ex.Message}", ex);
            }

            return entries;
        }

        private static void ParseEntryContent(TnsEntry entry, string content)
        {
            // Remove parentheses and clean up
            content = content.Trim();
            if (content.StartsWith("(") && content.EndsWith(")"))
            {
                content = content.Substring(1, content.Length - 2);
            }

            // Parse host
            var hostMatch = Regex.Match(content, @"HOST\s*=\s*([^\s,)]+)", RegexOptions.IgnoreCase);
            if (hostMatch.Success)
            {
                entry.Host = hostMatch.Groups[1].Value.Trim();
            }

            // Parse port
            var portMatch = Regex.Match(content, @"PORT\s*=\s*(\d+)", RegexOptions.IgnoreCase);
            if (portMatch.Success)
            {
                if (int.TryParse(portMatch.Groups[1].Value, out int port))
                {
                    entry.Port = port;
                }
            }

            // Parse service name
            var serviceMatch = Regex.Match(content, @"SERVICE_NAME\s*=\s*([^\s,)]+)", RegexOptions.IgnoreCase);
            if (serviceMatch.Success)
            {
                entry.ServiceName = serviceMatch.Groups[1].Value.Trim();
            }

            // Parse SID (fallback if no service name)
            var sidMatch = Regex.Match(content, @"SID\s*=\s*([^\s,)]+)", RegexOptions.IgnoreCase);
            if (sidMatch.Success)
            {
                entry.Sid = sidMatch.Groups[1].Value.Trim();
            }
        }

        public static string GetDefaultTnsNamesPath()
        {
            var oracleHome = Environment.GetEnvironmentVariable("ORACLE_HOME");
            if (!string.IsNullOrEmpty(oracleHome))
            {
                var tnsPath = Path.Combine(oracleHome, "network", "admin", "tnsnames.ora");
                if (File.Exists(tnsPath))
                    return tnsPath;
            }

            // Try common locations
            var commonPaths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "tnsnames.ora"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Oracle", "tnsnames.ora"),
                @"C:\oracle\product\*\dbhome_*\network\admin\tnsnames.ora"
            };

            foreach (var path in commonPaths)
            {
                if (path.Contains("*"))
                {
                    var directory = Path.GetDirectoryName(path);
                    var pattern = Path.GetFileName(path);
                    if (Directory.Exists(directory))
                    {
                        var files = Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);
                        if (files.Length > 0)
                            return files[0];
                    }
                }
                else if (File.Exists(path))
                {
                    return path;
                }
            }

            return string.Empty;
        }
    }
}
