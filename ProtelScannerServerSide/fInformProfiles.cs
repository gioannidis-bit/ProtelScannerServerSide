using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using log4net;
using Microsoft.Win32;
using Mrz;
using Newtonsoft.Json;
using PassToProtel.XmlDocsDescr;
using PassToProtel.XmlExecution;
using ProtelScannerServerSide.Classes;
using ProtelScannerServerSide.Enums;
using ProtelScannerServerSide.Helpers;
using ProtelScannerServerSide.MainLogic;
using ProtelScannerServerSide.Models;

namespace ProtelScannerServerSide;

public class fInformProfiles : Form
{
	private delegate void SetTextCallback(TextBox control, string text, MrzRecord record);

	private delegate void SetDatetimeCallBack(DateTimePicker control, string text, MrzRecord record);

	private delegate void SetComboboxCallBack(ComboBox control, TextBox countryName, string text, MrzRecord record);

	private delegate void SetScannerRecordToTextBox(string text);

	private long leistacc;

	private int StationId;

	private ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	private string sPath;

	private string Connection;

	private MainFlow mainFlow;

	private ConfigModel configModel;

	private List<ReservationModel> reservations;

	private List<NationalitiesModel> nationalities;

	private List<GenderModel> genders;

	private byte[] _Buffer = new byte[1024];

	private Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

	private List<DocDescr> mDocumentDescrList;

	private string mRowsSeparator = "\r\n";

	private XmlSettings mXmlSettings;

	private int nRow;

	private IContainer components;

	private GroupBox grbTop;

	private TextBox ctlDeparture;

	private TextBox ctlArrival;

	private TextBox ctlReservCode;

	private TextBox ctlRoom;

	private TextBox ctlRoomType;

	private Label lblDeparture;

	private Label lblArrival;

	private Label lblReservCode;

	private Label lblRoom;

	private Label lblRoomType;

	private GroupBox grbProfiles;

	private GroupBox grbBottom;

	private GroupBox grbProfCl;

	private GroupBox grbProfLeft;

	private ComboBox ctlGender;

	private DateTimePicker ctlExpired;

	private DateTimePicker ctlIssueDate;

	private TextBox ctlIssued;

	private TextBox ctlPassport;

	private DateTimePicker ctlBirthday;

	private ComboBox ctlCountry;

	private ComboBox ctlNationality;

	private TextBox ctlTitle;

	private TextBox ctlFirstName;

	private TextBox ctlLastName;

	private Label lblExpired;

	private Label lblIssueDate;

	private Label lblIssued;

	private Label lblPassport;

	private Label lblBirthday;

	private Label lblCountry;

	private Label lblNationality;

	private Label lblGender;

	private Label lblTitle;

	private Label lblFirstName;

	private Label lblLastName;

	private ComboBox ctlNewProfile;

	private Label lblNewProfile;

	private GroupBox grbClientFill;

	private DataGridView grdProf;

	private GroupBox grbClientBottom;

	private Button btnNext;

	private Button btnPrev;

	private Button btnSave;

	private TextBox ctlScanner;

	private Label lblScanner;

	public fInformProfiles()
	{
		InitializeComponent();
	}

	public fInformProfiles(string[] args)
	{
		InitializeComponent();
		long.TryParse(args[0], out leistacc);
		int.TryParse(args[1], out StationId);
	}

	private RegistryKey GetRegistryOdbc(string odbc)
	{
		RegistryKey reg = Registry.LocalMachine;
		string server = "";
		reg = reg.OpenSubKey("Software");
		reg = reg.OpenSubKey("ODBC");
		reg = reg.OpenSubKey("ODBC.INI");
		reg = reg.OpenSubKey(odbc);
		if (reg != null)
		{
			object value = reg.GetValue("Server");
			if (value != null)
			{
				server = value.ToString();
			}
		}
		if (reg == null || string.IsNullOrEmpty(server))
		{
			reg = Registry.CurrentUser;
			reg = reg.OpenSubKey("Software");
			reg = reg.OpenSubKey("ODBC");
			reg = reg.OpenSubKey("ODBC.INI");
			reg = reg.OpenSubKey(odbc);
		}
		if (reg != null)
		{
			object value = reg.GetValue("Server");
			if (value != null)
			{
				server = value.ToString();
			}
		}
		if (reg == null || string.IsNullOrEmpty(server))
		{
			reg = Registry.LocalMachine;
			reg = reg.OpenSubKey("Software");
			reg = reg.OpenSubKey("WOW6432Node");
			reg = reg.OpenSubKey("ODBC");
			reg = reg.OpenSubKey("ODBC.INI");
			reg = reg.OpenSubKey(odbc);
		}
		if (reg != null)
		{
			object value = reg.GetValue("Server");
			if (value != null)
			{
				server = value.ToString();
			}
		}
		if (reg == null || string.IsNullOrEmpty(server))
		{
			reg = Registry.CurrentUser;
			reg = reg.OpenSubKey("Software");
			reg = reg.OpenSubKey("WOW6432Node");
			reg = reg.OpenSubKey("ODBC");
			reg = reg.OpenSubKey("ODBC.INI");
			reg = reg.OpenSubKey(odbc);
		}
		return reg;
	}

	private void CreateConnectionstring()
	{
		if (!string.IsNullOrWhiteSpace(configModel.ConnectionString))
		{
			Connection = configModel.ConnectionString;
			return;
		}
		string odbc = "";
		string sVal = "";
		string[] array = File.ReadAllLines(configModel.ProtelIni);
		for (int i = 0; i < array.Length; i++)
		{
			sVal = array[i].Replace(" ", "");
			if (sVal.Replace(" ", "").Contains("sql_dsn="))
			{
				odbc = sVal.Substring(sVal.IndexOf('=') + 1, sVal.Length - sVal.IndexOf('=') - 1);
				break;
			}
		}
		string OdbcServer = "";
		RegistryKey reg = GetRegistryOdbc(odbc);
		if (reg == null)
		{
			logger.Error("ODBC " + odbc + " not found on registry");
			Connection = "";
			return;
		}
		logger.Error("ODBC " + odbc + " found on registry. connection string " + Connection);
		object value = reg.GetValue("Server");
		if (value != null)
		{
			OdbcServer = value.ToString();
		}
		value = reg.GetValue("Database");
		string OdbcDB = ((value == null) ? "Protel" : value.ToString());
		value = reg.GetValue("LastUser");
		string OdbcUser = ((value == null) ? "proteluser" : value.ToString());
		string OdbcPass = OdbcUser.ToLower().Replace("user", "915930");
		Connection = "server=" + OdbcServer + ";user id=" + OdbcUser + ";password=" + OdbcPass + ";database=" + OdbcDB + ";";
	}

	private void fInformProfiles_Load(object sender, EventArgs e)
	{
		try
		{
			sPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
			if (sPath[sPath.Length - 1] != '\\')
			{
				sPath += "\\";
			}
			if (!File.Exists(sPath + "\\Config\\configuration.json"))
			{
				logger.Error("Configuration file not found");
				MessageBox.Show("Configuration file not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				Close();
			}
			string sData = File.ReadAllText(sPath + "\\Config\\configuration.json");
			sData = EncryptionHelper.Decrypt(sData);
			configModel = JsonConvert.DeserializeObject<ConfigModel>(sData);
			if (string.IsNullOrWhiteSpace(configModel.ProtelIni) && string.IsNullOrWhiteSpace(configModel.ConnectionString))
			{
				logger.Error("No value for protel ini file exists on configuration file");
				MessageBox.Show("Configuration for protel ini file not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				Close();
			}
			if (!File.Exists(configModel.ProtelIni) && string.IsNullOrWhiteSpace(configModel.ConnectionString))
			{
				logger.Error("Protel ini  " + configModel.ProtelIni + " not foun");
				MessageBox.Show("Configuration for protel ini file not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				Close();
			}
			CreateConnectionstring();
			if (string.IsNullOrEmpty(Connection))
			{
				logger.Error("Protel ini file not found");
				MessageBox.Show("Configuration for protel ini file not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				Close();
			}
			if (string.IsNullOrWhiteSpace(configModel.DbSchema))
			{
				configModel.DbSchema = "proteluser";
			}
			mainFlow = new MainFlow(Connection, configModel.DbSchema);
			if (!mainFlow.CheckConnection())
			{
				logger.Error("Connection error");
				MessageBox.Show("Cannot connect to protel data base. Check conntection properties", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				Close();
			}
			DocsDescrParser ddp = new DocsDescrParser();
			mDocumentDescrList = ddp.GetDocsDescrList(sPath + "Documents.xml");
			if (!SetupServer())
			{
				MessageBox.Show("Error on socket initialization", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				Close();
			}
			nationalities = mainFlow.GetNationalities();
			ctlNationality.Items.Clear();
			ctlCountry.Items.Clear();
			if (nationalities != null)
			{
				foreach (NationalitiesModel item in nationalities)
				{
					ctlNationality.Items.Add(item);
					ctlCountry.Items.Add(item);
				}
			}
			ctlCountry.DisplayMember = "land";
			ctlCountry.ValueMember = "codenr";
			ctlNationality.DisplayMember = "land";
			ctlNationality.ValueMember = "codenr";
			genders = mainFlow.GetGenders();
			ctlGender.Items.Clear();
			if (genders != null)
			{
				foreach (GenderModel item2 in genders)
				{
					ctlGender.Items.Add(item2);
				}
			}
			ctlGender.DisplayMember = "bezeich";
			ctlGender.ValueMember = "nr";
			ctlNewProfile.Items.Clear();
			foreach (object item3 in Enum.GetValues(typeof(CompanionTypeEnum)))
			{
				ctlNewProfile.Items.Add(item3);
			}
			reservations = mainFlow.GetReservation(leistacc);
			if (reservations == null || reservations.Count < 1)
			{
				MessageBox.Show("No reservation with id " + leistacc + " found on protel data", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				Close();
			}
			if (reservations.Where((ReservationModel w) => w.kdnr < 1 && w.validFromBegl < 0).Count() > 0 && MessageBox.Show("Create not attached profiles?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) != DialogResult.Yes)
			{
				reservations.RemoveAll((ReservationModel r) => r.kdnr < 1 && r.validFromBegl < 0);
			}
			if (reservations != null && reservations.Count > 0)
			{
				ctlRoomType.Text = reservations[0].kat;
				ctlRoom.Text = reservations[0].ziname;
				ctlReservCode.Text = reservations[0].string1;
				ctlArrival.Text = reservations[0].globdvon.ToString("dd/MM/yyyy");
				ctlDeparture.Text = reservations[0].globdbis.ToString("dd/MM/yyyy");
			}
			MakeGrid();
		}
		catch (Exception ex)
		{
			ctlScanner.Text = ex.ToString();
			logger.Error(ex.ToString());
		}
		finally
		{
			btnNext.Focus();
			base.WindowState = FormWindowState.Normal;
			base.Width = 1040;
			base.Height = 575;
			base.TopMost = true;
		}
	}

	private void MakeGrid()
	{
		grdProf.DataSource = null;
		grdProf.AutoGenerateColumns = true;
		grdProf.AllowUserToAddRows = false;
		grdProf.AllowUserToDeleteRows = false;
		BindingSource bs = new BindingSource();
		bs.DataSource = reservations;
		grdProf.DataSource = bs;
		grdProf.Columns[0].Visible = false;
		grdProf.Columns[1].Visible = false;
		grdProf.Columns[2].Visible = false;
		grdProf.Columns[3].Visible = false;
		grdProf.Columns[4].Visible = false;
		grdProf.Columns[5].Visible = false;
		grdProf.Columns[6].Visible = false;
		grdProf.Columns[7].Visible = false;
		grdProf.Columns[8].HeaderText = "Type";
		grdProf.Columns[8].Width = 80;
		grdProf.Columns[8].ReadOnly = true;
		grdProf.Columns[9].HeaderText = "Last Name";
		grdProf.Columns[9].Width = 120;
		grdProf.Columns[9].ReadOnly = true;
		grdProf.Columns[10].HeaderText = "Last Name";
		grdProf.Columns[10].Width = 120;
		grdProf.Columns[10].ReadOnly = true;
		grdProf.Columns[11].Visible = false;
		grdProf.Columns[12].Visible = false;
		grdProf.Columns[13].Visible = false;
		grdProf.Columns[14].Visible = false;
		grdProf.Columns[15].Visible = false;
		grdProf.Columns[16].Visible = false;
		grdProf.Columns[17].Visible = false;
		grdProf.Columns[18].Visible = false;
		grdProf.Columns[19].Visible = false;
		grdProf.Columns[20].Visible = false;
		grdProf.Columns[21].Visible = false;
	}

	private void grdProf_SelectionChanged(object sender, EventArgs e)
	{
		if (grdProf.SelectedCells.Count < 1 || grdProf[0, grdProf.SelectedCells[0].RowIndex] == null || grdProf[0, grdProf.SelectedCells[0].RowIndex].Value == null)
		{
			return;
		}
		nRow = grdProf.SelectedCells[0].RowIndex;
		int.TryParse(grdProf[0, grdProf.SelectedCells[0].RowIndex].Value.ToString(), out var nId);
		ReservationModel fld = reservations.Find((ReservationModel f) => f.Id == nId);
		if (fld != null)
		{
			ctlLastName.Text = fld.name1;
			ctlFirstName.Text = fld.vorname;
			ctlTitle.Text = fld.titel;
			ctlBirthday.Value = fld.gebdat;
			GenderModel gend = genders.Find((GenderModel f) => f.nr == fld.gender);
			ctlGender.SelectedItem = gend;
			NationalitiesModel nat = nationalities.Find((NationalitiesModel f) => f.codenr == fld.nat);
			ctlNationality.SelectedItem = nat;
			nat = nationalities.Find((NationalitiesModel f) => f.codenr == fld.landkz);
			ctlCountry.SelectedItem = nat;
			ctlPassport.Text = fld.passnr;
			ctlIssued.Text = fld.issued;
			ctlIssueDate.Value = fld.issuedate;
			ctlExpired.Value = fld.docvalid;
			if (fld.kdnr < 1 && fld.validFromBegl < 0)
			{
				ctlNewProfile.Enabled = true;
				ctlNewProfile.SelectedItem = configModel.CompanionType;
			}
			else
			{
				ctlNewProfile.Enabled = false;
			}
		}
	}

	private bool SetupServer()
	{
		try
		{
			UsersServerPortsAssoc fld = configModel.UsersPorts.Find((UsersServerPortsAssoc f) => f.stationId == StationId);
			if (fld == null)
			{
				logger.Error("No port on configuration found for station id : " + StationId);
				return false;
			}
			_serverSocket.Bind(new IPEndPoint(IPAddress.Any, fld.portNumber));
			_serverSocket.Listen(1);
			_serverSocket.BeginAccept(AcceptCallback, null);
		}
		catch (Exception ex)
		{
			logger.Error(ex.ToString());
			return false;
		}
		return true;
	}

	private void AcceptCallback(IAsyncResult AR)
	{
		try
		{
			Socket socket = _serverSocket.EndAccept(AR);
			socket.BeginReceive(_Buffer, 0, _Buffer.Length, SocketFlags.None, ReceiveCallback, socket);
			_serverSocket.BeginAccept(AcceptCallback, null);
		}
		catch (Exception ex)
		{
			logger.Error(ex.ToString());
		}
	}

	private void ReceiveCallback(IAsyncResult AR)
	{
		try
		{
			Socket socket = (Socket)AR.AsyncState;
			int received = socket.EndReceive(AR);
			byte[] dataBuff = new byte[received];
			Array.Copy(_Buffer, dataBuff, received);
			string text = Encoding.ASCII.GetString(dataBuff);
			SetText(text);
			string responsMessage = string.Empty;
			responsMessage = (string.IsNullOrWhiteSpace(text) ? "No data received" : "Message received");
			byte[] data = Encoding.ASCII.GetBytes(responsMessage);
			socket.BeginSend(data, 0, data.Length, SocketFlags.None, SendCallBack, socket);
		}
		catch (Exception ex)
		{
			logger.Error(ex.ToString());
		}
	}

	private void SetValuetoTextBox(TextBox control, string text, MrzRecord record)
	{
		if (control.InvokeRequired)
		{
			SetTextCallback d = SetValuetoTextBox;
			Invoke(d, text);
			return;
		}
		switch (control.Name)
		{
		case "ctlLastName":
			control.Text = record.surname;
			break;
		case "ctlFirstName":
			control.Text = record.givenNames;
			break;
		case "ctlPassport":
			control.Text = record.documentNumber;
			break;
		default:
			control.Text = "";
			break;
		}
	}

	private void SetValuesToDateTimePicker(DateTimePicker control, string text, MrzRecord record)
	{
		if (control.InvokeRequired)
		{
			SetDatetimeCallBack d = SetValuesToDateTimePicker;
			Invoke(d, control, text, record);
			return;
		}
		string sDay = string.Empty;
		if (control.Name == "ctlBirthday")
		{
			if (record.dateOfBirth == null)
			{
				return;
			}
			try
			{
				if (record.dateOfBirth.year <= DateTime.Today.Year - 2000 && record.dateOfBirth.year >= 0)
				{
					record.dateOfBirth.year = 2000 + record.dateOfBirth.year;
				}
				else
				{
					record.dateOfBirth.year = 1900 + record.dateOfBirth.year;
				}
				sDay = record.dateOfBirth.day + "/" + record.dateOfBirth.month + "/" + record.dateOfBirth.year;
				control.Value = DateTime.Parse(sDay);
				return;
			}
			catch (Exception)
			{
				logger.Error("failed to parse dob value as date: " + sDay);
				control.Value = DateTime.Parse("01/01/1900");
				return;
			}
		}
		if (record.expirationDate == null)
		{
			return;
		}
		try
		{
			int expirationYear = record.expirationDate.year + ((record.expirationDate.year < 2000 && record.expirationDate.year.ToString().Length == 2) ? 2000 : 100);
			sDay = record.expirationDate.day + "/" + record.expirationDate.month + "/" + expirationYear;
			control.Value = DateTime.Parse(sDay);
		}
		catch (Exception ex2)
		{
			logger.Error("failed to parse expiration date value as date: " + sDay);
			logger.Error(ex2.ToString());
			control.Value = DateTime.Parse("01/01/1900");
		}
	}

	private void SetValuesToCombobox(ComboBox control, TextBox countryName, string text, MrzRecord record)
	{
		if (control.InvokeRequired)
		{
			SetComboboxCallBack d = SetValuesToCombobox;
			Invoke(d, control, text, record);
			return;
		}
		if (control.Name != "ctlGender")
		{
			NationalitiesModel fld = (NationalitiesModel)(control.SelectedItem = nationalities.Find((NationalitiesModel f) => f.abkuerz == record.nationality));
			countryName.Text = ((fld == null) ? "" : fld.land);
			return;
		}
		GenderMappingModel configMap = configModel.GenderMapping.Find((GenderMappingModel f) => f.scannerGender == record.sex._sex.ToString());
		GenderModel fld2 = (GenderModel)(control.SelectedItem = genders.Find((GenderModel f) => f.bezeich == configMap.protelGender));
		string tt = ctlGender.Text;
		tt = tt + Environment.NewLine + "model: " + JsonConvert.SerializeObject(record) + Environment.NewLine + "sex: " + record.sex._sex.ToString() + Environment.NewLine + "GenderMapping: " + JsonConvert.SerializeObject(configModel.GenderMapping) + Environment.NewLine + "selected item: " + JsonConvert.SerializeObject(fld2);
		SetScannerRecordToTextBox d2 = SetScannerDataToTextBox;
		Invoke(d2, tt);
	}

	private void SetScannerDataToTextBox(string text)
	{
		if (ctlScanner.InvokeRequired)
		{
			SetScannerRecordToTextBox d = SetScannerDataToTextBox;
			Invoke(d, text);
		}
		else
		{
			ctlScanner.Text = text;
		}
	}

	private void SetText(string text)
	{
		try
		{
			int num = int.Parse(text[0].ToString());
			text = text.Substring(1);
			if (num == 0)
			{
				mRowsSeparator = "\r";
			}
			else
			{
				mRowsSeparator = "\r\n";
			}
			MrzParser parser = new MrzParser(text, mRowsSeparator);
			MrzRecord record = parser.parse(bIgnoreCheckDigits: false, mDocumentDescrList);
			if (record == null)
			{
				logger.Error("SendToProtel : No matching document template");
				return;
			}
			record.issuingCountry = ((record.issuingCountry == "D") ? "DEU" : record.issuingCountry);
			record.nationality = ((record.nationality == "D") ? "DEU" : record.nationality);
			if (ctlScanner.InvokeRequired)
			{
				SetScannerRecordToTextBox d = SetScannerDataToTextBox;
				Invoke(d, text);
			}
			else
			{
				ctlLastName.Text = text;
			}
			if (ctlLastName.InvokeRequired)
			{
				SetTextCallback d2 = SetValuetoTextBox;
				Invoke(d2, ctlLastName, text, record);
			}
			else
			{
				ctlLastName.Text = record.surname;
			}
			if (ctlFirstName.InvokeRequired)
			{
				SetTextCallback d3 = SetValuetoTextBox;
				Invoke(d3, ctlFirstName, text, record);
			}
			else
			{
				ctlLastName.Text = record.givenNames;
			}
			if (ctlIssued.InvokeRequired)
			{
				SetTextCallback d4 = SetValuetoTextBox;
				Invoke(d4, ctlIssued, text, record);
			}
			else
			{
				ctlIssued.Text = record.issuingCountry;
			}
			if (ctlPassport.InvokeRequired)
			{
				SetTextCallback d5 = SetValuetoTextBox;
				Invoke(d5, ctlPassport, text, record);
			}
			else
			{
				ctlPassport.Text = record.documentNumber;
			}
			if (ctlBirthday.InvokeRequired)
			{
				SetDatetimeCallBack d6 = SetValuesToDateTimePicker;
				Invoke(d6, ctlBirthday, text, record);
			}
			else if (record.dateOfBirth != null)
			{
				string sDay = null;
				try
				{
					if (record.dateOfBirth.year.ToString().Length != 4)
					{
						if (record.dateOfBirth.year <= DateTime.Today.Year - 2000 && record.dateOfBirth.year >= 0)
						{
							record.dateOfBirth.year = 2000 + record.dateOfBirth.year;
						}
						else
						{
							record.dateOfBirth.year = 1900 + record.dateOfBirth.year;
						}
					}
					sDay = record.dateOfBirth.day + "/" + record.dateOfBirth.month + "/" + record.dateOfBirth.year;
					ctlBirthday.Value = DateTime.Parse(sDay, new CultureInfo("el-GR"));
				}
				catch (Exception ex)
				{
					logger.Error("failed to parse dob value as date: " + sDay);
					MessageBox.Show("Failed to parse dob value as date: " + sDay + "\r\n" + ex.ToString());
					ctlBirthday.Value = DateTime.Parse("01/01/1900", new CultureInfo("el-GR"));
				}
			}
			if (ctlExpired.InvokeRequired)
			{
				SetDatetimeCallBack d7 = SetValuesToDateTimePicker;
				Invoke(d7, ctlExpired, text, record);
			}
			else if (record.expirationDate != null)
			{
				string s1Day = null;
				try
				{
					s1Day = record.expirationDate.day + "/" + record.expirationDate.month + "/" + record.expirationDate.year;
					ctlExpired.Value = DateTime.Parse(s1Day);
				}
				catch (Exception)
				{
					logger.Error("failed to parse expiration date value as date: " + s1Day);
					ctlExpired.Value = DateTime.Parse("01/01/1900");
				}
			}
			if (ctlNationality.InvokeRequired)
			{
				SetComboboxCallBack d8 = SetValuesToCombobox;
				Invoke(d8, ctlNationality, ctlIssued, text, record);
			}
			else
			{
				NationalitiesModel fld = nationalities.Find((NationalitiesModel f) => f.abkuerz == record.nationality);
				ctlNationality.SelectedItem = fld;
			}
			if (ctlCountry.InvokeRequired)
			{
				SetComboboxCallBack d9 = SetValuesToCombobox;
				Invoke(d9, ctlCountry, ctlIssued, text, record);
			}
			else
			{
				NationalitiesModel fld2 = nationalities.Find((NationalitiesModel f) => f.abkuerz == record.nationality);
				ctlCountry.SelectedItem = fld2;
				ctlIssued.Text = fld2.land;
			}
			if (ctlGender.InvokeRequired)
			{
				SetComboboxCallBack d10 = SetValuesToCombobox;
				Invoke(d10, ctlGender, null, text, record);
			}
			else
			{
				GenderMappingModel configMap = configModel.GenderMapping.Find((GenderMappingModel f) => f.scannerGender == record.sex._sex.ToString());
				GenderModel fld3 = genders.Find((GenderModel f) => f.bezeich == configMap.protelGender);
				ctlGender.SelectedItem = fld3;
			}
		}
		catch (Exception ex3)
		{
			logger.Error(ex3.ToString());
			MessageBox.Show(ex3.Message + ((ex3.InnerException != null) ? (" " + ex3.InnerException.Message) : ""), "Scanner Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void SendCallBack(IAsyncResult AR)
	{
		try
		{
			((Socket)AR.AsyncState).EndSend(AR);
		}
		catch (Exception ex)
		{
			logger.Error(ex.ToString());
		}
	}

	private void btnNext_Click(object sender, EventArgs e)
	{
		if (nRow < grdProf.RowCount - 1)
		{
			grdProf.Rows[nRow].Selected = false;
			grdProf.Rows[++nRow].Selected = true;
		}
		else
		{
			grdProf.Rows[nRow].Selected = false;
			grdProf.Rows[0].Selected = true;
		}
	}

	private void btnPrev_Click(object sender, EventArgs e)
	{
		if (nRow > 0)
		{
			grdProf.Rows[nRow].Selected = false;
			grdProf.Rows[--nRow].Selected = true;
		}
	}

	private void btnSave_Click(object sender, EventArgs e)
	{
		try
		{
			if (grdProf.SelectedCells.Count < 1 || grdProf[0, grdProf.SelectedCells[0].RowIndex] == null || grdProf[0, grdProf.SelectedCells[0].RowIndex].Value == null)
			{
				return;
			}
			int.TryParse(grdProf[0, grdProf.SelectedCells[0].RowIndex].Value.ToString(), out var nId);
			ReservationModel fld = reservations.Find((ReservationModel f) => f.Id == nId);
			if (fld == null)
			{
				return;
			}
			fld.name1 = ctlLastName.Text;
			fld.vorname = ctlFirstName.Text;
			fld.titel = ctlTitle.Text;
			fld.gebdat = ctlBirthday.Value;
			if (fld.gebdat > DateTime.Now)
			{
				fld.gebdat = fld.gebdat.AddYears(-100);
			}
			if (ctlGender.SelectedItem == null)
			{
				fld.gender = -1;
			}
			else
			{
				fld.gender = ((GenderModel)ctlGender.SelectedItem).nr;
			}
			if (ctlNationality.SelectedItem == null)
			{
				fld.nat = -1;
			}
			else
			{
				fld.nat = ((NationalitiesModel)ctlNationality.SelectedItem).codenr;
			}
			if (ctlCountry.SelectedItem == null)
			{
				fld.landkz = -1;
			}
			else
			{
				fld.landkz = ((NationalitiesModel)ctlCountry.SelectedItem).codenr;
			}
			fld.land = ctlCountry.Text;
			fld.passnr = ctlPassport.Text;
			fld.issued = ctlIssued.Text;
			fld.issuedate = ctlIssueDate.Value;
			fld.docvalid = ctlExpired.Value;
			string error;
			long results = mainFlow.SaveProfile(fld, leistacc, (ctlNewProfile.SelectedIndex > -1) ? ((CompanionTypeEnum)ctlNewProfile.SelectedItem) : CompanionTypeEnum.None, out error);
			if (!string.IsNullOrWhiteSpace(error))
			{
				logger.Error(error);
				MessageBox.Show("Cannot make updates to profiles", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				string finError = ctlScanner.Text + " \r\n";
				finError += error;
				if (Directory.Exists("C:\\logs\\"))
				{
					Directory.CreateDirectory("C:\\logs\\");
				}
				File.WriteAllText("C:\\logs\\CompanionError.log", finError);
				ctlScanner.Text = error;
			}
			else
			{
				if (results != leistacc)
				{
					fld.kdnr = results;
				}
				if (ctlNewProfile.Enabled)
				{
					grdProf[8, grdProf.SelectedCells[0].RowIndex].Value = ctlNewProfile.Text;
				}
				grdProf[9, grdProf.SelectedCells[0].RowIndex].Value = ctlLastName.Text;
				grdProf[10, grdProf.SelectedCells[0].RowIndex].Value = ctlFirstName.Text;
				btnNext_Click(btnNext, e);
			}
		}
		catch (Exception ex)
		{
			logger.Error(ex.ToString());
		}
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProtelScannerServerSide.fInformProfiles));
		this.grbTop = new System.Windows.Forms.GroupBox();
		this.ctlDeparture = new System.Windows.Forms.TextBox();
		this.ctlArrival = new System.Windows.Forms.TextBox();
		this.ctlReservCode = new System.Windows.Forms.TextBox();
		this.ctlRoom = new System.Windows.Forms.TextBox();
		this.ctlRoomType = new System.Windows.Forms.TextBox();
		this.lblDeparture = new System.Windows.Forms.Label();
		this.lblArrival = new System.Windows.Forms.Label();
		this.lblReservCode = new System.Windows.Forms.Label();
		this.lblRoom = new System.Windows.Forms.Label();
		this.lblRoomType = new System.Windows.Forms.Label();
		this.grbProfiles = new System.Windows.Forms.GroupBox();
		this.grbProfCl = new System.Windows.Forms.GroupBox();
		this.ctlScanner = new System.Windows.Forms.TextBox();
		this.lblScanner = new System.Windows.Forms.Label();
		this.ctlNewProfile = new System.Windows.Forms.ComboBox();
		this.lblNewProfile = new System.Windows.Forms.Label();
		this.ctlGender = new System.Windows.Forms.ComboBox();
		this.ctlExpired = new System.Windows.Forms.DateTimePicker();
		this.ctlIssueDate = new System.Windows.Forms.DateTimePicker();
		this.ctlIssued = new System.Windows.Forms.TextBox();
		this.ctlPassport = new System.Windows.Forms.TextBox();
		this.ctlBirthday = new System.Windows.Forms.DateTimePicker();
		this.ctlCountry = new System.Windows.Forms.ComboBox();
		this.ctlNationality = new System.Windows.Forms.ComboBox();
		this.ctlTitle = new System.Windows.Forms.TextBox();
		this.ctlFirstName = new System.Windows.Forms.TextBox();
		this.ctlLastName = new System.Windows.Forms.TextBox();
		this.lblExpired = new System.Windows.Forms.Label();
		this.lblIssueDate = new System.Windows.Forms.Label();
		this.lblIssued = new System.Windows.Forms.Label();
		this.lblPassport = new System.Windows.Forms.Label();
		this.lblBirthday = new System.Windows.Forms.Label();
		this.lblCountry = new System.Windows.Forms.Label();
		this.lblNationality = new System.Windows.Forms.Label();
		this.lblGender = new System.Windows.Forms.Label();
		this.lblTitle = new System.Windows.Forms.Label();
		this.lblFirstName = new System.Windows.Forms.Label();
		this.lblLastName = new System.Windows.Forms.Label();
		this.grbProfLeft = new System.Windows.Forms.GroupBox();
		this.grbClientFill = new System.Windows.Forms.GroupBox();
		this.grdProf = new System.Windows.Forms.DataGridView();
		this.grbClientBottom = new System.Windows.Forms.GroupBox();
		this.btnNext = new System.Windows.Forms.Button();
		this.btnPrev = new System.Windows.Forms.Button();
		this.grbBottom = new System.Windows.Forms.GroupBox();
		this.btnSave = new System.Windows.Forms.Button();
		this.grbTop.SuspendLayout();
		this.grbProfiles.SuspendLayout();
		this.grbProfCl.SuspendLayout();
		this.grbProfLeft.SuspendLayout();
		this.grbClientFill.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.grdProf).BeginInit();
		this.grbClientBottom.SuspendLayout();
		this.grbBottom.SuspendLayout();
		base.SuspendLayout();
		this.grbTop.Controls.Add(this.ctlDeparture);
		this.grbTop.Controls.Add(this.ctlArrival);
		this.grbTop.Controls.Add(this.ctlReservCode);
		this.grbTop.Controls.Add(this.ctlRoom);
		this.grbTop.Controls.Add(this.ctlRoomType);
		this.grbTop.Controls.Add(this.lblDeparture);
		this.grbTop.Controls.Add(this.lblArrival);
		this.grbTop.Controls.Add(this.lblReservCode);
		this.grbTop.Controls.Add(this.lblRoom);
		this.grbTop.Controls.Add(this.lblRoomType);
		this.grbTop.Dock = System.Windows.Forms.DockStyle.Top;
		this.grbTop.Enabled = false;
		this.grbTop.Location = new System.Drawing.Point(0, 0);
		this.grbTop.Name = "grbTop";
		this.grbTop.Size = new System.Drawing.Size(1024, 52);
		this.grbTop.TabIndex = 0;
		this.grbTop.TabStop = false;
		this.grbTop.Text = "Reservation";
		this.ctlDeparture.Location = new System.Drawing.Point(838, 19);
		this.ctlDeparture.Name = "ctlDeparture";
		this.ctlDeparture.Size = new System.Drawing.Size(91, 20);
		this.ctlDeparture.TabIndex = 9;
		this.ctlArrival.Location = new System.Drawing.Point(647, 19);
		this.ctlArrival.Name = "ctlArrival";
		this.ctlArrival.Size = new System.Drawing.Size(91, 20);
		this.ctlArrival.TabIndex = 8;
		this.ctlReservCode.Location = new System.Drawing.Point(413, 19);
		this.ctlReservCode.Name = "ctlReservCode";
		this.ctlReservCode.Size = new System.Drawing.Size(164, 20);
		this.ctlReservCode.TabIndex = 7;
		this.ctlRoom.Location = new System.Drawing.Point(218, 19);
		this.ctlRoom.Name = "ctlRoom";
		this.ctlRoom.Size = new System.Drawing.Size(61, 20);
		this.ctlRoom.TabIndex = 6;
		this.ctlRoomType.Location = new System.Drawing.Point(80, 19);
		this.ctlRoomType.Name = "ctlRoomType";
		this.ctlRoomType.Size = new System.Drawing.Size(61, 20);
		this.ctlRoomType.TabIndex = 5;
		this.lblDeparture.AutoSize = true;
		this.lblDeparture.Location = new System.Drawing.Point(387, 23);
		this.lblDeparture.Name = "lblDeparture";
		this.lblDeparture.Size = new System.Drawing.Size(54, 13);
		this.lblDeparture.TabIndex = 4;
		this.lblDeparture.Text = "Departure";
		this.lblArrival.AutoSize = true;
		this.lblArrival.Location = new System.Drawing.Point(605, 23);
		this.lblArrival.Name = "lblArrival";
		this.lblArrival.Size = new System.Drawing.Size(36, 13);
		this.lblArrival.TabIndex = 3;
		this.lblArrival.Text = "Arrival";
		this.lblReservCode.AutoSize = true;
		this.lblReservCode.Location = new System.Drawing.Point(315, 23);
		this.lblReservCode.Name = "lblReservCode";
		this.lblReservCode.Size = new System.Drawing.Size(92, 13);
		this.lblReservCode.TabIndex = 2;
		this.lblReservCode.Text = "Reservation Code";
		this.lblRoom.AutoSize = true;
		this.lblRoom.Location = new System.Drawing.Point(178, 23);
		this.lblRoom.Name = "lblRoom";
		this.lblRoom.Size = new System.Drawing.Size(35, 13);
		this.lblRoom.TabIndex = 1;
		this.lblRoom.Text = "Room";
		this.lblRoomType.AutoSize = true;
		this.lblRoomType.Location = new System.Drawing.Point(12, 23);
		this.lblRoomType.Name = "lblRoomType";
		this.lblRoomType.Size = new System.Drawing.Size(62, 13);
		this.lblRoomType.TabIndex = 0;
		this.lblRoomType.Text = "Room Type";
		this.grbProfiles.Controls.Add(this.grbProfCl);
		this.grbProfiles.Controls.Add(this.grbProfLeft);
		this.grbProfiles.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grbProfiles.Location = new System.Drawing.Point(0, 52);
		this.grbProfiles.Name = "grbProfiles";
		this.grbProfiles.Size = new System.Drawing.Size(1024, 484);
		this.grbProfiles.TabIndex = 1;
		this.grbProfiles.TabStop = false;
		this.grbProfiles.Text = "Profiles";
		this.grbProfCl.Controls.Add(this.ctlScanner);
		this.grbProfCl.Controls.Add(this.lblScanner);
		this.grbProfCl.Controls.Add(this.ctlNewProfile);
		this.grbProfCl.Controls.Add(this.lblNewProfile);
		this.grbProfCl.Controls.Add(this.ctlGender);
		this.grbProfCl.Controls.Add(this.ctlExpired);
		this.grbProfCl.Controls.Add(this.ctlIssueDate);
		this.grbProfCl.Controls.Add(this.ctlIssued);
		this.grbProfCl.Controls.Add(this.ctlPassport);
		this.grbProfCl.Controls.Add(this.ctlBirthday);
		this.grbProfCl.Controls.Add(this.ctlCountry);
		this.grbProfCl.Controls.Add(this.ctlNationality);
		this.grbProfCl.Controls.Add(this.ctlTitle);
		this.grbProfCl.Controls.Add(this.ctlFirstName);
		this.grbProfCl.Controls.Add(this.ctlLastName);
		this.grbProfCl.Controls.Add(this.lblExpired);
		this.grbProfCl.Controls.Add(this.lblIssueDate);
		this.grbProfCl.Controls.Add(this.lblIssued);
		this.grbProfCl.Controls.Add(this.lblPassport);
		this.grbProfCl.Controls.Add(this.lblBirthday);
		this.grbProfCl.Controls.Add(this.lblCountry);
		this.grbProfCl.Controls.Add(this.lblNationality);
		this.grbProfCl.Controls.Add(this.lblGender);
		this.grbProfCl.Controls.Add(this.lblTitle);
		this.grbProfCl.Controls.Add(this.lblFirstName);
		this.grbProfCl.Controls.Add(this.lblLastName);
		this.grbProfCl.Location = new System.Drawing.Point(388, 16);
		this.grbProfCl.Name = "grbProfCl";
		this.grbProfCl.Size = new System.Drawing.Size(633, 465);
		this.grbProfCl.TabIndex = 3;
		this.grbProfCl.TabStop = false;
		this.ctlScanner.Location = new System.Drawing.Point(14, 227);
		this.ctlScanner.Multiline = true;
		this.ctlScanner.Name = "ctlScanner";
		this.ctlScanner.ReadOnly = true;
		this.ctlScanner.ScrollBars = System.Windows.Forms.ScrollBars.Both;
		this.ctlScanner.Size = new System.Drawing.Size(612, 194);
		this.ctlScanner.TabIndex = 11;
		this.lblScanner.AutoSize = true;
		this.lblScanner.Location = new System.Drawing.Point(11, 211);
		this.lblScanner.Name = "lblScanner";
		this.lblScanner.Size = new System.Drawing.Size(73, 13);
		this.lblScanner.TabIndex = 25;
		this.lblScanner.Text = "Scanner Data";
		this.ctlNewProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.ctlNewProfile.FormattingEnabled = true;
		this.ctlNewProfile.Location = new System.Drawing.Point(89, 13);
		this.ctlNewProfile.Name = "ctlNewProfile";
		this.ctlNewProfile.Size = new System.Drawing.Size(164, 21);
		this.ctlNewProfile.TabIndex = 11;
		this.lblNewProfile.AutoSize = true;
		this.lblNewProfile.Location = new System.Drawing.Point(11, 16);
		this.lblNewProfile.Name = "lblNewProfile";
		this.lblNewProfile.Size = new System.Drawing.Size(74, 13);
		this.lblNewProfile.TabIndex = 24;
		this.lblNewProfile.Text = "New profile as";
		this.ctlGender.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.ctlGender.FormattingEnabled = true;
		this.ctlGender.Location = new System.Drawing.Point(450, 71);
		this.ctlGender.Name = "ctlGender";
		this.ctlGender.Size = new System.Drawing.Size(176, 21);
		this.ctlGender.TabIndex = 4;
		this.ctlExpired.Format = System.Windows.Forms.DateTimePickerFormat.Short;
		this.ctlExpired.Location = new System.Drawing.Point(450, 154);
		this.ctlExpired.Name = "ctlExpired";
		this.ctlExpired.Size = new System.Drawing.Size(99, 20);
		this.ctlExpired.TabIndex = 10;
		this.ctlIssueDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
		this.ctlIssueDate.Location = new System.Drawing.Point(90, 154);
		this.ctlIssueDate.Name = "ctlIssueDate";
		this.ctlIssueDate.Size = new System.Drawing.Size(99, 20);
		this.ctlIssueDate.TabIndex = 9;
		this.ctlIssued.Location = new System.Drawing.Point(450, 125);
		this.ctlIssued.Name = "ctlIssued";
		this.ctlIssued.Size = new System.Drawing.Size(176, 20);
		this.ctlIssued.TabIndex = 8;
		this.ctlPassport.Location = new System.Drawing.Point(90, 125);
		this.ctlPassport.Name = "ctlPassport";
		this.ctlPassport.Size = new System.Drawing.Size(164, 20);
		this.ctlPassport.TabIndex = 7;
		this.ctlBirthday.Format = System.Windows.Forms.DateTimePickerFormat.Short;
		this.ctlBirthday.Location = new System.Drawing.Point(277, 71);
		this.ctlBirthday.Name = "ctlBirthday";
		this.ctlBirthday.Size = new System.Drawing.Size(99, 20);
		this.ctlBirthday.TabIndex = 3;
		this.ctlCountry.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.ctlCountry.FormattingEnabled = true;
		this.ctlCountry.Location = new System.Drawing.Point(450, 98);
		this.ctlCountry.Name = "ctlCountry";
		this.ctlCountry.Size = new System.Drawing.Size(176, 21);
		this.ctlCountry.TabIndex = 6;
		this.ctlNationality.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.ctlNationality.FormattingEnabled = true;
		this.ctlNationality.Location = new System.Drawing.Point(90, 98);
		this.ctlNationality.Name = "ctlNationality";
		this.ctlNationality.Size = new System.Drawing.Size(164, 21);
		this.ctlNationality.TabIndex = 5;
		this.ctlTitle.Location = new System.Drawing.Point(89, 71);
		this.ctlTitle.Name = "ctlTitle";
		this.ctlTitle.Size = new System.Drawing.Size(91, 20);
		this.ctlTitle.TabIndex = 2;
		this.ctlFirstName.Location = new System.Drawing.Point(450, 45);
		this.ctlFirstName.Name = "ctlFirstName";
		this.ctlFirstName.Size = new System.Drawing.Size(176, 20);
		this.ctlFirstName.TabIndex = 1;
		this.ctlLastName.Location = new System.Drawing.Point(89, 45);
		this.ctlLastName.Name = "ctlLastName";
		this.ctlLastName.Size = new System.Drawing.Size(287, 20);
		this.ctlLastName.TabIndex = 0;
		this.lblExpired.AutoSize = true;
		this.lblExpired.Location = new System.Drawing.Point(367, 158);
		this.lblExpired.Name = "lblExpired";
		this.lblExpired.Size = new System.Drawing.Size(77, 13);
		this.lblExpired.TabIndex = 11;
		this.lblExpired.Text = "Expiration date";
		this.lblIssueDate.AutoSize = true;
		this.lblIssueDate.Location = new System.Drawing.Point(11, 158);
		this.lblIssueDate.Name = "lblIssueDate";
		this.lblIssueDate.Size = new System.Drawing.Size(56, 13);
		this.lblIssueDate.TabIndex = 10;
		this.lblIssueDate.Text = "Issue date";
		this.lblIssued.AutoSize = true;
		this.lblIssued.Location = new System.Drawing.Point(387, 129);
		this.lblIssued.Name = "lblIssued";
		this.lblIssued.Size = new System.Drawing.Size(38, 13);
		this.lblIssued.TabIndex = 9;
		this.lblIssued.Text = "Issued";
		this.lblPassport.AutoSize = true;
		this.lblPassport.Location = new System.Drawing.Point(11, 129);
		this.lblPassport.Name = "lblPassport";
		this.lblPassport.Size = new System.Drawing.Size(48, 13);
		this.lblPassport.TabIndex = 8;
		this.lblPassport.Text = "Passport";
		this.lblBirthday.AutoSize = true;
		this.lblBirthday.Location = new System.Drawing.Point(208, 75);
		this.lblBirthday.Name = "lblBirthday";
		this.lblBirthday.Size = new System.Drawing.Size(45, 13);
		this.lblBirthday.TabIndex = 7;
		this.lblBirthday.Text = "Birthday";
		this.lblCountry.AutoSize = true;
		this.lblCountry.Location = new System.Drawing.Point(387, 102);
		this.lblCountry.Name = "lblCountry";
		this.lblCountry.Size = new System.Drawing.Size(43, 13);
		this.lblCountry.TabIndex = 6;
		this.lblCountry.Text = "Country";
		this.lblNationality.AutoSize = true;
		this.lblNationality.Location = new System.Drawing.Point(11, 102);
		this.lblNationality.Name = "lblNationality";
		this.lblNationality.Size = new System.Drawing.Size(56, 13);
		this.lblNationality.TabIndex = 5;
		this.lblNationality.Text = "Nationality";
		this.lblGender.AutoSize = true;
		this.lblGender.Location = new System.Drawing.Point(387, 75);
		this.lblGender.Name = "lblGender";
		this.lblGender.Size = new System.Drawing.Size(42, 13);
		this.lblGender.TabIndex = 4;
		this.lblGender.Text = "Gender";
		this.lblTitle.AutoSize = true;
		this.lblTitle.Location = new System.Drawing.Point(11, 75);
		this.lblTitle.Name = "lblTitle";
		this.lblTitle.Size = new System.Drawing.Size(27, 13);
		this.lblTitle.TabIndex = 3;
		this.lblTitle.Text = "Title";
		this.lblFirstName.AutoSize = true;
		this.lblFirstName.Location = new System.Drawing.Point(387, 49);
		this.lblFirstName.Name = "lblFirstName";
		this.lblFirstName.Size = new System.Drawing.Size(57, 13);
		this.lblFirstName.TabIndex = 2;
		this.lblFirstName.Text = "First Name";
		this.lblLastName.AutoSize = true;
		this.lblLastName.Location = new System.Drawing.Point(11, 49);
		this.lblLastName.Name = "lblLastName";
		this.lblLastName.Size = new System.Drawing.Size(58, 13);
		this.lblLastName.TabIndex = 1;
		this.lblLastName.Text = "Last Name";
		this.grbProfLeft.Controls.Add(this.grbClientFill);
		this.grbProfLeft.Controls.Add(this.grbClientBottom);
		this.grbProfLeft.Dock = System.Windows.Forms.DockStyle.Left;
		this.grbProfLeft.Location = new System.Drawing.Point(3, 16);
		this.grbProfLeft.Name = "grbProfLeft";
		this.grbProfLeft.Size = new System.Drawing.Size(385, 465);
		this.grbProfLeft.TabIndex = 2;
		this.grbProfLeft.TabStop = false;
		this.grbClientFill.Controls.Add(this.grdProf);
		this.grbClientFill.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grbClientFill.Location = new System.Drawing.Point(3, 16);
		this.grbClientFill.Name = "grbClientFill";
		this.grbClientFill.Size = new System.Drawing.Size(379, 383);
		this.grbClientFill.TabIndex = 1;
		this.grbClientFill.TabStop = false;
		this.grdProf.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.grdProf.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grdProf.Location = new System.Drawing.Point(3, 16);
		this.grdProf.Name = "grdProf";
		this.grdProf.Size = new System.Drawing.Size(373, 364);
		this.grdProf.TabIndex = 2;
		this.grdProf.SelectionChanged += new System.EventHandler(grdProf_SelectionChanged);
		this.grbClientBottom.Controls.Add(this.btnNext);
		this.grbClientBottom.Controls.Add(this.btnPrev);
		this.grbClientBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.grbClientBottom.Location = new System.Drawing.Point(3, 399);
		this.grbClientBottom.Name = "grbClientBottom";
		this.grbClientBottom.Size = new System.Drawing.Size(379, 63);
		this.grbClientBottom.TabIndex = 0;
		this.grbClientBottom.TabStop = false;
		this.btnNext.Location = new System.Drawing.Point(301, 6);
		this.btnNext.Name = "btnNext";
		this.btnNext.Size = new System.Drawing.Size(75, 23);
		this.btnNext.TabIndex = 0;
		this.btnNext.Text = "|>";
		this.btnNext.UseVisualStyleBackColor = true;
		this.btnNext.Click += new System.EventHandler(btnNext_Click);
		this.btnPrev.Location = new System.Drawing.Point(3, 6);
		this.btnPrev.Name = "btnPrev";
		this.btnPrev.Size = new System.Drawing.Size(75, 23);
		this.btnPrev.TabIndex = 1;
		this.btnPrev.Text = "<|";
		this.btnPrev.UseVisualStyleBackColor = true;
		this.btnPrev.Click += new System.EventHandler(btnPrev_Click);
		this.grbBottom.Controls.Add(this.btnSave);
		this.grbBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
		this.grbBottom.Location = new System.Drawing.Point(0, 495);
		this.grbBottom.Name = "grbBottom";
		this.grbBottom.Size = new System.Drawing.Size(1024, 41);
		this.grbBottom.TabIndex = 2;
		this.grbBottom.TabStop = false;
		this.btnSave.Location = new System.Drawing.Point(3, 12);
		this.btnSave.Name = "btnSave";
		this.btnSave.Size = new System.Drawing.Size(75, 23);
		this.btnSave.TabIndex = 0;
		this.btnSave.Text = "Save";
		this.btnSave.UseVisualStyleBackColor = true;
		this.btnSave.Click += new System.EventHandler(btnSave_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1024, 536);
		base.Controls.Add(this.grbBottom);
		base.Controls.Add(this.grbProfiles);
		base.Controls.Add(this.grbTop);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "fInformProfiles";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Update Profiles";
		base.Load += new System.EventHandler(fInformProfiles_Load);
		this.grbTop.ResumeLayout(false);
		this.grbTop.PerformLayout();
		this.grbProfiles.ResumeLayout(false);
		this.grbProfCl.ResumeLayout(false);
		this.grbProfCl.PerformLayout();
		this.grbProfLeft.ResumeLayout(false);
		this.grbClientFill.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.grdProf).EndInit();
		this.grbClientBottom.ResumeLayout(false);
		this.grbBottom.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
