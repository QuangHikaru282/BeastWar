using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace Antigravity.Editor
{
    public static class AutoSpriteImporter
    {
        private const int FrameSize = 192; // 192x192px for sprite_3x
        private const int Columns = 3;
        private const int Rows = 3;
        private const int TotalFrames = 8;
        private const int FrameRate = 12; // 12 FPS for animation playback

        [MenuItem("Tools/AutoSprite/Slice and Generate Animations")]
        public static void SliceAndGenerateAnimations()
        {
            string beastsRoot = "Assets/_Project/Sprites/Beasts";
            if (!Directory.Exists(beastsRoot))
            {
                Debug.LogError($"[AutoSpriteImporter] Beasts root folder not found at: {beastsRoot}");
                return;
            }

            Debug.Log("[AutoSpriteImporter] Starting batch import process...");
            int processedBeasts = 0;
            int slicedTextures = 0;

            // Get all Element folders (Fire, Water, Grass, Light, Dark)
            string[] elementDirs = Directory.GetDirectories(beastsRoot);
            foreach (string elementDir in elementDirs)
            {
                // Get all Beast species folders
                string[] speciesDirs = Directory.GetDirectories(elementDir);
                foreach (string speciesDir in speciesDirs)
                {
                    // Get Base and Evolved folders
                    string[] formDirs = Directory.GetDirectories(speciesDir);
                    foreach (string formDir in formDirs)
                    {
                        string formName = Path.GetFileName(formDir); // "Base" or "Evolved"
                        if (formName != "Base" && formName != "Evolved")
                            continue;

                        string animsDir = Path.Combine(formDir, "Anims");
                        if (!Directory.Exists(animsDir))
                            continue;

                        string beastName = Path.GetFileName(speciesDir);
                        string displayName = formName == "Base" ? beastName : GetEvolvedName(beastName);
                        
                        Debug.Log($"[AutoSpriteImporter] Processing: {displayName} ({formName}) at {formDir}");
                        
                        // Step 1: Slice all spritesheets in the Anims directory
                        string[] pngFiles = Directory.GetFiles(animsDir, "*.png")
                            .Where(f => !f.EndsWith(".meta"))
                            .ToArray();

                        List<AnimationClip> clips = new List<AnimationClip>();

                        foreach (string pngFile in pngFiles)
                        {
                            string animName = Path.GetFileNameWithoutExtension(pngFile).ToLower();
                            if (animName == "hurt")
                            {
                                // Skip hurt animation as per guidelines
                                continue;
                            }

                            bool success = SliceSpritesheet(pngFile, animName);
                            if (success)
                            {
                                slicedTextures++;
                                AnimationClip clip = CreateAnimationClip(pngFile, animName, displayName);
                                if (clip != null)
                                {
                                    clips.Add(clip);
                                }
                            }
                        }

                        // Step 2: Create or update Animator Controller
                        if (clips.Count > 0)
                        {
                            CreateAnimatorController(formDir, displayName, clips);
                            processedBeasts++;
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[AutoSpriteImporter] Completed! Processed {processedBeasts} beasts and sliced {slicedTextures} textures.");
        }

        private static bool SliceSpritesheet(string assetPath, string animName)
        {
            try
            {
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null) return false;

                // 1. Set type and configure basics
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;

                // Configure platform settings
                TextureImporterPlatformSettings platformSettings = importer.GetDefaultPlatformTextureSettings();
                platformSettings.maxTextureSize = 2048;
                platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(platformSettings);

                // Set multiple sprite mode using settings
                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMode = (int)SpriteImportMode.Multiple;
                importer.SetTextureSettings(settings);

                // Save basic importer changes before data provider operations
                importer.SaveAndReimport();

                // 2. Build grid slicing metadata using ISpriteEditorDataProvider
                var factory = new SpriteDataProviderFactories();
                factory.Init();
                var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
                dataProvider.InitSpriteEditorDataProvider();

                // Create sliced rects
                List<SpriteRect> rects = new List<SpriteRect>();
                int frameIndex = 0;

                // Unity TextureImporter coordinate space: bottom-left is (0, 0)
                for (int r = Rows - 1; r >= 0; r--)
                {
                    for (int c = 0; c < Columns; c++)
                    {
                        if (frameIndex >= TotalFrames) break;

                        SpriteRect spriteRect = new SpriteRect
                        {
                            name = $"{animName}_{frameIndex}",
                            rect = new Rect(c * FrameSize, r * FrameSize, FrameSize, FrameSize),
                            alignment = SpriteAlignment.Center,
                            pivot = new Vector2(0.5f, 0.5f),
                            spriteID = GUID.Generate()
                        };
                        rects.Add(spriteRect);
                        frameIndex++;
                    }
                }

                // 3. Set rects and apply changes
                dataProvider.SetSpriteRects(rects.ToArray());
                dataProvider.Apply();

                // Reimport the modified asset to ensure Unity internal assets update
                importer.SaveAndReimport();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[AutoSpriteImporter] Failed to slice texture at {assetPath}: {e.Message}");
                return false;
            }
        }

        private static AnimationClip CreateAnimationClip(string texturePath, string animName, string beastName)
        {
            string folder = Path.GetDirectoryName(texturePath);
            string clipPath = Path.Combine(folder, $"{animName}.anim").Replace("\\", "/");

            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            bool isNew = false;
            if (clip == null)
            {
                clip = new AnimationClip();
                isNew = true;
            }

            clip.frameRate = FrameRate;

            // Get sliced sprites from the reimported texture sheet
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath)
                .OfType<Sprite>()
                .OrderBy(s => GetFrameIndexFromName(s.name))
                .ToArray();

            if (sprites.Length == 0)
            {
                Debug.LogWarning($"[AutoSpriteImporter] No sliced sprites found inside {texturePath}!");
                return null;
            }

            // Set up bindings for the sprite property of SpriteRenderer
            EditorCurveBinding spriteBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];
            float timePerFrame = 1f / FrameRate;
            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i * timePerFrame,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

            // Configure loop settings
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = (animName == "idle" || animName == "run");
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            if (isNew)
            {
                AssetDatabase.CreateAsset(clip, clipPath);
            }
            else
            {
                EditorUtility.SetDirty(clip);
            }

            return clip;
        }

        private static void CreateAnimatorController(string formDir, string beastName, List<AnimationClip> clips)
        {
            string controllerPath = Path.Combine(formDir, $"{beastName}_Animator.controller").Replace("\\", "/");

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            }

            AnimatorControllerLayer baseLayer = controller.layers[0];
            AnimatorStateMachine stateMachine = baseLayer.stateMachine;

            foreach (var clip in clips)
            {
                string stateName = Capitalize(clip.name);
                
                // Try to find existing state, or create a new one
                ChildAnimatorState state = stateMachine.states.FirstOrDefault(s => s.state.name == stateName);
                if (state.state == null)
                {
                    AnimatorState newState = stateMachine.AddState(stateName);
                    newState.motion = clip;
                    
                    if (stateName == "Idle")
                    {
                        stateMachine.defaultState = newState;
                    }
                }
                else
                {
                    state.state.motion = clip;
                }
            }

            EditorUtility.SetDirty(controller);
        }

        private static int GetFrameIndexFromName(string spriteName)
        {
            // Expected format: "idle_0", "faint_7"
            try
            {
                string[] parts = spriteName.Split('_');
                if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out int index))
                {
                    return index;
                }
            }
            catch { }
            return 0;
        }

        private static string Capitalize(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        private static string GetEvolvedName(string beastName)
        {
            var evolvedNames = new Dictionary<string, string>
            {
                { "Foxpyre", "Blazefang" },
                { "Octokid", "Inferapus" },
                { "Larviglow", "Pyremoth" },
                { "Drizzlet", "Tidalax" },
                { "Finlet", "Sharkraze" },
                { "Turtide", "Whalegon" },
                { "Verdile", "Terravine" },
                { "Mantiseed", "Bladeleaf" },
                { "Sproutgrub", "Vinelock" },
                { "Cubshine", "Bearlux" },
                { "Pawnox", "Luminbear" },
                { "Glowsquid", "Radiantacle" },
                { "Crowling", "Ravenshade" },
                { "Duskling", "Voidjelly" },
                { "Drakshade", "Grimrex" }
            };
            return evolvedNames.ContainsKey(beastName) ? evolvedNames[beastName] : $"{beastName}Evolved";
        }
    }
}
