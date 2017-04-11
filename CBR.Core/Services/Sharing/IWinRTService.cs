using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;

namespace CBR.Core
{
    [MessageContract]
    public class FileRequestMessage
    {
        [MessageHeader(MustUnderstand = true)]
        public string FileInfo;
    }

    [MessageContract]
    public class FileRequestMessageReturn
    {
        [MessageHeader(MustUnderstand = true)]
        public string FileInfo;

        [MessageBodyMember(Order = 1)]
        public Stream FileData;
    }

    [ServiceContract]
    public interface IWinRTService
    {
        [OperationContract]
        bool IsAlive();

        [OperationContract]
        List<WCFCatalog> GetCatalogList();

        [OperationContract]
        List<WCFBook> GetCatalogContent(string ID);

        [OperationContract]
        WCFCatalog GetCatalog(string ID);

        [OperationContract]
        Stream GetBookStream(string ID);

        [OperationContract]
        FileRequestMessageReturn GetBookStreamByMessage(FileRequestMessage msg);
    }

    [DataContract]
    public class WCFCatalog
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int BookCount { get; set; }

        [DataMember]
        public byte[] Image { get; set; }
    }

    [DataContract]
    public class WCFBook
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string FileInfo { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Bookmark { get; set; }
        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public bool IsRead { get; set; }
        [DataMember]
        public bool IsSecured { get; set; }

        [DataMember]
        public int PageCount { get; set; }
        [DataMember]
        public long Size { get; set; }
        [DataMember]
        public int Rating { get; set; }

        [DataMember]
        public byte[] Image { get; set; }
    }

}
