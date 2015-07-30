namespace ProducerInterface
{
	public class Config : AnalitFramefork.Config
	{
		public override string Test
		{
			get { return "Test 0"; }
		}
	}

	public class GlobalProperties2 : Config
	{
		public override string Test
		{
			get { return "Test 1"; }
		}
	}

	public class GlobalProperties3 : GlobalProperties2
	{
		public override string Test
		{
			get { return "Test 2"; }
		}
	}
}