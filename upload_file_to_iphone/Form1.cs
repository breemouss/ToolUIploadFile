﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace upload_file_to_iphone
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string listIP;
        string fileUpload;

        private string GetSVContent(string url)
        {
            Console.WriteLine(url);
            string rs = "";
            try
            {

                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; " +
                                  "Windows NT 5.2; .NET CLR 1.0.3705;)");
                    rs = client.DownloadString(url);
                }
                if (string.IsNullOrEmpty(rs)) { rs = ""; }

            }
            catch { rs = ""; }
            return rs;
        }

        private void listIpBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openListIp = new OpenFileDialog();
            openListIp.ShowDialog();
            listIpTxt.Text = openListIp.FileName;

            listIP = openListIp.FileName;
        }

        private void fileUploadBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileUpload = new OpenFileDialog();
            openFileUpload.ShowDialog();
            fileUploadTxt.Text = openFileUpload.FileName;

            fileUpload = openFileUpload.FileName;
        }
        public async Task<string> uploadTool(string ip, string filePath)
        {
            string subPath="";
            Request api = new Request();
            return await api.Upload(ip, filePath, subPath);
        }

        private void uploadBtn_Click(object sender, EventArgs e)
        {
            var lines = File.ReadAllLines(listIP);

            foreach(var line  in lines)
            {
                string ip = line;
                uploadTool(ip, fileUpload);
                statusLbl.Text = "upload file "+ fileUpload+ " to " + ip + "success!!";
            }

            statusLbl.Text = "upload file success!!";
        }

        public async Task<string> uploadImageTool(string ip, string filePath)
        {
            Request api = new Request();
            var response = await api.Upload(ip, filePath);
            return response;
        }

        private void uploadImgBtn_Click(object sender, EventArgs e)
        {
            var lines = File.ReadAllLines(listIP);

            foreach (var line in lines)
            {
                string ip = line;
                uploadImageTool(ip, fileUpload);
                statusLbl.Text = "upload file " + fileUpload + " to " + ip + "success!!";
            }

            statusLbl.Text = "upload file success!!";
        }

        public void PlayScript(string address, string filePath)
        {
            string rs = GetSVContent($@"http://{address}:8080/control/start_playing?path=/{filePath}");
            if (rs != "")
            {
                try
                {
                    JObject rsObject = JObject.Parse(rs);
                    if (rsObject["status"].ToString() == "success")
                    {
                        return;
                    }
                    else
                    {
                        throw new Exception(rsObject["info"].ToString());
                    }
                }
                catch { }
            }
            else
            {
                throw new Exception("request server timeout!");
                //Console.WriteLine("request server timeout!");
            }
        }

        public void StopScript(string address, string filePath)
        {
            string rs = GetSVContent($@"http://{address}:8080/control/stop_playing?path=/{filePath}");
            if (rs != "")
            {
                try
                {
                    JObject rsObject = JObject.Parse(rs);
                    if (rsObject["status"].ToString() == "success")
                    {
                        return;
                    }
                    else
                    {
                        throw new Exception(rsObject["info"].ToString());
                    }
                }
                catch { }
            }
            else
            {
                throw new Exception("request server timeout!"); ;
            }
        }

        private void runBtn_Click(object sender, EventArgs e)
        {
            var lines = File.ReadAllLines(listIP);

            foreach (var line in lines)
            {
                string filePath = startTxt.Text;
                string ip = line;
                StopScript( ip,  filePath);
                statusLbl.Text = "upload file " + fileUpload + " to " + ip + "success!!";
            }

            statusLbl.Text = "upload file success!!";
        }

        private void stopBtn_Click(object sender, EventArgs e)
        {
            var lines = File.ReadAllLines(listIP);

            foreach (var line in lines)
            {
                string filePath = startTxt.Text;
                string ip = line;
                PlayScript(ip, filePath);
                statusLbl.Text = "upload file " + fileUpload + " to " + ip + "success!!";
            }

            statusLbl.Text = "upload file success!!";
        }
    }
    public class Request
    {
        public async Task<string> Get(string url)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    var errMess = await response.Content.ReadAsStringAsync();
                    throw new Exception(errMess);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> Post(string url, HttpContent c)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.PostAsync(url, c);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<string> Upload(string ip, string filepath, string subFilePath = "")
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                MultipartFormDataContent form = new MultipartFormDataContent();

                //FileStream fs = File.OpenRead(filepath);
                //form.Add(new StreamContent(fs), "file", Path.GetFileName(filepath));
                //var url = "http://" + ip + ":8080/file/upload?path=" + subFilePath + Path.GetFileName(filepath);
                //var response = await httpClient.PostAsync(url, form);
                //response.EnsureSuccessStatusCode();
                //fs.Close();
                using (FileStream fs = File.OpenRead(filepath))
                {
                    form.Add(new StreamContent(fs), "file", Path.GetFileName(filepath));
                    var url = "http://" + ip + ":8080/file/upload?path=" + subFilePath + Path.GetFileName(filepath);
                    var response = await httpClient.PostAsync(url, form);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex.Message}");
                return "error";
            }
        }
    }
}
