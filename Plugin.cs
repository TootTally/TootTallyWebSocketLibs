using BaboonAPI.Hooks.Initializer;
using BepInEx;

namespace TootTallyWebsocketLibs
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public static void LogInfo(string message) => Instance.Logger.LogInfo(message);
        public static void LogError(string message) => Instance.Logger.LogError(message);
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;

            GameInitializationEvent.Register(Info, delegate { });
        }
    }
}