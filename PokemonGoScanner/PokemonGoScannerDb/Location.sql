CREATE TABLE [dbo].[Location](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [text] NOT NULL,
	[Latitude] [float] NOT NULL,
	[Longitude] [float] NOT NULL,
	[ScannerId] [int] NOT NULL DEFAULT ((0)),
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))
GO

ALTER TABLE [dbo].[Location]  WITH CHECK ADD FOREIGN KEY([ScannerId])
REFERENCES [dbo].[Scanner] ([Id])
GO