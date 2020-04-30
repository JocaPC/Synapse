DROP CREDENTIAL  [https://synapseiotstorage.blob.core.windows.net];


CREATE CREDENTIAL  [https://synapseiotstorage.blob.core.windows.net]
WITH IDENTITY='SHARED ACCESS SIGNATURE',  
SECRET = 'sv=2019-10-10&ss=b&srt=sco&sp=rlx&se=2025-04-28T06:39:08Z&st=2020-03-27T23:39:08Z&spr=https&sig=69YR800hjRGXfGmRWoKY74rdKiATetJy5LywEMT8CJQ%3D'
go

SELECT time, level, device, temperature, humidity, info
FROM OPENROWSET( BULK 'https://synapseiotstorage.blob.core.windows.net/iotmessages/SynapseDemoIoTHub/2020-04-27/22/p-00-38.json',
                    FORMAT='CSV', FIELDTERMINATOR ='0x0b', FIELDQUOTE = '0x0b' )
    WITH ( message varchar(8000) ) AS json
    CROSS APPLY OPENJSON(json.message)
        WITH (  time datetime2 '$.EnqueuedTimeUtc', level varchar(20) '$.Properties.level',
                device varchar(100) '$.SystemProperties.connectionDeviceId', Body varbinary(max))
           CROSS APPLY OPENJSON (CONVERT(nvarchar(max), Body, 0))
                WITH (temperature float, humidity float, info nvarchar(200))