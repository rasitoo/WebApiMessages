namespace WebApiMessages.Controllers;

using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using WebApiMessages.Models.DTO;

public class MessageHub : Hub
{
    public async Task JoinChat(string chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        await Clients.Group(chatId).SendAsync("UserJoined", $"{Context.ConnectionId} joined the chat {chatId}");
    }

    public async Task LeaveChat(string chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
        await Clients.Group(chatId).SendAsync("UserLeft", $"{Context.ConnectionId} left chat {chatId}");
    }

    public async Task NotifyMessageCreated(MessageReadDTO message)
    {
        var chatId = message.ChatId.ToString();
        if (!string.IsNullOrEmpty(chatId))
        {
            await Clients.Group(chatId).SendAsync("ReceiveMessage", message);
        }
    }

    public async Task NotifyMessageUpdated(MessageReadDTO message)
    {
        var chatId = message.ChatId.ToString();
        if (!string.IsNullOrEmpty(chatId))
        {
            await Clients.Group(chatId).SendAsync("MessageUpdated", message);
        }
    }

    public async Task NotifyMessageDeleted(int chatId, int messageId)
    {
        await Clients.Group(chatId.ToString()).SendAsync("MessageDeleted", messageId);
    }

    //public async Task NotifyChatCreated(ChatReadDTO chat)
    //{
    //    var chatId = chat.Id.ToString();
    //    if (!string.IsNullOrEmpty(chatId))
    //    {
    //        await Clients.All.SendAsync("ChatCreated", chat);
    //    }
    //}

    public async Task NotifyChatUpdated(ChatReadDTO chat)
    {
        var chatId = chat.Id.ToString();
        if (!string.IsNullOrEmpty(chatId))
        {
            await Clients.Group(chatId).SendAsync("ChatUpdated", chat);
        }
    }

    public async Task NotifyChatDeleted(int chatId)
    {
        await Clients.Group(chatId.ToString()).SendAsync("ChatDeleted", chatId);
    }
}
