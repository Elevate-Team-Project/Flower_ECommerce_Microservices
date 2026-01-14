/*
    =============================================================================
    Consolidated Schema Refactoring Script
    =============================================================================
    Goal: 
    1. Merge Cart Service Schema -> INTO Ordering Service Database
    2. Merge Promotion Service Schema -> INTO Catalog Service Database
    3. Ensure MassTransit Outbox Tables exist in both.
    
    Warning: This script creates tables IF THEY DO NOT EXIST. 
    It does not migrate data.
    =============================================================================
*/

-- =============================================================================
-- 1. ORDERING SERVICE DATABASE (Target for Cart Service Merge)
-- =============================================================================
USE [GiftShop_OrderingDB]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 1.1 Create 'Carts' Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Carts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Carts](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](max) NOT NULL DEFAULT (N''),
        [CouponCode] [nvarchar](max) NULL,
        [TotalPrice] [decimal](18, 2) NOT NULL DEFAULT 0, -- Computed property, but often good to persist or ignore. DB Context ignores it usually, but let's see. Wait, "TotalPrice" in Cart.cs is a getter property (=>), so it's NOT stored in DB. I will SKIP it.
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [IsDeleted] [bit] NOT NULL,
        [DeletedAt] [datetime2](7) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
     CONSTRAINT [PK_Carts] PRIMARY KEY CLUSTERED 
    (
        [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO

-- 1.2 Create 'CartItems' Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CartItems]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[CartItems](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [CartId] [int] NOT NULL,
        [ProductId] [int] NOT NULL,
        [ProductName] [nvarchar](max) NOT NULL DEFAULT (N''),
        [UnitPrice] [decimal](18, 2) NOT NULL,
        [Quantity] [int] NOT NULL DEFAULT 1,
        [PictureUrl] [nvarchar](max) NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [IsDeleted] [bit] NOT NULL,
        [DeletedAt] [datetime2](7) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
     CONSTRAINT [PK_CartItems] PRIMARY KEY CLUSTERED 
    (
        [Id] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO

-- 1.3 Add Foreign Key for CartItems -> Carts
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_CartItems_Carts_CartId]') AND parent_object_id = OBJECT_ID(N'[dbo].[CartItems]'))
BEGIN
    ALTER TABLE [dbo].[CartItems]  WITH CHECK ADD  CONSTRAINT [FK_CartItems_Carts_CartId] FOREIGN KEY([CartId])
    REFERENCES [dbo].[Carts] ([Id])
    ON DELETE CASCADE
    
    ALTER TABLE [dbo].[CartItems] CHECK CONSTRAINT [FK_CartItems_Carts_CartId]
END
GO

-- 1.4 Index for CartId in CartItems
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_CartItems_CartId' AND object_id = OBJECT_ID(N'[dbo].[CartItems]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CartItems_CartId] ON [dbo].[CartItems]
    (
        [CartId] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
END
GO

-- 1.5 Ensure MassTransit Outbox Tables Exist (OrderingDB)
-- InboxState
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InboxState]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[InboxState](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [MessageId] [uniqueidentifier] NOT NULL,
        [ConsumerId] [uniqueidentifier] NOT NULL,
        [LockId] [uniqueidentifier] NOT NULL,
        [RowVersion] [timestamp] NOT NULL,
        [Received] [datetime2](7) NOT NULL,
        [ReceiveCount] [int] NOT NULL,
        [ExpirationTime] [datetime2](7) NULL,
        [Consumed] [datetime2](7) NULL,
        [Delivered] [datetime2](7) NULL,
        [LastUpdated] [datetime2](7) NULL,
     CONSTRAINT [PK_InboxState] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    CREATE UNIQUE NONCLUSTERED INDEX [IX_InboxState_MessageId_ConsumerId] ON [dbo].[InboxState] ([MessageId] ASC, [ConsumerId] ASC)
END
GO
-- OutboxMessage
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OutboxMessage]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OutboxMessage](
        [SequenceNumber] [bigint] IDENTITY(1,1) NOT NULL,
        [EnqueueTime] [datetime2](7) NULL,
        [SentTime] [datetime2](7) NOT NULL,
        [Headers] [nvarchar](max) NULL,
        [Properties] [nvarchar](max) NULL,
        [InboxMessageId] [uniqueidentifier] NULL,
        [InboxConsumerId] [uniqueidentifier] NULL,
        [OutboxId] [uniqueidentifier] NULL,
        [MessageId] [uniqueidentifier] NOT NULL,
        [ContentType] [nvarchar](256) NOT NULL,
        [Body] [nvarchar](max) NOT NULL,
        [ConversationId] [uniqueidentifier] NULL,
        [CorrelationId] [uniqueidentifier] NULL,
        [InitiatorId] [uniqueidentifier] NULL,
        [RequestId] [uniqueidentifier] NULL,
        [SourceAddress] [nvarchar](256) NULL,
        [DestinationAddress] [nvarchar](256) NULL,
        [ResponseAddress] [nvarchar](256) NULL,
        [FaultAddress] [nvarchar](256) NULL,
        [ExpirationTime] [datetime2](7) NULL,
     CONSTRAINT [PK_OutboxMessage] PRIMARY KEY CLUSTERED ([SequenceNumber] ASC)
    )
    CREATE NONCLUSTERED INDEX [IX_OutboxMessage_EnqueueTime] ON [dbo].[OutboxMessage] ([EnqueueTime] ASC)
    CREATE NONCLUSTERED INDEX [IX_OutboxMessage_ExpirationTime] ON [dbo].[OutboxMessage] ([ExpirationTime] ASC)
    CREATE UNIQUE NONCLUSTERED INDEX [IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber] ON [dbo].[OutboxMessage] ([InboxMessageId] ASC, [InboxConsumerId] ASC, [SequenceNumber] ASC)
    CREATE UNIQUE NONCLUSTERED INDEX [IX_OutboxMessage_OutboxId_SequenceNumber] ON [dbo].[OutboxMessage] ([OutboxId] ASC, [SequenceNumber] ASC)
END
GO
-- OutboxState
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OutboxState]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OutboxState](
        [OutboxId] [uniqueidentifier] NOT NULL,
        [LockId] [uniqueidentifier] NOT NULL,
        [RowVersion] [timestamp] NOT NULL,
        [Created] [datetime2](7) NOT NULL,
        [Delivered] [datetime2](7) NULL,
        [LastUpdated] [datetime2](7) NULL,
     CONSTRAINT [PK_OutboxState] PRIMARY KEY CLUSTERED ([OutboxId] ASC)
    )
    CREATE NONCLUSTERED INDEX [IX_OutboxState_Created] ON [dbo].[OutboxState] ([Created] ASC)
END
GO


-- =============================================================================
-- 2. CATALOG SERVICE DATABASE (Target for Promotion Service Merge)
-- =============================================================================
USE [GiftShop_CatalogDB]
GO

-- 2.1 Create 'Offers' Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Offers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Offers](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](max) NOT NULL DEFAULT (N''),
        [NameAr] [nvarchar](max) NULL,
        [Description] [nvarchar](max) NULL,
        [DescriptionAr] [nvarchar](max) NULL,
        [Type] [int] NOT NULL DEFAULT 0,
        [DiscountValue] [decimal](18, 2) NOT NULL,
        [MaxDiscountAmount] [decimal](18, 2) NULL,
        [TargetType] [int] NOT NULL DEFAULT 0,
        [ProductId] [int] NULL,
        [CategoryId] [int] NULL,
        [OccasionId] [int] NULL,
        [StartDate] [datetime2](7) NOT NULL,
        [EndDate] [datetime2](7) NOT NULL,
        [Status] [int] NOT NULL DEFAULT 0,
        [Priority] [int] NOT NULL DEFAULT 0,
        [AdminNotes] [nvarchar](max) NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [IsDeleted] [bit] NOT NULL,
        [DeletedAt] [datetime2](7) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
     CONSTRAINT [PK_Offers] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    -- Indices
    CREATE NONCLUSTERED INDEX [IX_Offers_Status] ON [dbo].[Offers] ([Status] ASC)
    CREATE NONCLUSTERED INDEX [IX_Offers_DateRange] ON [dbo].[Offers] ([StartDate] ASC, [EndDate] ASC)
    CREATE NONCLUSTERED INDEX [IX_Offers_ProductId] ON [dbo].[Offers] ([ProductId] ASC)
    CREATE NONCLUSTERED INDEX [IX_Offers_CategoryId] ON [dbo].[Offers] ([CategoryId] ASC)
    CREATE NONCLUSTERED INDEX [IX_Offers_OccasionId] ON [dbo].[Offers] ([OccasionId] ASC)
END
GO

-- 2.2 Create 'Coupons' Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Coupons]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Coupons](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Code] [nvarchar](450) NOT NULL DEFAULT (N''), -- Indexed, so not max
        [Name] [nvarchar](max) NOT NULL DEFAULT (N''),
        [NameAr] [nvarchar](max) NULL,
        [Description] [nvarchar](max) NULL,
        [DescriptionAr] [nvarchar](max) NULL,
        [Type] [int] NOT NULL DEFAULT 0,
        [DiscountValue] [decimal](18, 2) NOT NULL,
        [MaxDiscountAmount] [decimal](18, 2) NULL,
        [MinOrderAmount] [decimal](18, 2) NULL,
        [ApplicableCustomerGroups] [nvarchar](max) NULL,
        [ApplicableCategories] [nvarchar](max) NULL,
        [ApplicableProducts] [nvarchar](max) NULL,
        [MaxTotalUsage] [int] NULL,
        [MaxUsagePerUser] [int] NULL,
        [CurrentUsageCount] [int] NOT NULL DEFAULT 0,
        [ValidFrom] [datetime2](7) NOT NULL,
        [ValidUntil] [datetime2](7) NOT NULL,
        [AdminNotes] [nvarchar](max) NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [IsDeleted] [bit] NOT NULL,
        [DeletedAt] [datetime2](7) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
     CONSTRAINT [PK_Coupons] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Coupons_Code] ON [dbo].[Coupons] ([Code] ASC) WHERE ([IsDeleted] = 0)
    CREATE NONCLUSTERED INDEX [IX_Coupons_DateRange] ON [dbo].[Coupons] ([ValidFrom] ASC, [ValidUntil] ASC)
END
GO

-- 2.3 Create 'CouponUsages' Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CouponUsages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[CouponUsages](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [CouponId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL DEFAULT (N''),
        [OrderId] [int] NULL,
        [DiscountApplied] [decimal](18, 2) NOT NULL,
        [IpAddress] [nvarchar](max) NULL,
        [UsedAt] [datetime2](7) NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [IsDeleted] [bit] NOT NULL,
        [DeletedAt] [datetime2](7) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
     CONSTRAINT [PK_CouponUsages] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    ALTER TABLE [dbo].[CouponUsages] WITH CHECK ADD CONSTRAINT [FK_CouponUsages_Coupons_CouponId] FOREIGN KEY([CouponId])
    REFERENCES [dbo].[Coupons] ([Id])
    ON DELETE CASCADE
    
    CREATE NONCLUSTERED INDEX [IX_CouponUsages_UserId] ON [dbo].[CouponUsages] ([UserId] ASC)
    CREATE NONCLUSTERED INDEX [IX_CouponUsages_OrderId] ON [dbo].[CouponUsages] ([OrderId] ASC)
END
GO

-- 2.4 Create 'LoyaltyTiers' Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoyaltyTiers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LoyaltyTiers](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Name] [nvarchar](max) NOT NULL DEFAULT (N''),
        [NameAr] [nvarchar](max) NULL,
        [Description] [nvarchar](max) NULL,
        [MinPoints] [int] NOT NULL,
        [PointsMultiplier] [decimal](5, 2) NOT NULL,
        [DiscountPercentage] [decimal](5, 2) NULL,
        [FreeShipping] [bit] NOT NULL DEFAULT 0,
        [BonusPointsOnBirthday] [int] NULL,
        [IconUrl] [nvarchar](max) NULL,
        [BadgeColor] [nvarchar](max) NULL,
        [SortOrder] [int] NOT NULL DEFAULT 0,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [IsDeleted] [bit] NOT NULL,
        [DeletedAt] [datetime2](7) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
     CONSTRAINT [PK_LoyaltyTiers] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    CREATE NONCLUSTERED INDEX [IX_LoyaltyTiers_MinPoints] ON [dbo].[LoyaltyTiers] ([MinPoints] ASC)
END
GO

-- 2.5 Create 'LoyaltyAccounts' Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoyaltyAccounts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LoyaltyAccounts](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL DEFAULT (N''),
        [CurrentPoints] [int] NOT NULL DEFAULT 0,
        [TotalEarnedPoints] [int] NOT NULL DEFAULT 0,
        [TotalRedeemedPoints] [int] NOT NULL DEFAULT 0,
        [TierId] [int] NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [IsDeleted] [bit] NOT NULL,
        [DeletedAt] [datetime2](7) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
     CONSTRAINT [PK_LoyaltyAccounts] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    ALTER TABLE [dbo].[LoyaltyAccounts] WITH CHECK ADD CONSTRAINT [FK_LoyaltyAccounts_LoyaltyTiers_TierId] FOREIGN KEY([TierId])
    REFERENCES [dbo].[LoyaltyTiers] ([Id])
    ON DELETE NO ACTION -- Tier shouldn't change just because account changes
    
    CREATE UNIQUE NONCLUSTERED INDEX [IX_LoyaltyAccounts_UserId] ON [dbo].[LoyaltyAccounts] ([UserId] ASC) WHERE ([IsDeleted] = 0)
END
GO

-- 2.6 Create 'LoyaltyTransactions' Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoyaltyTransactions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LoyaltyTransactions](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [LoyaltyAccountId] [int] NOT NULL,
        [Type] [int] NOT NULL,
        [Points] [int] NOT NULL,
        [Description] [nvarchar](max) NOT NULL DEFAULT (N''),
        [OrderId] [int] NULL,
        [OrderAmount] [decimal](18, 2) NULL,
        [BalanceAfter] [int] NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [IsDeleted] [bit] NOT NULL,
        [DeletedAt] [datetime2](7) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
     CONSTRAINT [PK_LoyaltyTransactions] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    ALTER TABLE [dbo].[LoyaltyTransactions] WITH CHECK ADD CONSTRAINT [FK_LoyaltyTransactions_LoyaltyAccounts_LoyaltyAccountId] FOREIGN KEY([LoyaltyAccountId])
    REFERENCES [dbo].[LoyaltyAccounts] ([Id])
    ON DELETE CASCADE
    
    CREATE NONCLUSTERED INDEX [IX_LoyaltyTransactions_LoyaltyAccountId] ON [dbo].[LoyaltyTransactions] ([LoyaltyAccountId] ASC)
    CREATE NONCLUSTERED INDEX [IX_LoyaltyTransactions_OrderId] ON [dbo].[LoyaltyTransactions] ([OrderId] ASC)
END
GO

-- 2.7 Create 'RegistrationCodes' Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RegistrationCodes]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RegistrationCodes](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Code] [nvarchar](450) NOT NULL DEFAULT (N''),
        [Description] [nvarchar](max) NULL,
        [Type] [int] NOT NULL,
        [TargetCustomerGroupId] [int] NULL,
        [WelcomeCreditAmount] [decimal](18, 2) NULL,
        [Currency] [nvarchar](max) NOT NULL DEFAULT (N'EGP'),
        [MaxTotalUsage] [int] NULL,
        [MaxUsagePerUser] [int] NULL,
        [CurrentUsageCount] [int] NOT NULL DEFAULT 0,
        [ValidFrom] [datetime2](7) NOT NULL,
        [ValidUntil] [datetime2](7) NOT NULL,
        [AdminNotes] [nvarchar](max) NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [IsDeleted] [bit] NOT NULL,
        [DeletedAt] [datetime2](7) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
     CONSTRAINT [PK_RegistrationCodes] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    CREATE UNIQUE NONCLUSTERED INDEX [IX_RegistrationCodes_Code] ON [dbo].[RegistrationCodes] ([Code] ASC) WHERE ([IsDeleted] = 0)
END
GO

-- 2.8 Create 'RegistrationCodeUsages' Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RegistrationCodeUsages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RegistrationCodeUsages](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [RegistrationCodeId] [int] NOT NULL,
        [UserId] [nvarchar](450) NOT NULL DEFAULT (N''),
        [IpAddress] [nvarchar](max) NULL,
        [UsedAt] [datetime2](7) NOT NULL,
        [GroupUpgradeApplied] [bit] NOT NULL DEFAULT 0,
        [WalletCreditApplied] [bit] NOT NULL DEFAULT 0,
        [CreditAmount] [decimal](18, 2) NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [IsDeleted] [bit] NOT NULL,
        [DeletedAt] [datetime2](7) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
     CONSTRAINT [PK_RegistrationCodeUsages] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    ALTER TABLE [dbo].[RegistrationCodeUsages] WITH CHECK ADD CONSTRAINT [FK_RegistrationCodeUsages_RegistrationCodes_RegistrationCodeId] FOREIGN KEY([RegistrationCodeId])
    REFERENCES [dbo].[RegistrationCodes] ([Id])
    ON DELETE CASCADE

    CREATE NONCLUSTERED INDEX [IX_RegistrationCodeUsages_UserId] ON [dbo].[RegistrationCodeUsages] ([UserId] ASC)
END
GO

-- 2.9 Create 'Banners' Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Banners]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Banners](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Title] [nvarchar](max) NOT NULL DEFAULT (N''),
        [TitleAr] [nvarchar](max) NULL,
        [Subtitle] [nvarchar](max) NULL,
        [SubtitleAr] [nvarchar](max) NULL,
        [DesktopImageUrl] [nvarchar](max) NOT NULL DEFAULT (N''),
        [MobileImageUrl] [nvarchar](max) NULL,
        [CtaText] [nvarchar](max) NULL,
        [CtaTextAr] [nvarchar](max) NULL,
        [CtaLink] [nvarchar](max) NULL,
        [Position] [int] NOT NULL DEFAULT 0,
        [SortOrder] [int] NOT NULL DEFAULT 0,
        [ValidFrom] [datetime2](7) NOT NULL,
        [ValidUntil] [datetime2](7) NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [IsDeleted] [bit] NOT NULL,
        [DeletedAt] [datetime2](7) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
     CONSTRAINT [PK_Banners] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    CREATE NONCLUSTERED INDEX [IX_Banners_Position] ON [dbo].[Banners] ([Position] ASC)
END
GO

-- 2.10 Ensure MassTransit Outbox Tables Exist (CatalogDB)
-- InboxState
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InboxState]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[InboxState](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [MessageId] [uniqueidentifier] NOT NULL,
        [ConsumerId] [uniqueidentifier] NOT NULL,
        [LockId] [uniqueidentifier] NOT NULL,
        [RowVersion] [timestamp] NOT NULL,
        [Received] [datetime2](7) NOT NULL,
        [ReceiveCount] [int] NOT NULL,
        [ExpirationTime] [datetime2](7) NULL,
        [Consumed] [datetime2](7) NULL,
        [Delivered] [datetime2](7) NULL,
        [LastUpdated] [datetime2](7) NULL,
     CONSTRAINT [PK_InboxState] PRIMARY KEY CLUSTERED ([Id] ASC)
    )
    CREATE UNIQUE NONCLUSTERED INDEX [IX_InboxState_MessageId_ConsumerId] ON [dbo].[InboxState] ([MessageId] ASC, [ConsumerId] ASC)
END
GO
-- OutboxMessage
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OutboxMessage]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OutboxMessage](
        [SequenceNumber] [bigint] IDENTITY(1,1) NOT NULL,
        [EnqueueTime] [datetime2](7) NULL,
        [SentTime] [datetime2](7) NOT NULL,
        [Headers] [nvarchar](max) NULL,
        [Properties] [nvarchar](max) NULL,
        [InboxMessageId] [uniqueidentifier] NULL,
        [InboxConsumerId] [uniqueidentifier] NULL,
        [OutboxId] [uniqueidentifier] NULL,
        [MessageId] [uniqueidentifier] NOT NULL,
        [ContentType] [nvarchar](256) NOT NULL,
        [Body] [nvarchar](max) NOT NULL,
        [ConversationId] [uniqueidentifier] NULL,
        [CorrelationId] [uniqueidentifier] NULL,
        [InitiatorId] [uniqueidentifier] NULL,
        [RequestId] [uniqueidentifier] NULL,
        [SourceAddress] [nvarchar](256) NULL,
        [DestinationAddress] [nvarchar](256) NULL,
        [ResponseAddress] [nvarchar](256) NULL,
        [FaultAddress] [nvarchar](256) NULL,
        [ExpirationTime] [datetime2](7) NULL,
     CONSTRAINT [PK_OutboxMessage] PRIMARY KEY CLUSTERED ([SequenceNumber] ASC)
    )
    CREATE NONCLUSTERED INDEX [IX_OutboxMessage_EnqueueTime] ON [dbo].[OutboxMessage] ([EnqueueTime] ASC)
    CREATE NONCLUSTERED INDEX [IX_OutboxMessage_ExpirationTime] ON [dbo].[OutboxMessage] ([ExpirationTime] ASC)
    CREATE UNIQUE NONCLUSTERED INDEX [IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber] ON [dbo].[OutboxMessage] ([InboxMessageId] ASC, [InboxConsumerId] ASC, [SequenceNumber] ASC)
    CREATE UNIQUE NONCLUSTERED INDEX [IX_OutboxMessage_OutboxId_SequenceNumber] ON [dbo].[OutboxMessage] ([OutboxId] ASC, [SequenceNumber] ASC)
END
GO
-- OutboxState
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OutboxState]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[OutboxState](
        [OutboxId] [uniqueidentifier] NOT NULL,
        [LockId] [uniqueidentifier] NOT NULL,
        [RowVersion] [timestamp] NOT NULL,
        [Created] [datetime2](7) NOT NULL,
        [Delivered] [datetime2](7) NULL,
        [LastUpdated] [datetime2](7) NULL,
     CONSTRAINT [PK_OutboxState] PRIMARY KEY CLUSTERED ([OutboxId] ASC)
    )
    CREATE NONCLUSTERED INDEX [IX_OutboxState_Created] ON [dbo].[OutboxState] ([Created] ASC)
END
GO

PRINT 'Schema Refactoring Script Completed Successfully'
