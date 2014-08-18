﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;
using System.Threading;
using DMS.Store;
using DMS.View;
using DMS.Type;
using ZCommon;
using System.Diagnostics;

namespace DMS
{
    public partial class frmMain : Form
    {
        /// <summary>
        /// 本窗口，引用方法：
        /// DMS.frmMain.fm
        /// </summary>
        public static frmMain fm;


        public frmMain()
        {
            InitializeComponent();
            fm = this;
        }


        #region 窗体加载

        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                if (cAccess.load() == false) MessageBox.Show("加载数据库失败");

                cConfig.ReadConfig();

                string[] pi = { "10", "15", "20", "30", "50", cConfig.strNoLimit };
                foreach (string p in pi)
                {
                    comboBoxpaginal.Items.Add(p);
                }
                if (cConfig.paginalItems == 0)
                    comboBoxpaginal.Text = cConfig.strNoLimit;
                else
                    comboBoxpaginal.Text = cConfig.paginalItems.ToString();

                flashTypeList();
                tsslStatus.Text = "启动完成！欢迎使用公文管理系统~";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion


        #region 列出公文
        List<string> listID = new List<string>();
        /// <summary>
        /// 按条件列出公文
        /// </summary>
        /// <param name="_strSelected">用于筛选的字符串</param>
        /// <param name="column">数据库中的列明，默认为"DocType"</param>
        void getIDList(string _strSelected, string column = "DocType")
        {
            listID.Clear();
            for (int row = 0; row < cAccess.DtTable.Rows.Count; row++)
            {
                if (_strSelected == cAccess.DtTable.Rows[row][column].ToString())
                {
                    listID.Add(cAccess.DtTable.Rows[row]["ID"].ToString());
                }
            }
            if (cConfig.paginalItems > 0)
            {
                pagesAll = listID.Count / cConfig.paginalItems;
                if (listID.Count != pagesAll * cConfig.paginalItems) pagesAll++;
            }
            else
                pagesAll = 1;
            labPageAll.Text = pagesAll.ToString();
            list();
        }
        /// <summary>
        /// 列出所有公文
        /// </summary>
        void getIDList()
        {
            listID.Clear();
            for (int row = 0; row < cAccess.DtTable.Rows.Count; row++)
            {
                listID.Add(cAccess.DtTable.Rows[row]["ID"].ToString());
            }
            if (cConfig.paginalItems > 0)
            {
                pagesAll = listID.Count / cConfig.paginalItems;
                if (listID.Count != pagesAll * cConfig.paginalItems) pagesAll++;
            }
            else
                pagesAll = 1;
            labPageAll.Text = pagesAll.ToString();
            list();
        }
        /// <summary>
        /// 按"listID"列出公文
        /// </summary>
        /// <param name="startIndex"></param>
        void list(int startIndex = 0)
        {
            listDoc.Items.Clear();

            if (cConfig.paginalItems > 0)
                for (int i = 0; i < cConfig.paginalItems; i++)
                {
                    if (startIndex >= listID.Count) break;
                    for (int row = 0; row < cAccess.DtTable.Rows.Count; row++)
                    {
                        if (listID[startIndex] == cAccess.DtTable.Rows[row]["ID"].ToString())
                        {
                            ListViewItem lvitem = new ListViewItem();
                            lvitem = listDoc.Items.Add(cAccess.DtTable.Rows[row]["ID"].ToString());
                            lvitem.SubItems.Add(cAccess.DtTable.Rows[row]["DocTitle"].ToString());
                            lvitem.SubItems.Add(cAccess.DtTable.Rows[row]["ReleaseDate"].ToString());
                            lvitem.SubItems.Add(cAccess.DtTable.Rows[row]["Provider"].ToString());
                            lvitem.SubItems.Add(cAccess.DtTable.Rows[row]["AddTime"].ToString());
                            lvitem.SubItems.Add(cAccess.DtTable.Rows[row]["DocType"].ToString());
                            break;
                            //System.Diagnostics.Debug.WriteLine("vv" + cConfig.paginalItems);
                        }
                    }
                    startIndex++;
                }
            else if (cConfig.paginalItems == 0)
            {
                for (int i = 0; i < listID.Count; i++)
                {
                    for (int row = 0; row < cAccess.DtTable.Rows.Count; row++)
                    {
                        if (listID[i] == cAccess.DtTable.Rows[row]["ID"].ToString())
                        {
                            ListViewItem lvitem = new ListViewItem();
                            lvitem = listDoc.Items.Add(cAccess.DtTable.Rows[row]["ID"].ToString());
                            lvitem.SubItems.Add(cAccess.DtTable.Rows[row]["DocTitle"].ToString());
                            lvitem.SubItems.Add(cAccess.DtTable.Rows[row]["ReleaseDate"].ToString());
                            lvitem.SubItems.Add(cAccess.DtTable.Rows[row]["Provider"].ToString());
                            lvitem.SubItems.Add(cAccess.DtTable.Rows[row]["AddTime"].ToString());
                            lvitem.SubItems.Add(cAccess.DtTable.Rows[row]["DocType"].ToString());
                            break;
                        }
                    }
                }
            }
        }


        #endregion

        #region 更新类型列表
        public static List<string> TypeList = new List<string>();
        static int TypeIndex = 0;
        /// <summary>
        /// 刷新类型列表
        /// </summary>
        private void flashTypeList()
        {
            TypeList.Clear();
            if (!Directory.Exists(cConfig.strWorkPath))
                Directory.CreateDirectory(cConfig.strWorkPath);
            searchDirectory(cConfig.strWorkPath);

            for (int row = 0; row < cAccess.DtTable.Rows.Count; row++)
            {
                string str = cAccess.DtTable.Rows[row]["DocType"].ToString();
                if (string.IsNullOrEmpty(str)) str = cConfig.strNoType;
                bool different = true;
                for (int i = 0; i < TypeList.Count; i++)
                {
                    if (TypeList[i] == str)
                    {
                        different = false;
                        break;
                    }
                }
                if (different)
                    TypeList.Add(str);
            }

            listDocType.Items.Clear(); tscbMove.Items.Clear();
            listDocType.Items.Add(cConfig.strAllType);
            foreach (string str in TypeList)
            {
                tscbMove.Items.Add(str);
                listDocType.Items.Add(str);
            }
            tscbMove.Items.Add(cConfig.strNewType);

            if (listDocType.Items.Count == 1)
                listDocType.SelectedIndex = 0;
            else if (TypeIndex < listDocType.Items.Count)
                listDocType.SelectedIndex = TypeIndex;
        }


        /// <summary>
        /// 遍历文件夹，搜索符合条件的文件
        /// </summary>
        /// <param name="path"></param>
        private void searchDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            //foreach (FileInfo f in dir.GetFiles("*.sql"))
            //{
            //    //addlist(f.FullName);
            //}
            foreach (DirectoryInfo f in dir.GetDirectories())
            {
                if (f.Name != cConfig.strTemp)
                {
                    TypeList.Add(f.Name);
                    searchDirectory(f.FullName);
                }
            }
        }

        #endregion


        #region 工具栏窗体事件

        private void tssbAdd_ButtonClick(object sender, EventArgs e)
        {
            using (frmAddDoc frmAd = new frmAddDoc())
            {
                cConfig.working = true;
                timer1.Enabled = true;
                frmAd.ShowDialog();
            }
        }

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            using (frmAddDoc frmAd = new frmAddDoc())
            {
                cConfig.working = true;
                timer1.Enabled = true;
                frmAd.ShowDialog();
            }
        }

        private void tsmiAddMore_Click(object sender, EventArgs e)
        {
            using (crawler.frmCrawler cr = new crawler.frmCrawler())
            {
                cConfig.working = true;
                timer1.Enabled = true;
                cr.ShowDialog();
            }
        }

        private void tsbScan_Click(object sender, EventArgs e)
        {
            using (Camera.frmCamera frmScan = new Camera.frmCamera())
            {
                cConfig.working = true;
                timer1.Enabled = true;
                frmScan.ShowDialog();
            }
        }

        private void tsbSetting_Click(object sender, EventArgs e)
        {
            using (frmSettings frmS = new frmSettings())
            {
                frmS.ShowDialog();
            }
        }

        private void tsbAbout_Click(object sender, EventArgs e)
        {
            using (frmAboutBox frmAb = new frmAboutBox())
            {
                frmAb.ShowDialog();
            }
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region 工具栏其他事件

        private void tsbDelete_Click(object sender, EventArgs e)
        {
            if (listDoc.CheckedItems.Count > 0)
            {
                if (MessageBox.Show("确定要删除 " + listDoc.CheckedItems.Count + " 个文件?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    for (int i = 0; i < listDoc.CheckedItems.Count; i++)
                    {
                        cAccess.delect(listDoc.CheckedItems[i].SubItems[0].Text);
                    }
                    getIDList();
                }
            }
        }

        #region 搜索框

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == cConfig.strSearchTips) txtSearch.Text = "";

        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (txtSearch.Text == "" || txtSearch.Text == cConfig.strSearchTips)
            {
                txtSearch.Text = cConfig.strSearchTips;

            }
            else
            {//执行搜索命令
                listID = cAccess.search(txtSearch.Text);
                list();
            }
        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                //执行搜索命令
                listID = cAccess.search(txtSearch.Text);
                list();
            }
        }

        #endregion

        private void tscbMove_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tscbMove.SelectedIndex >= 0)
            {

                if (tscbMove.SelectedItem.ToString() == cConfig.strNewType)
                {
                    using (frmNewType frmNewtype = new frmNewType())
                    {
                        frmNewtype.labNewType.Text = "添加类型";
                        frmNewtype.Text = "添加类型";
                        frmNewtype.ShowDialog();
                    }
                    return;
                }
                for (int i = 0; i < listDoc.CheckedItems.Count; i++)
                {
                    string OldPath;
                    for (int row = 0; row < cAccess.DtTable.Rows.Count; row++)
                    {
                        if (listDoc.CheckedItems[i].Text == cAccess.DtTable.Rows[row]["ID"].ToString())
                        {
                            OldPath = cAccess.DtTable.Rows[row]["LocalPath"].ToString();

                            cAccess.ModifyTheType(listDoc.CheckedItems[i].SubItems[0].Text, tscbMove.Text);

                            File.Move(OldPath, cAccess.DtTable.Rows[row]["LocalPath"].ToString());//移动相应的word文档到所需类型的目录
                            string a = cAccess.DtTable.Rows[row]["LocalPath"].ToString();
                        }
                    }
                }
                tscbMove.Text = "";
                flashTypeList();

            }
        }

        public void AddNewType(string strNewType)
        {
            listDocType.Items.Add(strNewType);
            if (!Directory.Exists(cConfig.strWorkPath + "\\" + strNewType))//添加所添加类型的目录
            {
                Directory.CreateDirectory(cConfig.strWorkPath + "\\" + strNewType);
            }

            tscbMove.Items.Clear();
            foreach (string str in listDocType.Items)
            {
                if (str != cConfig.strAllType)
                {
                    tscbMove.Items.Add(str);
                }
            }
            tscbMove.Items.Add(cConfig.strNewType);

            for (int i = 0; i < listDoc.CheckedItems.Count; i++)
            {
                string OldPath;
                for (int row = 0; row < cAccess.DtTable.Rows.Count; row++)
                {
                    if (listDoc.CheckedItems[i].Text == cAccess.DtTable.Rows[row]["ID"].ToString())
                    {
                        OldPath = cAccess.DtTable.Rows[row]["LocalPath"].ToString();

                        cAccess.ModifyTheType(listDoc.CheckedItems[i].Text, strNewType);

                        File.Move(OldPath, cAccess.DtTable.Rows[row]["LocalPath"].ToString());//移动相应的word文档到所需类型的目录
                        string a = cAccess.DtTable.Rows[row]["LocalPath"].ToString();
                    }
                }
            }
        }

        private void tsbPrint_Click(object sender, EventArgs e)
        {
            if (listDoc.CheckedItems.Count > 0)
            {
                List<string> ls = new List<string>();
                for (int i = 0; i < listDoc.CheckedItems.Count; i++)
                {
                    //if (listDoc.CheckedItems[i].Checked == true)
                    for (int row = 0; row < cAccess.DtTable.Rows.Count; row++)
                    {
                        if (listDoc.CheckedItems[i].SubItems[0].Text == cAccess.DtTable.Rows[row]["ID"].ToString())
                        {
                            ls.Add(cAccess.DtTable.Rows[row]["LocalPath"].ToString());
                        }
                    }
                }
                cPrintFiles cpf = new cPrintFiles(ls);
                Thread th = new Thread(new ThreadStart(cpf.printFiles));
            }
        }

        #endregion


        #region 读取或储存类型列表（暂不使用）

        //public static List<string> DocTypeList;

        ///// <summary>
        ///// 储存公文类型列表
        ///// </summary>
        //private void saveDocTypeList()
        //{
        //    if (DocTypeList == null) return;
        //    XElement xe = new XElement("DocTypeList");
        //    for (int i = 0; i < DocTypeList.Count; i++)
        //    {
        //        XElement NoteList = new XElement("DocType", DocTypeList[i]);
        //        xe.Add(NoteList);
        //    }
        //    xe.Save(".\\DocTypeList.xml");
        //    xe.RemoveAll();
        //}

        ///// <summary>
        ///// 读取公文类型列表
        ///// </summary>
        //private void readDocTypeList()
        //{
        //    if (DocTypeList == null)
        //        DocTypeList = new List<string>();

        //    if (File.Exists(".\\DocTypeList.xml"))
        //    {
        //        XElement xe = XElement.Load(".\\DocTypeList.xml");
        //        IEnumerable<XElement> elements = from PInfo in xe.Elements("DocTypeList") select PInfo;
        //        foreach (XElement element in elements)
        //        {
        //            DocTypeList.Add(element.Element("DocType").Value);
        //        }
        //        xe.RemoveAll();
        //    }

        //    for (int row = 0; row < cAccess.DtTable.Rows.Count; row++)
        //    {
        //        string str = cAccess.DtTable.Rows[row]["DocType"].ToString();

        //        //if (DocTypeList.Count == 0)
        //        //{
        //        //    DocTypeList.Add(str);
        //        //    continue;
        //        //}

        //        bool different = true;
        //        for (int i = 0; i < DocTypeList.Count; i++)
        //        {
        //            if (DocTypeList[i] == str)
        //            {
        //                different = false;
        //                break;
        //            }
        //        }
        //        if (different)
        //            DocTypeList.Add(str);
        //    }

        //    listDocType.Items.Add("全部类型");
        //    foreach (string str in DocTypeList)
        //    {
        //        tscbMove.Items.Add(str);
        //        listDocType.Items.Add(str);
        //    }
        //    tscbMove.Items.Add("(新类型)");
        //    listDocType.SelectedIndex = 0;
        //}

        #endregion

        #region 类型列表事件 listDocType

        private void listDocType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listDocType.SelectedIndex >= 0)
            {
                TypeIndex = listDocType.SelectedIndex;
                listDoc.Items.Clear();
                string strSelected = listDocType.SelectedItem.ToString();
                //if (strSelected == cConfig.strNoType) strSelected = "";
                if (strSelected == cConfig.strAllType)
                {
                    getIDList();
                }
                else
                {
                    getIDList(strSelected);
                }
            }
        }

        #endregion

        #region 类型列表右键菜单

        private void tsmiAddType_Click(object sender, EventArgs e)
        {
            using (frmNewType frmNewtype = new frmNewType())
            {
                frmNewtype.labNewType.Text = "新增类型";
                frmNewtype.Text = "新增类型";
                frmNewtype.ShowDialog();
            }
        }

        /// <summary>
        /// 新增类型
        /// </summary>
        /// <param name="strNewType"></param>
        public void addType(string strNewType)
        {
            listDocType.Items.Add(strNewType);
            if (!Directory.Exists(cConfig.strWorkPath + "\\" + strNewType))//添加所添加类型的目录
            {
                Directory.CreateDirectory(cConfig.strWorkPath + "\\" + strNewType);
            }
            flashTypeList();
        }


        private void tsmiChangeType_Click(object sender, EventArgs e)
        {
            if (listDocType.SelectedIndex > 0)
            {
                using (frmNewType frmNewtype = new frmNewType())
                {
                    frmNewtype.labNewType.Text = "修改类型";
                    frmNewtype.Text = "修改类型";
                    frmNewtype.ShowDialog();
                }
            }
        }

        /// <summary>
        /// 更改类型
        /// </summary>
        /// <param name="strNewType"></param>
        public void changeType(string strNewType)
        {
            foreach (ListViewItem lvitem in listDoc.Items)//更改数据库的类型名称
            {
                for (int row = 0; row < cAccess.DtTable.Rows.Count; row++)
                {
                    if (lvitem.SubItems[5].Text == cAccess.DtTable.Rows[row]["DocType"].ToString())
                    {
                        cAccess.DtTable.Rows[row]["DocType"] = strNewType;
                        cAccess.DtAdapter.Update(cAccess.DtTable);
                    }
                }
            }

            int TypeIndex = listDocType.SelectedIndex;
            if (Directory.Exists(cConfig.strWorkPath + "\\" + listDocType.Items[TypeIndex]))//更改相应类型目录名称
            {
                Directory.Move(cConfig.strWorkPath + "\\" + listDocType.Items[TypeIndex], cConfig.strWorkPath + "\\" + strNewType);
            }
            listDocType.Items[TypeIndex] = strNewType;

            flashTypeList();
        }

        private void tsmiDeleteType_Click(object sender, EventArgs e)
        {
            if (listDocType.SelectedIndex > 0)
            {
                if (listDocType.SelectedItem.ToString() == cConfig.strNoType || listDocType.SelectedItem.ToString() == cConfig.strAllType)
                    MessageBox.Show("禁止删除" + listDocType.SelectedItem);
                else if (MessageBox.Show("确定删除该类型？", "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    if (Directory.Exists(cConfig.strWorkPath + "\\" + listDocType.SelectedItem.ToString()))
                    {
                        DirectoryInfo di = new DirectoryInfo(cConfig.strWorkPath + "\\" + listDocType.SelectedItem.ToString());
                        foreach (FileInfo fi in di.GetFiles("*.*"))//将数据库中的类型变为空白
                        {
                            for (int row = 0; row < cAccess.DtTable.Rows.Count; row++)
                            {
                                if (fi.ToString() == cAccess.DtTable.Rows[row]["DocTitle"].ToString() + ".doc")
                                {
                                    cAccess.DtTable.Rows[row]["DocType"] = cConfig.strNoType;
                                    cAccess.DtAdapter.Update(cAccess.DtTable);
                                }
                            }

                            string path1, path2;//移动删除类型的目录下的所有word文档
                            path1 = cConfig.strWorkPath + "\\" + listDocType.SelectedItem.ToString() + "\\" + fi.ToString();
                            path2 = cConfig.strWorkPath + "\\" + cConfig.strNoType + "\\" + fi.ToString();
                            if (File.Exists(path2)) { MessageBox.Show("无法删除该类型，因为未分类中存在与该类型相同名字的文件"); return; }
                            File.Move(path1, path2);
                            //File.Copy(path1, path2);
                            //File.Delete(cConfig.strWorkPath + "\\" + listDocType.SelectedItem.ToString() + "\\" + fi.ToString());
                        }
                        Directory.Delete(cConfig.strWorkPath + "\\" + listDocType.SelectedItem.ToString());
                    }
                    flashTypeList();
                }
            }
        }

        #endregion


        #region 公文列表事件 listDoc

        private void listDoc_SelectedIndexChanged(object sender, EventArgs e)
        {
            string notes = "";
            if (listDoc.SelectedItems.Count > 0)
            {
                for (int row = 0; row < cAccess.DtTable.Rows.Count; row++)
                {
                    if (listDoc.SelectedItems[0].SubItems[0].Text == cAccess.DtTable.Rows[row]["ID"].ToString())
                    {
                        notes = cAccess.DtTable.Rows[row]["Notes"].ToString();
                    }
                }
            }

            this.tipListDoc.SetToolTip(listDoc, notes);
        }

        private void listDoc_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (listDoc.CheckedItems.Count > 0)
            {
                tsbDelete.Enabled = true;
                tsbPrint.Enabled = true;
                tslMove.Enabled = true;
                tscbMove.Enabled = true;
            }
            else
            {
                tsbDelete.Enabled = false;
                tslMove.Enabled = false;
                tscbMove.Enabled = false;
                tsbPrint.Enabled = false;
            }
        }

        #endregion

        #region 公文列表右键菜单

        /// <summary>
        /// 查看公文
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmiLook_Click(object sender, EventArgs e)
        {
            if (listDoc.SelectedItems.Count > 0)
                for (int row = 0; row < cAccess.DtTable.Rows.Count; row++)
                {
                    if (listDoc.SelectedItems[0].SubItems[0].Text == cAccess.DtTable.Rows[row]["ID"].ToString())
                    {
                        System.Diagnostics.Process.Start(cAccess.DtTable.Rows[row]["LocalPath"].ToString());
                    }
                }
        }

        /// <summary>
        /// 查看公文位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmiLocation_Click(object sender, EventArgs e)
        {
            if (listDoc.SelectedItems.Count == 1)
            {
                for (int row = 0; row < cAccess.DtTable.Rows.Count; row++)
                {
                    if (listDoc.SelectedItems[0].SubItems[0].Text == cAccess.DtTable.Rows[row]["ID"].ToString())
                    {
                        int localint = cAccess.DtTable.Rows[row]["LocalPath"].ToString().LastIndexOf(@"\");
                        string loacaldirect = cAccess.DtTable.Rows[row]["LocalPath"].ToString().Substring(0, localint);
                        Process.Start(loacaldirect);
                    }
                }
            }
        }

        /// <summary>
        /// 打开公文的网站
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmiFindSource_Click(object sender, EventArgs e)
        {
            if (listDoc.SelectedItems.Count == 1)
            {
                for (int row = 0; row < cAccess.DtTable.Rows.Count; row++)
                {
                    if (listDoc.SelectedItems[0].SubItems[0].Text == cAccess.DtTable.Rows[row]["ID"].ToString())
                    {
                        Process.Start(cAccess.DtTable.Rows[row]["Source"].ToString());
                    }
                }
            }
        }
        /// <summary>
        /// 删除并刷新列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmiDelete_Click(object sender, EventArgs e)
        {
            if (listDoc.SelectedItems.Count > 0)
            {
                for (int i = 0; i < listDoc.SelectedItems.Count; i++)
                {
                    for (int row = 0; row < cAccess.DtTable.Rows.Count; row++)
                    {
                        if (listDoc.SelectedItems[i].Text == cAccess.DtTable.Rows[row]["ID"].ToString())
                        {
                            cAccess.delect(listDoc.SelectedItems[i].Text);
                            //System.Diagnostics.Debug.WriteLine(listDoc.CheckedItems[i].Text);
                        }
                    }
                }
                getIDList();
            }
        }
        /// <summary>
        /// 查看配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmiInfo_Click(object sender, EventArgs e)
        {
            using (frmProperty property = new frmProperty(listDoc.SelectedItems[0].SubItems[0].Text))
            {
                property.ShowDialog();
            }
        }
        #endregion


        #region 批量选择

        private void btnAll_Click(object sender, EventArgs e)
        {
            if (listDoc.Items.Count != listDoc.CheckedItems.Count)
            {
                foreach (ListViewItem tempItem in listDoc.Items)//循环遍历listView控件中的每一项
                {
                    tempItem.Checked = true;
                }
            }
            else
            {
                foreach (ListViewItem tempItem in listDoc.Items)//循环遍历listView控件中的每一项
                {
                    tempItem.Checked = false;
                }
            }
        }

        private void btnContrary_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem tempItem in listDoc.Items)//循环遍历listView控件中的每一项
            {
                if (tempItem.Checked == true)//如果当前项处于选中状态
                    tempItem.Checked = false;
                else
                    tempItem.Checked = true;
            }
        }
        #endregion


        #region 分页
        int pagesAll = 1, pageNow = 1;
        private void btnPageUp_Click(object sender, EventArgs e)
        {
            int temp = pageNow;
            if (pageNow-- <= 1)
            {
                pageNow = temp;
                MessageBox.Show("已经到首页");
                return;
            }
            textBoxNow.Text = pageNow.ToString();
            list((pageNow - 1) * cConfig.paginalItems);
        }

        private void btnPageDown_Click(object sender, EventArgs e)
        {
            int temp = pageNow;
            if (pageNow++ >= pagesAll)
            {
                pageNow = temp;
                MessageBox.Show("已经到末页");
                return;
            }
            textBoxNow.Text = pageNow.ToString();
            list((pageNow - 1) * cConfig.paginalItems);
        }

        #endregion

        #region 页码
        private void textBoxNow_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                int temp = pageNow;
                try
                {//执行“跳转”

                    pageNow = Convert.ToInt32(textBoxNow.Text);
                    if (pageNow < 1 || pageNow > pagesAll)
                    {
                        pageNow = temp;
                        return;
                    }
                    else
                    {
                        list((pageNow - 1) * cConfig.paginalItems);
                    }
                }
                catch (Exception ex) { pageNow = temp; MessageBox.Show(ex.Message); }
            }
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }
        private void textBoxNow_Leave(object sender, EventArgs e)
        {
            int temp = pageNow;
            //执行“跳转”
            try
            {
                pageNow = Convert.ToInt32(textBoxNow.Text);
                if (pageNow < 1 || pageNow > pagesAll)
                {
                    pageNow = temp;
                    MessageBox.Show("输入的页码超出范围！");
                    return;
                }
                else
                {
                    list((pageNow - 1) * cConfig.paginalItems);
                }
            }
            catch (Exception ex) { pageNow = temp; MessageBox.Show(ex.Message); }
        }
        #endregion


        #region 每页显示数目

        private void comboBoxpaginal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxpaginal.Text == cConfig.strNoLimit)
            {
                cConfig.paginalItems = 0;
                btnPageDown.Enabled = false;
                btnPageUp.Enabled = false;
                textBoxNow.Enabled = false;
            }
            else
            {
                cConfig.paginalItems = Convert.ToInt32(comboBoxpaginal.Text);
                btnPageDown.Enabled = true;
                btnPageUp.Enabled = true;
                textBoxNow.Enabled = true;
            }
            flashTypeList();
            cConfig.SaveConfig();
        }

        private void comboBoxpaginal_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
                comboBoxpaginal_SelectedIndexChanged(sender, e);
            if (e.KeyChar != '\b')//这是允许输入退格键
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字
                {
                    e.Handled = true;
                }
            }
        }

        private void comboBoxpaginal_Leave(object sender, EventArgs e)
        {
            comboBoxpaginal_SelectedIndexChanged(sender, e);
        }

        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (cConfig.working)
            {
                tsslStatus.Text = "正在工作中...";
            }
            else
            {
                if (cConfig.needFlash) { flashTypeList(); tsslStatus.Text = "OK!"; timer1.Enabled = false; }
            }
        }

    }
}
