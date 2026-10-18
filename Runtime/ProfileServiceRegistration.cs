using System;
using Bomome.Core.Messaging;
using Bomome.Core.Services;
using Bomome.Persistence;

namespace Bomome.Profile
{
    public static class ProfileServiceRegistration
    {
        public static void RegisterPlayerProfile(this IServiceRegistry registry,
            PlayerProfileConfig config, SaveKey saveKey)
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry));
            if (config == null) throw new ArgumentNullException(nameof(config));
            registry.RegisterFactory<IPlayerProfileService>(resolver => new PlayerProfileService(
                config, resolver.Resolve<ISaveStore>(), saveKey, resolver.Resolve<IMessageBus>()));
        }
    }
}
