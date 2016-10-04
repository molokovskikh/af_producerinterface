using System.Data.Entity.Validation;
using System.IO;
using log4net.ObjectRenderer;

namespace ProducerInterfaceCommon.Helpers
{
	public class ExceptionRenderer : IObjectRenderer
	{
		public void RenderObject(RendererMap rendererMap, object obj, TextWriter writer)
		{
			rendererMap.FindAndRender(obj, writer);
			var ex = obj as DbEntityValidationException;
			if (ex != null) {
				writer.WriteLine();
				foreach (var error in ex.EntityValidationErrors) {
					writer.WriteLine(error.Entry.Entity);
					foreach (var validationError in error.ValidationErrors) {
						writer.WriteLine($"{validationError.PropertyName} - {validationError.ErrorMessage}");
					}
				}
			}
		}
	}
}