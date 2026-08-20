using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace ProjectAllTime.VN.SaveLoad
{
    public enum VNThumbnailLoadStatus
    {
        Loaded,
        Placeholder,
    }

    /// <summary>Result whose Texture2D, when non-null, is caller-owned.</summary>
    public sealed class VNThumbnailLoadResult
    {
        public VNThumbnailLoadStatus Status { get; }
        public Texture2D Texture { get; }
        public string Diagnostic { get; }

        private VNThumbnailLoadResult(VNThumbnailLoadStatus status, Texture2D texture, string diagnostic)
        {
            Status = status;
            Texture = texture;
            Diagnostic = diagnostic;
        }

        public static VNThumbnailLoadResult Loaded(Texture2D texture) => new(VNThumbnailLoadStatus.Loaded, texture, null);
        public static VNThumbnailLoadResult Placeholder(string diagnostic = null) => new(VNThumbnailLoadStatus.Placeholder, null, diagnostic);
    }

    /// <summary>
    /// Non-authoritative JPG sidecar ownership. It never changes JSON slot
    /// validity: missing or invalid image bytes simply resolve to a placeholder.
    /// </summary>
    public sealed class VNThumbnailService
    {
        public const int Width = 480;
        public const int Height = 270;
        public const int JpegQuality = 75;
        private const float TargetAspect = 16f / 9f;

        public bool TryGetCanonicalFileName(VNSaveSlotKey slotKey, out string fileName)
            => slotKey.TryGetCanonicalThumbnailFileName(out fileName);

        public VNThumbnailLoadResult LoadThumbnail(VNSaveRepository repository, VNSaveSlotKey slotKey, string thumbnailFileName)
        {
            if (repository == null || !slotKey.TryGetCanonicalThumbnailFileName(out var canonicalFileName) || thumbnailFileName != canonicalFileName)
                return VNThumbnailLoadResult.Placeholder("Thumbnail metadata is missing or not canonical for this slot.");
            if (!repository.TryGetThumbnailSidecarPath(slotKey, thumbnailFileName, out var path))
                return VNThumbnailLoadResult.Placeholder("Thumbnail path was rejected.");

            try
            {
                if (!File.Exists(path)) return VNThumbnailLoadResult.Placeholder();
                var bytes = File.ReadAllBytes(path);
                if (bytes.Length == 0) return VNThumbnailLoadResult.Placeholder("Thumbnail file is empty.");

                var texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
                if (!ImageConversion.LoadImage(texture, bytes, false))
                {
                    ReleaseRuntimeTexture(texture);
                    return VNThumbnailLoadResult.Placeholder("Thumbnail image bytes could not be decoded.");
                }

                return VNThumbnailLoadResult.Loaded(texture);
            }
            catch (Exception)
            {
                return VNThumbnailLoadResult.Placeholder("Thumbnail sidecar could not be read.");
            }
        }

        /// <summary>
        /// Captures after the caller has hidden modal visuals and waited for the
        /// end of the frame. The actual capture is deliberately not EditMode
        /// tested; its presentation correctness is a later human Play Gate.
        /// </summary>
        public IEnumerator CaptureCurrentGameViewJpg(Action<byte[], string> completed)
        {
            yield return new WaitForEndOfFrame();

            Texture2D screenshot = null;
            Texture2D resized = null;
            RenderTexture renderTexture = null;
            var previousActive = RenderTexture.active;
            try
            {
                screenshot = ScreenCapture.CaptureScreenshotAsTexture();
                if (screenshot == null || screenshot.width <= 0 || screenshot.height <= 0)
                {
                    completed?.Invoke(null, "Game View screenshot capture returned no pixels.");
                    yield break;
                }

                renderTexture = RenderTexture.GetTemporary(Width, Height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                GetCenterCropScaleAndOffset(screenshot.width, screenshot.height, out var scale, out var offset);
                Graphics.Blit(screenshot, renderTexture, scale, offset);
                RenderTexture.active = renderTexture;
                resized = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                resized.ReadPixels(new Rect(0f, 0f, Width, Height), 0, 0, false);
                resized.Apply(false, false);
                completed?.Invoke(resized.EncodeToJPG(JpegQuality), null);
            }
            catch (Exception)
            {
                completed?.Invoke(null, "Game View thumbnail capture failed.");
            }
            finally
            {
                RenderTexture.active = previousActive;
                if (renderTexture != null) RenderTexture.ReleaseTemporary(renderTexture);
                if (screenshot != null) UnityEngine.Object.Destroy(screenshot);
                if (resized != null) UnityEngine.Object.Destroy(resized);
            }
        }

        public VNSaveOperationResult WriteJpgSidecar(VNSaveRepository repository, VNSaveSlotKey slotKey, byte[] jpgBytes)
        {
            if (repository == null || jpgBytes == null || jpgBytes.Length == 0)
                return VNSaveOperationResult.Failure("No thumbnail JPG bytes were available to write.");
            if (!slotKey.TryGetCanonicalThumbnailFileName(out var fileName) || !repository.TryGetThumbnailSidecarPath(slotKey, fileName, out var destinationPath))
                return VNSaveOperationResult.Failure("The thumbnail sidecar path is invalid.");

            string temporaryPath = null;
            try
            {
                Directory.CreateDirectory(repository.StorageRoot);
                temporaryPath = Path.Combine(repository.StorageRoot, fileName + "." + Guid.NewGuid().ToString("N") + ".tmp");
                using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(jpgBytes, 0, jpgBytes.Length);
                    stream.Flush(true);
                }

                if (File.Exists(destinationPath)) File.Replace(temporaryPath, destinationPath, null);
                else File.Move(temporaryPath, destinationPath);
                return VNSaveOperationResult.Success();
            }
            catch (Exception)
            {
                TryDeleteExactFile(temporaryPath);
                return VNSaveOperationResult.Failure("Thumbnail sidecar could not be written.");
            }
        }

        /// <summary>
        /// Best-effort invalidation after a new authoritative JSON write and
        /// before asynchronous capture. A failure is only a thumbnail warning.
        /// </summary>
        public bool TryRemoveJpgSidecar(VNSaveRepository repository, VNSaveSlotKey slotKey)
        {
            if (repository == null || !slotKey.TryGetCanonicalThumbnailFileName(out var fileName) || !repository.TryGetThumbnailSidecarPath(slotKey, fileName, out var path)) return false;
            try
            {
                if (File.Exists(path)) File.Delete(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void GetCenterCropScaleAndOffset(int sourceWidth, int sourceHeight, out Vector2 scale, out Vector2 offset)
        {
            scale = Vector2.one;
            offset = Vector2.zero;
            if (sourceWidth <= 0 || sourceHeight <= 0) return;

            var sourceAspect = sourceWidth / (float)sourceHeight;
            if (sourceAspect > TargetAspect)
            {
                var visibleWidth = TargetAspect / sourceAspect;
                scale = new Vector2(visibleWidth, 1f);
                offset = new Vector2((1f - visibleWidth) * 0.5f, 0f);
            }
            else if (sourceAspect < TargetAspect)
            {
                var visibleHeight = sourceAspect / TargetAspect;
                scale = new Vector2(1f, visibleHeight);
                offset = new Vector2(0f, (1f - visibleHeight) * 0.5f);
            }
        }

        /// <summary>Releases a caller-owned runtime thumbnail in both player and EditMode test contexts.</summary>
        public static void ReleaseRuntimeTexture(Texture2D texture)
        {
            if (texture == null) return;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(texture);
                return;
            }
#endif
            UnityEngine.Object.Destroy(texture);
        }

        private static void TryDeleteExactFile(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception)
            {
                // The JSON slot is never affected by thumbnail cleanup failure.
            }
        }
    }
}
