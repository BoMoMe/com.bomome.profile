# Bomome Profile (Beta)

Save-backed local identity extracted from BattleTroops. Requires Bomome Core and Persistence, Unity 6000.4+. No Firebase, Feel, combat, meta progression, TMP or prefab dependencies.

## Features

- Name and avatar identity with stable avatar IDs.
- Configurable lengths, default identity and simple text-file profanity filtering.
- Save normalization, change events and message-bus notifications.
- Explicit save-key injection and a registration helper.

## Integration

Register ISaveStore and IMessageBus, then call registry.RegisterPlayerProfile(config, new SaveKey("player_profile.v1")). Resolve IPlayerProfileService and observe ProfileChanged. Direct construction is also supported. The filter intentionally preserves simple substring matching, not comprehensive moderation.

The editor UI/prefab and scene wiring belong to ProtoFactory's Features/Profile example. Rank, trophies, fake opponents, authentication and remote accounts are not part of this package.

## Migration

Existing BattleTroops namespaces, saved field names and original script GUIDs are retained. Types move from assembly Bomome to Bomome.Profile. Do not retain duplicate local definitions or script GUIDs alongside this package. Keep the existing save key. Consumer avatar art/configs/profanity files stay consumer-owned.

This unpublished beta is referenced by relative file paths during the housekeeping experiment. Publish/version it and replace local paths before sharing projects with machines that lack this library checkout. No remote release/tag has been created.

Tests/Editor covers profile contracts. ProtoFactory provides the minimal no-combat integration; BattleTroops verifies existing config/prefab compatibility.
