namespace AYI.Presentation.WebApp.Util;

public static class TaskHelper
{
	public static async Task<T> WithMinimumDelay<T>(TimeSpan minimumDelay, Func<Task<T>> taskFactory)
	{
		var userTask = taskFactory();

		await Task.Delay(minimumDelay);

		return await userTask;
	}
}
