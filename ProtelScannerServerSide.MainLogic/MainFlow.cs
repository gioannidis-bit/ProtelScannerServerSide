using System.Collections.Generic;
using ProtelScannerServerSide.Enums;
using ProtelScannerServerSide.Models;

namespace ProtelScannerServerSide.MainLogic;

public class MainFlow
{
	private DataAccess dt;

	public MainFlow(string _Connection, string _DbSchema)
	{
		dt = new DataAccess(_Connection, _DbSchema);
	}

	public bool CheckConnection()
	{
		bool num = dt.CheckConnection();
		if (num)
		{
			dt.CheckSP_NewId();
		}
		return num;
	}

	public List<ReservationModel> GetReservation(long leistacc)
	{
		return dt.GetReservation(leistacc);
	}

	public List<NationalitiesModel> GetNationalities()
	{
		return dt.GetNationalities();
	}

	public List<GenderModel> GetGenders()
	{
		return dt.GetGenders();
	}

	public long SaveProfile(ReservationModel model, long leistacc, CompanionTypeEnum newCustomerAs, out string error)
	{
		return dt.SaveProfile(model, leistacc, newCustomerAs, out error);
	}
}
