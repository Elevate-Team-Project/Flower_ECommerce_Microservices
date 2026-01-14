BEGIN TRANSACTION;
GO

CREATE TABLE [Carts] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(max) NOT NULL,
    [CouponCode] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [DeletedAt] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Carts] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [DeliveryZones] (
    [Id] int NOT NULL IDENTITY,
    [ZoneName] nvarchar(max) NOT NULL,
    [City] nvarchar(max) NOT NULL,
    [State] nvarchar(max) NOT NULL,
    [Country] nvarchar(max) NOT NULL,
    [ShippingCost] decimal(18,2) NOT NULL,
    [EstimatedDeliveryDays] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [DeletedAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_DeliveryZones] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [UserAddresses] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [AddressLabel] nvarchar(max) NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [Phone] nvarchar(max) NOT NULL,
    [Latitude] float NULL,
    [Longitude] float NULL,
    [Governorate] nvarchar(max) NOT NULL,
    [City] nvarchar(max) NOT NULL,
    [Street] nvarchar(max) NOT NULL,
    [Building] nvarchar(max) NULL,
    [Floor] nvarchar(max) NULL,
    [Apartment] nvarchar(max) NULL,
    [PostalCode] nvarchar(max) NULL,
    [Country] nvarchar(max) NOT NULL,
    [IsDefault] bit NOT NULL,
    [Notes] nvarchar(max) NULL,
    [Landmark] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [DeletedAt] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_UserAddresses] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [CartItems] (
    [Id] int NOT NULL IDENTITY,
    [CartId] int NOT NULL,
    [ProductId] int NOT NULL,
    [ProductName] nvarchar(max) NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [Quantity] int NOT NULL,
    [PictureUrl] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [DeletedAt] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_CartItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CartItems_Carts_CartId] FOREIGN KEY ([CartId]) REFERENCES [Carts] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Shipments] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [DeliveryAddressId] int NOT NULL,
    [TrackingNumber] nvarchar(450) NOT NULL,
    [Carrier] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [EstimatedDeliveryDate] datetime2 NULL,
    [ActualDeliveryDate] datetime2 NULL,
    [CurrentLocation] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    [IsGift] bit NOT NULL,
    [RecipientName] nvarchar(max) NULL,
    [RecipientPhone] nvarchar(max) NULL,
    [GiftMessage] nvarchar(max) NULL,
    [DriverLatitude] float NULL,
    [DriverLongitude] float NULL,
    [LastLocationUpdate] datetime2 NULL,
    [DriverName] nvarchar(max) NULL,
    [DriverPhone] nvarchar(max) NULL,
    [DriverPhotoUrl] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [DeletedAt] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_Shipments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Shipments_UserAddresses_DeliveryAddressId] FOREIGN KEY ([DeliveryAddressId]) REFERENCES [UserAddresses] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_CartItems_CartId] ON [CartItems] ([CartId]);
GO

CREATE INDEX [IX_Shipments_DeliveryAddressId] ON [Shipments] ([DeliveryAddressId]);
GO

CREATE INDEX [IX_Shipments_OrderId] ON [Shipments] ([OrderId]);
GO

CREATE INDEX [IX_Shipments_TrackingNumber] ON [Shipments] ([TrackingNumber]);
GO

CREATE INDEX [IX_UserAddresses_UserId] ON [UserAddresses] ([UserId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260114223315_MergeDeliveryService', N'8.0.22');
GO

COMMIT;
GO

