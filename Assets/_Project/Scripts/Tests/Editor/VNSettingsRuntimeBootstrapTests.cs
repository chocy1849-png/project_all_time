using System;
using System.Reflection;
using NUnit.Framework;
using ProjectAllTime.VN.Dialogue;
using ProjectAllTime.VN.Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using Yarn.Unity;

namespace ProjectAllTime.Tests.Editor
{
    public sealed class VNSettingsRuntimeBootstrapTests
    {
        private GameObject root;
        private VNSettingsRuntimeBootstrap bootstrap;
        private InputActionAsset input;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Bootstrap contract test");
            root.SetActive(false);
            bootstrap = root.AddComponent<VNSettingsRuntimeBootstrap>();
            input = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<InputActionAsset>(VNMainM7SettingsWiringAuditTests.InputPath));
            Set("dialogueRunner", root.AddComponent<DialogueRunner>());
            Set("settingsPanel", root.AddComponent<VNSettingsPanel>());
            Set("audioMixer", AssetDatabase.LoadAssetAtPath<AudioMixer>(VNMainM7SettingsWiringAuditTests.MixerPath));
            Set("inputActions", input);
        }

        [TearDown]
        public void TearDown()
        {
            // Explicit cleanup also covers objects which were never activated in EditMode.
            if (bootstrap != null) Invoke("OnDestroy");
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(input);
        }

        [TestCase("dialogueRunner", "Dialogue Runner")]
        [TestCase("settingsPanel", "Settings Panel")]
        [TestCase("audioMixer", "Audio Mixer")]
        [TestCase("inputActions", "Input Actions")]
        public void MissingSerializedDependency_FailsBeforePersistenceOrApplication(string field, string expected)
        {
            Set(field, null);
            Invoke("Initialize");
            Assert.That(bootstrap.IsInitialized, Is.False);
            Assert.That(bootstrap.SettingsService, Is.Null);
            Assert.That(bootstrap.LastDiagnostic, Does.Contain(expected));
            Invoke("Initialize");
            Assert.That(bootstrap.SettingsService, Is.Null);
        }

        [Test]
        public void MissingSiblingController_FailsClearlyWithoutAddingIt()
        {
            root.AddComponent<VNConvenienceInputRouter>();
            Assert.That(bootstrap.TryValidateWiring(out var diagnostic), Is.False);
            Assert.That(diagnostic, Does.Contain("sibling VNConvenienceController"));
            Assert.That(root.GetComponent<VNConvenienceController>(), Is.Null);
        }

        [Test]
        public void MissingSiblingRouter_FailsClearlyWithoutAddingIt()
        {
            root.AddComponent<VNConvenienceController>();
            Assert.That(bootstrap.TryValidateWiring(out var diagnostic), Is.False);
            Assert.That(diagnostic, Does.Contain("sibling VNConvenienceInputRouter"));
            Assert.That(root.GetComponent<VNConvenienceInputRouter>(), Is.Null);
        }

        [Test]
        public void InvalidPanel_IsFatalBeforeLoadingSettings()
        {
            root.AddComponent<VNConvenienceController>();
            root.AddComponent<VNConvenienceInputRouter>();
            Invoke("Initialize");
            Assert.That(bootstrap.IsInitialized, Is.False);
            Assert.That(bootstrap.SettingsService, Is.Null);
            Assert.That(bootstrap.LastDiagnostic, Does.Contain("Settings Panel"));
        }

        [Test]
        public void RepeatedOwnerConstruction_RetainsSingleServiceAndAllControllerIdentities()
        {
            root.AddComponent<VNConvenienceController>();
            root.AddComponent<VNConvenienceInputRouter>();
            var service = UnloadedService();
            Invoke("CreateRuntimeOwners", service);
            var fields = new[] { "displayController", "textAutoController", "audioController", "gameplayController", "rebindService" };
            var owners = Array.ConvertAll(fields, Get);
            Invoke("CreateRuntimeOwners", UnloadedService());
            Assert.That(bootstrap.SettingsService, Is.SameAs(service));
            for (var i = 0; i < fields.Length; i++)
            {
                Assert.That(owners[i], Is.Not.Null);
                Assert.That(Get(fields[i]), Is.SameAs(owners[i]));
            }
            // A repeated lifecycle call must not load or replace the composed session.
            Set("initializationAttempted", true);
            Invoke("Initialize");
            Assert.That(bootstrap.SettingsService, Is.SameAs(service));
        }

        [Test]
        public void Destruction_DisposesOwnedCaptureAndReleasesRouterSuspension()
        {
            root.AddComponent<VNConvenienceController>();
            var router = root.AddComponent<VNConvenienceInputRouter>();
            Invoke("CreateRuntimeOwners", UnloadedService());
            var rebind = (VNInputRebindService)Get("rebindService");
            Assert.That(rebind.BeginRebind(VNRebindTarget.ToggleAuto, null, out var diagnostic), Is.True, diagnostic);
            Assert.That(router.IsRebindCaptureSuspended, Is.True);
            Invoke("OnDestroy");
            Invoke("OnDestroy");
            Assert.That(rebind.IsRebinding, Is.False);
            Assert.That(router.IsRebindCaptureSuspended, Is.False);
            Assert.That(rebind.BeginRebind(VNRebindTarget.ToggleAuto, null, out diagnostic), Is.False);
            Assert.That(diagnostic, Does.Contain("disposed"));
        }

        [Test]
        public void Destruction_BeforeComposition_IsSafe()
        {
            Assert.DoesNotThrow(() => Invoke("OnDestroy"));
        }

        private static VNSettingsService UnloadedService() => new(new VNSettingsRepository());
        private object Get(string field) => typeof(VNSettingsRuntimeBootstrap).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(bootstrap);
        private void Set(string field, object value) => typeof(VNSettingsRuntimeBootstrap).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(bootstrap, value);
        private void Invoke(string method, params object[] arguments) => typeof(VNSettingsRuntimeBootstrap).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(bootstrap, arguments);
    }
}
