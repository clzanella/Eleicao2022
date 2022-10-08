# Jobs para analizar os resultados públicos da eleicao

## O que é

Pequeno web scraper para baixar os arquivos json públicos do site do TSE e analiza-los.

[Documentação com maior detalhamento](Documentacao/README.md).


## Pré-requisitos

* DotNet 6.0
```
# instalar no Windows
choco install dotnet-6.0-sdk

# instalar no Linux deb
# https://learn.microsoft.com/en-us/dotnet/core/install/linux-debian
```

## Compilar

- Baixar arquivos.
```
dotnet run --project Eleicao2022.BaixarArquivosDoSite/Eleicao2022.BaixarArquivosDoSite.csproj
```

- Analizar arquivos
```
dotnet run --project Eleicao2022.Totalizar/Eleicao2022.Totalizar.csproj
```
