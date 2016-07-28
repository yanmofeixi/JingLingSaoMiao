MERGE INTO [dbo].[Scanner] AS Target 
USING (VALUES 
        ('duxiaoccnn@gmail.com', '%TGB6yhn^YHN5tgb'), 
        ('pkmscanner1@gmail.com', 'pokemongo'), 
        ('pkmscanner2@gmail.com', 'pokemongo')
) 
AS Source ([Email], [Password]) 
ON (Target.[Email] LIKE Source.[Email]
AND Target.[Password] LIKE Source.[Password])
WHEN NOT MATCHED BY TARGET THEN 
INSERT ([Email], [Password]) 
VALUES ([Email], [Password]);

MERGE INTO [dbo].[User] AS Target 
USING (VALUES 
        ('QiyangLu', 'yanmofeixi@gmail.com', '10,35,36,96,22,42,118,116,14,99,98,52,11,29,32,30,33,95,46,47,18,17,16,60,54,20,19,123,119,90,21,120,48,13,41', 1), 
        ('JunweiHu', '317772270@qq.com', '10,35,36,96,22,42,118,116,14,99,98,52,11,29,32,30,33,95,46,47,18,17,16,60,54,20,19,123,119,21,120,48,13,41', 1)
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
        ('JunweiHuHome', 47.678306, -122.13066, 1), 
        ('RedwestOffice', 47.659265, -122.140394, 2), 
        ('QiyangLuHome', 47.651168, -122.130718, 3)
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
        (1,2), 
        (1,3), 
        (2,1),
		(2,2)
) 
AS Source ([UserId], [LocationId]) 
ON (Target.[UserId] = Source.[UserId]
AND Target.[LocationId] = Source.[LocationId])
WHEN NOT MATCHED BY TARGET THEN 
INSERT ([UserId], [LocationId])  
VALUES ([UserId], [LocationId]) ;
