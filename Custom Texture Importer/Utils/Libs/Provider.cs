﻿using System.Net.NetworkInformation;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Versions;
using Custom_Texture_Importer.Models;
using Newtonsoft.Json;

namespace Custom_Texture_Importer.Utils.Libs;

public class MyFileProvider
{
    public readonly DefaultFileProvider Provider;

    public MyFileProvider()
    {
        if (!CheckForConnection())
            throw new HttpRequestException("No internet connection");
        
        Provider = new DefaultFileProvider(FortniteUtil.PakPath, SearchOption.TopDirectoryOnly, false,
            new VersionContainer(EGame.GAME_UE5_LATEST));
        Provider.Initialize();

        var client = new HttpClient();
        var response = client.GetAsync("https://fortnite-api.com/v2/aes").Result;
        var aes = JsonConvert.DeserializeObject<AES>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult())?.Data;

        var keys = new List<KeyValuePair<FGuid, FAesKey>>();
        if (aes.MainKey != null)
            keys.Add(new KeyValuePair<FGuid, FAesKey>(new FGuid(), new FAesKey(aes.MainKey)));
        keys.AddRange(from x in aes.DynamicKeys
            select new KeyValuePair<FGuid, FAesKey>(new FGuid(x.PakGuid), new FAesKey(x.Key)));
        
        Provider.SubmitKeys(keys);
    }

    private bool CheckForConnection()
    {
        using var ping = new Ping();
        try
        {
            var reply = ping.Send("google.com", 5000);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }
}