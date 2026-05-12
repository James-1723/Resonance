// TextToVFX_Combined_Minimal.cs
// One-script solution:
// 1) TMP text -> SDF-masked point cloud mesh -> VFX
// 2) Splitter sphere -> writes SplitCenter into VFX (Split params are tuned in VFX Graph)
//
// Assumes particle positions are stored in VFX LOCAL space (recommended).
//
// Inspector: only tmp / vfx / splitter.
// Tune SplitRadius / SplitFalloff / SplitStrength in VFX Graph (EXPOSED), not here.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using TMPro;

[ExecuteAlways]
public class TextToPointsController : MonoBehaviour
{
    [Header("Inputs (only)")]
    public TextMeshPro tmp;
    public VisualEffect vfx;
    public Transform splitter;

    [Header("Point Sampling")]
    [Tooltip("採樣嘗試次數的倍數上限（避免無窮迴圈）。目標粒子數請直接在 VFX Inspector 的 TargetPointCount 欄位設定")]
    public int maxAttemptsMultiplier = 20;

    [Header("Time Animation")]
    [Tooltip("若打勾，VFX 裡的 Time 從 0 漸變到 1")]
    public bool FadeOut = false;
    [Tooltip("漸進到 1 所需的秒數")]
    public float timeTransitionDuration = 1f;

    private float _timeValue = 0f;

    // -----------------------------
    // VFX property names
    // -----------------------------
    const string VFX_MESH_PROP         = "PointMesh";
    const string VFX_COUNT_PROP        = "PointCount";       // C# 寫入：實際採樣後的粒子數
    const string VFX_TARGET_COUNT_PROP = "TargetPointCount"; // C# 讀取：使用者在 VFX Inspector 設定的目標數量

    // Split center only (params tuned in VFX Graph)
    const string SPLIT_CENTER    = "SplitCenter";
    const string WORD_POSITION   = "WordPosition";
    const string VFX_TIME_PROP   = "Time";
    const string VFX_FADEOUT_PROP = "FadeOut";
    // NOTE: SplitRadius / SplitFalloff / SplitStrength are NOT written by C#
    // and should be exposed & tuned in VFX Graph.

    // -----------------------------
    // Point sampling settings
    // -----------------------------
    const float SDF_THRESHOLD    = 0.50f;
    const float Z_PLANE          = 0f;

    // Runtime-only mesh (never serialized)
    Mesh _mesh;

    // Change detection for rebuilding point mesh
    string _lastText;
    TMP_FontAsset _lastFont;
    Vector3 _lastLossyScale;
    int _lastTmpId;
    int _lastVfxId;

    // Cache: src atlas texture -> (readable copy, invert flag)
    readonly Dictionary<Texture, (Texture2D tex, bool invert)> _atlasCache = new();

    void OnEnable()
    {
        EnsureMesh();
        TryRebuild(force: true);
        PushSplitCenter();
        PushWordPosition();
        UpdateTimeParameter();
    }

    void OnDisable() => ReleaseAll();
    void OnDestroy() => ReleaseAll();

    void OnValidate()
    {
        EnsureMesh();
        TryRebuild(force: true);
        PushSplitCenter();
        PushWordPosition();
        UpdateTimeParameter();
    }

    void Update()
    {
        EnsureMesh();

        // Rebuild points only when TMP/VFX changed
        TryRebuild(force: false);

        // Always update split center (moving sphere updates every frame)
        PushSplitCenter();
        PushWordPosition();
        UpdateTimeParameter();
    }

    // -----------------------------
    // Point mesh
    // -----------------------------
    void EnsureMesh()
    {
        if (_mesh != null) return;

        _mesh = new Mesh
        {
            name = $"TMP_PointMesh_SDF_{GetInstanceID()}",
            hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild
        };
    }

    void ReleaseAll()
    {
        // Detach from VFX
        if (vfx != null)
        {
            try { vfx.SetMesh(VFX_MESH_PROP, null); } catch { }
            try { vfx.SetUInt(VFX_COUNT_PROP, 0u); } catch { }
        }

        // Destroy runtime mesh
        if (_mesh != null)
        {
            if (Application.isPlaying) Destroy(_mesh);
            else DestroyImmediate(_mesh);
            _mesh = null;
        }

        // Destroy cached readable atlases
        foreach (var kv in _atlasCache)
        {
            var t = kv.Value.tex;
            if (t == null) continue;
            if (Application.isPlaying) Destroy(t);
            else DestroyImmediate(t);
        }
        _atlasCache.Clear();
    }

    void TryRebuild(bool force)
    {
        if (tmp == null) return;

        bool changed =
            force
            || _lastText != tmp.text
            || _lastFont != tmp.font
            || _lastLossyScale != tmp.transform.lossyScale
            || _lastTmpId != tmp.GetInstanceID()
            || _lastVfxId != (vfx != null ? vfx.GetInstanceID() : 0);

        if (!changed) return;

        _lastText = tmp.text;
        _lastFont = tmp.font;
        _lastLossyScale = tmp.transform.lossyScale;
        _lastTmpId = tmp.GetInstanceID();
        _lastVfxId = (vfx != null ? vfx.GetInstanceID() : 0);

        BuildPointMeshOnce();
    }

    void BuildPointMeshOnce()
    {
        EnsureMesh();

        tmp.ForceMeshUpdate();

        // Robust guards (ExecuteAlways timing)
        if (tmp.textInfo == null || tmp.textInfo.characterInfo == null || tmp.textInfo.meshInfo == null)
        {
            PushEmptyMesh();
            return;
        }

        var ti = tmp.textInfo;
        if (ti.characterCount <= 0)
        {
            PushEmptyMesh();
            return;
        }

        // --- 第一輪：統計可見字符數量 ---
        int visibleCharCount = 0;
        for (int i = 0; i < ti.characterCount; i++)
            if (ti.characterInfo[i].isVisible) visibleCharCount++;

        if (visibleCharCount == 0) { PushEmptyMesh(); return; }

        // 直接從 VFX Inspector 的 TargetPointCount 讀取目標粒子數
        // （該屬性序列化在 prefab 裡，重開遊戲也不會消失）
        int totalTarget = 1000;
        if (vfx != null && vfx.HasUInt(VFX_TARGET_COUNT_PROP))
            totalTarget = (int)vfx.GetUInt(VFX_TARGET_COUNT_PROP);
        totalTarget = Mathf.Max(1, totalTarget);

        // 每個字符的目標點數（平均分配）
        int targetPerChar = Mathf.Max(1, Mathf.RoundToInt((float)totalTarget / visibleCharCount));
        int maxAttempts   = targetPerChar * Mathf.Max(1, maxAttemptsMultiplier);

        var positions = new List<Vector3>(totalTarget);

        // --- 第二輪：逐字符採樣直到達到目標 ---
        for (int i = 0; i < ti.characterCount; i++)
        {
            var ch = ti.characterInfo[i];
            if (!ch.isVisible) continue;

            // Font + atlas page
            var fa = ch.fontAsset;
            if (fa == null || fa.atlasTextures == null || fa.atlasTextures.Length == 0) continue;

            int page = ch.textElement?.glyph?.atlasIndex ?? 0;
            if (page < 0 || page >= fa.atlasTextures.Length) continue;

            var srcAtlas = fa.atlasTextures[page];
            if (srcAtlas == null) continue;

            if (!_atlasCache.TryGetValue(srcAtlas, out var entry))
            {
                var readable = ToReadableRGBA(srcAtlas);
                if (readable == null) continue;

                bool invert = DetectAtlasPolarity(readable);
                entry = (readable, invert);
                _atlasCache[srcAtlas] = entry;
            }

            var sdfTex = entry.tex;
            bool invertFlag = entry.invert;

            // MeshInfo for this character
            int mat = ch.materialReferenceIndex;
            if (mat < 0 || mat >= ti.meshInfo.Length) continue;

            var mi = ti.meshInfo[mat];
            int v = ch.vertexIndex;

            if (mi.vertices == null || mi.uvs0 == null) continue;
            if (v < 0 || v + 3 >= mi.vertices.Length) continue;
            if (v + 3 >= mi.uvs0.Length) continue;

            Vector3 v0 = mi.vertices[v + 0], v1 = mi.vertices[v + 1];
            Vector3 v2 = mi.vertices[v + 2], v3 = mi.vertices[v + 3];

            Vector2 u0 = mi.uvs0[v + 0], u1 = mi.uvs0[v + 1];
            Vector2 u2 = mi.uvs0[v + 2], u3 = mi.uvs0[v + 3];

            int kept = 0;

            // 採樣直到該字符達到目標點數，或超過最大嘗試次數
            for (int t = 0; t < maxAttempts && kept < targetPerChar; t++)
            {
                bool firstTri = Random.value < 0.5f;

                Vector3 A = v0;
                Vector3 B = firstTri ? v1 : v2;
                Vector3 C = firstTri ? v2 : v3;

                Vector2 Au = u0;
                Vector2 Bu = firstTri ? u1 : u2;
                Vector2 Cu = firstTri ? u2 : u3;

                // Uniform sampling in triangle via barycentric
                float r1 = Mathf.Sqrt(Random.value);
                float r2 = Random.value;
                float a = 1f - r1;
                float b = r1 * (1f - r2);
                float c = r1 * r2;

                Vector3 P = a * A + b * B + c * C;
                Vector2 Pu = a * Au + b * Bu + c * Cu;

                Color col = sdfTex.GetPixelBilinear(Pu.x, Pu.y);
                float sVal = (col.a > 0f) ? col.a : col.r;

                bool inside = invertFlag ? (sVal > SDF_THRESHOLD) : (sVal < SDF_THRESHOLD);
                if (!inside) continue;

                // TMP local -> world -> VFX local
                Vector3 localFromTMP = new Vector3(P.x, P.y, Z_PLANE);
                Vector3 worldP = tmp.transform.TransformPoint(localFromTMP);
                Vector3 vfxLocalP = (vfx != null) ? vfx.transform.InverseTransformPoint(worldP) : worldP;

                positions.Add(vfxLocalP);
                kept++;
            }
        }

        // Build points mesh
        _mesh.indexFormat = positions.Count > 65000
            ? UnityEngine.Rendering.IndexFormat.UInt32
            : UnityEngine.Rendering.IndexFormat.UInt16;

        _mesh.Clear();
        _mesh.SetVertices(positions);

        int cnt = positions.Count;
        var indices = new int[cnt];
        for (int i = 0; i < cnt; i++) indices[i] = i;

        _mesh.SetIndices(indices, MeshTopology.Points, 0, true);
        _mesh.UploadMeshData(false);

        if (vfx != null)
        {
            vfx.SetMesh(VFX_MESH_PROP, _mesh);
            vfx.SetUInt(VFX_COUNT_PROP, (uint)cnt);
        }
    }

    void PushEmptyMesh()
    {
        if (_mesh != null) _mesh.Clear();
        if (vfx != null)
        {
            vfx.SetMesh(VFX_MESH_PROP, _mesh);
            vfx.SetUInt(VFX_COUNT_PROP, 0u);
        }
    }

    // -----------------------------
    // Split center (ONLY)
    // -----------------------------
    void PushSplitCenter()
    {
        if (vfx == null || splitter == null) return;

        // Center in VFX LOCAL (must match particle position space)
        Vector3 centerLocal = vfx.transform.InverseTransformPoint(splitter.position);
        vfx.SetVector3(SPLIT_CENTER, centerLocal);

        // IMPORTANT:
        // Do NOT write SplitRadius / SplitFalloff / SplitStrength here.
        // Expose and tweak them directly in VFX Graph.
    }

    void PushWordPosition()
    {
        if (vfx == null || tmp == null) return;

        // Setting WordPosition to TMP's world position
        vfx.SetVector3(WORD_POSITION, tmp.transform.position);
    }

    void UpdateTimeParameter()
    {
        if (vfx == null) return;

        // 同步 FadeOut 布林值給 VFX
        vfx.SetBool(VFX_FADEOUT_PROP, FadeOut);

        if (Application.isPlaying)
        {
            // 在 Play Mode 時，根據 FadeOut 布林值平滑漸進 (0 -> 1 或是 1 -> 0)
            float targetValue = FadeOut ? 1f : 0f;
            _timeValue = Mathf.MoveTowards(_timeValue, targetValue, Time.deltaTime / Mathf.Max(0.001f, timeTransitionDuration));
        }
        else
        {
            // 在 Edit Mode 直接切換數值以利預覽
            _timeValue = FadeOut ? 1f : 0f;
        }

        vfx.SetFloat(VFX_TIME_PROP, _timeValue);
    }

    // -----------------------------
    // Helpers
    // -----------------------------
    static Texture2D ToReadableRGBA(Texture src)
    {
        if (src == null) return null;

        var rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
        var prev = RenderTexture.active;

        try
        {
            Graphics.Blit(src, rt);
            RenderTexture.active = rt;

            var tex = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false, true);
            tex.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
            tex.Apply(false, false);
            return tex;
        }
        finally
        {
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
        }
    }

    static bool DetectAtlasPolarity(Texture2D atlas)
    {
        Vector2[] probes =
        {
            new(0.01f, 0.01f), new(0.99f, 0.01f),
            new(0.01f, 0.99f), new(0.99f, 0.99f),
            new(0.50f, 0.01f), new(0.01f, 0.50f),
            new(0.99f, 0.50f), new(0.50f, 0.99f)
        };

        float sum = 0f;
        for (int i = 0; i < probes.Length; i++)
        {
            Color c = atlas.GetPixelBilinear(probes[i].x, probes[i].y);
            float v = (c.a > 0f) ? c.a : c.r;
            sum += v;
        }

        float bgMean = sum / probes.Length;
        return bgMean < 0.5f; // dark bg => inside bright => invert=true
    }
}
