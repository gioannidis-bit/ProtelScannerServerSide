using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using Newtonsoft.Json;
using ProtelScannerServerSide.Enums;
using ProtelScannerServerSide.Helpers;
using ProtelScannerServerSide.Models;

namespace ProtelServerSide;

public class fConfig : Form
{
	private const string connectionPlaceHolder = "server=<SERVER>;user id=<USER>;password=<PASSWORD>;database=<DATABASE>;";

	private ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	private string sPath;

	private ConfigModel configModel;

	private BindingSource bs;

	private BindingSource bsGender;

	private IContainer components;

	private GroupBox grbTop;

	private ComboBox ctlDefaultCompanion;

	private Label lblDefaultCompanion;

	private Button btnProtelIni;

	private TextBox ctlProtelIni;

	private Label lblProtelIni;

	private GroupBox grbBottom;

	private Button btnClose;

	private Button btnSave;

	private GroupBox grbGrand;

	private GroupBox grbGenderMap;

	private DataGridView grdGenderMap;

	private GroupBox grbClient;

	private DataGridView grdMain;

	private TextBox ctlConnection;

	private Label lbConnection;

	private TextBox ctlDbSchema;

	private Label lblDbSchema;

	public fConfig()
	{
		InitializeComponent();
		ctlConnection.Text = "server=<SERVER>;user id=<USER>;password=<PASSWORD>;database=<DATABASE>;";
		ctlConnection.GotFocus += RemoveText;
		ctlConnection.LostFocus += AddText;
	}

	public void RemoveText(object sender, EventArgs e)
	{
		if (ctlConnection.Text == "server=<SERVER>;user id=<USER>;password=<PASSWORD>;database=<DATABASE>;")
		{
			ctlConnection.Text = "";
		}
	}

	public void AddText(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(ctlConnection.Text))
		{
			ctlConnection.Text = "server=<SERVER>;user id=<USER>;password=<PASSWORD>;database=<DATABASE>;";
		}
	}

	private void btnProtelIni_Click(object sender, EventArgs e)
	{
		OpenFileDialog dl = new OpenFileDialog();
		dl.Title = "Select protel ini";
		dl.Filter = "Ini Files(*.ini)|*.ini";
		if (dl.ShowDialog() == DialogResult.OK)
		{
			ctlProtelIni.Text = dl.FileName;
		}
	}

	private void fConfig_Load(object sender, EventArgs e)
	{
		sPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
		if (sPath[sPath.Length - 1] != '\\')
		{
			sPath += "\\";
		}
		ctlDefaultCompanion.Items.Clear();
		foreach (object item in Enum.GetValues(typeof(CompanionTypeEnum)))
		{
			ctlDefaultCompanion.Items.Add(item);
		}
		if (File.Exists(sPath + "\\Config\\configuration.json"))
		{
			string sData = File.ReadAllText(sPath + "\\Config\\configuration.json");
			sData = EncryptionHelper.Decrypt(sData);
			configModel = JsonConvert.DeserializeObject<ConfigModel>(sData);
			if (configModel.GenderMapping == null)
			{
				configModel.GenderMapping = new List<GenderMappingModel>();
				configModel.GenderMapping.Add(new GenderMappingModel
				{
					scannerGender = "Male",
					protelGender = "Male"
				});
				configModel.GenderMapping.Add(new GenderMappingModel
				{
					scannerGender = "Female",
					protelGender = "Female"
				});
				configModel.GenderMapping.Add(new GenderMappingModel
				{
					scannerGender = "Unspecified",
					protelGender = "Unspecified"
				});
			}
		}
		else
		{
			configModel = new ConfigModel();
			configModel.UsersPorts = new List<UsersServerPortsAssoc>();
			configModel.CompanionType = CompanionTypeEnum.None;
			configModel.GenderMapping = new List<GenderMappingModel>();
			configModel.GenderMapping.Add(new GenderMappingModel
			{
				scannerGender = "Male",
				protelGender = "Male"
			});
			configModel.GenderMapping.Add(new GenderMappingModel
			{
				scannerGender = "Female",
				protelGender = "Female"
			});
			configModel.GenderMapping.Add(new GenderMappingModel
			{
				scannerGender = "Unspecified",
				protelGender = "Unspecified"
			});
		}
		ctlDefaultCompanion.SelectedItem = configModel.CompanionType;
		ctlProtelIni.Text = configModel.ProtelIni;
		ctlConnection.Text = (string.IsNullOrWhiteSpace(configModel.ConnectionString) ? "server=<SERVER>;user id=<USER>;password=<PASSWORD>;database=<DATABASE>;" : configModel.ConnectionString);
		ctlDbSchema.Text = (string.IsNullOrWhiteSpace(configModel.DbSchema) ? "proteluser" : configModel.DbSchema);
		MakeGrid();
	}

	private void MakeGrid()
	{
		grdMain.DataSource = null;
		grdMain.AutoGenerateColumns = true;
		grdMain.AllowUserToAddRows = true;
		grdMain.AllowUserToDeleteRows = true;
		bs = new BindingSource();
		bs.DataSource = configModel.UsersPorts;
		grdMain.DataSource = bs;
		grdMain.Columns[0].HeaderText = "User (Station Id)";
		grdMain.Columns[0].Width = 120;
		grdMain.Columns[1].HeaderText = "Server Port";
		grdMain.Columns[1].Width = 100;
		grdMain.Columns[1].DefaultCellStyle.Format = "N0";
		grdGenderMap.DataSource = null;
		grdGenderMap.AutoGenerateColumns = true;
		grdGenderMap.AllowUserToAddRows = false;
		grdGenderMap.AllowUserToDeleteRows = false;
		bsGender = new BindingSource();
		bsGender.DataSource = configModel.GenderMapping;
		grdGenderMap.DataSource = bsGender;
		grdGenderMap.Columns[0].HeaderText = "Scanner";
		grdGenderMap.Columns[0].Width = 60;
		grdGenderMap.Columns[0].ReadOnly = true;
		grdGenderMap.Columns[1].HeaderText = "Protel";
		grdGenderMap.Columns[1].Width = 60;
	}

	private void btnClose_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void btnSave_Click(object sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(ctlProtelIni.Text) && string.IsNullOrWhiteSpace(ctlConnection.Text))
		{
			MessageBox.Show("You have not select protel ini file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		if (configModel.UsersPorts.Count < 1)
		{
			MessageBox.Show("You have not select users (station ids) and server ports", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		if ((from w in (from g in configModel.UsersPorts
				group g by g.portNumber into s
				select new
				{
					counts = s.Count()
				}).ToList()
			where w.counts > 1
			select w).Count() > 0)
		{
			MessageBox.Show("You have set same ports on different users (station ids)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		if ((from w in (from g in configModel.UsersPorts
				group g by g.stationId into s
				select new
				{
					counts = s.Count()
				}).ToList()
			where w.counts > 1
			select w).Count() > 0)
		{
			MessageBox.Show("You have select more than one a user (station id)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		configModel.ProtelIni = ctlProtelIni.Text;
		configModel.ConnectionString = ((!ctlConnection.Text.Equals("server=<SERVER>;user id=<USER>;password=<PASSWORD>;database=<DATABASE>;")) ? ctlConnection.Text : "");
		configModel.DbSchema = ((!string.IsNullOrWhiteSpace(ctlDbSchema.Text)) ? ctlDbSchema.Text : "proteluser");
		configModel.CompanionType = (CompanionTypeEnum)ctlDefaultCompanion.SelectedItem;
		string sData = EncryptionHelper.Encrypt(JsonConvert.SerializeObject(configModel));
		if (!Directory.Exists(sPath + "\\Config"))
		{
			Directory.CreateDirectory(sPath + "\\Config");
		}
		File.WriteAllText(sPath + "\\Config\\configuration.json", sData);
		MessageBox.Show("Operation completed", "Info", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProtelServerSide.fConfig));
		this.grbTop = new System.Windows.Forms.GroupBox();
		this.ctlConnection = new System.Windows.Forms.TextBox();
		this.lbConnection = new System.Windows.Forms.Label();
		this.ctlDefaultCompanion = new System.Windows.Forms.ComboBox();
		this.lblDefaultCompanion = new System.Windows.Forms.Label();
		this.btnProtelIni = new System.Windows.Forms.Button();
		this.ctlProtelIni = new System.Windows.Forms.TextBox();
		this.lblProtelIni = new System.Windows.Forms.Label();
		this.grbBottom = new System.Windows.Forms.GroupBox();
		this.btnClose = new System.Windows.Forms.Button();
		this.btnSave = new System.Windows.Forms.Button();
		this.grbGrand = new System.Windows.Forms.GroupBox();
		this.grbGenderMap = new System.Windows.Forms.GroupBox();
		this.grdGenderMap = new System.Windows.Forms.DataGridView();
		this.grbClient = new System.Windows.Forms.GroupBox();
		this.grdMain = new System.Windows.Forms.DataGridView();
		this.lblDbSchema = new System.Windows.Forms.Label();
		this.ctlDbSchema = new System.Windows.Forms.TextBox();
		this.grbTop.SuspendLayout();
		this.grbBottom.SuspendLayout();
		this.grbGrand.SuspendLayout();
		this.grbGenderMap.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.grdGenderMap).BeginInit();
		this.grbClient.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.grdMain).BeginInit();
		base.SuspendLayout();
		this.grbTop.Controls.Add(this.ctlDbSchema);
		this.grbTop.Controls.Add(this.lblDbSchema);
		this.grbTop.Controls.Add(this.ctlConnection);
		this.grbTop.Controls.Add(this.lbConnection);
		this.grbTop.Controls.Add(this.ctlDefaultCompanion);
		this.grbTop.Controls.Add(this.lblDefaultCompanion);
		this.grbTop.Controls.Add(this.btnProtelIni);
		this.grbTop.Controls.Add(this.ctlProtelIni);
		this.grbTop.Controls.Add(this.lblProtelIni);
		this.grbTop.Dock = System.Windows.Forms.DockStyle.Top;
		this.grbTop.Location = new System.Drawing.Point(0, 0);
		this.grbTop.Name = "grbTop";
		this.grbTop.Size = new System.Drawing.Size(614, 101);
		this.grbTop.TabIndex = 5;
		this.grbTop.TabStop = false;
		this.ctlConnection.Location = new System.Drawing.Point(77, 45);
		this.ctlConnection.Name = "ctlConnection";
		this.ctlConnection.Size = new System.Drawing.Size(495, 20);
		this.ctlConnection.TabIndex = 2;
		this.lbConnection.AutoSize = true;
		this.lbConnection.Location = new System.Drawing.Point(12, 49);
		this.lbConnection.Name = "lbConnection";
		this.lbConnection.Size = new System.Drawing.Size(61, 13);
		this.lbConnection.TabIndex = 10;
		this.lbConnection.Text = "Connection";
		this.ctlDefaultCompanion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.ctlDefaultCompanion.FormattingEnabled = true;
		this.ctlDefaultCompanion.Location = new System.Drawing.Point(398, 71);
		this.ctlDefaultCompanion.Name = "ctlDefaultCompanion";
		this.ctlDefaultCompanion.Size = new System.Drawing.Size(174, 21);
		this.ctlDefaultCompanion.TabIndex = 4;
		this.lblDefaultCompanion.AutoSize = true;
		this.lblDefaultCompanion.Location = new System.Drawing.Point(282, 75);
		this.lblDefaultCompanion.Name = "lblDefaultCompanion";
		this.lblDefaultCompanion.Size = new System.Drawing.Size(110, 13);
		this.lblDefaultCompanion.TabIndex = 8;
		this.lblDefaultCompanion.Text = "Default companion as";
		this.btnProtelIni.Location = new System.Drawing.Point(578, 18);
		this.btnProtelIni.Name = "btnProtelIni";
		this.btnProtelIni.Size = new System.Drawing.Size(30, 23);
		this.btnProtelIni.TabIndex = 1;
		this.btnProtelIni.Text = "...";
		this.btnProtelIni.UseVisualStyleBackColor = true;
		this.btnProtelIni.Click += new System.EventHandler(btnProtelIni_Click);
		this.ctlProtelIni.Location = new System.Drawing.Point(77, 19);
		this.ctlProtelIni.Name = "ctlProtelIni";
		this.ctlProtelIni.Size = new System.Drawing.Size(495, 20);
		this.ctlProtelIni.TabIndex = 0;
		this.lblProtelIni.AutoSize = true;
		this.lblProtelIni.Location = new System.Drawing.Point(12, 23);
		this.lblProtelIni.Name = "lblProtelIni";
		this.lblProtelIni.Size = new System.Drawing.Size(47, 13);
		this.lblProtelIni.TabIndex = 5;
		this.lblProtelIni.Text = "Protel ini";
		this.grbBottom.Controls.Add(this.btnClose);
		this.grbBottom.Controls.Add(this.btnSave);
		this.grbBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.grbBottom.Location = new System.Drawing.Point(0, 344);
		this.grbBottom.Name = "grbBottom";
		this.grbBottom.Size = new System.Drawing.Size(614, 56);
		this.grbBottom.TabIndex = 7;
		this.grbBottom.TabStop = false;
		this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
		this.btnClose.Location = new System.Drawing.Point(535, 16);
		this.btnClose.Name = "btnClose";
		this.btnClose.Size = new System.Drawing.Size(76, 37);
		this.btnClose.TabIndex = 1;
		this.btnClose.Text = "Close";
		this.btnClose.UseVisualStyleBackColor = true;
		this.btnClose.Click += new System.EventHandler(btnClose_Click);
		this.btnSave.Dock = System.Windows.Forms.DockStyle.Left;
		this.btnSave.Location = new System.Drawing.Point(3, 16);
		this.btnSave.Name = "btnSave";
		this.btnSave.Size = new System.Drawing.Size(75, 37);
		this.btnSave.TabIndex = 0;
		this.btnSave.Text = "Save";
		this.btnSave.UseVisualStyleBackColor = true;
		this.btnSave.Click += new System.EventHandler(btnSave_Click);
		this.grbGrand.Controls.Add(this.grbGenderMap);
		this.grbGrand.Controls.Add(this.grbClient);
		this.grbGrand.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grbGrand.Location = new System.Drawing.Point(0, 101);
		this.grbGrand.Name = "grbGrand";
		this.grbGrand.Size = new System.Drawing.Size(614, 243);
		this.grbGrand.TabIndex = 8;
		this.grbGrand.TabStop = false;
		this.grbGenderMap.Controls.Add(this.grdGenderMap);
		this.grbGenderMap.Dock = System.Windows.Forms.DockStyle.Right;
		this.grbGenderMap.Location = new System.Drawing.Point(371, 16);
		this.grbGenderMap.Name = "grbGenderMap";
		this.grbGenderMap.Size = new System.Drawing.Size(240, 224);
		this.grbGenderMap.TabIndex = 8;
		this.grbGenderMap.TabStop = false;
		this.grbGenderMap.Text = "Gender Mapping";
		this.grdGenderMap.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.grdGenderMap.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grdGenderMap.Location = new System.Drawing.Point(3, 16);
		this.grdGenderMap.Name = "grdGenderMap";
		this.grdGenderMap.Size = new System.Drawing.Size(234, 205);
		this.grdGenderMap.TabIndex = 1;
		this.grbClient.Controls.Add(this.grdMain);
		this.grbClient.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grbClient.Location = new System.Drawing.Point(3, 16);
		this.grbClient.Name = "grbClient";
		this.grbClient.Size = new System.Drawing.Size(608, 224);
		this.grbClient.TabIndex = 7;
		this.grbClient.TabStop = false;
		this.grbClient.Text = "User (Station Ids) and Server Ports Assoc";
		this.grdMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.grdMain.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grdMain.Location = new System.Drawing.Point(3, 16);
		this.grdMain.Name = "grdMain";
		this.grdMain.Size = new System.Drawing.Size(602, 205);
		this.grdMain.TabIndex = 0;
		this.lblDbSchema.AutoSize = true;
		this.lblDbSchema.Location = new System.Drawing.Point(12, 75);
		this.lblDbSchema.Name = "lblDbSchema";
		this.lblDbSchema.Size = new System.Drawing.Size(63, 13);
		this.lblDbSchema.TabIndex = 11;
		this.lblDbSchema.Text = "Db Schema";
		this.ctlDbSchema.Location = new System.Drawing.Point(77, 71);
		this.ctlDbSchema.Name = "ctlDbSchema";
		this.ctlDbSchema.Size = new System.Drawing.Size(149, 20);
		this.ctlDbSchema.TabIndex = 3;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(614, 400);
		base.Controls.Add(this.grbGrand);
		base.Controls.Add(this.grbBottom);
		base.Controls.Add(this.grbTop);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "fConfig";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Configuration";
		base.Load += new System.EventHandler(fConfig_Load);
		this.grbTop.ResumeLayout(false);
		this.grbTop.PerformLayout();
		this.grbBottom.ResumeLayout(false);
		this.grbGrand.ResumeLayout(false);
		this.grbGenderMap.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grdGenderMap).EndInit();
		this.grbClient.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grdMain).EndInit();
		base.ResumeLayout(false);
	}
}
