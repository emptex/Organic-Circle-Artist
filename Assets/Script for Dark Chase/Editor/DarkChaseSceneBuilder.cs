using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public static class DarkChaseSceneBuilder
{
    [MenuItem("Dark Chase/Build Scene")]
    public static void BuildScene()
    {
        // Open the Dark Chase scene
        string scenePath = "Assets/Scenes/Dark Chase.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Clear all existing objects
        foreach (var root in scene.GetRootGameObjects())
            Object.DestroyImmediate(root);

        // ── Volume Profile ──
        var profile = CreateVolumeProfile();

        // ── Corridor ──
        var corridorRoot = new GameObject("Corridor");
        var darkMat = CreateDarkMaterial();

        BuildHallwaySegment("HallwaySegmentA", 0f, darkMat, corridorRoot.transform);
        BuildHallwaySegment("HallwaySegmentB", 20f, darkMat, corridorRoot.transform);

        var startAnchor = new GameObject("StartAnchor");
        startAnchor.transform.SetParent(corridorRoot.transform);
        startAnchor.transform.position = new Vector3(0f, 0f, 1f);

        var endTrigger = new GameObject("EndTrigger");
        endTrigger.transform.SetParent(corridorRoot.transform);
        endTrigger.transform.position = new Vector3(0f, 0f, 20f);
        var triggerBox = endTrigger.AddComponent<BoxCollider>();
        triggerBox.isTrigger = true;
        triggerBox.center = new Vector3(0f, 1.5f, 0f);
        triggerBox.size = new Vector3(5f, 4f, 1f);

        // ── Player Rig ──
        var player = new GameObject("Player");
        player.transform.position = new Vector3(0f, 0f, 2f);
        player.layer = 0;

        var cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.center = new Vector3(0f, 0.9f, 0f);
        cc.radius = 0.3f;

        var playerScript = player.AddComponent<DarkChasePlayer>();
        player.AddComponent<AudioListener>();

        // Camera
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        camGO.transform.SetParent(player.transform);
        camGO.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        var cam = camGO.AddComponent<Camera>();
        cam.fieldOfView = 70f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 30f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        // Add Universal Additional Camera Data for URP
        var camData = camGO.AddComponent<UniversalAdditionalCameraData>();
        camData.renderPostProcessing = true;

        // Player light
        var lightGO = new GameObject("PlayerLight");
        lightGO.transform.SetParent(player.transform);
        lightGO.transform.localPosition = new Vector3(0f, 1.5f, 0.2f);
        var playerLight = lightGO.AddComponent<Light>();
        playerLight.type = LightType.Point;
        playerLight.range = 4f;
        playerLight.intensity = 0.6f;
        playerLight.color = new Color(1f, 0.894f, 0.769f); // warm white #FFE4C4

        // ── Corridor Loop Script ──
        var loopScript = endTrigger.AddComponent<DarkChaseCorridorLoop>();
        SetPrivateSerializedField(loopScript, "player", player.transform);
        SetPrivateSerializedField(loopScript, "startAnchor", startAnchor.transform);
        SetPrivateSerializedField(loopScript, "corridorLength", 20f);

        // ── Audio Manager ──
        var audioManagerGO = new GameObject("AudioManager");

        var chaseDrone = CreateAudioSource(audioManagerGO, "ChaseDrone", true, true, 0.3f);
        var heartbeat = CreateAudioSource(audioManagerGO, "Heartbeat", true, true, 0f);
        var footstep = CreateAudioSource(audioManagerGO, "Footstep", false, false, 1f);
        var sting = CreateAudioSource(audioManagerGO, "Sting", false, false, 1f);

        var audioMgr = audioManagerGO.AddComponent<DarkChaseAudioManager>();
        SetPrivateSerializedField(audioMgr, "player", playerScript);
        SetPrivateSerializedField(audioMgr, "chaseDroneSource", chaseDrone);
        SetPrivateSerializedField(audioMgr, "heartbeatSource", heartbeat);
        SetPrivateSerializedField(audioMgr, "footstepSource", footstep);

        // Audio generator — creates placeholder clips at runtime if no clips assigned
        var audioGen = audioManagerGO.AddComponent<DarkChaseAudioGenerator>();
        SetPrivateSerializedField(audioGen, "chaseDroneSource", chaseDrone);
        SetPrivateSerializedField(audioGen, "heartbeatSource", heartbeat);
        SetPrivateSerializedField(audioGen, "footstepSource", footstep);
        SetPrivateSerializedField(audioGen, "stingSource", sting);

        // ── Post-Processing Volume ──
        var volumeGO = new GameObject("Global Volume");
        var volume = volumeGO.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.profile = profile;

        var ppScript = volumeGO.AddComponent<DarkChasePostProcessing>();
        SetPrivateSerializedField(ppScript, "audioManager", audioMgr);
        SetPrivateSerializedField(ppScript, "globalVolume", volume);

        // ── Death Sequence UI ──
        var canvas = CreateDeathCanvas(out var flashGroup, out var deathGroup, out var restartBtn);

        var deathMgrGO = new GameObject("DeathManager");
        var deathScript = deathMgrGO.AddComponent<DarkChaseDeathSequence>();
        SetPrivateSerializedField(deathScript, "player", playerScript);
        SetPrivateSerializedField(deathScript, "audioManager", audioMgr);
        SetPrivateSerializedField(deathScript, "stingSource", sting);
        SetPrivateSerializedField(deathScript, "flashOverlay", flashGroup);
        SetPrivateSerializedField(deathScript, "deathOverlay", deathGroup);
        SetPrivateSerializedField(deathScript, "restartButton", restartBtn);

        // Wire restart button onClick to DeathSequence.Restart()
        var btnComponent = restartBtn.GetComponent<Button>();
        var targetInfo = new UnityEngine.Events.UnityAction(deathScript.Restart);
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btnComponent.onClick, targetInfo);

        // ── Lighting Settings ──
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.04f, 0.04f, 0.04f);
        RenderSettings.skybox = null;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = Color.black;
        RenderSettings.fogDensity = 0.15f;

        // ── Add to Build Settings ──
        AddSceneToBuildSettings(scenePath);

        // ── Save ──
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("Dark Chase scene built successfully! Import audio clips into Assets/Audio/DarkChase/ and assign them to the AudioManager's AudioSources.");
    }

    static VolumeProfile CreateVolumeProfile()
    {
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();

        var vignette = profile.Add<Vignette>();
        vignette.active = true;
        vignette.intensity.Override(0.4f);
        vignette.color.Override(Color.black);

        var chromatic = profile.Add<ChromaticAberration>();
        chromatic.active = true;
        chromatic.intensity.Override(0f);

        var grain = profile.Add<FilmGrain>();
        grain.active = true;
        grain.intensity.Override(0.2f);
        grain.type.Override(FilmGrainLookup.Medium3);

        var bloom = profile.Add<Bloom>();
        bloom.active = true;
        bloom.intensity.Override(0.1f);

        var tonemapping = profile.Add<Tonemapping>();
        tonemapping.active = true;
        tonemapping.mode.Override(TonemappingMode.ACES);

        string dir = "Assets/Settings";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "Settings");

        AssetDatabase.CreateAsset(profile, "Assets/Settings/DarkChaseProfile.asset");
        AssetDatabase.SaveAssets();
        return profile;
    }

    static Material CreateDarkMaterial()
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        var mat = new Material(shader);
        mat.SetColor("_BaseColor", new Color(0.1f, 0.1f, 0.1f, 1f));
        mat.SetFloat("_Smoothness", 0.2f);
        mat.SetFloat("_Metallic", 0f);

        string dir = "Assets/Material";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "Material");

        AssetDatabase.CreateAsset(mat, "Assets/Material/DarkChaseConcrete.mat");
        AssetDatabase.SaveAssets();
        return mat;
    }

    static void BuildHallwaySegment(string name, float zOffset, Material mat, Transform parent)
    {
        float length = 20f;
        float width = 4f;
        float height = 3f;

        var segment = new GameObject(name);
        segment.transform.SetParent(parent);
        segment.transform.position = new Vector3(0f, 0f, zOffset);

        // Floor
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(segment.transform);
        floor.transform.localPosition = new Vector3(0f, 0f, length / 2f);
        floor.transform.localScale = new Vector3(width, 0.1f, length);
        floor.GetComponent<Renderer>().sharedMaterial = mat;

        // Ceiling
        var ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling";
        ceiling.transform.SetParent(segment.transform);
        ceiling.transform.localPosition = new Vector3(0f, height, length / 2f);
        ceiling.transform.localScale = new Vector3(width, 0.1f, length);
        ceiling.GetComponent<Renderer>().sharedMaterial = mat;

        // Left wall
        var wallL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallL.name = "WallLeft";
        wallL.transform.SetParent(segment.transform);
        wallL.transform.localPosition = new Vector3(-width / 2f, height / 2f, length / 2f);
        wallL.transform.localScale = new Vector3(0.1f, height, length);
        wallL.GetComponent<Renderer>().sharedMaterial = mat;

        // Right wall
        var wallR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wallR.name = "WallRight";
        wallR.transform.SetParent(segment.transform);
        wallR.transform.localPosition = new Vector3(width / 2f, height / 2f, length / 2f);
        wallR.transform.localScale = new Vector3(0.1f, height, length);
        wallR.GetComponent<Renderer>().sharedMaterial = mat;

        // Flickering ceiling lights (3 per segment)
        for (int i = 0; i < 3; i++)
        {
            float z = length * (i + 1) / 4f;
            var lightGO = new GameObject($"CeilingLight_{i}");
            lightGO.transform.SetParent(segment.transform);
            lightGO.transform.localPosition = new Vector3(0f, height - 0.2f, z);

            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Spot;
            light.range = 5f;
            light.intensity = 20f;
            light.spotAngle = 90f;
            light.color = new Color(1f, 0.95f, 0.8f);
            lightGO.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            var flicker = lightGO.AddComponent<DarkChaseLightFlicker>();
            SetPrivateSerializedField(flicker, "targetLight", light);
        }
    }

    static AudioSource CreateAudioSource(GameObject parent, string childName, bool loop, bool playOnAwake, float volume)
    {
        var go = new GameObject(childName);
        go.transform.SetParent(parent.transform);
        var src = go.AddComponent<AudioSource>();
        src.loop = loop;
        src.playOnAwake = playOnAwake;
        src.volume = volume;
        src.spatialBlend = 0f; // 2D audio
        return src;
    }

    static GameObject CreateDeathCanvas(out CanvasGroup flashGroup, out CanvasGroup deathGroup, out GameObject restartButton)
    {
        var canvasGO = new GameObject("DeathCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Flash overlay (white)
        var flashGO = new GameObject("FlashOverlay");
        flashGO.transform.SetParent(canvasGO.transform, false);
        var flashImg = flashGO.AddComponent<Image>();
        flashImg.color = Color.white;
        flashImg.raycastTarget = false;
        StretchToFill(flashGO.GetComponent<RectTransform>());
        flashGroup = flashGO.AddComponent<CanvasGroup>();
        flashGroup.alpha = 0f;
        flashGroup.blocksRaycasts = false;
        flashGroup.interactable = false;

        // Death overlay (dark red)
        var deathGO = new GameObject("DeathOverlay");
        deathGO.transform.SetParent(canvasGO.transform, false);
        var deathImg = deathGO.AddComponent<Image>();
        deathImg.color = new Color(0.3f, 0f, 0f, 1f);
        deathImg.raycastTarget = false;
        StretchToFill(deathGO.GetComponent<RectTransform>());
        deathGroup = deathGO.AddComponent<CanvasGroup>();
        deathGroup.alpha = 0f;
        deathGroup.blocksRaycasts = false;
        deathGroup.interactable = false;

        // Restart button (hidden until death)
        restartButton = new GameObject("RestartButton");
        restartButton.transform.SetParent(canvasGO.transform, false);
        var btnImg = restartButton.AddComponent<Image>();
        btnImg.color = new Color(0.8f, 0.1f, 0.1f, 0.9f);
        var btnRT = restartButton.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0.5f);
        btnRT.anchorMax = new Vector2(0.5f, 0.5f);
        btnRT.sizeDelta = new Vector2(240f, 60f);
        btnRT.anchoredPosition = new Vector2(0f, -40f);
        restartButton.AddComponent<Button>();

        // Button label
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(restartButton.transform, false);
        var label = labelGO.AddComponent<TextMeshProUGUI>();
        label.text = "RESTART";
        label.fontSize = 28;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        StretchToFill(labelGO.GetComponent<RectTransform>());

        restartButton.SetActive(false);

        return canvasGO;
    }

    static void StretchToFill(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in scenes)
        {
            if (s.path == scenePath) return; // already added
        }
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    static void SetPrivateSerializedField(Object target, string fieldName, object value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop == null)
        {
            Debug.LogWarning($"Could not find serialized field '{fieldName}' on {target.GetType().Name}");
            return;
        }

        switch (value)
        {
            case Object unityObj:
                prop.objectReferenceValue = unityObj;
                break;
            case float f:
                prop.floatValue = f;
                break;
            case int i:
                prop.intValue = i;
                break;
            case bool b:
                prop.boolValue = b;
                break;
            case string s:
                prop.stringValue = s;
                break;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
