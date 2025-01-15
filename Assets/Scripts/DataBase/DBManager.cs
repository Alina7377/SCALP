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
            // Каждый раз, как мы попадаем в меню - отправляем данные на сервер            
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
            Debug.Log("Нет подключения у");
            _isConnect = false;
        }
    }*/

    private SProfileData ParsData(string record) 
    {
        // Отсекаем ненужную часть из строки вида { "_id" : ObjectId("676aa424e7b73712ec982283"), "TestUser1" : "0|2" } //
        string recordPars = record.Substring(record.IndexOf("),") + 4, record.IndexOf("}") - record.IndexOf("),") - 6);
        string data = recordPars.Substring(recordPars.IndexOf(":") + 3);
        string[] recordString = data.Split(_recordSeparator);
        SProfileData profileRecord = new SProfileData();
        profileRecord.UserName = recordPars.Substring(0, record.IndexOf(":") + 1);
        profileRecord.BestTime = float.Parse(recordString[0]);
        profileRecord.CountWin = float.Parse(recordString[1]);
        profileRecord.CountGame = float.Parse(recordString[2]);
        return profileRecord;
    }


    // Это для тестов
    public async void SaveProfileData(string userName,string recordData) 
    {
        bool isCreat = false;
        var allRecords = _collection.FindAsync(new BsonDocument());
        var recordAwaited = await allRecords;

        var filter = new BsonDocument { };
        var docum = new BsonDocument { { userName, recordData } };

        foreach (var record in recordAwaited.ToList())
        {
            
            if (ParsData(record.ToString()).UserName == userName)
            {
                filter = record;
                isCreat = true;
                break;
            }
        }
        if (isCreat)
            await _collection.FindOneAndUpdateAsync(filter, docum);
        else 
            await _collection.InsertOneAsync(docum);
    }

   /* public async Task<List<SProfileData>> GetDBcollection() 
    {
        var allRecords = _collection.FindAsync(new BsonDocument());
        var recordAwaited = await allRecords;

        List<SProfileData> profileDats = new List<SProfileData>();
        foreach (var record in recordAwaited.ToList())
        {
            profileDats.Add(ParsData(record.ToString()));
        }

        return profileDats;
    }*/
      

    public async Task UpdateNikname(string newName) 
    {
        bool isCreat = false;
        SProfileData profilData = Profile.Instance.GetProfilData;
        string userName = profilData.UserName;
        string recordVal = profilData.BestTime.ToString() + "|" +
                           profilData.CountWin.ToString() + "|" +
                           profilData.CountGame.ToString();

        var allRecords = _collection.FindAsync(new BsonDocument());
        var recordAwaited = await allRecords;

        var filter = new BsonDocument ();
        var docum = new BsonDocument { { newName, recordVal } };

        foreach (var record in recordAwaited.ToList())
        {
            if (ParsData(record.ToString()).UserName == userName)
            {
                Debug.Log("Найдена запись с именем " + userName);
                filter = record;
                isCreat = true;
                break;
            }
        }

        if (isCreat)
            await _collection.FindOneAndUpdateAsync(filter, docum);
        else
           await _collection.InsertOneAsync(docum);
    }


    /// <summary>
    /// Удаление всех данных из бд
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
        
        // Выполняем обновление только в том случае, если у нас есть имя профили
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
    /// Только для обновления ника игрока
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
    /// Вложенный клас для уарвления данными из документа
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
