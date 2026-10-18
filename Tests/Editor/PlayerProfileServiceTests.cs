using System;
using Bomome.Core.Messaging;
using Bomome.Persistence;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Bomome.Profile.Tests
{
    public sealed class PlayerProfileServiceTests
    {
        private PlayerProfileConfig config;
        private SaveStore store;
        private MessageBus bus;
        private readonly SaveKey key = new SaveKey("profile.contract.v1");

        [SetUp]
        public void SetUp()
        {
            config = ScriptableObject.CreateInstance<PlayerProfileConfig>();
            store = new SaveStore(new InMemorySaveBackend(), new JsonUtilitySaveCodec());
            bus = new MessageBus();
        }

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(config);

        [Test]
        public void NameIsTrimmedAndSurvivesServiceRecreation()
        {
            var service = Create();
            Assert.That(service.TrySetName("  Pilot_42  ", out _), Is.True);
            Assert.That(Create().Snapshot.PlayerName, Is.EqualTo("Pilot_42"));
        }

        [TestCase("")]
        [TestCase("ab")]
        [TestCase("12345678901234567")]
        public void InvalidNamesDoNotReplaceSavedName(string name)
        {
            var service = Create();
            Assert.That(service.TrySetName(name, out _), Is.False);
            Assert.That(Create().Snapshot.PlayerName, Is.EqualTo("Player"));
        }

        [Test]
        public void SameNameDoesNotEmitDuplicateChanges()
        {
            var service = Create();
            int local = 0, published = 0;
            service.ProfileChanged += _ => local++;
            using (bus.Subscribe<PlayerProfileChangedMessage>(_ => published++))
            {
                service.TrySetName("Pilot_42", out _);
                service.TrySetName("Pilot_42", out _);
            }
            Assert.That(local, Is.EqualTo(1));
            Assert.That(published, Is.EqualTo(1));
        }

        [Test]
        public void ExistingSaveFieldNamesRemainReadable()
        {
            store.Save(key, JsonUtility.FromJson<PlayerProfileSaveData>(
                "{\"playerName\":\"Legacy_7\",\"profilePictureId\":\"\"}"));
            Assert.That(Create().Snapshot.PlayerName, Is.EqualTo("Legacy_7"));
        }

        [Test]
        public void ProfanityNormalizationPreservesExistingSubstringPolicy()
        {
            var words = new TextAsset("bad\nblocked");
            try
            {
                var so = new SerializedObject(config);
                so.FindProperty("profanityWordsFile").objectReferenceValue = words;
                so.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(Create().ValidateName("B.a.d").IsValid, Is.False);
                Assert.That(Create().ValidateName("Good_7").IsValid, Is.True);
            }
            finally { UnityEngine.Object.DestroyImmediate(words); }
        }

        [Test]
        public void ConfiguredAvatarIdPersistsAndMissingIdFallsBack()
        {
            var texture = new Texture2D(2, 2);
            var sprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.one * 0.5f);
            try
            {
                var so = new SerializedObject(config);
                var pictures = so.FindProperty("profilePictures");
                pictures.arraySize = 1;
                pictures.GetArrayElementAtIndex(0).FindPropertyRelative("id").stringValue = "pilot";
                pictures.GetArrayElementAtIndex(0).FindPropertyRelative("sprite").objectReferenceValue = sprite;
                so.ApplyModifiedPropertiesWithoutUndo();
                var service = Create();
                Assert.That(service.TrySetProfilePicture("pilot"), Is.True);
                Assert.That(Create().Snapshot.ProfilePictureId, Is.EqualTo("pilot"));
                Assert.That(service.TrySetProfilePicture("removed-id"), Is.True);
                Assert.That(service.Snapshot.ProfilePictureId, Is.EqualTo("pilot"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(sprite);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private PlayerProfileService Create() => new PlayerProfileService(config, store, key, bus);
    }
}
