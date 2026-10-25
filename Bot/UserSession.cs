using GitAssist.bot;
using GitAssist.Models;
namespace GitAssist.Bot;

public class UserSession
{
    public UserState State { get; set; } = UserState.None;
    public string? PendingOwner { get; set; }
    public ActiveRepository? ActiveRepository { get; set; }
}