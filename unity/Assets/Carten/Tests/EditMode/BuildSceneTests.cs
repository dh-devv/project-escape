using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Carten.Tests
{
    public class BuildSceneTests
    {
        [TestCase("Assets/Scenes/UI/Main UI.unity")]
        [TestCase("Assets/Scenes/Stage1_Map01.unity")]
        [TestCase("Assets/Scenes/Stage1_Map02.unity")]
        [TestCase("Assets/Scenes/Stage1_Map03.unity")]
        [TestCase("Assets/Scenes/Stage1_Map04.unity")]
        [TestCase("Assets/Scenes/Stage1_Map05.unity")]
        [TestCase("Assets/Scenes/Stage1_Map06.unity")]
        [TestCase("Assets/Scenes/Stage1_Map07.unity")]
        [TestCase("Assets/Scenes/Stage1_Map08.unity")]
        public void SavedScenePassesBuildValidation(string path)
        {
            Scene scene = SceneManager.GetSceneByPath(path);
            bool alreadyLoaded = scene.IsValid() && scene.isLoaded;
            if (!alreadyLoaded) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                var type = Type.GetType("LastSpark.Editor.LastSparkBuildContentCheck, Assembly-CSharp-Editor", true);
                MethodInfo validate = type.GetMethod("ValidateScene", BindingFlags.Static | BindingFlags.NonPublic);
                Assert.That(validate, Is.Not.Null, "Build validation must also be callable before starting a build.");
                try { validate.Invoke(null, new object[] { scene }); }
                catch (TargetInvocationException error) { throw error.InnerException; }
            }
            finally { if (!alreadyLoaded) EditorSceneManager.CloseScene(scene, true); }
        }

        [Test]
        public void HealthBarOnlyHudDoesNotRequireOptionalLabels()
        {
            var root = new GameObject("Test HUD", typeof(RectTransform), typeof(Canvas), typeof(GameHUD));
            var fill = new GameObject("HP_Fill", typeof(RectTransform), typeof(Image));
            try
            {
                fill.transform.SetParent(root.transform);
                GameHUD hud = root.GetComponent<GameHUD>();
                using (var data = new SerializedObject(hud))
                {
                    data.FindProperty("playerHpFill").objectReferenceValue = fill.GetComponent<Image>();
                    data.ApplyModifiedPropertiesWithoutUndo();
                }
                Assert.DoesNotThrow(() => hud.UpdatePlayerHP(40f, 100f));
                Assert.That(fill.GetComponent<Image>().fillAmount, Is.EqualTo(.4f).Within(.001f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}
