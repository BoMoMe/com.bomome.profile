using System;
using UnityEngine;

namespace Bomome.Profile
{
    public interface IPlayerProfileService
    {
        event Action<PlayerProfileSnapshot> ProfileChanged;

        PlayerProfileSnapshot Snapshot { get; }
        PlayerProfileConfig Config { get; }

        ProfileNameValidationResult ValidateName(string playerName);
        bool TrySetName(string playerName, out ProfileNameValidationResult result);
        bool TrySetProfilePicture(string profilePictureId);
        bool TrySetProfilePictureAt(int index);
    }

    public interface IProfileEditorService
    {
        void Show();
        void Hide();
    }

    public readonly struct PlayerProfileSnapshot
    {
        public PlayerProfileSnapshot(string playerName, string profilePictureId, Sprite profilePicture)
        {
            PlayerName = playerName;
            ProfilePictureId = profilePictureId;
            ProfilePicture = profilePicture;
        }

        public string PlayerName { get; }
        public string ProfilePictureId { get; }
        public Sprite ProfilePicture { get; }
    }

    public readonly struct ProfileNameValidationResult
    {
        public ProfileNameValidationResult(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message ?? string.Empty;
        }

        public bool IsValid { get; }
        public string Message { get; }

        public static ProfileNameValidationResult Valid => new(true, string.Empty);
    }

    public readonly struct PlayerProfileChangedMessage
    {
        public PlayerProfileChangedMessage(PlayerProfileSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public PlayerProfileSnapshot Snapshot { get; }
    }
}
