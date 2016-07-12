namespace ProducerInterfaceCommon.ContextModels
{
	public partial class AccountRegion
	{
		public AccountRegion()
		{
		}

		public AccountRegion(Account account, Region region)
		{
			Account = account;
			RegionId = region.Id;
		}

		public ulong RegionId
		{
			get { return (ulong)RegionCode; }
			set { RegionCode = (long)value; }
		}
	}

	public partial class ReportRegion
	{
		public ulong RegionId
		{
			get { return (ulong)RegionCode; }
			set { RegionCode = (long)value; }
		}
	}
}