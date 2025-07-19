-- Create database only if it doesn't exist
IF DB_ID('SkypeDb') IS NULL
    CREATE DATABASE SkypeDb;
GO

-- Use SkypeDb
USE SkypeDb;
GO

-- Create Users table if not exists
IF OBJECT_ID('Users', 'U') IS NULL
BEGIN
    CREATE TABLE Users (
        Id INT PRIMARY KEY IDENTITY(1,1),
        FullName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(256) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
		EnableNotification BIT NOT NULL DEFAULT 1,         -- 1 = enabled, 0 = disabled
		AppearInSearchResult BIT NOT NULL DEFAULT 1,       -- 1 = visible, 0 = hidden
	    ShowOnlineStatus BIT NOT NULL DEFAULT 1,           -- 1 = show online status
		DefaultTheme INT DEFAULT 0,                        -- 0 = 'Light', 1 = 'Dark'
		ChatAvatar NVARCHAR(255) NULL,                     -- Path or URL to avatar image
		ChatRoomID INT NULL
    );
END
GO

-- Create Roles table if not exists
IF OBJECT_ID('Roles', 'U') IS NULL
BEGIN
    CREATE TABLE Roles (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(50) NOT NULL UNIQUE
    );
END
GO

-- Create UserRoles table if not exists
IF OBJECT_ID('UserRoles', 'U') IS NULL
BEGIN
    CREATE TABLE UserRoles (
        UserId INT NOT NULL,
        RoleId INT NOT NULL,
        PRIMARY KEY (UserId, RoleId),
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
        FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
    );
END
GO

-- Insert default roles if not exists
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Admin')
    INSERT INTO Roles (Name) VALUES ('Admin');
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'User')
    INSERT INTO Roles (Name) VALUES ('User');
GO




-- Drop the RegisterUser procedure if it exists
IF OBJECT_ID('RegisterUser', 'P') IS NOT NULL BEGIN DROP PROCEDURE RegisterUser; END
GO

-- Create RegisterUser procedure with custom error codes and affected row count
CREATE PROCEDURE RegisterUser
    @FullName NVARCHAR(100),
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(256)
AS
BEGIN
    -- Check if the user already exists
    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
    BEGIN
        -- Return custom error code: 1001 for email already exists
        SELECT 1001 AS ErrorCode, 'User with this email already exists.' AS ErrorMessage, 0 AS AffectedRows , 0 AS UserId;
        RETURN;
    END

    -- Insert new user
    INSERT INTO Users (FullName, Email, PasswordHash)
    VALUES (@FullName, @Email, @PasswordHash);
	-- Get the last inserted ID
    DECLARE @NewUserId INT = SCOPE_IDENTITY();

    SELECT 0 AS ErrorCode, 'User registered successfully.' AS ErrorMessage, 
           @@ROWCOUNT AS AffectedRows, @NewUserId AS UserId;
END
GO


-- Drop the LoginUser procedure if it exists
IF OBJECT_ID('LoginUser', 'P') IS NOT NULL BEGIN DROP PROCEDURE LoginUser; END
GO

-- Create LoginUser procedure with custom error codes and affected row count
CREATE PROCEDURE LoginUser
    @Email NVARCHAR(100)
AS
BEGIN
    -- Declare variable to hold UserId and PasswordHash
    DECLARE @UserId INT, @PasswordHash NVARCHAR(256) , @FullName NVARCHAR(256) , @ChatRoomID INT ;

    -- Select user by email
    SELECT @UserId = Id, @PasswordHash = PasswordHash , @FullName = FullName , @ChatRoomID = ChatRoomID 
    FROM Users
    WHERE Email = @Email;

    IF @UserId IS NULL
    BEGIN
        -- Return custom error code: 1003 for user not found
        SELECT 1003 AS ErrorCode, 'User not found.' AS ErrorMessage, 0 AS AffectedRows;
        RETURN;
    END

    -- Return success code: 0, with affected rows count (1 row is returned), UserId, and PasswordHash for validation
    SELECT 0 AS ErrorCode, 'User found successfully.' AS ErrorMessage, 1 AS AffectedRows, @UserId AS UserId, @PasswordHash AS PasswordHash , @FullName AS FullName , @ChatRoomID AS ChatRoomID;
END
GO



-- Drop the AssignRoleToUser procedure if it exists
IF OBJECT_ID('AssignRoleToUser', 'P') IS NOT NULL BEGIN DROP PROCEDURE AssignRoleToUser; END
GO

-- Create AssignRoleToUser procedure with custom error codes and affected row count
CREATE PROCEDURE AssignRoleToUser
    @UserId INT,
    @RoleId INT
AS
BEGIN
    -- Check if the user-role relationship already exists
    IF EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
    BEGIN
        -- Return custom error code: 1002 for user already has the role
        SELECT 1002 AS ErrorCode, 'User already has this role.' AS ErrorMessage, 0 AS AffectedRows;
        RETURN;
    END

    -- Assign role to the user
    INSERT INTO UserRoles (UserId, RoleId)
    VALUES (@UserId, @RoleId);

    -- Return success code: 0, with affected rows count
    SELECT 0 AS ErrorCode, 'Role assigned to user successfully.' AS ErrorMessage, @@ROWCOUNT AS AffectedRows;
END
GO



---  Refresh TOken 
IF OBJECT_ID('RefreshTokens', 'U') IS NULL
BEGIN
CREATE TABLE RefreshTokens (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Token NVARCHAR(256) NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    UserId INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
END 
GO 


-- Drop the AssignRoleToUser procedure if it exists
IF OBJECT_ID('InsertRefreshToken', 'P') IS NOT NULL BEGIN DROP PROCEDURE InsertRefreshToken; END
GO

CREATE PROCEDURE InsertRefreshToken
    @Token NVARCHAR(256),
    @ExpiresAt DATETIME,
    @UserId INT
AS
BEGIN
    INSERT INTO RefreshTokens (Token, ExpiresAt, UserId)
    VALUES (@Token, @ExpiresAt, @UserId);

    SELECT SCOPE_IDENTITY() AS NewId;
END
Go

-- Drop the AssignRoleToUser procedure if it exists
IF OBJECT_ID('UpdateRefreshToken', 'P') IS NOT NULL BEGIN DROP PROCEDURE UpdateRefreshToken; END
GO

CREATE PROCEDURE UpdateRefreshToken
    @Id INT,
    @Token NVARCHAR(256),
    @ExpiresAt DATETIME
AS
BEGIN
    UPDATE RefreshTokens
    SET Token = @Token,
        ExpiresAt = @ExpiresAt
    WHERE Id = @Id;

    SELECT @@ROWCOUNT AS AffectedRows;
END
GO


-- Drop the AssignRoleToUser procedure if it exists
IF OBJECT_ID('DeleteRefreshToken', 'P') IS NOT NULL BEGIN DROP PROCEDURE DeleteRefreshToken; END
GO

CREATE PROCEDURE DeleteRefreshToken
    @Id INT
AS
BEGIN
    DELETE FROM RefreshTokens
    WHERE Id = @Id;

    SELECT @@ROWCOUNT AS AffectedRows;
END
Go

-- Drop the AssignRoleToUser procedure if it exists
IF OBJECT_ID('GetRefreshTokenByUserId', 'P') IS NOT NULL BEGIN DROP PROCEDURE GetRefreshTokenByUserId; END
GO

CREATE PROCEDURE GetRefreshTokenByUserId
    @UserId INT
AS
BEGIN
    SELECT TOP 1 *
    FROM RefreshTokens
    WHERE UserId = @UserId
    ORDER BY ExpiresAt DESC;
END;
GO


-- Drop the AssignRoleToUser procedure if it exists
IF OBJECT_ID('GetRefreshTokenByID', 'P') IS NOT NULL BEGIN DROP PROCEDURE GetRefreshTokenByID; END
GO

CREATE PROCEDURE GetRefreshTokenByID
    @Id INT
AS
BEGIN
    SELECT TOP 1 *
    FROM RefreshTokens
    WHERE Id = @Id
    ORDER BY ExpiresAt DESC;
END;
GO


-- Drop the AssignRoleToUser procedure if it exists
IF OBJECT_ID('UserRole_Select', 'P') IS NOT NULL BEGIN DROP PROCEDURE UserRole_Select ; END
GO

CREATE PROCEDURE UserRole_Select
    @UserId INT
AS
BEGIN
    SELECT TOP 1 ur.UserId , r.Id , r.Name
    FROM UserRoles ur
	Inner JOIn Roles r on ur.RoleId = r.Id
    WHERE UserId = @UserId;
END;
GO


-- Drop the AssignRoleToUser procedure if it exists
IF OBJECT_ID('GetRefreshToken', 'P') IS NOT NULL BEGIN DROP PROCEDURE GetRefreshToken; END
GO

CREATE PROCEDURE GetRefreshToken
    @Token nvarchar(200)
AS
BEGIN
    SELECT TOP 1 *
    FROM RefreshTokens
    WHERE Token = @Token
    ORDER BY ExpiresAt DESC;
END;
GO


---  Refresh TOken 
IF OBJECT_ID('ChatRooms', 'U') IS NULL
BEGIN
CREATE TABLE ChatRooms (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100),
    IsGroup BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);
END 
GO 


---  ChatRoomMembers  
IF OBJECT_ID('ChatRoomMembers', 'U') IS NULL
BEGIN
CREATE TABLE ChatRoomMembers (
    Id INT PRIMARY KEY IDENTITY,
    ChatRoomId INT FOREIGN KEY REFERENCES ChatRooms(Id) ON DELETE CASCADE,
    UserId INT FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    JoinedAt DATETIME DEFAULT GETDATE()
);
END 
GO


---  ChatRoomMembers  
IF OBJECT_ID('Messages', 'U') IS NULL
BEGIN
CREATE TABLE Messages (
    Id INT PRIMARY KEY IDENTITY,
    ChatRoomId INT FOREIGN KEY REFERENCES ChatRooms(Id) ON DELETE CASCADE,
    SenderId INT FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    Content NVARCHAR(MAX),
	Status NVARCHAR(20),
    SentAt DATETIME DEFAULT GETDATE()
);
END 
GO


---  MessageAttachments  
IF OBJECT_ID('MessageAttachments', 'U') IS NULL
BEGIN
CREATE TABLE MessageAttachments (
    Id INT PRIMARY KEY IDENTITY,
    MessageId INT FOREIGN KEY REFERENCES Messages(Id) ON DELETE CASCADE,
    FileUrl NVARCHAR(255),
    FileType NVARCHAR(50),
	FileName NVARCHAR(50),
    UploadedAt DATETIME DEFAULT GETDATE()
);
END 
GO

---  MessageReactions  
IF OBJECT_ID('MessageReactions', 'U') IS NULL
BEGIN
CREATE TABLE MessageReactions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MessageId INT NOT NULL,
	UserId INT FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    ReactionType NVARCHAR(50) NOT NULL, -- e.g., 'Like', 'Love', 'Haha'
    ReactedAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_MessageReactions_Messages FOREIGN KEY (MessageId) REFERENCES Messages(Id),
);
END 
GO


/* Chat Related Procedures  */

-- Drop the AssignRoleToUser procedure if it exists
IF OBJECT_ID('CreateChatRoom', 'P') IS NOT NULL BEGIN DROP PROCEDURE CreateChatRoom; END
GO

CREATE PROCEDURE CreateChatRoom
    @Name NVARCHAR(100),
    @IsGroup BIT,
    @CreatedAt DATETIME
AS
BEGIN
    INSERT INTO ChatRooms (Name, IsGroup, CreatedAt)
    VALUES (@Name, @IsGroup, @CreatedAt);

    SELECT SCOPE_IDENTITY() AS ChatRoomId;
END;
GO



-- Drop the AssignRoleToUser procedure if it exists
IF OBJECT_ID('AddChatRoomMember', 'P') IS NOT NULL BEGIN DROP PROCEDURE AddChatRoomMember; END
Go
CREATE PROCEDURE AddChatRoomMember
    @ChatRoomId INT,
    @UserId INT,
    @JoinedAt DATETIME
AS
BEGIN
   
    IF NOT EXISTS (
        SELECT 1 
        FROM ChatRoomMembers 
        WHERE ChatRoomId = @ChatRoomId AND UserId = @UserId
    )
    BEGIN
        INSERT INTO ChatRoomMembers (ChatRoomId, UserId, JoinedAt)
        VALUES (@ChatRoomId, @UserId, @JoinedAt);
       
    END
	 SELECT @@ROWCOUNT AS AffectedRows ; -- Inserted successfully
END
GO


-- Drop the AssignRoleToUser procedure if it exists
IF OBJECT_ID('SendMessage', 'P') IS NOT NULL BEGIN DROP PROCEDURE SendMessage; END
Go
CREATE PROCEDURE SendMessage
    @ChatRoomId INT,
    @SenderId INT,
    @Content NVARCHAR(MAX),
    @SentAt DATETIME
AS
BEGIN
    INSERT INTO Messages (ChatRoomId, SenderId, Content, SentAt,Status)
    VALUES (@ChatRoomId, @SenderId, @Content, @SentAt,'Sent');

    SELECT SCOPE_IDENTITY() AS MessageId;
END;
GO



-- Drop the AssignRoleToUser procedure if it exists
IF OBJECT_ID('AddMessageAttachment', 'P') IS NOT NULL BEGIN DROP PROCEDURE AddMessageAttachment; END
Go
CREATE PROCEDURE [dbo].[AddMessageAttachment]
    @MessageId INT,
    @FileUrl NVARCHAR(255),
    @FileType NVARCHAR(50),
	@FileName NVARCHAR(50),
    @UploadedAt DATETIME
AS
BEGIN
    INSERT INTO MessageAttachments (MessageId, FileUrl, FileType , FileName, UploadedAt)
    VALUES (@MessageId, @FileUrl, @FileType,@FileName, @UploadedAt);

	SELECT SCOPE_IDENTITY() AS AttachmentId;
END;
Go


-- Drop the AssignRoleToUser procedure if it exists
IF OBJECT_ID('GetChatRoomMessages', 'P') IS NOT NULL BEGIN DROP PROCEDURE GetChatRoomMessages; END
Go
CREATE PROCEDURE [dbo].[GetChatRoomMessages]
    @ChatRoomId INT,
    @LastMessageId INT = NULL,
    @PageSize INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    -- Parameter validation
    IF @PageSize < 1 OR @PageSize > 1000 SET @PageSize = 50;
    IF @LastMessageId = 0 SET @LastMessageId = NULL;

    -- Step 1: Create temp table
    CREATE TABLE #PaginatedMessages (
        Id INT,
        ChatRoomId INT,
        SenderId INT,
        SenderName NVARCHAR(100),
        Content NVARCHAR(MAX),
        Status NVARCHAR(50),
        SentAt DATETIME
    );

    -- Step 2: Populate temp table using keyset pagination
    INSERT INTO #PaginatedMessages
    SELECT TOP(@PageSize)
        m.Id,
        m.ChatRoomId,
        m.SenderId,
        u.FullName AS SenderName,
        m.Content,
        ISNULL(m.Status, 'Sent') AS Status,
        m.SentAt
    FROM Messages m
    JOIN Users u ON m.SenderId = u.Id
    WHERE m.ChatRoomId = @ChatRoomId
      AND (@LastMessageId IS NULL OR m.Id < @LastMessageId)
    ORDER BY m.Id DESC;

    -- Step 3: Return messages
    SELECT * FROM #PaginatedMessages;

    -- Step 4: Return attachments
    SELECT 
        ma.MessageId,
        ma.Id AS AttachmentId,
        ma.FileUrl,
        ma.FileType,
        ma.FileName,
        ma.UploadedAt
    FROM MessageAttachments ma
    JOIN #PaginatedMessages pm ON pm.Id = ma.MessageId
    UNION ALL
    SELECT NULL, NULL, NULL, NULL, NULL, NULL
    WHERE NOT EXISTS (
        SELECT 1 FROM MessageAttachments ma
        JOIN #PaginatedMessages pm ON pm.Id = ma.MessageId
    );

    -- Step 5: Return reactions
    SELECT 
        mr.Id,
        mr.MessageId,
        mr.ReactionType,
        COUNT_BIG(*) OVER (PARTITION BY mr.MessageId, mr.ReactionType) AS Count,
        u.Id AS UserId,
        u.FullName AS UserName
    FROM MessageReactions mr
    JOIN Users u ON mr.UserId = u.Id
    JOIN #PaginatedMessages pm ON pm.Id = mr.MessageId
    UNION ALL
    SELECT NULL, NULL, NULL, NULL, NULL, NULL
    WHERE NOT EXISTS (
        SELECT 1 FROM MessageReactions mr
        JOIN #PaginatedMessages pm ON pm.Id = mr.MessageId
    )
    ORDER BY MessageId, ReactionType;

    -- Cleanup
    DROP TABLE #PaginatedMessages;
END
GO


-- Drop the AssignRoleToUser procedure if it exists
IF OBJECT_ID('AddOrUpdateMessageReaction', 'P') IS NOT NULL BEGIN DROP PROCEDURE AddOrUpdateMessageReaction; END
Go

CREATE PROCEDURE AddOrUpdateMessageReaction
    @MessageId INT,
    @UserId INT,
    @ReactionType NVARCHAR(50)
AS
BEGIN
    DECLARE @ReactionId INT;
    DECLARE @ActionType NVARCHAR(10) = 'INSERT'; -- Default to INSERT
    
    IF EXISTS (SELECT 1 FROM Messages WHERE Id = @MessageId)
    BEGIN
        IF EXISTS (
            SELECT 1 FROM MessageReactions
            WHERE MessageId = @MessageId AND UserId = @UserId
        )
        BEGIN
            -- Update existing reaction
            UPDATE MessageReactions
            SET ReactionType = @ReactionType,
                ReactedAt = GETDATE()
            WHERE MessageId = @MessageId AND UserId = @UserId;
            
            SET @ActionType = 'UPDATE';
            
            -- Get the existing reaction ID
            SELECT @ReactionId = Id 
            FROM MessageReactions
            WHERE MessageId = @MessageId AND UserId = @UserId;
        END
        ELSE
        BEGIN
            -- Insert new reaction
            INSERT INTO MessageReactions (MessageId, UserId, ReactionType, ReactedAt)
            VALUES (@MessageId, @UserId, @ReactionType, GETDATE());
            
            -- Get the newly inserted reaction ID
            SET @ReactionId = SCOPE_IDENTITY();
        END
    END
    
    -- Return both the reaction ID and affected rows
    SELECT 
        @ReactionId AS ReactionId,
        @@ROWCOUNT AS AffectedRows,
        @ActionType AS ActionType;
END
GO


-- Drop the AssignRoleToUser procedure if it exists
IF OBJECT_ID('RemoveMessageReaction', 'P') IS NOT NULL BEGIN DROP PROCEDURE RemoveMessageReaction; END
Go
CREATE PROCEDURE RemoveMessageReaction
    @MessageId INT,
    @UserId INT
AS
BEGIN
    DELETE FROM MessageReactions
    WHERE MessageId = @MessageId AND UserId = @UserId;
END;
GO


-- Drop the AssignRoleToUser procedure if it exists
IF OBJECT_ID('GetMessageReactions', 'P') IS NOT NULL BEGIN DROP PROCEDURE GetMessageReactions; END
Go
CREATE PROCEDURE GetMessageReactions
    @MessageId INT
AS
BEGIN
    SELECT 
        mr.Id,
        mr.MessageId,
        mr.UserId,
        u.FullName,
        mr.ReactionType,
        mr.ReactedAt
    FROM MessageReactions mr
    INNER JOIN Users u ON mr.UserId = u.Id
    WHERE mr.MessageId = @MessageId;
END;
GO


-- Drop the AssignRoleToUser procedure if it exists
IF OBJECT_ID('SearchUsers', 'P') IS NOT NULL BEGIN DROP PROCEDURE SearchUsers; END
Go
CREATE PROCEDURE SearchUsers
    @SearchTerm NVARCHAR(100),
    @CurrentUserId INT
AS
BEGIN
    SELECT 
        Id,
        FullName,
        Email
    FROM Users
    WHERE 
        (FullName LIKE '%' + @SearchTerm + '%' 
        OR Email LIKE '%' + @SearchTerm + '%')
        AND Id <> @CurrentUserId
    ORDER BY FullName;
END;

GO

-- Drop the AssignRoleToUser procedure if it exists
IF OBJECT_ID('SearchUsersWithChatStatus', 'P') IS NOT NULL BEGIN DROP PROCEDURE SearchUsersWithChatStatus; END
Go
CREATE PROCEDURE SearchUsersWithChatStatus
    @SearchTerm NVARCHAR(100),
    @CurrentUserId INT
AS
BEGIN
    SELECT 
        u.Id,
        u.FullName,
        u.Email,
        CASE 
            WHEN EXISTS (
                SELECT 1 
                FROM ChatRoomMembers cru1
                JOIN ChatRoomMembers cru2 ON cru1.ChatRoomId = cru2.ChatRoomId
                JOIN ChatRooms cr ON cr.Id = cru1.ChatRoomId
                WHERE cru1.UserId = @CurrentUserId 
                  AND cru2.UserId = u.Id 
                  AND cr.IsGroup = 0
            ) THEN 1 ELSE 0 
        END AS HasChattedBefore
    FROM Users u
    WHERE 
        (u.FullName LIKE '%' + @SearchTerm + '%' OR u.Email LIKE '%' + @SearchTerm + '%')
        AND u.Id <> @CurrentUserId
    ORDER BY HasChattedBefore DESC, u.FullName;
END;
GO



IF NOT EXISTS (
    SELECT * 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Users' 
      AND COLUMN_NAME = 'IsOnline'
)
BEGIN
    ALTER TABLE Users ADD IsOnline BIT DEFAULT 0;
END
GO

IF OBJECT_ID('UpdateUserOnlineStatus', 'P') IS NOT NULL BEGIN DROP PROCEDURE UpdateUserOnlineStatus; END
Go
CREATE PROCEDURE UpdateUserOnlineStatus
    @UserId INT,
    @IsOnline BIT
AS
BEGIN
    UPDATE Users SET IsOnline = @IsOnline WHERE Id = @UserId;
END
GO


IF OBJECT_ID('Contacts', 'U') IS  NULL BEGIN 

CREATE TABLE Contacts (
    Id INT PRIMARY KEY IDENTITY,
    UserId1 INT NOT NULL,
    UserId2 INT NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Contacts_User1 FOREIGN KEY (UserId1) REFERENCES Users(Id),
    CONSTRAINT FK_Contacts_User2 FOREIGN KEY (UserId2) REFERENCES Users(Id),
    CONSTRAINT UQ_Contacts UNIQUE (UserId1, UserId2)
);
END
GO


IF OBJECT_ID('AddContact', 'p') IS NOT NULL BEGIN DROP Procedure AddContact; END
Go
CREATE PROCEDURE AddContact
    @UserIdA INT,
    @UserIdB INT
AS
BEGIN
    -- Normalize user ID order
    DECLARE @UserId1 INT, @UserId2 INT;

    IF @UserIdA < @UserIdB
    BEGIN
        SET @UserId1 = @UserIdA;
        SET @UserId2 = @UserIdB;
    END
    ELSE
    BEGIN
        SET @UserId1 = @UserIdB;
        SET @UserId2 = @UserIdA;
    END

    -- Check if the contact already exists
    IF NOT EXISTS (
        SELECT 1 FROM Contacts 
        WHERE UserId1 = @UserId1 AND UserId2 = @UserId2
    )
    BEGIN
        INSERT INTO Contacts (UserId1, UserId2)
        VALUES (@UserId1, @UserId2);
    END

    SELECT @@ROWCOUNT AS AffectedRows;

END
GO



IF OBJECT_ID('GetContactsByUserId', 'p') IS NOT NULL BEGIN DROP Procedure GetContactsByUserId; END
Go
CREATE PROCEDURE GetContactsByUserId
    @UserId INT
AS
BEGIN
    SELECT 
        u.Id,
        u.FullName,
        u.Email,
        u.IsOnline,
		CAST(1 AS BIT) AS IsContact ,
		u.ChatAvatar
    FROM Contacts c
    INNER JOIN Users u ON 
        (c.UserId1 = @UserId AND u.Id = c.UserId2) OR
        (c.UserId2 = @UserId AND u.Id = c.UserId1)
    WHERE u.Id <> @UserId
    ORDER BY u.FullName;
END
GO


IF OBJECT_ID('SearchUsersContact', 'p') IS NOT NULL BEGIN DROP Procedure SearchUsersContact; END
Go

CREATE PROCEDURE SearchUsersContact
    @SearchTerm NVARCHAR(100),
    @CurrentUserId INT
AS
BEGIN
    -- Step 1: Get UserIds already in contact list
    SELECT 
        u.Id,
        u.FullName,
        u.Email,
        CAST(1 AS BIT) AS IsContact
    FROM Contacts c
    INNER JOIN Users u ON 
        (u.Id = c.UserId1 AND c.UserId2 = @CurrentUserId)
        OR (u.Id = c.UserId2 AND c.UserId1 = @CurrentUserId)
    WHERE 
        (u.FullName LIKE '%' + @SearchTerm + '%' OR u.Email LIKE '%' + @SearchTerm + '%')
        AND u.Id <> @CurrentUserId

    UNION

    -- Step 2: Get users not in contact list
    SELECT 
        u.Id,
        u.FullName,
        u.Email,
        CAST(0 AS BIT) AS IsContact
    FROM Users u
    WHERE 
        (u.FullName LIKE '%' + @SearchTerm + '%' OR u.Email LIKE '%' + @SearchTerm + '%')
		 AND u.Id <> @CurrentUserId
		 AND NOT EXISTS (
             SELECT 1 FROM Contacts c
             WHERE 
                (c.UserId1 = @CurrentUserId AND c.UserId2 = u.Id)
                OR (c.UserId2 = @CurrentUserId AND c.UserId1 = u.Id)
        )
END
GO



IF OBJECT_ID('SearchUsersByFilters', 'P') IS NOT NULL
    DROP PROCEDURE SearchUsersByFilters;
GO

CREATE PROCEDURE SearchUsersByFilters
    @UserId INT = NULL,
    @FullName NVARCHAR(100) = NULL,
    @Email NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id,
        FullName,
        Email,
        IsOnline
    FROM Users
    WHERE 
        (@UserId IS NULL OR Id = @UserId)
        AND (@FullName IS NULL OR FullName LIKE '%' + @FullName + '%')
        AND (@Email IS NULL OR Email LIKE '%' + @Email + '%')
    ORDER BY FullName;
END
GO


IF OBJECT_ID('GetUserChatRoomsWithDetails', 'P') IS NOT NULL
    DROP PROCEDURE GetUserChatRoomsWithDetails;
GO

CREATE PROCEDURE GetUserChatRoomsWithDetails
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

     DECLARE @PersonalChatRoomId INT;
     DECLARE @MyFullName NVARCHAR(255);
     DECLARE @MyAvatar NVARCHAR(255);
     DECLARE @MyOnline BIT;


     -- Step 1: Get user info
    SELECT @PersonalChatRoomId = ChatRoomId,
           @MyFullName = FullName,
           @MyAvatar = ChatAvatar,
           @MyOnline = IsOnline
    FROM Users
    WHERE Id = @UserId;

    -- Step 2: Create personal chat room if not exists
    IF @PersonalChatRoomId IS NULL OR @PersonalChatRoomId = 0
    BEGIN
        -- Create a new personal chat room
        INSERT INTO ChatRooms (IsGroup, Name, CreatedAt)
        VALUES (0, NULL, GETUTCDATE());

        SET @PersonalChatRoomId = SCOPE_IDENTITY();

        -- Add user as the only member
        INSERT INTO ChatRoomMembers (ChatRoomId, UserId)
        VALUES (@PersonalChatRoomId, @UserId);

        -- Update user with their personal chat room ID
        UPDATE Users
        SET ChatRoomId = @PersonalChatRoomId
        WHERE Id = @UserId;
    END

    -- Step 3: Return all chat rooms the user is part of
    SELECT 
        cr.Id AS ChatRoomId,
        cr.IsGroup,

        -- Other user info (only for one-to-one chats)
        CASE 
            WHEN cr.IsGroup = 0 AND cru2.UserId IS NOT NULL THEN cru2.UserId
			WHEN cr.Id = @PersonalChatRoomId THEN @UserId
            ELSE NULL 
        END AS UserId,

        -- Chat Name
        CASE 
            WHEN cr.Id = @PersonalChatRoomId THEN @MyFullName
            WHEN cr.IsGroup = 1 THEN cr.Name 
            ELSE cru2.FullName 
        END AS ChatName,

        -- Avatar
        CASE 
			WHEN cr.Id = @PersonalChatRoomId THEN @MyAvatar
            WHEN cr.IsGroup = 1  THEN NULL
            ELSE cru2.ChatAvatar 
        END AS ChatAvatar,

        -- Online status
        CASE 
            WHEN cr.IsGroup = 1 OR cr.Id = @PersonalChatRoomId THEN NULL
            ELSE cru2.IsOnline 
        END AS IsOnline,

        -- Last Message Info
        lm.SentAt AS LastMessageTime,
        lm.Content AS LastMessageText,

        -- Attachment
        a.FileUrl AS LastMessageAttachmentPath,
        a.FileType AS LastMessageAttachmentType,

        -- Reactions
        r.Reactions AS LastMessageReactions,

        -- Unread Messages
        uc.UnreadCount

    FROM ChatRoomMembers cru
    INNER JOIN ChatRooms cr ON cr.Id = cru.ChatRoomId

    -- Get other user info (only for one-to-one chats)
    OUTER APPLY (
        SELECT TOP 1 u.Id AS UserId, u.FullName, u.ChatAvatar, u.IsOnline
        FROM ChatRoomMembers cm
        JOIN Users u ON u.Id = cm.UserId
        WHERE cm.ChatRoomId = cr.Id AND cm.UserId <> @UserId
    ) cru2

    -- Latest message
    OUTER APPLY (
        SELECT TOP 1 m.Id, m.Content, m.SentAt
        FROM Messages m
        WHERE m.ChatRoomId = cr.Id
        ORDER BY m.SentAt DESC
    ) lm

    -- Attachment of last message
    OUTER APPLY (
        SELECT TOP 1 a.FileUrl, a.FileType
        FROM MessageAttachments a
        WHERE a.MessageId = lm.Id
    ) a

    -- Reactions of last message
    OUTER APPLY (
        SELECT STRING_AGG(r.ReactionType, ', ') AS Reactions
        FROM MessageReactions r
        WHERE r.MessageId = lm.Id
    ) r

    -- Unread message count
    OUTER APPLY (
        SELECT COUNT(*) AS UnreadCount
        FROM Messages m
        WHERE m.ChatRoomId = cr.Id
          AND m.Id NOT IN (
              SELECT MessageId FROM MessageSeenStatus
              WHERE UserId = @UserId
          )
    ) uc

    WHERE cru.UserId = @UserId
    ORDER BY 
        ISNULL(lm.SentAt, '1900-01-01') DESC;
END
GO



IF OBJECT_ID('DeleteMessageReactions', 'P') IS NOT NULL
    DROP PROCEDURE DeleteMessageReactions;
GO

CREATE  PROCEDURE [dbo].[DeleteMessageReactions]
    @MessageId  INT,
    @UserId INT
AS
BEGIN
     Delete from MessageReactions where MessageId = @MessageId AND UserId = @UserId

	 SELECT @@ROWCOUNT AS AffectedRows;
END
GO


IF OBJECT_ID('trg_DeleteMessage_Cascade', 'TR') IS NOT NULL  
   DROP TRIGGER trg_DeleteMessage_Cascade
GO

CREATE TRIGGER trg_DeleteMessage_Cascade
ON Messages
After DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Delete related message reactions
    DELETE FROM MessageReactions
    WHERE MessageId IN (SELECT Id FROM DELETED)

    -- Delete related message attachments
    DELETE FROM MessageAttachments
    WHERE MessageId IN (SELECT Id FROM DELETED)

	Delete FROM MessageSeenStatus Where 
	MessageId IN (SELECT Id FROM DELETED)
END
GO



IF OBJECT_ID('trg_DeleteChatRoom_Cascade', 'TR') IS NOT NULL  
   DROP TRIGGER trg_DeleteChatRoom_Cascade
GO

CREATE TRIGGER trg_DeleteChatRoom_Cascade
ON ChatRooms
AFTER DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Delete related message reactions
    DELETE FROM Messages
    WHERE ChatRoomId IN (SELECT Id FROM DELETED)

END
GO


IF OBJECT_ID('MessageSeenStatus', 'U') IS  NULL
 
CREATE TABLE MessageSeenStatus (
    Id INT IDENTITY PRIMARY KEY,
    MessageId INT NOT NULL,
    UserId INT NOT NULL,
    SeenAt DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_MessageSeenStatus_Messages FOREIGN KEY (MessageId) REFERENCES Messages(Id),
    CONSTRAINT FK_MessageSeenStatus_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
Go



IF OBJECT_ID('trg_InsertMessage_Cascade', 'TR') IS NOT NULL  
   DROP TRIGGER trg_InsertMessage_Cascade
GO

CREATE TRIGGER trg_InsertMessage_Cascade
ON Messages
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO MessageSeenStatus (MessageId, UserId, SeenAt)
    SELECT Id, SenderId, GETDATE()
    FROM inserted;
END;
GO


--IF OBJECT_ID('Message', 'T') IS NOT NULL
--    DROP TYPE Message 

--CREATE TYPE Message AS TABLE
--(
--    MessageId INT
--);

--GO

IF OBJECT_ID('MarkAllUnseenMessagesAsSeen', 'P') IS NOT NULL  
   DROP Procedure MarkAllUnseenMessagesAsSeen
GO

CREATE PROCEDURE MarkAllUnseenMessagesAsSeen
    @UserId INT,
	@ChatRoomID INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Step 1: Insert seen statuses
    INSERT INTO MessageSeenStatus (MessageId, UserId, SeenAt)
    SELECT M.Id, @UserId, GETDATE()
    FROM Messages M
    INNER JOIN ChatRoomMembers CRM ON M.ChatRoomId = CRM.ChatRoomId
    WHERE CRM.UserId = @UserId AND CRM.ChatRoomId = @ChatRoomID
      AND NOT EXISTS (
          SELECT 1 FROM MessageSeenStatus MS
          WHERE MS.UserId = @UserId AND MS.MessageId = M.Id
      );

    -- Step 2: Update status to 'Seen' for those messages
    UPDATE M
    SET M.Status = 'Seen'
    FROM Messages M
    INNER JOIN ChatRoomMembers CRM ON M.ChatRoomId = CRM.ChatRoomId
    WHERE CRM.UserId = @UserId AND CRM.ChatRoomId = @ChatRoomID AND M.SenderId <> @UserId
END

GO


IF OBJECT_ID('UpdateUserInfo', 'P') IS NOT NULL  
   DROP Procedure UpdateUserInfo
GO

CREATE PROCEDURE UpdateUserInfo
    @UserId INT,
    @FullName NVARCHAR(100),
    @EnableNotification BIT,
    @AppearInSearchResult BIT,
    @ShowOnlineStatus BIT,
    @DefaultTheme NVARCHAR(50),
    @ChatAvatar NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Users
    SET 
        FullName = @FullName,
        EnableNotification = @EnableNotification,
        AppearInSearchResult = @AppearInSearchResult,
        ShowOnlineStatus = @ShowOnlineStatus,
        DefaultTheme = @DefaultTheme,
        ChatAvatar = @ChatAvatar
    WHERE Id = @UserId;

	SELECT @@ROWCOUNT AS AffectedRows;
END
GO




IF OBJECT_ID('GetUserInfo', 'P') IS NOT NULL  
   DROP Procedure GetUserInfo
GO

CREATE PROCEDURE GetUserInfo
	   @UserId INT
AS
BEGIN
    SELECT U.Id as UserId, U.FullName, U.Email, U.IsOnline, R.Name as UserRole, 
           U.EnableNotification, U.AppearInSearchResult, 
           U.ShowOnlineStatus, ISNULL(U.DefaultTheme,0) as DefaultTheme, U.ChatAvatar , U.ChatRoomID
    FROM Users U
	Inner Join UserRoles Ur on U.Id = Ur.UserId
	Inner Join Roles R on Ur.RoleId = R.Id
    WHERE U.Id = @UserId;
END

GO

IF OBJECT_ID('UpdateUserChatRoomId', 'P') IS NOT NULL  
   DROP Procedure UpdateUserChatRoomId
GO

CREATE PROCEDURE UpdateUserChatRoomId
	   @UserId INT,
	   @ChatRoomId INT
AS
BEGIN
     SET NOCOUNT ON;
    
    BEGIN TRY
		UPDATE Users 
        SET ChatRoomID = @ChatRoomId
        WHERE Id = @UserId;
        
        SELECT @@ROWCOUNT AS AffectedRows;
    END TRY
    BEGIN CATCH
        SELECT 0 AS AffectedRows;
    END CATCH
END
GO

