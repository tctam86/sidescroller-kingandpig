#if UNITY_EDITOR
using System;
using MCPForUnity.Editor.Services;
using MCPForUnity.Editor.Services.Transport;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal static class UnityMcpBootstrap
{
    private const string SessionKey = "ProjectWoods.UnityMcpBootstrap.Started";
    private const string AutoStartKey = "MCPForUnity.AutoStartOnLoad";
    private const string HttpTransportKey = "MCPForUnity.UseHttpTransport";

    static UnityMcpBootstrap()
    {
        EditorPrefs.SetBool(AutoStartKey, true);
        EditorPrefs.SetBool(HttpTransportKey, true);

        if (SessionState.GetBool(SessionKey, false))
        {
            return;
        }

        SessionState.SetBool(SessionKey, true);
        EditorApplication.delayCall += StartBridge;
    }

    private static async void StartBridge()
    {
        try
        {
            if (MCPServiceLocator.TransportManager.IsRunning(TransportMode.Http))
            {
                return;
            }

            bool connected = await MCPServiceLocator.Bridge.StartAsync();
            Debug.Log(connected
                ? "[Unity MCP] Project_woods bridge connected."
                : "[Unity MCP] Bridge connection failed; open Window > MCP for Unity to inspect status.");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}
#endif
