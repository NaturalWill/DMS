﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace DMS
{
    public class cConfig
    {
        /// <summary>
        /// 扫描的配置
        /// </summary>
        public static int CameraIndex = -1, resolutionIndex = -1;

        #region 字段

        public const string strSearchTips = "搜索公文";
        public const string strAllType = "全部类型";
        public const string strNoType = "(未分类)";
        public const string strNewType = "(新类型)";
        public const string strScanType = "扫描文档";
        public const string strNoLimit = "无限制";
        //public const string strAutoSize = "自适应";
        public static string TempPath = System.IO.Path.GetTempPath() + "DMS";

        public static string strOaURL = "http://oa.gdmc.edu.cn";


        ///// <summary>
        ///// 是否自动同步
        ///// </summary>
        //private static bool isautosync = false;
        ///// <summary>
        ///// 是否自动同步
        ///// </summary>
        //public static bool isAutoSync
        //{
        //    get
        //    {
        //        if (string.IsNullOrWhiteSpace(FTP_IP))
        //            return false;
        //        else
        //            return isautosync;
        //    }
        //    set
        //    {
        //        if (string.IsNullOrWhiteSpace(FTP_IP))
        //            isautosync = false;
        //        else
        //            isautosync = value;
        //    }
        //}
        /// <summary>
        /// 是否默认工作目录
        /// </summary>
        public static bool isDefaultPath = true;

        /// <summary>
        /// 工作目录文件夹名称
        /// </summary>
        public static string strWorkFolder
        {
            get
            {
                if (isDefaultPath)
                    return "DMS";
                else
                    return strWorkPath.Substring(strWorkPath.LastIndexOf('\\') + 1);
            }
        }

        private static string workpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + strWorkFolder;
        /// <summary>
        /// 工作目录路径
        /// </summary>
        public static string strWorkPath
        {
            get
            {
                if (isDefaultPath)
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + strWorkFolder;
                else
                    return workpath;
            }
            set
            {
                try
                {
                    Uri u = new Uri(value);
                    if (u.Scheme.ToLower() == "file")
                    {
                        if (!Directory.Exists(u.LocalPath))
                            Directory.CreateDirectory(u.LocalPath);
                        workpath = u.LocalPath;
                    }
                }
                catch
                {
                }
            }
        }

        public static string strFtpRoot = "DMS";
        /// <summary>
        /// FTP设置
        /// </summary>
        public static string FTP_IP = "", FTP_user = "", FTP_password = "";
        static string stmp = System.Reflection.Assembly.GetExecutingAssembly().Location;

        /// <summary>
        /// 数据库路径
        /// </summary>
        public static string strDatabasePath = stmp.Substring(0, stmp.LastIndexOf('\\')) + "\\DMS.mdb";

        /// <summary>
        /// 每页的条目数量
        /// </summary>
        public static int paginalItems = 20;

        #endregion

        #region 日志
        public static string logPath = stmp.Substring(0, stmp.LastIndexOf('\\')) + "\\run.log";
        public static void writeLog(string[] operate)
        {
            using (StreamWriter sw = new StreamWriter(logPath, true, Encoding.Default))
            {
                foreach (string s in operate)
                    sw.WriteLine(DateTime.Now + " \t" + s);
            }
        }
        #endregion
        #region 配置
        /// <summary>
        /// 保存配置
        /// </summary>
        public static void SaveConfig()
        {
            char dp = '0';
            //char sync = '0';
            if (isDefaultPath) dp = '1';
            //if (isAutoSync) sync = '1';
            XElement xe = new XElement("Config",
                new XElement("defaultPath", dp.ToString()),
                new XElement("WorkPath", strWorkPath),
                new XElement("FTP_IP", FTP_IP),
                new XElement("FTP_user", FTP_user),
                new XElement("FTP_password", FTP_password),
                new XElement("paginalItems", paginalItems),
                new XElement("CameraIndex", CameraIndex),
                new XElement("resolutionIndex", resolutionIndex)
                //new XElement("AutoSync", sync.ToString())
                );
            xe.Save(".\\Config.xml");
            xe.RemoveAll();
        }

        /// <summary>
        /// 读取配置
        /// </summary>
        public static void ReadConfig()
        {
            try
            {
                if (File.Exists(".\\Config.xml"))
                {
                    XElement xe = XElement.Load(".\\Config.xml");

                    if (xe.Element("defaultPath").Value == "0")
                    {
                        isDefaultPath = false;
                    }
                    strWorkPath = xe.Element("WorkPath").Value;

                    //if (xe.Element("AutoSync").Value == "1")
                    //{
                    //    isAutoSync = true;
                    //}
                    //else
                    //{
                    //    isAutoSync = false;
                    //}
                    FTP_IP = xe.Element("FTP_IP").Value;
                    FTP_user = xe.Element("FTP_user").Value;
                    FTP_password = xe.Element("FTP_password").Value;
                    paginalItems = Convert.ToInt32(xe.Element("paginalItems").Value);
                    CameraIndex = Convert.ToInt32(xe.Element("CameraIndex").Value);
                    resolutionIndex = Convert.ToInt32(xe.Element("resolutionIndex").Value);
                    xe.RemoveAll();
                }
                else
                {
                    SaveConfig();
                }
            }
            catch
            {
                SaveConfig();
            }
        }
        #endregion
    }
}
