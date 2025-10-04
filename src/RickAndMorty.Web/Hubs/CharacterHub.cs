namespace RickAndMorty.Web.Hubs;

using Microsoft.AspNetCore.SignalR;

public sealed class CharacterHub : Hub
{
    public async Task NotifyCharacterAdded(string characterName)
    {
        await Clients.All.SendCoreAsync("CharacterAdded", [characterName]);
    }
    
    public async Task NotifyCacheInvalidated()
    {
        await Clients.All.SendCoreAsync("CacheInvalidated", []);
    }
}