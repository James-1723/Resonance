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
public class TextToVFX_Combined_Minimal : MonoBehaviour
{
    [Header("Inputs (only)")]
    public TextMeshPro tmp;
    public VisualEffect vfx;
    public Transform splitter;

    // -----------------------------
    // VFX property names
    // -----------------------------
    const string VFX_MESH_PROP  = "PointMesh";
    const string VFX_COUNT_PROP = "PointCount";

    // Split center only (params tuned in VFX Graph)
    const string SPLIT_CENTER   = "SplitCenter";
    // NOTE: SplitRadius / SplitFalloff / SplitStrength are NOT written by C#
    // and should be exposed & tuned in VFX Graph.

    // -----------------------------
    // Point sampling settings (still hard-coded in this version)
    // -----------------------------
    const int   SAMPLES_PER_CHAR = 4000; // attempts per visible character
    const float KEEP_RATIO       = 0.60f;
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
    }

    void OnDisable() => ReleaseAll();
    void OnDestroy() => ReleaseAll();

    void OnValidate()
    {
        EnsureMesh();
        TryRebuild(force: true);
        PushSplitCenter();
    }

    void Update()
    {
        EnsureMesh();

        // Rebuild points only when TMP/VFX changed
        TryRebuild(force: false);

        // Always update split center (moving sphere updates every frame)
        PushSplitCenter();
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

        int approx = Mathf.Max(1, ti.characterCount * Mathf.CeilToInt(SAMPLES_PER_CHAR * Mathf.Clamp01(KEEP_RATIO)));
        var positions = new List<Vector3>(approx);

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

            int tries = Mathf.Max(1, SAMPLES_PER_CHAR);
            int targetKeep = Mathf.Max(1, Mathf.RoundToInt(tries * Mathf.Clamp01(KEEP_RATIO)));
            int kept = 0;

            for (int t = 0; t < tries && kept < targetKeep; t++)
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
