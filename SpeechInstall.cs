using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

internal static partial class Setup
{
    const string OriginalMain = "99791cf93ec829d4784ee514ab78eba19e5e4a7ea841b291d979432a5c911d8f";
    const string OriginalScripts = "4cf9f54e3760c2392952ef17f981e01c06fb483c655f9953ba9f6386889f0b2e";

    static Assembly ResolveTool(object sender, ResolveEventArgs args)
    {
        string name=new AssemblyName(args.Name).Name;
        if(name!="Mono.Cecil"&&name!="Mono.Cecil.Rocks")return null;
        using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("FamousCallers."+name))
        using(var memory=new MemoryStream()){if(stream==null)return null;stream.CopyTo(memory);return Assembly.Load(memory.ToArray());}
    }

    static bool EqualHash(string a,string b){return a!=null&&b!=null&&string.Equals(a,b,StringComparison.OrdinalIgnoreCase);}
    static string Nested(Dictionary<string,object> map,string group,string key)
    {
        object value;var nested=map!=null&&map.TryGetValue(group,out value)?value as Dictionary<string,object>:null;
        return nested==null?null:Text(nested,key);
    }
    static bool SupportedBase(byte[] main,string managed)
    {
        string mainHash=Hash(main);
        string scripts=Within(managed,"ScriptsAssDef.dll");
        if(EqualHash(mainHash,OriginalMain)&&File.Exists(scripts)&&EqualHash(Hash(File.ReadAllBytes(scripts)),OriginalScripts))return true;
        // An optional, separately installed Ollama mod is preserved byte for byte.
        var other=ReadMap(Within(game,"ScamOllama/install.json"));
        string plugin=Within(managed,"ScamOllama.dll");
        return other!=null&&(Text(other,"version")=="1.0.3"||Text(other,"version")=="1.0.4")&&
            EqualHash(Nested(other,"originalHashes","Assembly-CSharp.dll"),OriginalMain)&&
            EqualHash(Nested(other,"originalHashes","ScriptsAssDef.dll"),OriginalScripts)&&
            EqualHash(mainHash,Nested(other,"patchedHashes","Assembly-CSharp.dll"))&&
            File.Exists(scripts)&&EqualHash(Hash(File.ReadAllBytes(scripts)),Nested(other,"patchedHashes","ScriptsAssDef.dll"))&&
            File.Exists(plugin)&&EqualHash(Hash(File.ReadAllBytes(plugin)),Text(other,"pluginHash"));
    }

    static Dictionary<string,byte[]> PrepareSpeech(Dictionary<string,byte[]> payload,bool uninstall)
    {
        string managed=Within(game,DataRelative+"/Managed");
        string main=Within(managed,"Assembly-CSharp.dll"),runtime=Within(managed,"FamousCallers.dll");
        string recordPath=Within(ModRoot,"speech-install.json"),backup=Within(ModRoot,"speech-before-install.dll");
        var record=ReadMap(recordPath);
        var changes=new Dictionary<string,byte[]>(StringComparer.OrdinalIgnoreCase);
        if(!File.Exists(main))throw new IOException("The game's speech assembly is missing. Verify game files in Steam first.");
        byte[] current=File.ReadAllBytes(main),baseBytes=current;
        bool active=record!=null&&EqualHash(Hash(current),Text(record,"patchedHash"));
        if(File.Exists(runtime)&&!(record!=null&&EqualHash(Hash(File.ReadAllBytes(runtime)),Text(record,"runtimeHash"))))
            throw new IOException("FamousCallers.dll was modified or is not owned by this installer. It was left untouched.");
        if(active)
        {
            if(!File.Exists(backup)||!EqualHash(Hash(File.ReadAllBytes(backup)),Text(record,"baseHash")))
                throw new IOException("The speech backup is missing or modified. No game files were changed.");
            baseBytes=File.ReadAllBytes(backup);
        }
        if(!SupportedBase(baseBytes,managed))
            throw new IOException("Unsupported game build or another modification to its speech assembly. This installer supports game 0.1.74-swyf. No files were changed.");
        if(uninstall)
        {
            if(record==null)return changes; // Upgrading/removing an older data-only pack.
            if(active)changes.Add(main,baseBytes);
            changes.Add(runtime,null);changes.Add(recordPath,null);changes.Add(backup,null);
            return changes;
        }
        byte[] hook=payload["runtime/FamousCallers.dll"];
        byte[] patched=SpeechPatch.Build(baseBytes,hook,managed);
        changes.Add(backup,baseBytes);
        changes.Add(runtime,hook);
        changes.Add(main,patched);
        changes.Add(recordPath,Encoding.UTF8.GetBytes(Json.Serialize(new Dictionary<string,object>{
            {"version",Version},{"baseHash",Hash(baseBytes)},{"patchedHash",Hash(patched)},{"runtimeHash",Hash(hook)}
        })));
        return changes;
    }
}
