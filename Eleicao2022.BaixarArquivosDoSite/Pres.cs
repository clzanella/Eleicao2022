using System.Collections.Generic;
class Pres {
	public IDictionary<int, int> Votos {get; set;} = new Dictionary<int, int>();
	public int EleitoresAptos {get; set;}
	public int Nominais {get; set;}
	public int Brancos {get; set;}
	public int Nulos {get; set;}
	public int TotalApuracao {get; set;}
}