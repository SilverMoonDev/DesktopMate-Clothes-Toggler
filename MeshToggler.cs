using MelonLoader;
using UnityEngine;

namespace Mesh_Toggler
{
    public class MeshToggler : MelonMod
    {
        private int characterID;
        private Transform rootTransform;
        private GameObject character;
        private bool isUIPanelVisible = false;
        private SkinnedMeshRenderer[] meshRenderers;

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

            GUILayout.BeginArea(new Rect(300, 500, 220, 400), "Mesh Toggler", GUI.skin.window);

            foreach (var renderer in meshRenderers)
            {
                bool isVisible = renderer.gameObject.activeSelf;
                Color buttonColor = isVisible ? Color.green : Color.red;

                GUI.skin.button.normal.textColor = buttonColor;
                GUI.skin.button.hover.textColor = buttonColor;

                if (GUILayout.Button($"Toggle {renderer.gameObject.name}"))
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
            }
        }
    }
}
