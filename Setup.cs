using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using Microsoft.Win32;

[assembly: AssemblyTitle("Scam With Your Friends - Famous Callers Setup")]
[assembly: AssemblyProduct("Famous Callers")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: System.Runtime.Versioning.TargetFramework(".NETFramework,Version=v4.7.2")]

internal static class Setup
{
    const string Version = "1.0.0";
    const string GameExe = "Scam With Your Friends.exe";
    const string DataRelative = "Scam With Your Friends_Data";
    const string CatalogRelative = "kolkata-json/caller-identities/famous-callers-pack.json";
    static readonly JavaScriptSerializer Json = new JavaScriptSerializer { MaxJsonLength = 4194304 };
    static bool noPause;
    static string game, data;

    static int Main(string[] args)
    {
        Console.OutputEncoding = new UTF8Encoding(false);
        noPause = args.Contains("--no-pause") || Console.IsInputRedirected;
        bool check = false, uninstall = false, queue = true;
        try
        {
            for(int i=0;i<args.Length;i++) switch(args[i])
            {
                case "--game-path": if(++i>=args.Length)throw new ArgumentException("--game-path needs a folder.");game=args[i];break;
                case "--data-path": if(++i>=args.Length)throw new ArgumentException("--data-path needs a folder.");data=args[i];break;
                case "--check":check=true;break;
                case "--uninstall":uninstall=true;break;
                case "--no-queue":queue=false;break;
                case "--no-pause":break;
                case "--help":Console.WriteLine("setup.exe [--game-path <game folder>] [--check | --uninstall] [--no-queue] [--no-pause]\n--data-path <Unity save folder> overrides the default save location.");Pause();return 0;
                default:throw new ArgumentException("Unknown option: "+args[i]);
            }
            if(check&&uninstall)throw new ArgumentException("Choose --check or --uninstall.");
            Console.WriteLine("SCAM WITH YOUR FRIENDS - FAMOUS CALLERS "+Version);
            Console.WriteLine("20 new personalities, opening lines and original fan portraits.\n");
            Console.WriteLine("[1/4] Finding the game...");
            game=ChooseGame(game);
            if(data==null)data=Path.Combine(Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)),"LocalLow","Jatater Worldwide","Scam With Your Friends");
            data=Path.GetFullPath(data);
            Console.WriteLine("      Game: "+game);
            Console.WriteLine("[2/4] Checking the pack and installed game assets...");
            var payload=ReadPayload();
            var callers=ValidatePayload(payload);
            var files=InstallFiles(payload);
            if(!uninstall) foreach(var caller in callers)
            {
                string voice=Within(Streaming,Text(caller,"voiceId"));
                if(!File.Exists(voice))throw new IOException("A required built-in voice sample is missing: "+Text(caller,"voiceId")+". Verify the game's files in Steam.");
            }
            var record=ReadMap(Path.Combine(ModRoot,"install.json"));
            var recorded=record!=null&&record.ContainsKey("files")?record["files"] as Dictionary<string,object>:null;
            ValidateOwned(files,recorded,uninstall);
            if(check)
            {
                Console.WriteLine("      Pack and voice references verified. No files were changed.");
                Console.WriteLine(GameRunning()?"      Close the game before installing.":"      Game is closed.");
                Instructions();Pause();return 0;
            }
            if(GameRunning())throw new IOException("Close the game, then run setup again. Your session has been left open.");
            Console.WriteLine(uninstall?"[3/4] Removing this pack...":"[3/4] Installing the 20 callers and portraits...");
            var prior=new Dictionary<string,byte[]>(StringComparer.OrdinalIgnoreCase);
            var createdDirectories=new List<string>();
            try
            {
                foreach(var file in files)
                {
                    string target=Within(Streaming,file.Key);
                    if(uninstall){if(File.Exists(target))Change(target,null,prior,createdDirectories);}
                    else Change(target,file.Value,prior,createdDirectories);
                }
                Console.WriteLine("[4/4] Updating the caller lineup...");
                string queuePath=Within(data,"daily-callers.json");
                if(queue&&File.Exists(queuePath))
                {
                    var saved=ReadMap(queuePath);
                    if(saved==null||!saved.ContainsKey("callerIds")||!(saved["callerIds"] is IEnumerable)||saved["callerIds"] is string)throw new IOException("The existing caller lineup could not be read. Nothing was replaced. Use --no-queue to leave it alone.");
                    var ids=Sequence(saved["callerIds"]).Select(v=>v as string).ToList();
                    if(ids.Any(v=>v==null))throw new IOException("The existing caller lineup contains invalid entries. Use --no-queue to leave it alone.");
                    var packIds=new HashSet<string>(callers.Select(c=>Text(c,"id")),StringComparer.OrdinalIgnoreCase);
                    var updated=uninstall?ids.Where(id=>!packIds.Contains(id)).ToList():ids.Concat(packIds.Where(id=>!ids.Contains(id,StringComparer.OrdinalIgnoreCase))).ToList();
                    if(!ids.SequenceEqual(updated))
                    {
                        string backup=Within(ModRoot,"lineup-before-first-install.json");
                        if(!uninstall&&!File.Exists(backup))Change(backup,File.ReadAllBytes(queuePath),prior,createdDirectories);
                        saved["callerIds"]=updated.ToArray();
                        Change(queuePath,Encoding.UTF8.GetBytes(Json.Serialize(saved)),prior,createdDirectories);
                        Console.WriteLine(uninstall?"      Removed pack entries; other lineup settings preserved.":"      Added pack entries; existing callers and lineup settings preserved.");
                    }
                }
                else Console.WriteLine("      The game will include the pack when it creates a new default lineup.");
                string recordPath=Within(ModRoot,"install.json");
                if(uninstall)Change(recordPath,null,prior,createdDirectories);
                else
                {
                    var hashes=files.ToDictionary(p=>p.Key,p=>Hash(p.Value));
                    Change(recordPath,Encoding.UTF8.GetBytes(Json.Serialize(new Dictionary<string,object>{{"version",Version},{"files",hashes}})),prior,createdDirectories);
                    foreach(var file in files)if(Hash(File.ReadAllBytes(Within(Streaming,file.Key)))!=hashes[file.Key])throw new IOException("Installed-file verification failed.");
                }
            }
            catch
            {
                Console.WriteLine("Restoring files changed by this attempt...");
                foreach(var previous in prior.Reverse())
                { if(previous.Value==null){if(File.Exists(previous.Key))File.Delete(previous.Key);}else AtomicWrite(previous.Key,previous.Value); }
                foreach(var directory in createdDirectories.AsEnumerable().Reverse())if(Directory.Exists(directory)&&!Directory.EnumerateFileSystemEntries(directory).Any())Directory.Delete(directory);
                throw;
            }
            Console.WriteLine(uninstall?"DONE: Famous Callers removed. Game DLLs and other callers were untouched.":"DONE: 20 callers installed. Game DLLs and other callers were untouched.");
            if(!uninstall)Instructions();
            Pause();return 0;
        }
        catch(Exception e){Console.WriteLine("\nSETUP STOPPED: "+e.Message);Pause();return 1;}
    }
    static string Streaming { get { return Within(game,DataRelative+"/StreamingAssets"); } }
    static string ModRoot { get { return Within(game,"FamousCallers"); } }
    static void Pause(){if(!noPause){Console.WriteLine("\nPress Enter to close.");Console.ReadLine();}}
    static string Text(Dictionary<string,object> map,string key){object value;return map.TryGetValue(key,out value)?value as string:null;}
    static IEnumerable<object> Sequence(object value){var list=value as IEnumerable;if(list==null||value is string)throw new InvalidDataException("Expected a JSON array.");foreach(var item in list)yield return item;}
    static Dictionary<string,object> ReadMap(string path){if(!File.Exists(path))return null;return Json.DeserializeObject(File.ReadAllText(path)) as Dictionary<string,object>;}
    static string Hash(byte[] bytes){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();}
    static string Within(string root,string relative)
    {
        if(string.IsNullOrEmpty(relative)||Path.IsPathRooted(relative))throw new IOException("Invalid relative pack path.");
        string prefix=Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar)+Path.DirectorySeparatorChar;
        string result=Path.GetFullPath(Path.Combine(prefix,relative.Replace('/',Path.DirectorySeparatorChar)));
        if(!result.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))throw new IOException("A file path escaped its destination folder.");
        return result;
    }
    static Dictionary<string,byte[]> ReadPayload()
    {
        var files=new Dictionary<string,byte[]>(StringComparer.Ordinal);
        using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("FamousCallers.Payload"))
        using(var zip=new ZipArchive(stream,ZipArchiveMode.Read)) foreach(var entry in zip.Entries)
        {
            if(entry.Length==0||entry.Length>4194304||files.ContainsKey(entry.FullName))throw new InvalidDataException("Invalid embedded pack entry.");
            using(var read=entry.Open())using(var buffer=new MemoryStream()){read.CopyTo(buffer);files.Add(entry.FullName,buffer.ToArray());}
        }
        return files;
    }
    static List<Dictionary<string,object>> ValidatePayload(Dictionary<string,byte[]> payload)
    {
        byte[] bytes;if(!payload.TryGetValue("callers.json",out bytes))throw new InvalidDataException("Caller catalog missing.");
        var callers=Sequence(Json.DeserializeObject(Encoding.UTF8.GetString(bytes))).Select(v=>v as Dictionary<string,object>).ToList();
        if(callers.Count!=20||callers.Any(c=>c==null))throw new InvalidDataException("The pack must contain exactly 20 callers.");
        var expected=new HashSet<string>(StringComparer.Ordinal){"callers.json"};var ids=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach(var caller in callers)
        {
            string id=Text(caller,"id"),portrait=Text(caller,"webcamImage"),voice=Text(caller,"voiceId");
            if(id==null||!Regex.IsMatch(id,@"\Afamous_callers_[a-z0-9_]+\z")||!ids.Add(id)||portrait!="FamousCallers/"+id.Substring("famous_callers_".Length)+".png"||string.IsNullOrWhiteSpace(Text(caller,"displayName"))||string.IsNullOrWhiteSpace(Text(caller,"personalityPrompt"))||voice==null||!voice.StartsWith("kolkata-voices/Connor/",StringComparison.Ordinal)||voice.Contains("..")||Path.IsPathRooted(voice))throw new InvalidDataException("Invalid caller metadata.");
            int trust=Convert.ToInt32(caller["initialTrustPercent"]);
            if(trust<0||trust>100||!new[]{"NEUTRAL","TRUSTING","SUSPICIOUS","ANGRY"}.Contains(Text(caller,"initialEmotion"))||Text(caller,"callType")!="victim"||Sequence(caller["firstMessages"]).Count()<3)throw new InvalidDataException("Invalid initial caller state.");
            string name="portraits/"+Path.GetFileName(portrait);expected.Add(name);
            byte[] image;if(!payload.TryGetValue(name,out image)||image.Length<33||!new byte[]{137,80,78,71,13,10,26,10}.SequenceEqual(image.Take(8))||image[16]!=0||image[17]!=0||image[18]!=2||image[19]!=0||image[20]!=0||image[21]!=0||image[22]!=2||image[23]!=0)throw new InvalidDataException("Invalid 512-pixel PNG portrait.");
        }
        if(!expected.SetEquals(payload.Keys))throw new InvalidDataException("Unexpected file in embedded pack.");
        return callers;
    }
    static Dictionary<string,byte[]> InstallFiles(Dictionary<string,byte[]> payload)
    {
        var files=new Dictionary<string,byte[]>(StringComparer.Ordinal){{CatalogRelative,payload["callers.json"]}};
        foreach(var entry in payload.Where(p=>p.Key.StartsWith("portraits/",StringComparison.Ordinal)))files.Add("Callers/FamousCallers/"+Path.GetFileName(entry.Key),entry.Value);
        return files;
    }
    static void ValidateOwned(Dictionary<string,byte[]> files,Dictionary<string,object> recorded,bool uninstall)
    {
        if(uninstall&&recorded==null)throw new IOException("No Famous Callers installation record was found. No files were removed.");
        foreach(var file in files)
        {
            string target=Within(Streaming,file.Key);if(!File.Exists(target))continue;
            string hash=Hash(File.ReadAllBytes(target));object old;
            bool ours=recorded!=null&&recorded.TryGetValue(file.Key,out old)&&old as string==hash;
            if(uninstall?!ours:!ours&&hash!=Hash(file.Value))throw new IOException("An existing pack file was modified or belongs to another install: "+file.Key+". Back it up before retrying.");
        }
    }
    static void Change(string path,byte[] value,Dictionary<string,byte[]> prior,List<string> created)
    {
        if(!prior.ContainsKey(path))prior.Add(path,File.Exists(path)?File.ReadAllBytes(path):null);
        if(value==null){if(File.Exists(path))File.Delete(path);return;}
        string parent=Path.GetDirectoryName(path);var missing=new Stack<string>();for(string p=parent;!Directory.Exists(p);p=Path.GetDirectoryName(p))missing.Push(p);
        while(missing.Count>0){string p=missing.Pop();Directory.CreateDirectory(p);created.Add(p);}
        AtomicWrite(path,value);
    }
    static void AtomicWrite(string path,byte[] value)
    {
        string temporary=path+"."+Guid.NewGuid().ToString("N")+".tmp";
        try{File.WriteAllBytes(temporary,value);if(File.Exists(path))File.Replace(temporary,path,null);else File.Move(temporary,path);}
        finally{if(File.Exists(temporary))File.Delete(temporary);}
    }
    static bool GameRunning()
    {
        foreach(var process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(GameExe)))using(process)
        {try{if(string.Equals(process.MainModule.FileName,Path.Combine(game,GameExe),StringComparison.OrdinalIgnoreCase))return true;}catch(System.ComponentModel.Win32Exception){return true;}catch(InvalidOperationException){}}
        return false;
    }
    static bool IsGame(string path){return File.Exists(Path.Combine(path,GameExe))&&Directory.Exists(Path.Combine(path,DataRelative,"StreamingAssets","kolkata-json","caller-identities"));}
    static string ChooseGame(string explicitPath)
    {
        if(!string.IsNullOrWhiteSpace(explicitPath)){string path=Path.GetFullPath(explicitPath.Trim().Trim('"'));if(!IsGame(path))throw new DirectoryNotFoundException("That folder is not a compatible game installation.");return path;}
        var games=DiscoverGames().Distinct(StringComparer.OrdinalIgnoreCase).ToArray();if(games.Length==1)return games[0];
        if(Console.IsInputRedirected)throw new DirectoryNotFoundException("Use --game-path to select your game installation.");
        Console.WriteLine(games.Length==0?"Paste your game folder path:":"Multiple installations found. Paste the folder you want:");return ChooseGame(Console.ReadLine());
    }
    static IEnumerable<string> DiscoverGames()
    {
        var roots=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach(var location in new[]{new[]{@"HKEY_CURRENT_USER\Software\Valve\Steam","SteamPath"},new[]{@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam","InstallPath"}})
        {try{var root=Registry.GetValue(location[0],location[1],null) as string;if(!string.IsNullOrWhiteSpace(root))roots.Add(root);}catch(System.Security.SecurityException){}}
        roots.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),"Steam"));
        var libraries=new HashSet<string>(roots,StringComparer.OrdinalIgnoreCase);
        foreach(string root in roots){string vdf=Path.Combine(root,"steamapps","libraryfolders.vdf");if(File.Exists(vdf))foreach(Match match in Regex.Matches(File.ReadAllText(vdf),"\"path\"\\s*\"((?:\\\\.|[^\"])*)\""))libraries.Add(match.Groups[1].Value.Replace("\\\\","\\").Replace("\\\"","\""));}
        foreach(string library in libraries){string candidate=Path.Combine(library,"steamapps","common","Scam With Your Friends");if(IsGame(candidate))yield return Path.GetFullPath(candidate);}
    }
    static void Instructions()
    {
        Console.WriteLine("\nHOW TO PLAY\n1. Reopen the game and host a lobby.\n2. These callers are part of the normal caller pool and the daily lineup.\n3. Install the same pack version on every player's PC for multiplayer.\n4. Keep using your usual AI provider, including the separate Ollama mod.\n\nPortraits and personalities are fan-made. Speech uses existing game voices;\nthese are not actor voice impressions. No game files or voice samples are bundled.\n\nRemove the pack later: setup.exe --uninstall\nA game update may remove the pack; rerun setup if needed.");
    }
}
