﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grimoire.Utilities;

namespace Grimoire.GUI
{
    public partial class BitFlag : Form
    {

        #region Constructors

        public BitFlag()
        {
            InitializeComponent();
            lManager = Logs.Manager.Instance;

            localize();
        }

        public BitFlag(int vector)
        {
            InitializeComponent();
            lManager = Logs.Manager.Instance;

            defaultFlag = vector;
            localize();
        }

        #endregion

        #region Properties

        readonly Logs.Manager lManager;
        protected bool calculating = false;
        public List<string> lists = new List<string>();
        public string listPath = null;

        int defaultFlag = 0;

        public string DefaultFlagFile;

        private int flag;
        public int Flag
        {
            get
            {
                if (flagList.SelectedItems.Count == 0 && flagIO.Text == "0")
                    flag = defaultFlag;
                else
                    flag = calculate();

                return flag;
            }
            set { flag = value; }
        }

        XmlManager xMan = XmlManager.Instance;

        #endregion

        #region Private Methods

        void generate_file_list()
        {
            string dir = Grimoire.Utilities.OPT.GetString("flag.directory") ?? string.Format(@"{0}\Flags", Directory.GetCurrentDirectory());

            if (!Directory.Exists(dir))
            {
                string msg = string.Format("flag.directory does not exist!\n\tDirectory: {0}", dir);

                MessageBox.Show(msg, "Flag Utility Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lManager.Enter(Logs.Sender.FLAG, Logs.Level.ERROR, msg);

                return;
            }

            string[] paths = Directory.GetFiles(dir);

            foreach (string filePath in paths)
                lists.Add(Path.GetFileNameWithoutExtension(filePath));

            lManager.Enter(Logs.Sender.FLAG, Logs.Level.DEBUG, "{0} flag files loaded from {1}", paths.Length, dir);
        }

        void generate_flag_list()
        {
            int prevCnt = flagList.SelectedItems.Count;

            flagList.Items.Clear();

            if (Utilities.OPT.GetBool("flag.clear_on_list_change"))
                flagIO.Text = "0";

            List<string> flags = new List<string>();

            if (File.Exists(listPath))
            {
                using (StreamReader sR = new StreamReader(listPath))
                {
                    string lineVal = null;

                    while ((lineVal = sR.ReadLine()) != null)
                        flags.Add(lineVal);
                }

                flagFiles.Text = Path.GetFileNameWithoutExtension(listPath);

                lManager.Enter(Logs.Sender.FLAG, Logs.Level.NOTICE, "{0} Flags loaded to the Flag Editor", flags.Count);

                flagList.Items.AddRange(flags.ToArray());

               if (prevCnt > 0 && prevCnt < flags.Count)
                    reverse();

                flags = null;
            }
            else
            {
                string msg = string.Format("Could not load flag list at:\n{0}", listPath);
                lManager.Enter(Logs.Sender.FLAG, Logs.Level.ERROR, msg);
                MessageBox.Show(msg, "Load Flags Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Events

        private void BitFlag_Load(object sender, EventArgs e)
        {
            generate_file_list();

            if (lists.Count == 0)
            {
                string msg = "generate_file_list() failed to generate the flag list!";

                MessageBox.Show(msg, "Flag Utility Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lManager.Enter(Logs.Sender.FLAG, Logs.Level.ERROR, msg);

                return;
            }

            foreach (string list in lists)
                flagFiles.Items.Add(list);

            string flagsDir = Grimoire.Utilities.OPT.GetString("flag.directory");
            string listName = DefaultFlagFile ?? Grimoire.Utilities.OPT.GetString("flag.default_list");
            if (listName == null)
            {
                string msg = "No default path for flag file defined!";

                MessageBox.Show(msg, "Flag Path Exception", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                lManager.Enter(Logs.Sender.FLAG, Logs.Level.DEBUG, msg);

                return;
            }
            else
            {
                flagFiles.Text = listName;

                lManager.Enter(Logs.Sender.FLAG, Logs.Level.DEBUG, "Default Flag Path: {0} selected.", listName);
            }


            if (Flag > 0)
                flagIO.Text = Flag.ToString();
        }

        private int calculate()
        {
            calculating = true;

            int v = 0;

            for (int index = 0; index < flagList.Items.Count; index++)
                if (flagList.GetSelected(index))
                    v |= (int)Math.Pow(2.0, index);

            calculating = false;

            return v;
        }

        private void reverse()
        {
            calculating = true;

            flagList.ClearSelected();

            for (int index = 0; index < flagList.Items.Count; index++)
                flagList.SetSelected(index, ((flag >> index) & 1) == 1);

            calculating = false;
        }

        private void flagIO_TextChanged(object sender, EventArgs e)
        {
            if (!calculating)
                reverse();
        }

        private void flagFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagFiles.SelectedIndex == -1)
                return;

            listPath = string.Format(@"{0}\{1}.txt", Grimoire.Utilities.OPT.GetString("flag.directory"), flagFiles.Text);

            generate_flag_list();
        }

        private void flagList_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!calculating)
                flagIO.Text = Flag.ToString();
        }

        #endregion

        private void clear_on_list_change_CheckedChanged(object sender, EventArgs e)
        {
            Utilities.OPT.Update("flag.clear_on_list_change", Convert.ToInt32(clear_on_change_chkBx.Checked).ToString());
        }

        void localize()
        {
            xMan.Localize(this, Localization.Enums.SenderType.GUI);
        }
    }
}
