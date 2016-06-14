using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.IO.Compression;
using Spright.Web.Models.Requests;
using Microsoft.Practices.Unity;
using Spright.Web.Services;

namespace Spright.Web.Classes.Processes.EAV
{
    public class DownloadDTCZip
    {

        public ProcessCSV _ProcessCSV { get; set; }

        public IndexCsv _indexer { get; set; }

        public DownloadDTCZip(ProcessCSV ProcessCSV, IndexCsv indexer)
        {
            _ProcessCSV = ProcessCSV;

            _indexer = indexer;
        }

        public Task<int> DownloadUnZip()
        {
            return Task.Run(() =>
            {

                string filename = ConfigService.DTCFileName;
                string username = ConfigService.DTCUserName;
                string password = ConfigService.DTCPassword;
                String downloadDate = DateTime.Now.ToString("dd.MM.yyyy");
                string destinationFolder = AppDomain.CurrentDomain.BaseDirectory + "dtc/" + downloadDate;
                string destination = destinationFolder + "/dtcinventory.zip";

                int progress = 0;
                double progressP = 0;

                if (!Directory.Exists(destinationFolder))
                {
                    Directory.CreateDirectory(destinationFolder);
                }

                FtpWebRequest frequest = (FtpWebRequest)WebRequest.Create(filename);
                frequest.Method = WebRequestMethods.Ftp.GetFileSize;
                frequest.Credentials = new NetworkCredential(username, password);
                frequest.UsePassive = true;
                frequest.UseBinary = true;
                frequest.KeepAlive = true; //don't close the connection
                int dataLength = (int)frequest.GetResponse().ContentLength;

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(filename);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(username, password);
                request.UseBinary = true;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    using (Stream rs = response.GetResponseStream())
                    {
                        using (FileStream ws = new FileStream(destination, FileMode.Create))
                        {
                            byte[] buffer = new byte[2048];
                            int bytesRead = rs.Read(buffer, 0, buffer.Length);

                            System.Diagnostics.Debug.WriteLine("Downloading {0} ", filename);
                            while (bytesRead > 0)
                            {
                                ws.Write(buffer, 0, bytesRead);
                                bytesRead = rs.Read(buffer, 0, buffer.Length);
                                progress += bytesRead;
                                progressP = (double)progress / dataLength * 100;

                                System.Diagnostics.Debug.WriteLine("\r{0:N2}%   ", progressP);
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("Download completed on {0:HH:mm:ss tt}", DateTime.Now);
                System.Diagnostics.Debug.WriteLine("Extracting file...");

                ZipFile.ExtractToDirectory(destination, destinationFolder);

                System.Diagnostics.Debug.WriteLine("File extracted!  =)");

                //////Calls the Process CSV once Download and Unzip is done//////
                AdminImportRequestModel Model = new AdminImportRequestModel();
                Model.FilePath = destinationFolder;
                Model.EntityId = Int32.Parse(ConfigService.AutoDealioEntityID);
                Model.WebsiteId = Int32.Parse(ConfigService.AutoDealioWebsiteID);



                _indexer.parseCSV(Model);
                return 5;

            });

        }
    }
}