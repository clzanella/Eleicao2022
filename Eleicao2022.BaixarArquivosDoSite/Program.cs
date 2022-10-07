using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

// json com estados e municípios
string urlArquivoPrincipal = "https://resultados.tse.jus.br/oficial/comum/config/ele-c.json";

string jsonArquivoPrincipal;
using (var webClient = new System.Net.WebClient()) {
    jsonArquivoPrincipal = webClient.DownloadString(urlArquivoPrincipal);
}

const string baseDir = "ArquivosPublicos";
if(!Directory.Exists(baseDir))
	Directory.CreateDirectory(baseDir);

File.WriteAllText(Path.Combine(baseDir, "ele-c.json"), jsonArquivoPrincipal);

dynamic principal = JObject.Parse(jsonArquivoPrincipal);
dynamic pleito01 = ((IEnumerable<dynamic>) principal.pl).FirstOrDefault(p => p.cd == "406");

dynamic eleicaoPresidente = ((IEnumerable<dynamic>) pleito01.e).FirstOrDefault(p => p.cd == "544");

// buscar lista de unidades federativas e municípios
// exemplo: https://resultados.tse.jus.br/oficial/ele2022/544/config/mun-e000544-cm.json
string urlArquivoUfsEMunicipios = $"https://resultados.tse.jus.br/oficial/ele2022/544/config/mun-e000{eleicaoPresidente.cd}-cm.json";
string jsonArquivoUfsEMunicipios;
using (var webClient = new System.Net.WebClient()) {
    jsonArquivoUfsEMunicipios = webClient.DownloadString(urlArquivoUfsEMunicipios);
}

File.WriteAllText(Path.Combine(baseDir, $"mun-e000{eleicaoPresidente.cd}-cm.json"), jsonArquivoUfsEMunicipios);

dynamic ufsEMunicipios = JObject.Parse(jsonArquivoUfsEMunicipios);

string[] ufs = new string[]
{
	"se"
};

int count = 0;

foreach(string uf in ufs){

	//dynamic maceio = ((IEnumerable<dynamic>) ((IEnumerable<dynamic>) ufsEMunicipios.abr).FirstOrDefault(uf => uf.cd == "AL").mu).FirstOrDefault(m => m.cd == "27855");
	// maceio.z // lista de zonas

	// lista de municipios, zonas e secoes
	// exemplo: https://resultados.tse.jus.br/oficial/ele2022/arquivo-urna/406/config/al/al-p000406-cs.json
	string urlMunicipiosZonasSecoes = $"https://resultados.tse.jus.br/oficial/ele2022/arquivo-urna/{pleito01.cd}/config/{uf}/{uf}-p000{pleito01.cd}-cs.json";

	string jsonMunicipiosZonasSecoes;
	using (var webClient = new System.Net.WebClient()) {
		jsonMunicipiosZonasSecoes = webClient.DownloadString(urlMunicipiosZonasSecoes);
	}

	File.WriteAllText(Path.Combine(baseDir, $"{uf}-p000{pleito01.cd}-cs.json"), jsonMunicipiosZonasSecoes);
	dynamic municipiosZonasSecoes = JObject.Parse(jsonMunicipiosZonasSecoes);

	if(!Directory.Exists(Path.Combine(baseDir,"urnas")))
		Directory.CreateDirectory(Path.Combine(baseDir,"urnas"));


	int munCount = 0;
	foreach(dynamic mun in municipiosZonasSecoes.abr[0].mu)
	{
		munCount++;

		dynamic zonas = mun.zon;//((IEnumerable<dynamic>) municipiosZonasSecoes.abr[0].mu).FirstOrDefault(m => m.cd == maceio.cd).zon;

		Console.WriteLine($"Baixando municipios {munCount} de {municipiosZonasSecoes.abr[0].mu.Count} - municipio {mun.cd} zonas {zonas.Count}");

		foreach(dynamic zona in zonas)
		{

			// iterar urnas
			foreach(dynamic secao in zona.sec)
			{
				count++;

				// exemplo: https://resultados.tse.jus.br/oficial/ele2022/arquivo-urna/406/dados/al/27855/0001/0001/p000406-al-m27855-z0001-s0001-aux.json
				string urlSecaoArquivoAux = $"https://resultados.tse.jus.br/oficial/ele2022/arquivo-urna/{pleito01.cd}/dados/{uf}/{mun.cd}/{zona.cd}/{secao.ns}/p000{pleito01.cd}-{uf}-m{mun.cd}-z{zona.cd}-s{secao.ns}-aux.json";
				string jsonSecaoArquivoAux;
				using (var webClient = new System.Net.WebClient()) {
					jsonSecaoArquivoAux = webClient.DownloadString(urlSecaoArquivoAux);
				}

				File.WriteAllText(Path.Combine(baseDir,"urnas", $"p000{pleito01.cd}-{uf}-m{mun.cd}-z{zona.cd}-s{secao.ns}-aux.json"), jsonSecaoArquivoAux);
				dynamic secaoArquivoAux = JObject.Parse(jsonSecaoArquivoAux);

				string hash = secaoArquivoAux.hashes[0].hash;
				string secaoArquivoBuConteudo = null;
				foreach(string nomeArquivo in secaoArquivoAux.hashes[0].nmarq)
				{

					// exemplo: https://resultados.tse.jus.br/oficial/ele2022/arquivo-urna/406/dados/al/27855/0001/0001/4e4c557a6d556e644f42777348696a497631317061714a3635345943387772653379565a6776394c706f493d/o00406-2785500010001.bu
					string urlSecaoArquivo = $"https://resultados.tse.jus.br/oficial/ele2022/arquivo-urna/{pleito01.cd}/dados/{uf}/{mun.cd}/{zona.cd}/{secao.ns}/{hash}/{nomeArquivo}";
					string secaoArquivoConteudo;
					using (var webClient = new System.Net.WebClient()) {
						secaoArquivoConteudo = webClient.DownloadString(urlSecaoArquivo);
					}
					
					if(nomeArquivo.EndsWith(".imgbu"))
					{
						secaoArquivoBuConteudo = secaoArquivoConteudo;
					}

					File.WriteAllText(Path.Combine(baseDir,"urnas",  nomeArquivo), secaoArquivoConteudo);

				}

				// extrair votação para presidente
			/*
			--------------PRESIDENTE--------------
			Nome do candidato       Num cand Votos

			  CIRO GOMES                  12  0012
			  LULA                        13  0110
			  SIMONE TEBET                15  0011
			  VERA                        16  0001
			  JAIR BOLSONARO              22  0143
			  SORAYA THRONICKE            44  0002

			--------------------------------------
			Eleitores Aptos                   0340
			Total de votos Nominais           0279
			Brancos                           0006
			Nulos                             0008
			Total Apurado                     0293
			*/

				if(secaoArquivoBuConteudo == null) 
					continue;

				var pres = new Pres();

				string[] lines = secaoArquivoBuConteudo.Split("\n");

				int index = Array.FindIndex(lines, line => line.Contains("----PRESIDENTE----"));
				index += 3;

				while(true){

					string line = lines[index++];

					if(line.Contains("-----") || line.Trim().Length == 0){
						break;
					}

					// trim spaces
					line = Regex.Replace(line, @"\s+", " ");

					string[] partes = line.Trim().Split(" ");

					pres.Votos[int.Parse(partes[partes.Length - 2])] = int.Parse(partes[partes.Length - 1]);

				}

				// outras estatisticas
				int countd = 0;
				while(countd < 5){

					string line = lines[index++];

					if(line.Contains("Eleitores Aptos")){
						countd++;
						pres.EleitoresAptos = int.Parse(line.Replace("Eleitores Aptos", "").Trim());
					}

					if(line.Contains("Total de votos Nominais")){
						countd++;
						pres.Nominais = int.Parse(line.Replace("Total de votos Nominais", "").Trim());
					}

					if(line.Contains("Brancos")){
						countd++;
						pres.Brancos = int.Parse(line.Replace("Brancos", "").Trim());
					}

					if(line.Contains("Nulos")){
						countd++;
						pres.Nulos = int.Parse(line.Replace("Nulos", "").Trim());
					}

					if(line.Contains("Total Apurado")){
						countd++;
						pres.TotalApuracao = int.Parse(line.Replace("Total Apurado", "").Trim());
					}

				}

				string votacaoPresidente = Newtonsoft.Json.JsonConvert.SerializeObject(pres);

				File.WriteAllText(Path.Combine(baseDir,"urnas", $"presid{pleito01.cd}-{uf}-m{mun.cd}-z{zona.cd}-s{secao.ns}.json"), votacaoPresidente);


			}

		}
		
	}

	
}


Console.WriteLine($"Bus baixados: {count}");
