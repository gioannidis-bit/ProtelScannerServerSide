using System;

namespace ProtelScannerServerSide.Models;

public class ReservationModel
{
	public int Id { get; set; }

	public long kdnr { get; set; }

	public int fromKunden { get; set; }

	public string kat { get; set; }

	public string ziname { get; set; }

	public string string1 { get; set; }

	public DateTime globdvon { get; set; }

	public DateTime globdbis { get; set; }

	public string isMaster { get; set; }

	public string name1 { get; set; }

	public string vorname { get; set; }

	public string titel { get; set; }

	public DateTime gebdat { get; set; }

	public int gender { get; set; }

	public int nat { get; set; }

	public int landkz { get; set; }

	public string passnr { get; set; }

	public string issued { get; set; }

	public DateTime issuedate { get; set; }

	public DateTime docvalid { get; set; }

	public string land { get; set; }

	public int validFromBegl { get; set; }
}
