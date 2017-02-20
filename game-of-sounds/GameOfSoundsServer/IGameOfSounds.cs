namespace GameOfSoundsServer
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.Text;

    [ServiceContract]
    public interface IGameOfSounds
    {
        [OperationContract]
        [WebGet]
        string EchoWithGet(string s);

        [OperationContract]
        [WebGet]
        string AzureStore(string input);

        [OperationContract]
        [WebGet]
        string AzureGet(string input);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/EchoWithPost", BodyStyle = WebMessageBodyStyle.Bare)]
        string EchoWithPost(Stream s);
    }
}
