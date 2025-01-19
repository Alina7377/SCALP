using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using System.Runtime.InteropServices.ComTypes;
using System.Collections.ObjectModel;
using MongoDB.Bson.Serialization.Attributes;

public class DBManager : MonoBehaviour
{
    private MongoClient _clientDB = new MongoClient("mongodb+srv://SCALP_Lab:777777sem@cluster0.8rp7p.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0");
    private IMongoDatabase _database;
    private IMongoCollection<BsonDocument> _collection;
    private IMongoCollection<Person> _collectionPerson;
    private char _recordSeparator = '|';
    // private bool _isConnect;

    private async void Awake()
    {
        // ConnectBaseAsync();
        _database = _clientDB.GetDatabase("SCALP");
        _collection = _database.GetCollection<BsonDocument>("PlayerProfiles");
        _collectionPerson = _database.GetCollection<Person>("PlayerProfiles");
    }

    /* private async void  ConnectBaseAsync() 
     {
         await Task.Run(ConectionBase);

         if (_isConnect)
         {
             // ������ ���, ��� �� �������� � ���� - ���������� ������ �� ������            
             _database = _clientDB.GetDatabase("SCALP");
             _collection = _database.GetCollection<BsonDocument>("PlayerProfiles");
             SaveProfileData();
         }
     }

     private void ConectionBase() 
     {
         try
         {
             _clientDB = new MongoClient("mongodb+srv://SCALP_Lab:777777sem@cluster0.8rp7p.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0");
             _isConnect = true;
         }
         catch
         {
             Debug.Log("��� ����������� �");
             _isConnect = false;
         }
     }*/

    /// <summary>
    /// �������� ���� ������ �� ��
    /// </summary>
    public async void RemoveAllData()
    {
        var allRecords = _collection.FindAsync(new BsonDocument());
        var recordAwaited = await allRecords;

        foreach (var record in recordAwaited.ToList())
        {
            _collection.DeleteOne(record);
        }
       
    }

    public async void RemoveDataForName(string name) 
    {
        var allRecords = _collection.FindAsync(new BsonDocument { { "Name", name } });
        var recordAwaited = await allRecords;

        foreach (var record in recordAwaited.ToList())
        {
            _collection.DeleteOne(record);
        }
    }

    private Person GetProfilData() 
    {
        SProfileData profileData = Profile.Instance.GetProfilData;
        Person person = new Person();
        person.Name = profileData.UserName;
        person.BestTime = profileData.BestTime;
        person.CountGame = profileData.CountGame;
        person.CountWin = profileData.CountWin;
        return person;
    }

    public async Task<List<BsonDocument>> CheackRecord(string newName) 
    {
        Person person = GetProfilData();
        var collection = _database.GetCollection<BsonDocument>("PlayerProfiles");
        var filter = new BsonDocument { { "Name", newName } };
        //  var filter = Builders<Person>.Filter.Eq(p => p.Name, person.Name);
        var allRec = collection.FindAsync(filter);
        var res = await allRec;
        return res.ToList();
        

    }

    public async Task UpdateData() 
    {
        Person person = GetProfilData();
        
        // ��������� ���������� ������ � ��� ������, ���� � ��� ���� ��� �������
        if (person.Name != "")
        {
            var collection = _database.GetCollection<BsonDocument>("PlayerProfiles");
            var filter = new BsonDocument { { "Name", person.Name } };
            var docum = new BsonDocument { { "Name", person.Name }, { "BestTime", person.BestTime }, { "CountWin", person.CountWin }, { "CountGame", person.CountGame } };
            var result = await collection.ReplaceOneAsync(filter, docum, new UpdateOptions { IsUpsert = true });
        }
        return;
    }

    /// <summary>
    /// ������ ��� ���������� ���� ������
    /// </summary>
    public async Task UpdateNiknameData(string newNikname)
    {
        Person person = GetProfilData();
        if (person.Name != "")
        {
            var collection = _database.GetCollection<BsonDocument>("PlayerProfiles");
            var filter = new BsonDocument { { "Name", person.Name } };
            var docum  = new BsonDocument { { "Name", newNikname }, { "BestTime", person.BestTime },{ "CountWin",person.CountWin },{ "CountGame", person.CountGame } };
            var result = await collection.ReplaceOneAsync(filter, docum);
        }
        else
        {
            person.Name = newNikname;
            await _collectionPerson.InsertOneAsync(person);
        }
    }

    public async Task<List<SProfileData>> GetDBcollection()
    {
        var collection = _database.GetCollection<Person>("PlayerProfiles");
        List<Person> persons = await collection.Find(new BsonDocument()).ToListAsync();

        List<SProfileData> profileDats = new List<SProfileData>();
        SProfileData profileData = new SProfileData();
        foreach (var person in persons) 
        {
            profileData.UserName = person.Name;
            profileData.BestTime = person.BestTime;
            profileData.CountGame = person.CountGame;
            profileData.CountWin = person.CountWin;

            profileDats.Add(profileData);
        }

        return profileDats;
    }

    /// <summary>
    /// ��������� ���� ��� ��������� ������� �� ���������
    /// </summary>
    [BsonIgnoreExtraElements]
    class Person
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; } = "";
        public float BestTime { get; set; }
        public float CountWin { get; set; }
        public float CountGame { get; set; }
        
    }
}
