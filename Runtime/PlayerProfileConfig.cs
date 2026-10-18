using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Bomome.Profile
{
    [MovedFrom(true, sourceNamespace: "Bomome.Profile", sourceAssembly: "Bomome")]
    [CreateAssetMenu(fileName = "PlayerProfileConfig", menuName = "Bomome/Profile/Player Profile Config")]
    public sealed class PlayerProfileConfig : ScriptableObject
    {
        [SerializeField] private string defaultPlayerName = "Player";
        [SerializeField] private int minNameLength = 3;
        [SerializeField] private int maxNameLength = 16;
        [SerializeField] private bool blockProfanity = true;
        [SerializeField] private TextAsset profanityWordsFile;
        [SerializeField] private List<ProfilePictureDefinition> profilePictures = new();

        public string DefaultPlayerName => string.IsNullOrWhiteSpace(defaultPlayerName) ? "Player" : defaultPlayerName.Trim();
        public int MinNameLength => Mathf.Max(1, minNameLength);
        public int MaxNameLength => Mathf.Max(MinNameLength, maxNameLength);
        public bool BlockProfanity => blockProfanity;
        public IReadOnlyList<ProfilePictureDefinition> ProfilePictures => profilePictures;

        public static PlayerProfileConfig RuntimeDefault
        {
            get
            {
                PlayerProfileConfig config = CreateInstance<PlayerProfileConfig>();
                config.name = "RuntimePlayerProfileConfig";
                return config;
            }
        }

        public ProfilePictureDefinition GetDefaultProfilePicture()
        {
            return profilePictures != null && profilePictures.Count > 0 ? profilePictures[0] : default;
        }

        public ProfilePictureDefinition GetProfilePicture(string id)
        {
            if (profilePictures == null || profilePictures.Count == 0)
            {
                return default;
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return profilePictures[0];
            }

            for (int i = 0; i < profilePictures.Count; i++)
            {
                ProfilePictureDefinition definition = profilePictures[i];
                if (string.Equals(definition.Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    return definition;
                }
            }

            return profilePictures[0];
        }

        public int IndexOfProfilePicture(string id)
        {
            if (profilePictures == null || profilePictures.Count == 0)
            {
                return -1;
            }

            for (int i = 0; i < profilePictures.Count; i++)
            {
                if (string.Equals(profilePictures[i].Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return 0;
        }

        public bool ContainsProfanity(string value)
        {
            if (!blockProfanity || string.IsNullOrWhiteSpace(value) || profanityWordsFile == null)
            {
                return false;
            }

            string normalizedValue = NormalizeForProfanity(value);
            string[] words = profanityWordsFile.text.Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                string word = NormalizeForProfanity(words[i]);
                if (word.Length == 0)
                {
                    continue;
                }

                if (normalizedValue.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static string NormalizeForProfanity(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(char.ToLowerInvariant(character));
                }
            }

            return builder.ToString();
        }
    }

    [Serializable]
    public struct ProfilePictureDefinition
    {
        [SerializeField] private string id;
        [SerializeField] private Sprite sprite;

        public string Id => string.IsNullOrWhiteSpace(id) ? (sprite != null ? sprite.name : string.Empty) : id.Trim();
        public Sprite Sprite => sprite;
        public bool IsValid => !string.IsNullOrWhiteSpace(Id) && sprite != null;
    }
}
