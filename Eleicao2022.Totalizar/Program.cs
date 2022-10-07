
using System.IO;
using System.Globalization;
using Newtonsoft.Json;
using System.Collections.Concurrent;

var files = Directory.GetFiles(
    @"Eleicao2022.BaixarArquivosDoSite\ArquivosPublicos\urnas",
    "presid*.json",
    SearchOption.AllDirectories);

var list = new ConcurrentBag<Pres>();

Parallel.ForEach(files, arqJson =>
{
    //if(!arqJson.Contains("al-m27855")){
    //    return; // só Maceió
    //}

    string jsonContent = File.ReadAllText(arqJson);
    var pres = JsonConvert.DeserializeObject<Pres>(jsonContent);
    list.Add(pres);
});

// gerar CSV com os votos e CSV com totais
var totais = new ConcurrentDictionary<int, int>();

foreach(var pres in list)
{

    foreach (var (partido, qtde) in pres.Votos.Select(x => (x.Key, x.Value)))
    {
        if(!totais.ContainsKey(partido)){
            totais[partido] = qtde;
        } else {
            totais[partido] += qtde;
        }
    }

}

var bolso = list.Where(p => (p.Votos.ContainsKey(13) ? p.Votos[13] : 0) < (p.Votos.ContainsKey(22) ? p.Votos[22] : 0));
var bolsonaroVenceCount = bolso.Count();
var bolsonaroVenceMax = bolso.Select( p => (p.Votos.ContainsKey(22) ? p.Votos[22] : 0) - (p.Votos.ContainsKey(13) ? p.Votos[13] : 0)).Max();
var bolsonaroVenceAvg = bolso.Select( p => (p.Votos.ContainsKey(22) ? p.Votos[22] : 0) - (p.Votos.ContainsKey(13) ? p.Votos[13] : 0)).Average();

var lula = list.Where(p => (p.Votos.ContainsKey(13) ? p.Votos[13] : 0) > (p.Votos.ContainsKey(22) ? p.Votos[22] : 0));
var lulaVenceCount = lula.Count();
var lulaVenceMax = lula.Select( p => (p.Votos.ContainsKey(13) ? p.Votos[13] : 0) - (p.Votos.ContainsKey(22) ? p.Votos[22] : 0)).Max();
var lulaVenceAvg = lula.Select( p => (p.Votos.ContainsKey(13) ? p.Votos[13] : 0) - (p.Votos.ContainsKey(22) ? p.Votos[22] : 0)).Average();

Console.WriteLine($"Bolsonaro venceu: {bolsonaroVenceCount}, avg: {bolsonaroVenceAvg}, max: {bolsonaroVenceMax}");
Console.WriteLine($"Lula venceu: {lulaVenceCount}, avg: {lulaVenceAvg}, max: {lulaVenceMax}");

string linha1 = "";
string linha2 = "";
string linha3 = "";

decimal totalVotosGeral = totais.Select(kv => kv.Value).Sum();

foreach (var (partido, qtde) in totais.Select(x => (x.Key, x.Value)))
{
    linha1 += partido + ",";
    linha2 += qtde + ",";
    linha3 += ((qtde/totalVotosGeral) * 100).ToString("#0.00", CultureInfo.InvariantCulture) + ",";
}

Console.WriteLine(linha1);
Console.WriteLine(linha2);
Console.WriteLine(linha3);