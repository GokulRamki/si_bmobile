using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using si_bmobile.Models;
using si_bmobile.DAL;
using si_bmobile.Utils;
using System.Configuration;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;

namespace si_bmobile.bkService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "downloadbkup" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select downloadbkup.svc or downloadbkup.svc.cs at the Solution Explorer and start debugging.
    public class downloadbkup : Idownloadbkup
    {
        private IUtilityRepository _util_repo;

        #region for  zip process1
        public string ftpDir = ConfigurationManager.AppSettings["ftp_folder_fname"];
        public string down_path = ConfigurationManager.AppSettings["download_path"];

        public string temp_destination_path = ConfigurationManager.AppSettings["temp_destination_path"];
        public string source_path_details = ConfigurationManager.AppSettings["source_folder_details"];
        #endregion

        public downloadbkup()
        {
           
            this._util_repo = new UtilityRepository();
        }

        public bool zipprocess(string id)
        {
            try
            {


                if (!string.IsNullOrWhiteSpace(id))
                {
                    var source_folder_details = source_path_details.Split(',').ToList();
                    List<string> source_details = new List<string>();
                    if (source_folder_details.Count > 0)
                    {
                        foreach (var folder_details in source_folder_details)
                        {
                            source_details.Add(folder_details);
                        }
                    }
                    if (source_details.Count > 0)
                    {
                        foreach (var path in source_details)
                        {
                            DirectoryInfo d = new DirectoryInfo(path);     //Assuming  your Folder
                            if (d.Exists)
                            {

                                var Files = d.GetFiles().ToList();   //Getting Text files


                                Files = Files.Where(x => x.Name.ToLower().Trim().Equals(id.ToLower().Trim())).ToList();

                                if (Files.Count > 0)
                                {
                                    foreach (var file in Files)
                                    {
                                        string sourceName = path;

                                        ProcessStartInfo p = new ProcessStartInfo();
                                        p.FileName = @"C:\Program Files\7-Zip\7z.exe";
                                        p.Arguments = "a  -t7z \"" + sourceName + Path.GetFileNameWithoutExtension(file.ToString()) + "\" \"" + sourceName + file + "\" -mx=9";
                                        p.WindowStyle = ProcessWindowStyle.Hidden;
                                        Process x = Process.Start(p);
                                        x.WaitForExit();
                                    }
                                    DirectoryInfo Get_bak_files = new DirectoryInfo(path);
                                    string destinationPath = temp_destination_path;
                                    FileInfo[] fileList = Get_bak_files.GetFiles("*.7z");
                                    if (fileList != null)
                                    {
                                        foreach (FileInfo file in fileList)
                                        {

                                            string fileToMove = path + file;
                                            string moveTo = destinationPath + file;

                                            System.IO.File.Move(fileToMove, moveTo);
                                        }
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;

                }

            }
            catch (Exception ex)
            {
                _util_repo.ErrorLog_Txt(ex);
            }
            return false;
        }
    }
}
