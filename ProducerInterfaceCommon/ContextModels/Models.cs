namespace ProducerInterfaceCommon.ContextModels
{
	public partial class AccountRegion
	{
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