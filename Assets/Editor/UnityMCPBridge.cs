using UnityEngine;
using UnityEditor;
using System.Net;
using System.Text;
using System.Threading;
using System;
using System.IO;

[InitializeOnLoad]
public static class UnityMCPBridge
{
    private static HttpListener listener;
    private static Thread listenerThread;
    private const string URI = "http://localhost:8080/unity-agent/";

    static UnityMCPBridge()
    {
        StartServer();
        AssemblyReloadEvents.beforeAssemblyReload += StopServer;
    }

    private static void StartServer()
    {
        try
        {
            listener = new HttpListener();
            listener.Prefixes.Add(URI);
            listener.Start();
            listenerThread = new Thread(ListenLoop) { IsBackground = true };
            listenerThread.Start();
            Debug.Log($"[Unity-MCP] Bridge đang chạy tại: {URI}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Unity-MCP] Khởi động thất bại: {ex.Message}");
        }
    }

    private static void StopServer()
    {
        if (listener != null && listener.IsListening)
        {
            listener.Stop();
            listener.Close();
        }
        if (listenerThread != null && listenerThread.IsAlive)
            listenerThread.Abort();
    }

    private static void ListenLoop()
    {
        while (listener != null && listener.IsListening)
        {
            try
            {
                var ctx = listener.GetContext();
                using var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding);
                string jsonPayload = reader.ReadToEnd();

                // Đẩy vào Main Thread của Unity Editor
                EditorApplication.delayCall += () => ProcessCommand(jsonPayload);

                byte[] res = Encoding.UTF8.GetBytes("{\"status\":\"received\"}");
                ctx.Response.ContentType = "application/json";
                ctx.Response.OutputStream.Write(res, 0, res.Length);
                ctx.Response.Close();
            }
            catch { }
        }
    }

    [Serializable]
    public class CommandData
    {
        public string action;
        public string targetName;
        public string componentName;
    }

    private static void ProcessCommand(string json)
    {
        try
        {
            CommandData cmd = JsonUtility.FromJson<CommandData>(json);
            switch (cmd.action)
            {
                case "create_gameobject":
                    GameObject go = new GameObject(cmd.targetName);
                    Undo.RegisterCreatedObjectUndo(go, "Agent Created GameObject");
                    Debug.Log($"[Unity-MCP] Đã tạo GameObject: {cmd.targetName}");
                    break;

                case "add_component":
                    GameObject target = GameObject.Find(cmd.targetName);
                    if (target != null)
                    {
                        var type = Type.GetType(cmd.componentName) ?? Type.GetType(cmd.componentName + ", Assembly-CSharp");
                        if (type != null)
                        {
                            target.AddComponent(type);
                            Debug.Log($"[Unity-MCP] Đã gắn {cmd.componentName} vào {cmd.targetName}");
                        }
                    }
                    break;

                case "refresh_assets":
                    AssetDatabase.Refresh();
                    Debug.Log("[Unity-MCP] Đã recompile scripts!");
                    break;

                case "build_memory_game":
                    var builderType = System.Type.GetType("MemoryGameEditor.MemoryGameBuilder, Assembly-CSharp-Editor");
                    if (builderType != null)
                    {
                        builderType.GetMethod("BuildScene")?.Invoke(null, null);
                        Debug.Log("[Unity-MCP] Đã dựng Memory Game Scene thành công!");
                    }
                    else
                    {
                        Debug.LogError("[Unity-MCP] Không tìm thấy class MemoryGameEditor.MemoryGameBuilder");
                    }
                    break;

                case "capture_screenshot":
                    var screenType = System.Type.GetType("MemoryGameEditor.MemoryGameScreenshot, Assembly-CSharp-Editor");
                    if (screenType != null)
                    {
                        screenType.GetMethod("CaptureScreenshot")?.Invoke(null, null);
                        Debug.Log("[Unity-MCP] Đã chụp ảnh màn hình thành công!");
                    }
                    else
                    {
                        Debug.LogError("[Unity-MCP] Không tìm thấy class MemoryGameEditor.MemoryGameScreenshot");
                    }
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Unity-MCP] Lỗi thực thi: {e.Message}");
        }
    }
}