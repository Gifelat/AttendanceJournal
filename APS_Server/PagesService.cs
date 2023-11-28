namespace ASP_Server
{
	public class PagesService
	{
		public List<List<T>> Pages<T> (List<T> data, int countRow)
		{
			List<List<T>> pages = new List<List<T>>();

			int count = data.Count;
			int totalPages = (int)Math.Ceiling((double)count / countRow);

			for (int page = 1; page <= totalPages; page++)
			{
				int startIndex = (page - 1) * countRow;
				int endIndex = Math.Min(startIndex + countRow, count);

				List<T> currentPage = data.GetRange(startIndex, endIndex - startIndex);
				pages.Add(currentPage);
			}

			return pages;
		}
	}
}
