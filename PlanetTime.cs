public class PlanetTime
{
	public int Years { get; set; }

	public int Days { get; set; }

	public int Hours { get; set; }

	public int Minutes { get; set; }

	public int Seconds { get; set; }

	public PlanetTime(double universalTime, double secondsPerYear, double secondsPerDay)
	{
		var secondsInCurrentYear = universalTime % secondsPerYear;
		var secondsInCurrentDay = secondsInCurrentYear % secondsPerDay;
		var secondsInCurrentHour = secondsInCurrentDay % 3600;
		var secondsInCurrentMinute = secondsInCurrentHour % 60;


		Years = (int)(universalTime / secondsPerYear) + 1;

		Days = (int)(secondsInCurrentYear / secondsPerDay) + 1;

		Hours = (int)(secondsInCurrentDay / 3600);

		Minutes = (int)(secondsInCurrentHour / 60);

		Seconds = (int)secondsInCurrentMinute;
	}

	public string ToCompactSortableTimeStamp()
	{
		return $"Y{Years:D5}_D{Days:D3}_{Hours:D2}_{Minutes:D2}_{Seconds:D2}";
	}
}
