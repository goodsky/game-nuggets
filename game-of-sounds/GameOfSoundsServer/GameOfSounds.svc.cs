namespace GameOfSoundsServer
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.Text;

    public class GameOfSounds : IGameOfSounds
    {
        public string EchoWithGet(string s)
        {
            return "You said " + s;
        }

        public static int GuidCount = 0;
        public string AzureStore(string input)
        {
            string[] paramz = input.Split('|');
            string key = paramz[0];
            string msg = paramz[1];

            StreamReader keyFile = new StreamReader(Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, @"azurestring.key"));
            string azureKey = keyFile.ReadLine();

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(string.Format("DefaultEndpointsProtocol=https;AccountName=gameofsounds;AccountKey={0}", azureKey));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("test");
            table.CreateIfNotExists();

            TestEntity testEntity = new TestEntity(key);
            testEntity.message = msg;
            testEntity.GUID = GuidCount++;

            TableOperation insertOperation = TableOperation.Insert(testEntity);
            table.Execute(insertOperation);

            return "Store Complete. Object stored: " + testEntity.ToString();
        }

        public string AzureGet(string input)
        {
            StreamReader keyFile = new StreamReader(Path.Combine(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath, @"azurestring.key"));
            string azureKey = keyFile.ReadLine();

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(string.Format("DefaultEndpointsProtocol=https;AccountName=gameofsounds;AccountKey={0}", azureKey));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("test");
            
            if (!table.Exists())
            {
                return "Table test does not exist";
            }

            TableOperation retrieveOperation = TableOperation.Retrieve<TestEntity>("partition0", input);
            TableResult result = table.Execute(retrieveOperation);

            if (result.Result != null)
            {
                return "Get Complete. Object returned: " + ((TestEntity)result.Result).ToString();
            }

            return "Azure Get did not find an object for key: " + input;
        }

        // Test Table Entity Created
        public class TestEntity : TableEntity
        {
            public TestEntity(string key)
            {
                this.PartitionKey = "partition0";
                this.RowKey = key;
            }

            public TestEntity() { }

            public string message { get; set; }
            public int GUID { get; set; }

            public string ToString()
            {
                return string.Format("Key: {0} Message: {1} GUID: {2}", this.PartitionKey, this.message, this.GUID);
            }
        }

        public string EchoWithPost(Stream s)
        {
            return "You said " + new StreamReader(s).ReadLine();
        }
    }
}
    