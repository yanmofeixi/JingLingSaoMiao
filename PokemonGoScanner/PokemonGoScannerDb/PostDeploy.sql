/* debug data only */

MERGE INTO [dbo].[Scanner] AS Target 
USING (VALUES 
        ('duxiaoccnn@gmail.com', '%TGB6yhn^YHN5tgb'), 
        ('pkmscanner1@gmail.com', 'pokemongo'), 
        ('pkmscanner2@gmail.com', 'pokemongo'),
		('pkmscanner3@gmail.com', 'pokemongo')
) 
AS Source ([Email], [Password]) 
ON (Target.[Email] LIKE Source.[Email]
AND Target.[Password] LIKE Source.[Password])
WHEN NOT MATCHED BY TARGET THEN 
INSERT ([Email], [Password]) 
VALUES ([Email], [Password]);

MERGE INTO [dbo].[User] AS Target 
USING (VALUES 
        ('QiyangLu', 'yanmofeixi@gmail.com', '150', 1)
) 
AS Source ([UserName], [EmailForAlert], [IgnoreList], [IsActive]) 
ON (Target.[UserName] LIKE Source.[UserName]
AND Target.[EmailForAlert] LIKE Source.[EmailForAlert]
AND Target.[IgnoreList] LIKE Source.[IgnoreList]
AND Target.[IsActive] = Source.[IsActive])
WHEN NOT MATCHED BY TARGET THEN 
INSERT ([UserName], [EmailForAlert], [IgnoreList], [IsActive]) 
VALUES ([UserName], [EmailForAlert], [IgnoreList], [IsActive]);

MERGE INTO [dbo].[Location] AS Target 
USING (VALUES 
        ('BellevuePark', 47.612504, -122.204155, 1)
) 
AS Source ([Name], [Latitude], [Longitude], [ScannerId]) 
ON (Target.[Name] LIKE Source.[Name]
AND Target.[Latitude] = Source.[Latitude]
AND Target.[Longitude] = Source.[Longitude]
AND Target.[ScannerId] = Source.[ScannerId])
WHEN NOT MATCHED BY TARGET THEN 
INSERT ([Name], [Latitude], [Longitude], [ScannerId]) 
VALUES ([Name], [Latitude], [Longitude], [ScannerId]);

MERGE INTO [dbo].[LocationSubscription] AS Target 
USING (VALUES 
        (1,1)
) 
AS Source ([UserId], [LocationId]) 
ON (Target.[UserId] = Source.[UserId]
AND Target.[LocationId] = Source.[LocationId])
WHEN NOT MATCHED BY TARGET THEN 
INSERT ([UserId], [LocationId])  
VALUES ([UserId], [LocationId]) ;
