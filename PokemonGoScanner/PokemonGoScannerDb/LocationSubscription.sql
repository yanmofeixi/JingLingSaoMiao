CREATE TABLE [dbo].[LocationSubscription](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[LocationId] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))

GO

ALTER TABLE [dbo].[LocationSubscription]  WITH CHECK ADD FOREIGN KEY([LocationId])
REFERENCES [dbo].[Location] ([Id])
GO

ALTER TABLE [dbo].[LocationSubscription]  WITH CHECK ADD FOREIGN KEY([UserId])
REFERENCES [dbo].[User] ([Id])
GO