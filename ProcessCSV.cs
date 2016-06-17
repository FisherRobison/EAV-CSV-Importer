using CsvHelper;
using Spright.Web.Domain;
using Spright.Web.Enums;
using Spright.Web.Models.Requests;
using Spright.Web.Models.Requests.EVA;
using Spright.Web.Services;
using Spright.Web.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace Spright.Web.Classes.Processes.EAV
{
    public class ProcessCSV
    {
        //Injecting our services that we are going to need in this class
        
        public IRecordServices _recordService { get; set; }
        public IEntityServices _entityService { get; set; }
        private AdminImportRequestModel _AdminImport { get; set; }
        public InsertRecord _InsertRecord { get; set; }

        public ProcessCSV(IRecordServices recordService, IEntityServices entityService, InsertRecord insertRecord)
        {
            _recordService = recordService;
            _entityService = entityService;
            _InsertRecord = insertRecord;

        }
    
        public Task<int> parseCSV(AdminImportRequestModel model)
        {
            return Task.Run(() =>
            {
                //Use downloadDate because when the file gets downloaded it is saved by date and then this function runs
                //Directly after the file has been downloaded
                String downloadDate = DateTime.Now.ToString("dd.MM.yyyy");
                string destination = AppDomain.CurrentDomain.BaseDirectory + "dtc/" + downloadDate + "/dtcinventory.txt";
                _AdminImport = model;

                EVA_Entity Entity = _entityService.GetByID(model.EntityId);

                using (var sr = new StreamReader(destination))
                {
                    var csv = new CsvReader(sr);
                    csv.Configuration.Delimiter = "\t";
                    csv.Configuration.IgnoreHeaderWhiteSpace = true;
                    var records = csv.GetRecords<TestDTCDealerRequestModel>();


                    foreach (var record in records)
                    {
                        if (record == null)
                        {
                            break;
                        }
                //Im making a new instance of RRM which is going to get passed in at the end
                        RecordRequestModel RRM = new RecordRequestModel();
                        RRM.EntityId = this._AdminImport.EntityId;
                        RRM.WebsiteId = this._AdminImport.WebsiteId;
                        RRM.AttributeId = 136;
                        RRM.Values = new List<ValueRequestModel>();
                        RRM.Medias = new List<MediaRequestModel>();

                        TestDTCDealerRequestModel VehicleRM = new TestDTCDealerRequestModel();

                        PropertyInfo[] Y = VehicleRM.GetType().GetProperties();

                        for (var p = 0; p < Y.Length; p++)
                        {
                            string slug = UtilityService.camelCaseToDash(Y[p].Name);

                            foreach (var attribute in Entity.Attributes)
                            {
                                if (slug == attribute.Slug)
                                {
                                    ValueRequestModel ValueRm = new ValueRequestModel();
                                    ValueRm.AttributeId = attribute.ID;
                                    ValueRm.ValueString = record.GetType().GetProperty(Y[p].Name).GetValue(record, null).ToString();
                                    System.Diagnostics.Debug.WriteLine(ValueRm.ValueString);
                                    RRM.Values.Add(ValueRm);
                                }
                            }
                        }

                //Within the CSV there are numbers seperated by pipes which are the img number
                //Here we seperate them on the pipe and insert them into the url
                        if (record.Images != null)
                        {
                            string[] RecordMediaArray = record.Images.Split('|');
                            var x = 0;
                            foreach (string recordMediaPath in RecordMediaArray)
                            {
                                RecordMediaRequestModel RecordMedia = new RecordMediaRequestModel();
                                RecordMedia.FileName = "http://img.leaddelivery.net/images/" + record.VIN + "/Original/" + recordMediaPath + ".jpg";
                                RecordMedia.MediaType = "3";
                                RecordMedia.FileType = "image/jpeg";
                                System.Diagnostics.Debug.WriteLine(RecordMedia.FileName);
                    //Setting first image equal to the cover photo
                                if (x == 0)
                                {
                                    RecordMedia.IsCoverPhoto = true;
                                }
                                else
                                {
                                    RecordMedia.IsCoverPhoto = false;
                                }

                                RRM.Medias.Add(RecordMedia);

                                x++;
                            }
                        }
                        //The RRM final gets inserted into the ProcessAsync Task
                        _InsertRecord.ProcessAsync(RRM);
                    }
                }
                return 5;
            });

        }
    }
}
