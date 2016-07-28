CREATE TABLE [dbo].[Scanner](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Email] [text] NOT NULL,
	[Password] [text] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))