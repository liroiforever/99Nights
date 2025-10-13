// Place this file into Assets/Editor/ScriptAnalyzerWindow.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Linq;

public class ScriptAnalyzerWindow : EditorWindow
{
    [MenuItem("Window/Script Analyzer")]
    public static void ShowWindow()
    {
        var w = GetWindow<ScriptAnalyzerWindow>("Script Analyzer");
        w.minSize = new Vector2(600, 400);
    }

    Vector2 scroll;
    string outputFolder = "Assets/ScriptAnalysis";
    List<ScriptInfo> lastResults = null;
    string status = "Готов.";

    void OnGUI()
    {
        GUILayout.Label("Script Analyzer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Сканирует все .cs файлы в Assets и создает report.json и report.md в Assets/ScriptAnalysis", MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Output folder:", GUILayout.Width(90));
        outputFolder = EditorGUILayout.TextField(outputFolder);
        if (GUILayout.Button("Use default", GUILayout.Width(100)))
            outputFolder = "Assets/ScriptAnalysis";
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(6);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Scan entire project", GUILayout.Height(30)))
            RunScan(Application.dataPath);
        if (GUILayout.Button("Scan selected folder/file", GUILayout.Height(30)))
            RunScanForSelection();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(8);

        EditorGUILayout.BeginHorizontal();
        if (lastResults != null)
        {
            if (GUILayout.Button("Open JSON report", GUILayout.Height(24)))
                EditorUtility.RevealInFinder(Path.Combine(outputFolder, "report.json"));
            if (GUILayout.Button("Open Markdown report", GUILayout.Height(24)))
                EditorUtility.RevealInFinder(Path.Combine(outputFolder, "report.md"));
        }
        else
        {
            GUI.enabled = false;
            GUILayout.Button("Open JSON report", GUILayout.Height(24));
            GUILayout.Button("Open Markdown report", GUILayout.Height(24));
            GUI.enabled = true;
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(8);

        GUILayout.Label("Status: " + status, EditorStyles.helpBox);

        GUILayout.Space(8);
        GUILayout.Label("Последние результаты (кратко):", EditorStyles.boldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        if (lastResults == null)
        {
            GUILayout.Label("Ничего не просканировано ещё.");
        }
        else
        {
            GUILayout.Label($"Файлов: {lastResults.Count}");
            foreach (var s in lastResults.Take(200))
            {
                EditorGUILayout.BeginVertical("box");
                GUILayout.Label(Path.GetFileName(s.path) + $"  — {s.typeHint}  ({s.lines} lines, {s.publicFields.Count} public fields, {s.methodsCount} methods)");
                GUILayout.Label(s.summary, EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndScrollView();
    }

    void RunScanForSelection()
    {
        string path = null;
        if (Selection.activeObject != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(assetPath))
            {
                path = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length), assetPath);
                if (Directory.Exists(path) || File.Exists(path))
                {
                    RunScan(path);
                    return;
                }
            }
        }
        EditorUtility.DisplayDialog("Script Analyzer", "Выберите папку или файл в Project view, затем нажмите кнопку снова.", "OK");
    }

    void RunScan(string rootPath)
    {
        try
        {
            status = "Scanning...";
            lastResults = new List<ScriptInfo>();

            var files = new List<string>();
            if (File.Exists(rootPath) && Path.GetExtension(rootPath).ToLower() == ".cs")
            {
                files.Add(rootPath);
            }
            else if (Directory.Exists(rootPath))
            {
                files.AddRange(Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories));
            }
            else
            {
                status = "Неверный путь.";
                return;
            }

            int i = 0;
            foreach (var file in files)
            {
                i++;
                status = $"Parsing {i}/{files.Count}: {Path.GetFileName(file)}";
                var info = ParseScriptFile(file);
                lastResults.Add(info);
            }

            // Ensure output folder
            if (!AssetDatabase.IsValidFolder(outputFolder))
            {
                string[] parts = outputFolder.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                string pathAccum = parts[0];
                for (int p = 1; p < parts.Length; p++)
                {
                    string next = string.Join("/", parts.Take(p + 1).ToArray());
                    if (!AssetDatabase.IsValidFolder(next))
                    {
                        AssetDatabase.CreateFolder(pathAccum, parts[p]);
                    }
                    pathAccum = next;
                }
            }

            // Write JSON
            var container = new ScriptInfoContainer() { generatedAt = DateTime.Now.ToString("s"), items = lastResults.ToArray() };
            string json = JsonHelper.ToJson(container, true);
            string jsonPath = Path.Combine(outputFolder, "report.json");
            File.WriteAllText(jsonPath, json);
            Debug.Log($"Script Analyzer: Wrote JSON to {jsonPath}");

            // Write Markdown
            string md = BuildMarkdownReport(container);
            string mdPath = Path.Combine(outputFolder, "report.md");
            File.WriteAllText(mdPath, md);
            Debug.Log($"Script Analyzer: Wrote Markdown to {mdPath}");

            AssetDatabase.Refresh();
            status = $"Сканирование завершено. {lastResults.Count} файлов. Отчёты в {outputFolder}";
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            status = "Ошибка при сканировании: " + ex.Message;
        }
    }

    // -------------------------
    // Parsing logic (best-effort)
    // -------------------------
    ScriptInfo ParseScriptFile(string file)
    {
        var txt = File.ReadAllText(file);
        var lines = txt.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        var info = new ScriptInfo();
        info.path = file;
        info.fileName = Path.GetFileName(file);
        info.sizeBytes = new FileInfo(file).Length;
        info.lines = lines.Length;

        // using directives
        var usingMatches = Regex.Matches(txt, @"using\s+([A-Za-z0-9_.]+)\s*;");
        foreach (Match m in usingMatches) info.usings.Add(m.Groups[1].Value);

        // namespace
        var nsMatch = Regex.Match(txt, @"namespace\s+([A-Za-z0-9_.]+)");
        if (nsMatch.Success) info.@namespace = nsMatch.Groups[1].Value;

        // classes/interfaces/structs
        var classPattern = @"(?:(public|private|internal|protected|sealed|static|partial)\s+)*\b(class|struct|interface)\s+([A-Za-z0-9_<>]+)(\s*:\s*([A-Za-z0-9_<>,\s\.]+))?";
        var classMatches = Regex.Matches(txt, classPattern);
        foreach (Match m in classMatches)
        {
            var si = new ScriptClassInfo();
            si.visibility = m.Groups[1].Success ? m.Groups[1].Value : "internal";
            si.kind = m.Groups[2].Value;
            si.name = m.Groups[3].Value;
            if (m.Groups[5].Success)
            {
                si.baseTypes = m.Groups[5].Value.Split(',').Select(s => s.Trim()).ToList();
            }
            info.classes.Add(si);
        }

        // fields (public) and [SerializeField]
        var fieldPattern = @"(?:(public|private|protected|internal)\s+)?(static\s+)?([A-Za-z0-9_<>,\.\[\]\s]+)\s+([A-Za-z0-9_]+)\s*(=.+?)?;";
        var fieldMatches = Regex.Matches(txt, fieldPattern);
        for (int i = 0; i < fieldMatches.Count; i++)
        {
            var m = fieldMatches[i];
            string visibility = m.Groups[1].Success ? m.Groups[1].Value : "private";
            string type = m.Groups[3].Value.Trim();
            string name = m.Groups[4].Value.Trim();
            bool isPublic = visibility == "public";
            bool hasSerialize = false;

            // look back a few lines for [SerializeField]
            int lineIndex = FindLineIndexOfMatch(lines, m.Value);
            int searchFrom = Math.Max(0, lineIndex - 3);
            for (int j = searchFrom; j < lineIndex; j++)
            {
                if (lines[j].Contains("[SerializeField]")) { hasSerialize = true; break; }
            }

            var fi = new ScriptFieldInfo() { name = name, type = type, visibility = visibility, isSerialized = hasSerialize };
            if (isPublic) info.publicFields.Add(fi);
            if (hasSerialize && !isPublic) info.serializedPrivateFields.Add(fi);
            info.allFields.Add(fi);
        }

        // properties (auto)
        var propPattern = @"(?:public|protected|private|internal)\s+([A-Za-z0-9_<>,\.\[\]\s]+)\s+([A-Za-z0-9_]+)\s*\{\s*(get;|get; set;|set; get;|set;)\s*\}";
        var propMatches = Regex.Matches(txt, propPattern);
        info.propertiesCount = propMatches.Count;

        // methods (crude)
        var methodPattern = @"(?:public|private|protected|internal|static|virtual|override|\s)+\s*([A-Za-z0-9_<>,\.\[\]\s]+)\s+([A-Za-z0-9_]+)\s*\(([^\)]*)\)\s*\{";
        var methodMatches = Regex.Matches(txt, methodPattern);
        info.methodsCount = methodMatches.Count;

        // common Unity methods
        var unityMethods = new[] { "Start", "Update", "FixedUpdate", "LateUpdate", "OnEnable", "OnDisable", "Awake", "OnDestroy", "OnValidate", "Reset", "OnTriggerEnter", "OnCollisionEnter" };
        foreach (var um in unityMethods)
            if (Regex.IsMatch(txt, @"\b" + um + @"\s*\(")) info.unityMethods.Add(um);

        // TODO/FIXME comments
        var todoMatches = Regex.Matches(txt, @"//\s*(TODO|FIXME|NOTE)\s*:?\s*(.*)");
        foreach (Match m in todoMatches)
        {
            info.todos.Add(new TodoItem() { tag = m.Groups[1].Value, text = m.Groups[2].Value.Trim() });
        }

        // type hint (if any class inherits MonoBehaviour etc)
        if (info.classes.Any(c => c.baseTypes.Any(bt => bt.Contains("MonoBehaviour"))))
            info.typeHint = "MonoBehaviour";
        else if (info.classes.Any(c => c.baseTypes.Any(bt => bt.Contains("ScriptableObject"))))
            info.typeHint = "ScriptableObject";
        else if (txt.Contains("UnityEditor") || file.ToLower().Contains("/editor/"))
            info.typeHint = "Editor";
        else
            info.typeHint = "General";

        // small summary (first comment block or first non-empty line)
        string summary = "";
        foreach (var l in lines)
        {
            var t = l.Trim();
            if (t.StartsWith("//"))
            {
                summary = t.TrimStart('/').Trim();
                break;
            }
            if (!string.IsNullOrWhiteSpace(t))
            {
                summary = t.Length > 120 ? t.Substring(0, 120) + "..." : t;
                break;
            }
        }
        info.summary = summary;

        // crude dependency detection: look for other class names mentioned (from class list in same project is not available here)
        var wordMatches = Regex.Matches(txt, @"\b([A-Z][A-Za-z0-9_]+)\b");
        var distinctWords = new HashSet<string>();
        foreach (Match m in wordMatches) distinctWords.Add(m.Groups[1].Value);
        info.possibleTypeReferences = distinctWords.Take(40).ToArray();

        return info;
    }

    int FindLineIndexOfMatch(string[] lines, string matchSnippet)
    {
        for (int i = 0; i < lines.Length; i++)
            if (lines[i].Contains(matchSnippet.Trim())) return i;
        return 0;
    }

    string BuildMarkdownReport(ScriptInfoContainer container)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# Script Analysis Report");
        sb.AppendLine();
        sb.AppendLine($"Generated: {container.generatedAt}");
        sb.AppendLine();
        sb.AppendLine($"Total scripts: {container.items.Length}");
        sb.AppendLine();
        sb.AppendLine("## Summary by type");
        var groups = container.items.GroupBy(x => x.typeHint).OrderByDescending(g => g.Count());
        foreach (var g in groups)
            sb.AppendLine($"- **{g.Key}**: {g.Count()}");

        sb.AppendLine();
        sb.AppendLine("## Files (detailed)");
        foreach (var s in container.items.OrderBy(x => x.fileName))
        {
            sb.AppendLine($"### {s.fileName}");
            sb.AppendLine($"- Path: `{s.path}`");
            sb.AppendLine($"- Lines: {s.lines}  — Size: {s.sizeBytes} bytes");
            sb.AppendLine($"- Namespace: {s.@namespace}");
            sb.AppendLine($"- Type hint: {s.typeHint}");
            if (s.classes.Any())
            {
                sb.AppendLine($"- Classes:");
                foreach (var c in s.classes)
                    sb.AppendLine($"  - `{c.name}` ({c.kind}) inherits: {(c.baseTypes.Count > 0 ? string.Join(", ", c.baseTypes) : "—")}");
            }
            sb.AppendLine($"- Public fields: {s.publicFields.Count}");
            if (s.publicFields.Count > 0)
            {
                foreach (var f in s.publicFields.Take(10))
                    sb.AppendLine($"  - `{f.type}` `{f.name}`");
            }
            if (s.serializedPrivateFields.Count > 0)
            {
                sb.AppendLine($"- [SerializeField] private fields:");
                foreach (var f in s.serializedPrivateFields.Take(10))
                    sb.AppendLine($"  - `{f.type}` `{f.name}`");
            }
            sb.AppendLine($"- Methods (approx): {s.methodsCount}, Properties: {s.propertiesCount}");
            if (s.unityMethods.Count > 0) sb.AppendLine($"- Unity methods: {string.Join(", ", s.unityMethods)}");
            if (s.todos.Count > 0)
            {
                sb.AppendLine($"- TODOs:");
                foreach (var t in s.todos) sb.AppendLine($"  - [{t.tag}] {t.text}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    // -------------------------
    // Data classes
    // -------------------------
    [Serializable]
    public class ScriptInfoContainer
    {
        public string generatedAt;
        public ScriptInfo[] items;
    }

    [Serializable]
    public class ScriptInfo
    {
        public string path;
        public string fileName;
        public long sizeBytes;
        public int lines;
        public string @namespace;
        public List<string> usings = new List<string>();
        public List<ScriptClassInfo> classes = new List<ScriptClassInfo>();
        public List<ScriptFieldInfo> allFields = new List<ScriptFieldInfo>();
        public List<ScriptFieldInfo> publicFields = new List<ScriptFieldInfo>();
        public List<ScriptFieldInfo> serializedPrivateFields = new List<ScriptFieldInfo>();
        public int propertiesCount;
        public int methodsCount;
        public List<string> unityMethods = new List<string>();
        public List<TodoItem> todos = new List<TodoItem>();
        public string typeHint;
        public string summary;
        public string[] possibleTypeReferences;
    }

    [Serializable]
    public class ScriptClassInfo
    {
        public string visibility;
        public string kind;
        public string name;
        public List<string> baseTypes = new List<string>();
    }

    [Serializable]
    public class ScriptFieldInfo
    {
        public string name;
        public string type;
        public string visibility;
        public bool isSerialized;
    }

    [Serializable]
    public class TodoItem
    {
        public string tag;
        public string text;
    }

    // -------------------------
    // JSON helper for arrays
    // -------------------------
    public static class JsonHelper
    {
        public static T FromJson<T>(string json) { return JsonUtility.FromJson<T>(json); }
        public static string ToJson(object obj, bool pretty = false)
        {
            return pretty ? JsonUtility.ToJson(obj, true) : JsonUtility.ToJson(obj);
        }
    }
}
