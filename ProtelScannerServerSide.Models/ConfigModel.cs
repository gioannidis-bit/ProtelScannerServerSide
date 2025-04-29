using System.Collections.Generic;
using ProtelScannerServerSide.Enums;

namespace ProtelScannerServerSide.Models;

public class ConfigModel
{
	public string ProtelIni { get; set; }

	public string ConnectionString { get; set; }

	public string DbSchema { get; set; } = "proteluser";

	public CompanionTypeEnum CompanionType { get; set; }

	public List<UsersServerPortsAssoc> UsersPorts { get; set; }

	public List<GenderMappingModel> GenderMapping { get; set; }
}
