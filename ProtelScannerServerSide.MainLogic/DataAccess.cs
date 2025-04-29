using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using Dapper;
using log4net;
using ProtelScannerServerSide.Enums;
using ProtelScannerServerSide.Models;

namespace ProtelScannerServerSide.MainLogic;

public class DataAccess
{
	private ILog logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

	private string Connection;

	private string DbSchema = "proteluser";

	public DataAccess(string _Connection, string _DbSchema)
	{
		Connection = _Connection;
		DbSchema = _DbSchema;
	}

	private string SqlNewIdProcedureCode()
	{
		return " PROCEDURE " + DbSchema + ".hit_NewId @mpehotel INT,  @actionType INT, @kdnr INT OUTPUT AS      \r\n\t\t\t\t\t\tBEGIN      \r\n\t\t\t\t\t\t\tBEGIN TRY      \r\n\t\t\t\t\t\t\t\tBEGIN TRANSACTION      \r\n\t\t\t\t\t\t\t\t\tDECLARE @tmpId INT      \r\n\t\t\t\t\t\t\t\t\tSET @tmpId = -1       \r\n\t\t\t\t\t\t\t\t\tIF (@actionType = 1)    \r\n\t\t\t\t\t\t\t\t\tBEGIN    \r\n\t\t\t\t\t\t\t\t\t\tIF EXISTS (SELECT 1 FROM " + DbSchema + ".bnr_neu)   \r\n\t\t\t\t\t\t\t\t\t\tBEGIN   \r\n\t\t\t\t\t\t\t\t\t\t\tSELECT @tmpId = ISNULL(kdnr,0)+1 FROM " + DbSchema + ".bnr_neu    \r\n\t\t\t\t\t\t\t\t\t\t\tUPDATE " + DbSchema + ".bnr_neu SET kdnr = @tmpId    \r\n\t\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\t\tELSE   \r\n\t\t\t\t\t\t\t\t\t\tBEGIN   \r\n\t\t\t\t\t\t\t\t\t\t\tSET @tmpId = 1   \r\n\t\t\t\t\t\t\t\t\t\t\tINSERT INTO " + DbSchema + ".bnr_neu(mpehotel,kdnr) VALUES (1, @tmpId)   \r\n\t\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\tELSE    \r\n\t\t\t\t\t\t\t\t\tIF (@actionType = 2)    \r\n\t\t\t\t\t\t\t\t\tBEGIN    \r\n\t\t\t\t\t\t\t\t\t\tIF EXISTS (SELECT 1 FROM " + DbSchema + ".kundennr)   \r\n\t\t\t\t\t\t\t\t\t\tBEGIN   \r\n\t\t\t\t\t\t\t\t\t\t\tSELECT @tmpId = ISNULL(kdnr,0)+1 FROM " + DbSchema + ".kundennr    \r\n\t\t\t\t\t\t\t\t\t\t\tUPDATE " + DbSchema + ".kundennr SET kdnr = @tmpId    \r\n\t\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\t\tELSE   \r\n\t\t\t\t\t\t\t\t\t\tBEGIN   \r\n\t\t\t\t\t\t\t\t\t\t\tSET @tmpId = 1   \r\n\t\t\t\t\t\t\t\t\t\t\tINSERT INTO " + DbSchema + ".kundennr(mpehotel,kdnr) VALUES (1, @tmpId)   \r\n\t\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\tELSE    \r\n\t\t\t\t\t\t\t\t\tIF (@actionType = 3)    \r\n\t\t\t\t\t\t\t\t\tBEGIN    \r\n\t\t\t\t\t\t\t\t\t\tIF EXISTS (SELECT 1 FROM " + DbSchema + ".profcnt)   \r\n\t\t\t\t\t\t\t\t\t\tBEGIN   \r\n\t\t\t\t\t\t\t\t\t\t\tSELECT @tmpId = ISNULL(kdnr,0)+1 FROM " + DbSchema + ".profcnt    \r\n\t\t\t\t\t\t\t\t\t\t\tUPDATE " + DbSchema + ".profcnt SET kdnr = @tmpId    \r\n\t\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\t\tELSE   \r\n\t\t\t\t\t\t\t\t\t\tBEGIN   \r\n\t\t\t\t\t\t\t\t\t\t\tSET @tmpId = 1   \r\n\t\t\t\t\t\t\t\t\t\t\tINSERT INTO " + DbSchema + ".profcnt(mpehotel,kdnr) VALUES (1, @tmpId)   \r\n\t\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\tEND    \r\n\t\t\t\t\t\t\t\t ELSE   \r\n\t\t\t\t\t\t\t\t\tIF (@actionType = 4)    \r\n\t\t\t\t\t\t\t\t\tBEGIN    \r\n\t\t\t\t\t\t\t\t\t\tIF EXISTS (SELECT 1 FROM " + DbSchema + ".texthinr)   \r\n\t\t\t\t\t\t\t\t\t\tBEGIN   \r\n\t\t\t\t\t\t\t\t\t\t\tSELECT @tmpId = ISNULL(kdnr,0)+1 FROM " + DbSchema + ".texthinr    \r\n\t\t\t\t\t\t\t\t\t\t\tUPDATE " + DbSchema + ".texthinr SET kdnr = @tmpId   \r\n\t\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\t\tELSE   \r\n\t\t\t\t\t\t\t\t\t\tBEGIN   \r\n\t\t\t\t\t\t\t\t\t\t\tSET @tmpId = 1   \r\n\t\t\t\t\t\t\t\t\t\t\tINSERT INTO " + DbSchema + ".texthinr(mpehotel,kdnr) VALUES (1, @tmpId)   \r\n\t\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\tEND    \r\n\t\t\t\t\t\t\t\t ELSE   \r\n\t\t\t\t\t\t\t\t\tIF (@actionType = 5)    \r\n\t\t\t\t\t\t\t\t\tBEGIN    \r\n\t\t\t\t\t\t\t\t\t\tIF EXISTS (SELECT 1 FROM " + DbSchema + ".reslinr)   \r\n\t\t\t\t\t\t\t\t\t\tBEGIN   \r\n\t\t\t\t\t\t\t\t\t\t\tSELECT @tmpId = ISNULL(kdnr,0)+1 FROM " + DbSchema + ".reslinr    \r\n\t\t\t\t\t\t\t\t\t\t\tUPDATE " + DbSchema + ".reslinr SET kdnr = @tmpId    \r\n\t\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\t\tELSE   \r\n\t\t\t\t\t\t\t\t\t\tBEGIN   \r\n\t\t\t\t\t\t\t\t\t\t\tSET @tmpId = 1   \r\n\t\t\t\t\t\t\t\t\t\t\tINSERT INTO " + DbSchema + ".reslinr(mpehotel,kdnr) VALUES (1, @tmpId)   \r\n\t\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\tEND    \r\n\t\t\t\t\t\t\t\t\tELSE   \r\n\t\t\t\t\t\t\t\t\tIF (@actionType = 6)    \r\n\t\t\t\t\t\t\t\t\tBEGIN    \r\n\t\t\t\t\t\t\t\t\t\tIF EXISTS (SELECT 1 FROM " + DbSchema + ".xsetupref)   \r\n\t\t\t\t\t\t\t\t\t\tBEGIN   \r\n\t\t\t\t\t\t\t\t\t\t\tSELECT @tmpId = ISNULL(kdnr,0)+1 FROM " + DbSchema + ".xsetupref    \r\n\t\t\t\t\t\t\t\t\t\t\tUPDATE " + DbSchema + ".xsetupref SET kdnr = @tmpId    \r\n\t\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\t\tELSE   \r\n\t\t\t\t\t\t\t\t\t\tBEGIN   \r\n\t\t\t\t\t\t\t\t\t\t\tSET @tmpId = 1   \r\n\t\t\t\t\t\t\t\t\t\t\tINSERT INTO " + DbSchema + ".xsetupref(mpehotel,kdnr) VALUES (1, @tmpId)   \r\n\t\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\tELSE   \r\n\t\t\t\t\t\t\t\t\tIF (@actionType = 7)    \r\n\t\t\t\t\t\t\t\t\tBEGIN    \r\n\t\t\t\t\t\t\t\t\t\tIF EXISTS (SELECT 1 FROM " + DbSchema + ".messnr)   \r\n\t\t\t\t\t\t\t\t\t\tBEGIN   \r\n\t\t\t\t\t\t\t\t\t\t\tSELECT @tmpId = ISNULL(kdnr,0)+1 FROM " + DbSchema + ".messnr    \r\n\t\t\t\t\t\t\t\t\t\t\tUPDATE " + DbSchema + ".messnr SET kdnr = @tmpId    \r\n\t\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\t\tELSE   \r\n\t\t\t\t\t\t\t\t\t\tBEGIN   \r\n\t\t\t\t\t\t\t\t\t\t\tSET @tmpId = 1   \r\n\t\t\t\t\t\t\t\t\t\t\tINSERT INTO " + DbSchema + ".messnr(mpehotel,kdnr) VALUES (1, @tmpId)   \r\n\t\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\tEND  \r\n\t\t\t\t\t\t\t\t\tELSE   \r\n\t\t\t\t\t\t\t\t\tIF (@actionType = 8)    \r\n\t\t\t\t\t\t\t\t\tBEGIN   \r\n\t\t\t\t\t\t\t\t\t\tIF EXISTS (SELECT 1 FROM " + DbSchema + ".loycrdno)   \r\n\t\t\t\t\t\t\t\t\t\tBEGIN   \r\n\t\t\t\t\t\t\t\t\t\t\tSELECT @tmpId = ISNULL(kdnr,0)+1 FROM " + DbSchema + ".loycrdno    \r\n\t\t\t\t\t\t\t\t\t\t\tUPDATE " + DbSchema + ".loycrdno SET kdnr = @tmpId    \r\n\t\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\t\tELSE   \r\n\t\t\t\t\t\t\t\t\t\tBEGIN   \r\n\t\t\t\t\t\t\t\t\t\t\tSET @tmpId = 1   \r\n\t\t\t\t\t\t\t\t\t\t\tINSERT INTO " + DbSchema + ".loycrdno(mpehotel,kdnr) VALUES (1, @tmpId)   \r\n\t\t\t\t\t\t\t\t\t\tEND   \r\n\t\t\t\t\t\t\t\t\tEND   \r\n \r\n\t\t\t\t\t\t\t\t\tSET @kdnr = @tmpId      \r\n\t\t\t\t\t\t\t\tCOMMIT TRANSACTION      \r\n\t\t\t\t\t\t\tEND TRY      \r\n\t\t\t\t\t\t\tBEGIN CATCH      \r\n\t\t\t\t\t\t\t\tSET @kdnr = -1      \r\n\t\t\t\t\t\t\t\tROLLBACK TRANSACTION      \r\n\t\t\t\t\t\t\tEND CATCH      \r\n\t\t\t\t\t\tEND";
	}

	public void CheckSP_NewId()
	{
		try
		{
			string sql = "IF (OBJECT_ID('hit_NewId') IS NULL)\t\r\n\t\t\t\t\t\t\t\t\tSELECT 'CREATE '\r\n\t\t\t\t\t\t\t\tELSE\r\n\t\t\t\t\t\t\t\t\tSELECT 'ALTER '";
			using IDbConnection db = new SqlConnection(Connection);
			db.Open();
			sql = db.Query<string>(sql).FirstOrDefault() + SqlNewIdProcedureCode();
			db.Execute(sql);
			db.Close();
		}
		catch
		{
		}
	}

	public bool CheckConnection()
	{
		bool result = true;
		try
		{
			using IDbConnection db = new SqlConnection(Connection);
			db.Open();
			db.Close();
		}
		catch
		{
			result = false;
		}
		return result;
	}

	private string ReservationSql()
	{
		return "DECLARE @Paxes INT, @idx INT, @tRec INT\r\n\t\t\t\t\tSELECT @Paxes = b.anzerw+b.anzkin1+b.anzkin2+b.anzkin3+b.anzkin4+b.zbett+b.kbett \r\n\t\t\t\t\tFROM " + DbSchema + ".buch AS b\r\n\t\t\t\t\tWHERE b.leistacc = @resId AND ((b.globbnr < 1) OR (b.globbnr > 0 AND b.umzdurch = 1))\r\n\r\n\t\t\t\t\tDECLARE @result TABLE(Id INT IDENTITY(1,1), kdnr BIGINT, fromKunden INT, \r\n\t\t\t\t\t\tkat VARCHAR(20), ziname VARCHAR(20), string1 VARCHAR(50),\r\n\t\t\t\t\t\tglobdvon DATETIME, globdbis DATETIME, isMaster VARCHAR(50), \r\n\t\t\t\t\t\tname1 VARCHAR(80),vorname VARCHAR(50), titel VARCHAR(20), \r\n\t\t\t\t\t\tgebdat DATETIME, gender INT, nat INT, landkz INT,\r\n\t\t\t\t\t\tpassnr VARCHAR(30), issued VARCHAR(50), issuedate DATETIME , docvalid DATETIME, land VARCHAR(80), validFromBegl INT)\r\n\t\t\t\t\t\t\r\n\t\t\t\t\tINSERT INTO @result (kdnr, fromKunden, kat, ziname, string1, globdvon, globdbis,\r\n\t\t\t\t\t\tisMaster, name1, vorname, titel, gebdat, gender, nat, landkz, passnr,issued, issuedate, docvalid, land,validFromBegl)\r\n\t\t\t\t\tSELECT k.kdnr, 1 fromKunden,kt.kat, z.ziname, b.string1, b.globdvon, b.globdbis, \r\n\t\t\t\t\t\t'Master Profile' isMaster,\tk.name1, k.vorname, k.titel, k.gebdat, \r\n\t\t\t\t\t\tCASE WHEN k.gender < 1 THEN 0 ELSE k.gender END gender,\r\n\t\t\t\t\t\tCASE WHEN k.nat < 1 THEN 0 ELSE k.nat END nat,\r\n\t\t\t\t\t\tCASE WHEN k.landkz < 1 THEN 0 ELSE k.landkz END landkz,\r\n\t\t\t\t\t\tk.passnr, k.issued,\tk.issuedate , k.docvalid, k.land, -1 validFromBegl  \r\n\t\t\t\t\tFROM " + DbSchema + ".buch AS b\r\n\t\t\t\t\tINNER JOIN " + DbSchema + ".kunden AS k ON k.kdnr = b.kundennr\r\n\t\t\t\t\tINNER JOIN " + DbSchema + ".kat AS kt ON kt.katnr = b.katnr\r\n\t\t\t\t\tLEFT OUTER JOIN " + DbSchema + ".zimmer AS z ON z.zinr = b.zimmernr\r\n\t\t\t\t\tLEFT OUTER JOIN " + DbSchema + ".restyp AS r ON r.nr = b.gender\r\n\t\t\t\t\tLEFT OUTER JOIN " + DbSchema + ".natcode AS n ON n.codenr = k.nat\r\n\t\t\t\t\tLEFT OUTER JOIN " + DbSchema + ".natcode AS l ON l.codenr = k.landkz\r\n\t\t\t\t\tWHERE b.leistacc = @resId AND ((b.globbnr < 1) OR (b.globbnr > 0 AND b.umzdurch = 1))\r\n\r\n\t\t\t\t\tINSERT INTO @result (kdnr, fromKunden, kat, ziname, string1, globdvon, globdbis,\r\n\t\t\t\t\t\tisMaster, name1, vorname, titel, gebdat, gender, nat, landkz, passnr,issued, issuedate, docvalid,land,validFromBegl)\r\n\t\t\t\t\tSELECT k.kdnr, 1 fromKunden,kt.kat, z.ziname, b.string1, b.globdvon, b.globdbis, \r\n\t\t\t\t\t\t'Attached Profile' isMaster,\tk.name1, k.vorname, k.titel, k.gebdat, \r\n\t\t\t\t\t\tCASE WHEN k.gender < 1 THEN 0 ELSE k.gender END gender,\r\n\t\t\t\t\t\tCASE WHEN k.nat < 1 THEN 0 ELSE k.nat END nat,\r\n\t\t\t\t\t\tCASE WHEN k.landkz < 1 THEN 0 ELSE k.landkz END landkz,\r\n\t\t\t\t\t\tk.passnr, k.issued,\tk.issuedate , k.docvalid, k.land, -1 validFromBegl  \t\r\n\t\t\t\t\tFROM " + DbSchema + ".buch AS b\r\n\t\t\t\t\tINNER JOIN " + DbSchema + ".reslinkp AS rs ON rs.leistacc = b.leistacc\r\n\t\t\t\t\tINNER JOIN " + DbSchema + ".kunden AS k ON k.kdnr = rs.kundennr\r\n\t\t\t\t\tINNER JOIN " + DbSchema + ".kat AS kt ON kt.katnr = b.katnr\r\n\t\t\t\t\tLEFT OUTER JOIN " + DbSchema + ".zimmer AS z ON z.zinr = b.zimmernr\r\n\t\t\t\t\tLEFT OUTER JOIN " + DbSchema + ".restyp AS r ON r.nr = b.gender\r\n\t\t\t\t\tLEFT OUTER JOIN " + DbSchema + ".natcode AS n ON n.codenr = k.nat\r\n\t\t\t\t\tLEFT OUTER JOIN " + DbSchema + ".natcode AS l ON l.codenr = k.landkz\r\n\t\t\t\t\tWHERE b.leistacc = @resId AND ((b.globbnr < 1) OR (b.globbnr > 0 AND b.umzdurch = 1))\r\n\t\r\n\t\t\t\t\tINSERT INTO @result (kdnr, fromKunden, kat, ziname, string1, globdvon, globdbis,\r\n\t\t\t\t\t\tisMaster, name1, vorname, titel, gebdat, gender, nat, landkz, passnr,issued, issuedate, docvalid,land,validFromBegl)\r\n\t\t\t\t\tSELECT -1 kdnr, 0 fromKunden,kt.kat, z.ziname, b.string1, b.globdvon, b.globdbis, \r\n\t\t\t\t\t\t'Companion' isMaster, bg.name1, bg.vorname, bg.titel, bg.gebdat, \r\n\t\t\t\t\t\t0 gender,\r\n\t\t\t\t\t\tCASE WHEN bg.nat < 1 THEN 0 ELSE bg.nat END nat,\r\n\t\t\t\t\t\t0 landkz,\r\n\t\t\t\t\t\tbg.passnr, '' issued, '1900-01-01' issuedate, '1900-01-01'  docvalid, '' land, bgPosition validFromBegl\r\n\t\t\t\t\tFROM " + DbSchema + ".buch AS b\r\n\t\t\t\t\tINNER JOIN " + DbSchema + ".kat AS kt ON kt.katnr = b.katnr\r\n\t\t\t\t\tLEFT OUTER JOIN " + DbSchema + ".zimmer AS z ON z.zinr = b.zimmernr\r\n\t\t\t\t\tCROSS APPLY (\r\n\t\t\t\t\t\tSELECT bg.ehepart name1, bg.vorname0 vorname, bg.anrede0 titel, bg.pass0 passnr, bg.nat0 nat, bg.geb0 gebdat, 0 bgPosition\r\n\t\t\t\t\t\tFROM " + DbSchema + ".begl AS bg\r\n\t\t\t\t\t\tWHERE ISNULL(bg.ehepart,'') <> '' AND bg.leistacc = b.leistacc\r\n\t\t\t\t\t\tUNION ALL\r\n\t\t\t\t\t\tSELECT bg.kind1, bg.vorname1, bg.anrede1, bg.pass1, bg.nat1, bg.geb1, 1 bgPosition\r\n\t\t\t\t\t\tFROM " + DbSchema + ".begl AS bg\r\n\t\t\t\t\t\tWHERE ISNULL(bg.kind1,'') <> '' AND bg.leistacc = b.leistacc\r\n\t\t\t\t\t\tUNION ALL\r\n\t\t\t\t\t\tSELECT bg.kind2, bg.vorname2, bg.anrede2, bg.pass2, bg.nat2, bg.geb2, 2 bgPosition\r\n\t\t\t\t\t\tFROM " + DbSchema + ".begl AS bg\r\n\t\t\t\t\t\tWHERE ISNULL(bg.kind2,'') <> '' AND bg.leistacc = b.leistacc\r\n\t\t\t\t\t\tUNION ALL\r\n\t\t\t\t\t\tSELECT bg.kind3, bg.vorname3, bg.anrede3, bg.pass3, bg.nat3, bg.geb3, 3 bgPosition\r\n\t\t\t\t\t\tFROM " + DbSchema + ".begl AS bg\r\n\t\t\t\t\t\tWHERE ISNULL(bg.kind3,'') <> '' AND bg.leistacc = b.leistacc\r\n\t\t\t\t\t\tUNION ALL\r\n\t\t\t\t\t\tSELECT bg.kind4, bg.vorname4, bg.anrede4, bg.pass4, bg.nat4, bg.geb4, 4 bgPosition\r\n\t\t\t\t\t\tFROM " + DbSchema + ".begl AS bg\r\n\t\t\t\t\t\tWHERE ISNULL(bg.kind4,'') <> '' AND bg.leistacc = b.leistacc\r\n\t\t\t\t\t\tUNION ALL\r\n\t\t\t\t\t\tSELECT bg.kind5, bg.vorname5, bg.anrede5, bg.pass5, bg.nat5, bg.geb5, 5 bgPosition\r\n\t\t\t\t\t\tFROM " + DbSchema + ".begl AS bg\r\n\t\t\t\t\t\tWHERE ISNULL(bg.kind5,'') <> '' AND bg.leistacc = b.leistacc\r\n\t\t\t\t\t\tUNION ALL\r\n\t\t\t\t\t\tSELECT bg.kind6, bg.vorname6, bg.anrede6, bg.pass6, bg.nat6, bg.geb6, 6 bgPosition\r\n\t\t\t\t\t\tFROM " + DbSchema + ".begl AS bg\r\n\t\t\t\t\t\tWHERE ISNULL(bg.kind6,'') <> '' AND bg.leistacc = b.leistacc\r\n\t\t\t\t\t) bg\r\n\t\t\t\t\tWHERE b.leistacc = @resId AND ((b.globbnr < 1) OR (b.globbnr > 0 AND b.umzdurch = 1))\r\n\r\n\t\t\t\t\tSELECT @tRec = @Paxes - COUNT(*), @idx = 1 FROM @result AS r\r\n\r\n\t\t\t\t\tWHILE @idx <= @tRec\r\n\t\t\t\t\tBEGIN\r\n\t\t\t\t\t\tINSERT INTO @result (kdnr, fromKunden, kat, ziname, string1, globdvon, globdbis,\r\n\t\t\t\t\t\t\tisMaster, name1, vorname, titel, gebdat, gender, nat, landkz, passnr,issued, issuedate, docvalid, land,validFromBegl)\r\n\t\r\n\t\t\t\t\t\tSELECT -1 kdnr, 1 fromKunden,kt.kat, z.ziname, b.string1, b.globdvon, b.globdbis, \r\n\t\t\t\t\t\t    'Companion' isMaster, 'Companion '+CAST(@idx AS VARCHAR(10)) name1, '' vorname, '' titel, '1900-01-01' gebdat, \r\n\t\t\t\t\t\t    0 gender,0 nat,\t0 landkz,'' passnr, '' issued, '1900-01-01' issuedate, '1900-01-01'  docvalid , '' land,-1 validFromBegl\r\n\t\t\t\t\t\tFROM " + DbSchema + ".buch AS b\r\n\t\t\t\t\t\tINNER JOIN " + DbSchema + ".kat AS kt ON kt.katnr = b.katnr\r\n\t\t\t\t\t\tLEFT OUTER JOIN " + DbSchema + ".zimmer AS z ON z.zinr = b.zimmernr\r\n\t\t\t\t\t\tWHERE b.leistacc = @resId AND ((b.globbnr < 1) OR (b.globbnr > 0 AND b.umzdurch = 1))\r\n\r\n\t\t\t\t\t\tSET @idx = @idx + 1\r\n\t\t\t\t\tEND\r\n\t\t\t\t\tSELECT * FROM @result";
	}

	public List<ReservationModel> GetReservation(long leistacc)
	{
		string sql = ReservationSql();
		List<ReservationModel> result;
		using (IDbConnection db = new SqlConnection(Connection))
		{
			db.Open();
			result = db.Query<ReservationModel>(sql, new
			{
				resId = leistacc
			}).ToList();
			db.Close();
		}
		if (result == null)
		{
			result = new List<ReservationModel>();
		}
		return result;
	}

	public List<NationalitiesModel> GetNationalities()
	{
		List<NationalitiesModel> result;
		using (IDbConnection db = new SqlConnection(Connection))
		{
			db.Open();
			result = db.Query<NationalitiesModel>("SELECT '' abkuerz,'' land,0 codenr,'' isocode UNION ALL SELECT n.abkuerz, n.land, n.codenr, n.isocode FROM " + DbSchema + ".natcode AS n").ToList();
			db.Close();
		}
		if (result == null)
		{
			result = new List<NationalitiesModel>();
		}
		return result;
	}

	public List<GenderModel> GetGenders()
	{
		List<GenderModel> result;
		using (IDbConnection db = new SqlConnection(Connection))
		{
			db.Open();
			result = db.Query<GenderModel>("SELECT 0 nr, '' bezeich\r\n\t\t\t\t\t\t\t\t\t\t\t\tUNION ALL\r\n\t\t\t\t\t\t\t\t\t\t\t\tSELECT r.nr, r.bezeich \r\n\t\t\t\t\t\t\t\t\t\t\t\tFROM " + DbSchema + ".restyp AS r").ToList();
			db.Close();
		}
		if (result == null)
		{
			result = new List<GenderModel>();
		}
		return result;
	}

	private int ReturnProtelRef(int tableType)
	{
		int res = 0;
		try
		{
			using SqlConnection db = new SqlConnection(Connection);
			db.Open();
			DynamicParameters p = new DynamicParameters();
			p.Add("mpehotel", 1);
			p.Add("actionType", tableType);
			p.Add("kdnr", null, DbType.Int32, ParameterDirection.Output);
			res = db.Query<int>(DbSchema + ".hit_NewId", p, null, buffered: true, null, CommandType.StoredProcedure).FirstOrDefault();
			res = p.Get<int>("kdnr");
			db.Close();
			return res;
		}
		catch (Exception ex)
		{
			logger.Error(ex.ToString());
			return 0;
		}
	}

	public long SaveProfile(ReservationModel model, long leistacc, CompanionTypeEnum newCustomerAs, out string error)
	{
		long result = model.kdnr;
		string sql = "";
		error = "";
		try
		{
			if (model.kdnr < 1 && model.validFromBegl < 0)
			{
				result = ReturnProtelRef(2);
				if (result < 1)
				{
					return 0L;
				}
			}
			using SqlConnection db = new SqlConnection(Connection);
			db.Open();
			if (model.kdnr < 1 && model.validFromBegl < 0)
			{
				if (newCustomerAs == CompanionTypeEnum.AttachedProfile)
				{
					sql = "INSERT INTO " + DbSchema + ".kunden (kdnr) VALUES (@kdnr)";
					db.Execute(sql, new
					{
						kdnr = result
					});
					model.kdnr = result;
					int resLinkRef = ReturnProtelRef(5);
					if (resLinkRef < 1)
					{
						return 0L;
					}
					sql = "INSERT INTO " + DbSchema + ".reslinkp(leistacc,kundennr,datumvon,datumbis,buchstatus,cino,refnr,typ,string2,_del)\r\n\t\t\t\t\t\t\t\t\tVALUES(@leistacc,@kundennr,@datumvon,@datumbis,@buchstatus,@cino,@refnr,@typ,@string2,@_del)";
					db.Execute(sql, new
					{
						leistacc = leistacc,
						kundennr = model.kdnr,
						datumvon = new DateTime(1900, 1, 1),
						datumbis = new DateTime(1900, 1, 1),
						buchstatus = -1,
						cino = 0,
						refnr = resLinkRef,
						typ = 0,
						string2 = "",
						_del = 0
					});
				}
				else
				{
					sql = "IF EXISTS(SELECT 1 FROM " + DbSchema + ".begl AS b WHERE b.leistacc = @ResId)\r\n\t\t\t\t\t\t\t\t\tSELECT 1 nCnt\r\n\t\t\t\t\t\t\t\tELSE\r\n\t\t\t\t\t\t\t\t\tSELECT 0 nCnt";
					if (!db.Query<bool>(sql, new
					{
						ResId = leistacc
					}).FirstOrDefault())
					{
						string name1 = ((model.name1.Length <= 50) ? model.name1 : model.name1.Substring(0, 49));
						string vorname = ((model.vorname.Length <= 15) ? model.vorname : model.vorname.Substring(0, 14));
						sql = "INSERT INTO " + DbSchema + ".begl (leistacc, ehepart, vorname0, geb0) VALUES (@leistacc, @ehepart, @vorname0, @geb0)";
						db.Execute(sql, new
						{
							leistacc = leistacc,
							ehepart = name1,
							vorname0 = vorname,
							geb0 = model.gebdat
						});
						model.validFromBegl = 0;
					}
					else
					{
						sql = "SELECT CASE WHEN ISNULL(b.ehepart,'') = '' THEN 0\r\n\t\t\t\t\t\t\t\t\t\t\t\tWHEN ISNULL(b.kind1,'') = '' THEN 1\r\n\t\t\t\t\t\t\t\t\t\t\t\tWHEN ISNULL(b.kind2,'') = '' THEN 2\r\n\t\t\t\t\t\t\t\t\t\t\t\tWHEN ISNULL(b.kind3,'') = '' THEN 3\r\n\t\t\t\t\t\t\t\t\t\t\t\tWHEN ISNULL(b.kind4,'') = '' THEN 4\r\n\t\t\t\t\t\t\t\t\t\t\t\tWHEN ISNULL(b.kind5,'') = '' THEN 5\r\n\t\t\t\t\t\t\t\t\t\t\t\tWHEN ISNULL(b.kind6,'') = '' THEN 6 END position\r\n\t\t\t\t\t\t\t\t\tFROM " + DbSchema + ".begl AS b\r\n\t\t\t\t\t\t\t\t\tWHERE b.leistacc = @leistacc";
						int newPosition = db.Query<int>(sql, new { leistacc }).FirstOrDefault();
						string name1 = ((newPosition == 0) ? ((model.name1.Length <= 50) ? model.name1 : model.name1.Substring(0, 49)) : ((model.name1.Length <= 30) ? model.name1 : model.name1.Substring(0, 29)));
						string vorname = ((model.vorname.Length <= 15) ? model.vorname : model.vorname.Substring(0, 14));
						sql = "UPDATE " + DbSchema + ".begl SET " + ((newPosition == 0) ? "ehepart" : ("kind" + newPosition)) + " = @ehepart, vorname" + newPosition + " = @vorname, geb" + newPosition + "=@gebdat WHERE leistacc = @leistacc";
						db.Execute(sql, new
						{
							ehepart = name1,
							vorname = vorname,
							leistacc = leistacc,
							gebdat = model.gebdat
						});
						model.validFromBegl = newPosition;
					}
					result = leistacc;
				}
			}
			if (model.kdnr > 0)
			{
				sql = "UPDATE " + DbSchema + ".kunden SET name1 = @name1, vorname = @vorname, titel = @titel, gebdat = @gebdat, gender = @gender, \r\n\t\t\t\t\t\t\t\tnat = @nat, landkz = @landkz, land = @land, passnr = @passnr, issued = @issued, issuedate = @issuedate, docvalid = @docvalid \r\n\t\t\t\t\t\t\tWHERE kdnr = @kdnr";
				db.Execute(sql, new
				{
					name1 = ((model.name1.Length > 80) ? model.name1.Substring(0, 79) : model.name1),
					vorname = ((model.vorname.Length > 50) ? model.vorname.Substring(0, 49) : model.vorname),
					titel = ((model.titel.Length > 20) ? model.titel.Substring(0, 19) : model.titel),
					gebdat = model.gebdat,
					gender = model.gender,
					nat = model.nat,
					landkz = model.landkz,
					land = ((model.land.Length > 80) ? model.land.Substring(0, 79) : model.land),
					passnr = ((model.passnr.Length > 30) ? model.passnr.Substring(0, 29) : model.passnr),
					issued = ((model.issued.Length > 50) ? model.issued.Substring(0, 49) : model.issued),
					issuedate = model.issuedate,
					docvalid = model.docvalid,
					kdnr = model.kdnr
				});
			}
			else
			{
				result = leistacc;
				string lastName = ((model.validFromBegl == 0 && model.name1.Length > 50) ? model.name1.Substring(0, 49) : ((model.name1.Length <= 30) ? model.name1 : model.name1.Substring(0, 29)));
				sql = "UPDATE " + DbSchema + ".begl SET \\n" + ((model.validFromBegl == 0) ? "ehepart" : ("kind" + model.validFromBegl)) + " = @ehepart, \nvorname" + model.validFromBegl + " = @vorname, \ngeb" + model.validFromBegl + " = @geb, \nanrede" + model.validFromBegl + " = @titel, \npass" + model.validFromBegl + " = @pass, \nnat" + model.validFromBegl + " = @nat \nWHERE leistacc = @leistacc";
				db.Execute(sql, new
				{
					ehepart = lastName,
					vorname = ((model.vorname.Length > 15) ? model.vorname.Substring(0, 14) : model.vorname),
					geb = model.gebdat,
					titel = ((model.titel.Length > 40) ? model.titel.Substring(0, 30) : model.titel),
					pass = ((model.passnr.Length > 30) ? model.passnr.Substring(0, 29) : model.passnr),
					nat = model.nat,
					leistacc = leistacc
				});
			}
			db.Close();
		}
		catch (Exception ex)
		{
			error = ex.ToString();
			logger.Error(ex.ToString());
			result = 0L;
		}
		return result;
	}
}
