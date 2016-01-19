using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ProducerInterface.Models
{
	[MetadataType(typeof(drugmnnMetaData))]
	public partial class drugmnn
	{
	}

	public class drugmnnMetaData
	{
		[ScaffoldColumn(false)]
		public long? MnnId { get; set; }

		[Display(Name = "Международное непатентованное наименование")]
		[UIHint("TextBox")]
		[StringLength(255, ErrorMessage = "МНН должно быть не длиннее 255 символов")]
		[Required(ErrorMessage = "Не указано МНН")]
		public string Mnn { get; set; }

		[Display(Name = "Международное непатентованное наименование (рус.)")]
		[UIHint("TextBox")]
		[StringLength(255, ErrorMessage = "МНН (рус.) должно быть не длиннее 255 символов")]
    public string RussianMnn { get; set; }

	}
}