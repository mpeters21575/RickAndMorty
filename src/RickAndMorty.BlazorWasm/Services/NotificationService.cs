using RickAndMorty.BlazorWasm.Models;

public interface INotificationService
{
    event Action? OnNotificationsChanged;
    List<CharacterNotification> Notifications { get; }
    int UnreadCount { get; }
    void AddNotification(string characterName, string imageUrl = "");
    void MarkAllAsRead();
    void ClearAll();
}

public sealed class NotificationService : INotificationService
{
    private readonly List<CharacterNotification> _notifications = new();
    private const int MaxNotifications = 50;

    public event Action? OnNotificationsChanged;
    
    public List<CharacterNotification> Notifications => _notifications;
    
    public int UnreadCount => _notifications.Count(n => !n.IsRead);

    public void AddNotification(string characterName, string imageUrl = "")
    {
        var notification = new CharacterNotification
        {
            Id = Guid.NewGuid(),
            CharacterName = characterName,
            ImageUrl = imageUrl,
            Timestamp = DateTime.UtcNow,
            IsRead = false
        };
        
        _notifications.Insert(0, notification);

        if (_notifications.Count > MaxNotifications)
        {
            _notifications.RemoveAt(_notifications.Count - 1);
        }

        OnNotificationsChanged?.Invoke();
    }

    public void MarkAllAsRead()
    {
        foreach (var notification in _notifications)
        {
            notification.IsRead = true;
        }
        
        OnNotificationsChanged?.Invoke();
    }

    public void ClearAll()
    {
        _notifications.Clear();
        OnNotificationsChanged?.Invoke();
    }
}