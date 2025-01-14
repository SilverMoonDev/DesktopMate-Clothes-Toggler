using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace Mesh_Toggler.Patches
{
    internal class PatchManager
    {
        public static HarmonyLib.Harmony Instance = new HarmonyLib.Harmony("MeshToggler");

        public static bool defaultAvatar = true;
        private static readonly string DefaultAvatarsPath = "/MenuCanvas/MenuParent/ModelPage/Scroll View/Viewport/Content";

        private static HarmonyMethod GetPatch(string name)
        {
            return new HarmonyMethod(typeof(PatchManager).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));
        }

        public static unsafe void InitPatch()
        {
            try
            {
                var loadCharacterMethod = typeof(MelonLoaderMod1.Core).GetMethod("LoadCharacter", BindingFlags.Instance | BindingFlags.Public);

                if (loadCharacterMethod != null) {
                    Instance.Patch(loadCharacterMethod, GetPatch("OnCustomAvatarLoading"), null);
                }
                Instance.Patch(typeof(Button).GetMethod("Press"), GetPatch("OnButtonClicked"), null);
            }
            catch (Exception arg)
            {
                MelonLogger.Msg(string.Format("Failed Patching\n{0}", arg, ConsoleColor.Green));
            }
        }


        private static void OnCustomAvatarLoading()
        {
            defaultAvatar = false;
        }


        private static void OnButtonClicked(Button __instance)
        {
            var path = GetPath(__instance.transform.parent.gameObject);
            if (path == DefaultAvatarsPath)
            {
                if (__instance.interactable)
                    defaultAvatar = true;
            }
        }

        private static string GetPath(GameObject gameObject)
        {
            var path = "/" + gameObject.name;
            while (gameObject.transform.parent != null)
            {
                gameObject = gameObject.transform.parent.gameObject;
                path = "/" + gameObject.name + path;
            }
            return path;
        }
    }
}
