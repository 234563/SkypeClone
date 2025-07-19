# ChatHub Caching Implementation

## Overview
The ChatHub now includes caching functionality to store online user IDs and provide real-time online user information to new users when they join.

## Features

### 1. Online Users Caching
- Automatically stores user IDs when they connect
- Removes user IDs when they disconnect
- Provides structured data with `OnlineUserDTO`

### 2. Real-time Online Users List
- New users receive the list of online users when they join a room
- Clients can request the current online users list at any time

## Client-Side Usage

### JavaScript/TypeScript Example

```typescript
// Connect to SignalR hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub", { accessTokenFactory: () => token })
    .build();

// Listen for online users list
connection.on("OnlineUsersList", (onlineUsers) => {
    console.log("Online users:", onlineUsers);
    // onlineUsers is an array of OnlineUserDTO objects
    // Each object contains: UserId, ConnectionId, ConnectedAt, Status
});

// Listen for user status changes
connection.on("UserStatusChanged", (userId, status) => {
    console.log(`User ${userId} is now ${status}`);
});

// Request online users list
await connection.invoke("GetOnlineUsers");

// Join a room (automatically receives online users list)
await connection.invoke("JoinRoom", roomId);
```

### OnlineUserDTO Structure
```typescript
interface OnlineUserDTO {
    userId: string;
    connectionId: string;
    connectedAt: string; // ISO date string
    status: string; // "Online" or "Offline"
}
```

## Server-Side Methods

### Available Hub Methods
- `JoinRoom(int roomId)` - Join a chat room and receive online users list
- `LeaveRoom(int roomId)` - Leave a chat room
- `GetOnlineUsers()` - Request current online users list
- `SendMessageToRoom(string roomId, CreateSendMessageDTO message)` - Send message to room

### Available Client Events
- `OnlineUsersList(OnlineUserDTO[] users)` - Received when joining room or requesting online users
- `UserStatusChanged(string userId, string status)` - Received when user status changes
- `UserJoined(string userId)` - Received when user joins room
- `UserLeft(string userId)` - Received when user leaves room
- `ReceiveMessage(CreateSendMessageDTO message)` - Received when new message arrives

## Cache Configuration
- Cache duration: 24 hours
- Cache key: "online_users"
- Automatic cleanup on user disconnect

## Error Handling
The implementation includes comprehensive error handling and logging for all operations. 