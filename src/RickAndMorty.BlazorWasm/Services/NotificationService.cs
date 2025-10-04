namespace RickAndMorty.BlazorWasm.Services;

public interface INotificationService
{
    event Action? OnNotificationsChanged;
    List<CharacterNotification> Notifications { get; }
    int UnreadCount { get; }
    DateTime? LastCheckTime { get; }
    void AddNotification(string characterName);
    void MarkAllAsRead();
    void ClearAll();
}

public sealed class NotificationService : INotificationService
{
    private readonly List<CharacterNotification> _notifications = new();

    public event Action? OnNotificationsChanged;
    
    public List<CharacterNotification> Notifications => _notifications;
    
    public int UnreadCount => _notifications.Count(n => !n.IsRead);
    
    public DateTime? LastCheckTime { get; private set; }

    public void AddNotification(string characterName)
    {
        var notification = new CharacterNotification
        {
            Id = Guid.NewGuid(),
            CharacterName = characterName,
            Timestamp = DateTime.UtcNow,
            IsRead = false
        };
        
        _notifications.Insert(0, notification);
        
        OnNotificationsChanged?.Invoke();
        
        Console.WriteLine($"Notification added: {characterName}. Total: {_notifications.Count}, Unread: {UnreadCount}");
    }

    public void MarkAllAsRead()
    {
        foreach (var notification in _notifications)
        {
            notification.IsRead = true;
        }
        
        LastCheckTime = DateTime.UtcNow;
        OnNotificationsChanged?.Invoke();
    }

    public void ClearAll()
    {
        _notifications.Clear();
        LastCheckTime = DateTime.UtcNow;
        OnNotificationsChanged?.Invoke();
    }
}

public sealed class CharacterNotification
{
    public Guid Id { get; init; }
    public string CharacterName { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public bool IsRead { get; set; }
}