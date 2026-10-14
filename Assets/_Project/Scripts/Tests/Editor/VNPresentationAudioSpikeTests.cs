using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.Audio;
using UnityEngine;

namespace ProjectAllTime.Tests.Editor
{
    /// <summary>TECHNICAL / NON-CANON generated audio, never production IDs.</summary>
    public sealed class VNPresentationAudioSpikeTests
    {
        private GameObject root;
        private VNAudioCatalog catalog;
        private AudioClip clip;
        private VNAudioController audio;
        private AudioSource bgm;
        private AudioSource sfx;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Technical audio");
            audio = root.AddComponent<VNAudioController>();
            bgm = root.AddComponent<AudioSource>();
            var second = root.AddComponent<AudioSource>();
            sfx = root.AddComponent<AudioSource>();
            clip = AudioClip.Create("Finite equipment to rain TECHNICAL", 44100, 1, 44100, false);
            catalog = ScriptableObject.CreateInstance<VNAudioCatalog>();
            var music = new VNBgmCatalogEntry();
            Set(music, "id", "technical_music"); Set(music, "clip", clip); Set(music, "defaultVolume", 0.6f);
            var sounds = new List<VNSfxCatalogEntry>();
            foreach (var id in new[] { "technical_handoff", "technical_drop", "technical_bus" })
            {
                var sound = new VNSfxCatalogEntry();
                Set(sound, "id", id); Set(sound, "clip", clip); sounds.Add(sound);
            }
            Set(catalog, "bgm", new List<VNBgmCatalogEntry> { music }); Set(catalog, "sfx", sounds);
            Set(audio, "catalog", catalog); Set(audio, "bgmSourceA", bgm);
            Set(audio, "bgmSourceB", second); Set(audio, "sfxSource", sfx);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root); Object.DestroyImmediate(catalog); Object.DestroyImmediate(clip);
        }

        [Test]
        public void SourceOnlyLowering_IsLostByExistingSnapshotRestore_RequiresDecision()
        {
            Assert.That(audio.PlayBgm("technical_music"), Is.True);
            bgm.Pause(); bgm.timeSamples = 11025;
            var position = bgm.timeSamples;
            bgm.volume *= 0.5f;
            Assert.That(audio.CurrentBgmId, Is.EqualTo("technical_music"));
            Assert.That(bgm.timeSamples, Is.EqualTo(position));
            Assert.That(bgm.volume, Is.EqualTo(0.3f).Within(0.001f));
            Assert.That(audio.TryCaptureStableState(out var snapshot, out _), Is.True);
            Assert.That(audio.TryPrepareRestore(snapshot, out var plan, out _), Is.True);
            audio.NormalizeTransientForLoad();
            Assert.That(audio.RestorePreparedState(plan, out _), Is.True);
            bgm.Pause();
            Assert.That(audio.CurrentBgmId, Is.EqualTo("technical_music"));
            Assert.That(bgm.timeSamples, Is.EqualTo(position));
            Assert.That(bgm.volume, Is.EqualTo(0.6f).Within(0.001f), "Story gain is absent from AudioState.");
            Assert.That(bgm.outputAudioMixerGroup, Is.Null, "No mixer preference is touched by this proof.");
        }

        [Test]
        public void FiniteHandoff_AndSeparateFollowingOneShots_UseExistingSource_AndLoadStopsThem()
        {
            Assert.That(clip.length, Is.EqualTo(1f));
            foreach (var id in new[] { "technical_handoff", "technical_drop", "technical_bus" })
                Assert.That(audio.PlaySfx(id), Is.True);
            Assert.That(sfx.loop, Is.False);
            Assert.That(sfx.clip, Is.Null, "PlayOneShot does not replace the source clip or add an ambient loop.");
            Assert.That(root.GetComponents<AudioSource>().Length, Is.EqualTo(3));
            audio.NormalizeTransientForLoad();
            Assert.That(sfx.isPlaying, Is.False);
            Assert.That(audio.TryCaptureStableState(out var state, out _), Is.True);
            Assert.That(state.bgmId, Is.Empty);
        }

        private static void Set(object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
