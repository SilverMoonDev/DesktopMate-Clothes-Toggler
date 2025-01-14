using MelonLoader;
using UnityEngine;
using System.IO;
using System.Linq;
using MelonLoader.Utils;
using Mesh_Toggler.Patches;

namespace Mesh_Toggler
{
    public class MeshToggler : MelonMod
    {
        private int characterID;
        private Transform rootTransform;
        private GameObject character;
        private bool isUIPanelVisible = false;
        private SkinnedMeshRenderer[] meshRenderers;
        private string avatarName;

        private string iniFilePath;
        private string configFilePath = Path.Combine(MelonEnvironment.UserDataDirectory, "MelonPreferences.cfg");

        private IniFile iniFile;

        public override void OnInitializeMelon()
        {
            PatchManager.InitPatch();

            iniFilePath = Path.Combine(MelonEnvironment.UserDataDirectory, "MeshToggler.ini");

            iniFile = new IniFile(iniFilePath);

            LoadMeshStates();
        }

        public override void OnLateUpdate()
        {
            if (rootTransform == null || rootTransform.childCount == 0) return;

            var newCharacter = rootTransform.GetChild(0).gameObject;
            if (newCharacter.GetInstanceID() != characterID)
            {
                AvatarChange(newCharacter);
            }
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                isUIPanelVisible = !isUIPanelVisible;
            }
        }

        private void AvatarChange(GameObject avatar)
        {
            if (avatar == null) return;

            MelonLogger.Msg("Avatar Changed");
            characterID = avatar.GetInstanceID();
            character = avatar;
            UpdateMeshRenderers();

            if (PatchManager.defaultAvatar) return;

            avatarName = GetAvatarNameFromConfig();
            LoadMeshStates();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == "Main" && rootTransform == null)
            {
                rootTransform = GameObject.Find("CharactersRoot")?.transform;
            }
        }

        private void UpdateMeshRenderers()
        {
            if (character != null)
            {
                meshRenderers = character.GetComponentsInChildren<SkinnedMeshRenderer>();
            }
        }

        public override void OnGUI()
        {
            if (!isUIPanelVisible || meshRenderers == null || meshRenderers.Length == 0) return;

            int numButtons = meshRenderers.Length;
            float UIHeight = (numButtons * 26f) + 15f;

            GUILayout.BeginArea(new Rect(300, 500, 220, UIHeight), "Mesh Toggler", GUI.skin.window);

            foreach (var renderer in meshRenderers)
            {
                bool isVisible = renderer.gameObject.activeSelf;
                Color buttonColor = isVisible ? Color.green : Color.red;

                GUI.skin.button.normal.textColor = buttonColor;
                GUI.skin.button.hover.textColor = buttonColor;

                if (GUILayout.Button($"{renderer.gameObject.name}"))
                {
                    ToggleMeshVisibility(renderer);
                }
            }

            GUILayout.EndArea();
        }

        private void ToggleMeshVisibility(SkinnedMeshRenderer renderer)
        {
            if (renderer != null)
            {
                renderer.gameObject.SetActive(!renderer.gameObject.activeSelf);
                SaveMeshStates();
            }
        }

        private void SaveMeshStates()
        {
            if (PatchManager.defaultAvatar) return;
            if (meshRenderers == null || meshRenderers.Length == 0) return;

            string states = string.Join(",", meshRenderers.Select(renderer => renderer.gameObject.activeSelf ? "1" : "0"));

            iniFile.SetString(avatarName, "MeshStates", states);
        }

        private void LoadMeshStates()
        {
            if (meshRenderers == null || meshRenderers.Length == 0 || avatarName == null) return;

            string savedStates = iniFile.GetString(avatarName, "MeshStates", string.Empty);

            if (string.IsNullOrEmpty(savedStates)) return;

            string[] states = savedStates.Split(',');

            for (int i = 0; i < meshRenderers.Length && i < states.Length; i++)
            {
                meshRenderers[i].gameObject.SetActive(states[i] == "1");
            }
        }

        private string GetAvatarNameFromConfig()
        {
            if (!File.Exists(configFilePath))
            {
                MelonLogger.Error("Config file not found!");
                return string.Empty;
            }

            var lines = File.ReadAllLines(configFilePath);
            foreach (var line in lines)
            {
                if (line.StartsWith("vrmPath"))
                {
                    string vrmPath = line.Split('=')[1].Trim().Trim('\'');

                    return Path.GetFileNameWithoutExtension(vrmPath);
                }
            }

            return string.Empty;
        }
    }
}
