using System.IO;
using System.Text;
using UnityEngine;
using XLua;
namespace TaoTie
{
    public class XLuaManager:IManager, IUpdateManager
    {
        const string gameMainScriptName = "GameMain";
        public static XLuaManager Instance { get; private set; }
        private LuaEnv luaEnv;
        #region override

        public void Init()
        {
            Instance = this;
            luaEnv = new LuaEnv();
            luaEnv.AddLoader(CustomLoader);
            LoadScript(gameMainScriptName);
            SafeDoString("GameMain.Start()");
        }

        public void Destroy()
        {
            Instance = null;
            luaEnv.Dispose();
            luaEnv = null;
        }
        public void Update()
        {
            if (luaEnv != null)
            {
                luaEnv.Tick();

                if (Time.frameCount % 1000 == 0)
                {
                    luaEnv.FullGc();
                }
            }
        }
        #endregion
        
        private static byte[] CustomLoader(ref string filepath)
        {
            StringBuilder scriptPath = new StringBuilder();
            scriptPath.Append("LuaScript/");
            scriptPath.Append(filepath.Replace(".", "/")).Append(".lua");

            var luaAddress = scriptPath.Append(".bytes").ToString();
            var asset = ResourcesManager.Instance.Load<TextAsset>(luaAddress);
            if (asset != null)
            {
                return asset.bytes;
            }
            return null;
        }
        
        public void SafeDoString(string scriptContent)
        {
            if (luaEnv != null)
            {
                try
                {
                    luaEnv.DoString(scriptContent);
                }
                catch (System.Exception ex)
                {
                    string msg = string.Format("xLua exception : {0}\n {1}", ex.Message, ex.StackTrace);
                    Log.Error(msg);
                }
            }
        }
        
        public void ReloadScript(string scriptName)
        {
            SafeDoString(string.Format("package.loaded['{0}'] = nil", scriptName));
            LoadScript(scriptName);
        }

        void LoadScript(string scriptName)
        {
            SafeDoString(string.Format("require('{0}')", scriptName));
        }
        
        public LuaFunction GetGlobalFunc(string funcName)
        {
            if (luaEnv != null)
            {
                try
                {
                    LuaTable tabGameUser = luaEnv.Global.Get<LuaTable>("GameMain");
                    var res = tabGameUser.Get<LuaFunction>(funcName);
                    return res;
                }
                catch (System.Exception ex)
                {
                    string msg = string.Format("xLua exception : {0}\n {1}", ex.Message, ex.StackTrace);
                    Log.Error(msg);
                }
            }
            return null;
        }
        
    }
}