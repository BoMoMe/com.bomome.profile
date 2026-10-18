using System;
using Bomome.Core.Messaging;
using Bomome.Persistence;
using UnityEngine;

namespace Bomome.Profile
{
    public sealed class PlayerProfileService : IPlayerProfileService
    {
        private readonly PlayerProfileConfig config;
        private readonly ISaveStore saveStore;
        private readonly SaveKey saveKey;
        private readonly IMessageBus messageBus;
        private PlayerProfileSaveData saveData;

        public PlayerProfileService(
            PlayerProfileConfig config,
            ISaveStore saveStore,
            SaveKey saveKey,
            IMessageBus messageBus)
        {
            this.config = config != null ? config : PlayerProfileConfig.RuntimeDefault;
            this.saveStore = saveStore ?? throw new ArgumentNullException(nameof(saveStore));
            this.saveKey = saveKey;
            this.messageBus = messageBus;

            saveData = this.saveStore.LoadOrDefault(this.saveKey, new PlayerProfileSaveData(), IsValidSaveData);
            NormalizeAndSaveIfNeeded();
        }

        public event Action<PlayerProfileSnapshot> ProfileChanged;

        public PlayerProfileSnapshot Snapshot => CreateSnapshot();
        public PlayerProfileConfig Config => config;

        public ProfileNameValidationResult ValidateName(string playerName)
        {
            string normalizedName = NormalizeName(playerName);
            if (normalizedName.Length < config.MinNameLength)
            {
                return new ProfileNameValidationResult(false, $"Name must be at least {config.MinNameLength} characters.");
            }

            if (normalizedName.Length > config.MaxNameLength)
            {
                return new ProfileNameValidationResult(false, $"Name must be {config.MaxNameLength} characters or less.");
            }

            if (config.ContainsProfanity(normalizedName))
            {
                return new ProfileNameValidationResult(false, "Pick a different name.");
            }

            return ProfileNameValidationResult.Valid;
        }

        public bool TrySetName(string playerName, out ProfileNameValidationResult result)
        {
            string normalizedName = NormalizeName(playerName);
            result = ValidateName(normalizedName);
            if (!result.IsValid)
            {
                return false;
            }

            if (string.Equals(saveData.playerName, normalizedName, StringComparison.Ordinal))
            {
                return true;
            }

            saveData.playerName = normalizedName;
            SaveAndPublish();
            return true;
        }

        public bool TrySetProfilePicture(string profilePictureId)
        {
            ProfilePictureDefinition definition = config.GetProfilePicture(profilePictureId);
            if (!definition.IsValid)
            {
                return false;
            }

            if (string.Equals(saveData.profilePictureId, definition.Id, StringComparison.Ordinal))
            {
                return true;
            }

            saveData.profilePictureId = definition.Id;
            SaveAndPublish();
            return true;
        }

        public bool TrySetProfilePictureAt(int index)
        {
            if (config.ProfilePictures == null || index < 0 || index >= config.ProfilePictures.Count)
            {
                return false;
            }

            return TrySetProfilePicture(config.ProfilePictures[index].Id);
        }

        private void NormalizeAndSaveIfNeeded()
        {
            bool changed = false;
            if (saveData == null)
            {
                saveData = new PlayerProfileSaveData();
                changed = true;
            }

            string normalizedName = NormalizeName(saveData.playerName);
            if (string.IsNullOrWhiteSpace(normalizedName) || !ValidateName(normalizedName).IsValid)
            {
                normalizedName = config.DefaultPlayerName;
            }

            if (!string.Equals(saveData.playerName, normalizedName, StringComparison.Ordinal))
            {
                saveData.playerName = normalizedName;
                changed = true;
            }

            ProfilePictureDefinition picture = config.GetProfilePicture(saveData.profilePictureId);
            string normalizedPictureId = picture.IsValid ? picture.Id : string.Empty;
            if (!string.Equals(saveData.profilePictureId, normalizedPictureId, StringComparison.Ordinal))
            {
                saveData.profilePictureId = normalizedPictureId;
                changed = true;
            }

            if (changed)
            {
                Save();
            }
        }

        private PlayerProfileSnapshot CreateSnapshot()
        {
            ProfilePictureDefinition picture = config.GetProfilePicture(saveData.profilePictureId);
            return new PlayerProfileSnapshot(
                string.IsNullOrWhiteSpace(saveData.playerName) ? config.DefaultPlayerName : saveData.playerName,
                picture.Id,
                picture.Sprite);
        }

        private void SaveAndPublish()
        {
            Save();
            PlayerProfileSnapshot snapshot = Snapshot;
            ProfileChanged?.Invoke(snapshot);
            messageBus?.Publish(new PlayerProfileChangedMessage(snapshot));
        }

        private void Save()
        {
            saveStore.Save(saveKey, saveData);
            saveStore.Flush();
        }

        private static string NormalizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.Trim();
        }

        private static bool IsValidSaveData(PlayerProfileSaveData data)
        {
            return data != null;
        }
    }
}
