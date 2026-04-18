namespace IngestorOpenSky.Models;

public class OpenSkyResponse
{
 public int Time { get; set; }

 public List<FlightState>? States { get; set; }
}   