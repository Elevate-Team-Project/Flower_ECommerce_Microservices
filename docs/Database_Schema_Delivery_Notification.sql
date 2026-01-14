-- =============================================================================
-- Flower E-Commerce - Delivery & Notification Services Database Schema
-- Generated for Production SQL Server Deployment
-- =============================================================================
-- This script creates all tables for 2 microservice databases:
--   1. GiftShop_DeliveryDB
--   2. GiftShop_NotificationDB
-- Includes MassTransit Transactional Outbox tables for both databases.
-- Uses IF NOT EXISTS checks for idempotent execution.
-- =============================================================================


-- #############################################################################
-- DATABASE 1: GiftShop_DeliveryDB (Delivery Service)
-- #############################################################################
USE [GiftShop_DeliveryDB]
GO

PRINT '========================================='
PRINT 'Creating GiftShop_DeliveryDB Schema...'
PRINT '========================================='

-- -----------------------------------------------------------------------------
-- MassTransit Transactional Outbox Tables
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InboxState')
BEGIN
    CREATE TABLE [dbo].[InboxState] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [MessageId] UNIQUEIDENTIFIER NOT NULL,
        [ConsumerId] UNIQUEIDENTIFIER NOT NULL,
        [LockId] UNIQUEIDENTIFIER NOT NULL,
        [RowVersion] ROWVERSION NOT NULL,
        [Received] DATETIME2 NOT NULL,
        [ReceiveCount] INT NOT NULL DEFAULT 0,
        [ExpirationTime] DATETIME2 NULL,
        [Consumed] DATETIME2 NULL,
        [Delivered] DATETIME2 NULL,
        [LastSequenceNumber] BIGINT NULL
    );
    CREATE UNIQUE INDEX [IX_InboxState_MessageId_ConsumerId] ON [dbo].[InboxState]([MessageId], [ConsumerId]);
    PRINT 'Created table: InboxState'
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OutboxMessage')
BEGIN
    CREATE TABLE [dbo].[OutboxMessage] (
        [SequenceNumber] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [EnqueueTime] DATETIME2 NULL,
        [SentTime] DATETIME2 NOT NULL,
        [Headers] NVARCHAR(MAX) NULL,
        [Properties] NVARCHAR(MAX) NULL,
        [InboxMessageId] UNIQUEIDENTIFIER NULL,
        [InboxConsumerId] UNIQUEIDENTIFIER NULL,
        [OutboxId] UNIQUEIDENTIFIER NULL,
        [MessageId] UNIQUEIDENTIFIER NOT NULL,
        [ContentType] NVARCHAR(256) NOT NULL,
        [MessageType] NVARCHAR(MAX) NOT NULL,
        [Body] NVARCHAR(MAX) NOT NULL,
        [ConversationId] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [InitiatorId] UNIQUEIDENTIFIER NULL,
        [RequestId] UNIQUEIDENTIFIER NULL,
        [SourceAddress] NVARCHAR(256) NULL,
        [DestinationAddress] NVARCHAR(256) NULL,
        [ResponseAddress] NVARCHAR(256) NULL,
        [FaultAddress] NVARCHAR(256) NULL,
        [ExpirationTime] DATETIME2 NULL
    );
    CREATE INDEX [IX_OutboxMessage_EnqueueTime] ON [dbo].[OutboxMessage]([EnqueueTime]);
    CREATE INDEX [IX_OutboxMessage_ExpirationTime] ON [dbo].[OutboxMessage]([ExpirationTime]);
    CREATE INDEX [IX_OutboxMessage_OutboxId_SequenceNumber] ON [dbo].[OutboxMessage]([OutboxId], [SequenceNumber]);
    CREATE INDEX [IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber] ON [dbo].[OutboxMessage]([InboxMessageId], [InboxConsumerId], [SequenceNumber]);
    PRINT 'Created table: OutboxMessage'
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OutboxState')
BEGIN
    CREATE TABLE [dbo].[OutboxState] (
        [OutboxId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [LockId] UNIQUEIDENTIFIER NOT NULL,
        [RowVersion] ROWVERSION NOT NULL,
        [Created] DATETIME2 NOT NULL,
        [Delivered] DATETIME2 NULL,
        [LastSequenceNumber] BIGINT NULL
    );
    CREATE INDEX [IX_OutboxState_Created] ON [dbo].[OutboxState]([Created]);
    PRINT 'Created table: OutboxState'
END
GO

-- -----------------------------------------------------------------------------
-- Delivery Domain Tables
-- -----------------------------------------------------------------------------

-- UserAddresses Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserAddresses')
BEGIN
    CREATE TABLE [dbo].[UserAddresses] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(100) NOT NULL,
        [AddressLabel] NVARCHAR(50) NOT NULL,
        [FullName] NVARCHAR(200) NOT NULL,
        [Phone] NVARCHAR(50) NOT NULL,
        -- Map Location (US-F03)
        [Latitude] FLOAT NULL,
        [Longitude] FLOAT NULL,
        -- Detailed Address Fields (Egyptian format)
        [Governorate] NVARCHAR(100) NOT NULL,
        [City] NVARCHAR(100) NOT NULL,
        [Street] NVARCHAR(200) NOT NULL,
        [Building] NVARCHAR(50) NULL,
        [Floor] NVARCHAR(20) NULL,
        [Apartment] NVARCHAR(20) NULL,
        [PostalCode] NVARCHAR(20) NULL,
        [Country] NVARCHAR(100) NOT NULL DEFAULT 'Egypt',
        [IsDefault] BIT NOT NULL DEFAULT 0,
        [Notes] NVARCHAR(500) NULL,
        [Landmark] NVARCHAR(200) NULL,
        -- Audit Fields (BaseEntity)
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [DeletedAt] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0
    );
    CREATE INDEX [IX_UserAddresses_UserId] ON [dbo].[UserAddresses]([UserId]);
    PRINT 'Created table: UserAddresses'
END
GO

-- DeliveryZones Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DeliveryZones')
BEGIN
    CREATE TABLE [dbo].[DeliveryZones] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ZoneName] NVARCHAR(200) NOT NULL,
        [City] NVARCHAR(100) NOT NULL,
        [State] NVARCHAR(100) NOT NULL,
        [Country] NVARCHAR(100) NOT NULL,
        [ShippingCost] DECIMAL(18,2) NOT NULL,
        [EstimatedDeliveryDays] INT NOT NULL,
        -- Audit Fields (BaseEntity)
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [DeletedAt] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0
    );
    PRINT 'Created table: DeliveryZones'
END
GO

-- Shipments Table (with Driver Tracking fields - US-E02)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Shipments')
BEGIN
    CREATE TABLE [dbo].[Shipments] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [OrderId] INT NOT NULL,
        [DeliveryAddressId] INT NOT NULL,
        [TrackingNumber] NVARCHAR(100) NOT NULL,
        [Carrier] NVARCHAR(100) NOT NULL,
        [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
        [EstimatedDeliveryDate] DATETIME2 NULL,
        [ActualDeliveryDate] DATETIME2 NULL,
        [CurrentLocation] NVARCHAR(500) NULL,
        [Notes] NVARCHAR(1000) NULL,
        -- Gift Order Fields
        [IsGift] BIT NOT NULL DEFAULT 0,
        [RecipientName] NVARCHAR(200) NULL,
        [RecipientPhone] NVARCHAR(50) NULL,
        [GiftMessage] NVARCHAR(1000) NULL,
        -- Driver Location Tracking (US-E02)
        [DriverLatitude] FLOAT NULL,
        [DriverLongitude] FLOAT NULL,
        [LastLocationUpdate] DATETIME2 NULL,
        -- Delivery Hero Info (US-E02)
        [DriverName] NVARCHAR(200) NULL,
        [DriverPhone] NVARCHAR(50) NULL,
        [DriverPhotoUrl] NVARCHAR(500) NULL,
        -- Audit Fields (BaseEntity)
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [DeletedAt] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        -- Foreign Key
        CONSTRAINT [FK_Shipments_UserAddresses] FOREIGN KEY ([DeliveryAddressId]) 
            REFERENCES [dbo].[UserAddresses]([Id]) ON DELETE NO ACTION
    );
    CREATE INDEX [IX_Shipments_OrderId] ON [dbo].[Shipments]([OrderId]);
    CREATE INDEX [IX_Shipments_TrackingNumber] ON [dbo].[Shipments]([TrackingNumber]);
    CREATE INDEX [IX_Shipments_Status] ON [dbo].[Shipments]([Status]);
    PRINT 'Created table: Shipments'
END
GO

PRINT 'GiftShop_DeliveryDB Schema Complete!'
GO


-- #############################################################################
-- DATABASE 2: GiftShop_NotificationDB (Notification Service)
-- #############################################################################
USE [GiftShop_NotificationDB]
GO

PRINT '========================================='
PRINT 'Creating GiftShop_NotificationDB Schema...'
PRINT '========================================='

-- -----------------------------------------------------------------------------
-- MassTransit Transactional Outbox Tables
-- -----------------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InboxState')
BEGIN
    CREATE TABLE [dbo].[InboxState] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [MessageId] UNIQUEIDENTIFIER NOT NULL,
        [ConsumerId] UNIQUEIDENTIFIER NOT NULL,
        [LockId] UNIQUEIDENTIFIER NOT NULL,
        [RowVersion] ROWVERSION NOT NULL,
        [Received] DATETIME2 NOT NULL,
        [ReceiveCount] INT NOT NULL DEFAULT 0,
        [ExpirationTime] DATETIME2 NULL,
        [Consumed] DATETIME2 NULL,
        [Delivered] DATETIME2 NULL,
        [LastSequenceNumber] BIGINT NULL
    );
    CREATE UNIQUE INDEX [IX_InboxState_MessageId_ConsumerId] ON [dbo].[InboxState]([MessageId], [ConsumerId]);
    PRINT 'Created table: InboxState'
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OutboxMessage')
BEGIN
    CREATE TABLE [dbo].[OutboxMessage] (
        [SequenceNumber] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [EnqueueTime] DATETIME2 NULL,
        [SentTime] DATETIME2 NOT NULL,
        [Headers] NVARCHAR(MAX) NULL,
        [Properties] NVARCHAR(MAX) NULL,
        [InboxMessageId] UNIQUEIDENTIFIER NULL,
        [InboxConsumerId] UNIQUEIDENTIFIER NULL,
        [OutboxId] UNIQUEIDENTIFIER NULL,
        [MessageId] UNIQUEIDENTIFIER NOT NULL,
        [ContentType] NVARCHAR(256) NOT NULL,
        [MessageType] NVARCHAR(MAX) NOT NULL,
        [Body] NVARCHAR(MAX) NOT NULL,
        [ConversationId] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [InitiatorId] UNIQUEIDENTIFIER NULL,
        [RequestId] UNIQUEIDENTIFIER NULL,
        [SourceAddress] NVARCHAR(256) NULL,
        [DestinationAddress] NVARCHAR(256) NULL,
        [ResponseAddress] NVARCHAR(256) NULL,
        [FaultAddress] NVARCHAR(256) NULL,
        [ExpirationTime] DATETIME2 NULL
    );
    CREATE INDEX [IX_OutboxMessage_EnqueueTime] ON [dbo].[OutboxMessage]([EnqueueTime]);
    CREATE INDEX [IX_OutboxMessage_ExpirationTime] ON [dbo].[OutboxMessage]([ExpirationTime]);
    CREATE INDEX [IX_OutboxMessage_OutboxId_SequenceNumber] ON [dbo].[OutboxMessage]([OutboxId], [SequenceNumber]);
    CREATE INDEX [IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber] ON [dbo].[OutboxMessage]([InboxMessageId], [InboxConsumerId], [SequenceNumber]);
    PRINT 'Created table: OutboxMessage'
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OutboxState')
BEGIN
    CREATE TABLE [dbo].[OutboxState] (
        [OutboxId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [LockId] UNIQUEIDENTIFIER NOT NULL,
        [RowVersion] ROWVERSION NOT NULL,
        [Created] DATETIME2 NOT NULL,
        [Delivered] DATETIME2 NULL,
        [LastSequenceNumber] BIGINT NULL
    );
    CREATE INDEX [IX_OutboxState_Created] ON [dbo].[OutboxState]([Created]);
    PRINT 'Created table: OutboxState'
END
GO

-- -----------------------------------------------------------------------------
-- Notification Domain Tables
-- -----------------------------------------------------------------------------

-- Notifications Table (US-F07: View Notifications)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE [dbo].[Notifications] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] NVARCHAR(450) NOT NULL,
        [Title] NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(1000) NULL,
        [Type] NVARCHAR(50) NOT NULL DEFAULT 'System',
        [IsRead] BIT NOT NULL DEFAULT 0,
        [ReadAt] DATETIME2 NULL,
        [ReferenceId] NVARCHAR(100) NULL,
        [ActionUrl] NVARCHAR(500) NULL,
        -- Audit Fields (BaseEntity)
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [DeletedAt] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsDeleted] BIT NOT NULL DEFAULT 0
    );
    CREATE INDEX [IX_Notifications_UserId] ON [dbo].[Notifications]([UserId]);
    CREATE INDEX [IX_Notifications_UserId_IsRead] ON [dbo].[Notifications]([UserId], [IsRead]);
    CREATE INDEX [IX_Notifications_Type] ON [dbo].[Notifications]([Type]);
    CREATE INDEX [IX_Notifications_CreatedAt] ON [dbo].[Notifications]([CreatedAt] DESC);
    PRINT 'Created table: Notifications'
END
GO

PRINT 'GiftShop_NotificationDB Schema Complete!'
GO


-- =============================================================================
-- EXECUTION COMPLETE
-- =============================================================================
PRINT ''
PRINT '=============================================='
PRINT '✅ ALL DATABASE SCHEMAS CREATED SUCCESSFULLY!'
PRINT '=============================================='
PRINT 'Databases Configured:'
PRINT '  1. GiftShop_DeliveryDB     - Delivery Service'
PRINT '     - UserAddresses (with Egyptian address format)'
PRINT '     - DeliveryZones (shipping cost zones)'
PRINT '     - Shipments (with Driver Tracking: DriverLatitude,'
PRINT '       DriverLongitude, LastLocationUpdate, DriverName,'
PRINT '       DriverPhone, DriverPhotoUrl)'
PRINT ''
PRINT '  2. GiftShop_NotificationDB - Notification Service'
PRINT '     - Notifications (user alerts with Type, IsRead, etc.)'
PRINT ''
PRINT 'MassTransit Outbox Tables Created in Each DB:'
PRINT '  - InboxState'
PRINT '  - OutboxMessage'
PRINT '  - OutboxState'
PRINT ''
PRINT 'Script is IDEMPOTENT - safe to run multiple times.'
PRINT '=============================================='
GO
