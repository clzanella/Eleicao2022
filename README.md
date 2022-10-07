# Jobs para analizar os resultados públicos da eleicao

## O que é

Pequenos scapper para baixar os arquivos públicos do site do TSE e analiza-los

[Documentação com mais detalhes](Documentacao/README.md).


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
