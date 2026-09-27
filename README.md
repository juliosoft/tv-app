# TvApp - Player IPTV (WPF / .NET 10)

App desktop no estilo XCIPTV Player / IPTV Smarters: voce informa a URL do
servidor, usuario e senha (padrao **Xtream Codes**), e o app carrega os
grupos e canais exatamente como configurados no servidor, com suporte a
favoritos.

## Stack

- .NET 10 (`net10.0-windows`) + WPF, padrao MVVM (sem framework externo, so
  `ICommand`/`INotifyPropertyChanged` proprios em `ViewModels/`).
- **LibVLCSharp** + **LibVLCSharp.WPF** + **VideoLAN.LibVLC.Windows** para
  reproducao de video (engine baseada no VLC, com suporte amplo a
  codecs/containers usados em IPTV - TS, HLS, H.264/H.265 etc).
- API Xtream Codes (`player_api.php`) para autenticacao, categorias
  (`get_live_categories`) e canais (`get_live_streams`).

## Estrutura

```
TvApp/
  Models/        DTOs da API Xtream (Channel, ChannelCategory, LoginResult...)
  Services/      XtreamService (HTTP), AccountStore (credenciais/DPAPI),
                 FavoritesStore (favoritos em JSON por conta)
  ViewModels/    LoginViewModel, MainViewModel, RelayCommand, ViewModelBase
  Views/         LoginWindow, MainWindow
  Converters/    Conversores usados no XAML
  Resources/     Estilos/tema dark (AppStyles.xaml)
```

## Como abrir e rodar

Este projeto precisa ser compilado no **Windows** (WPF nao roda em Linux/macOS).

1. Abra `TvApp.sln` no Visual Studio 2026 (com o workload ".NET desktop
   development") **ou** rode via linha de comando na pasta do projeto:

   ```
   cd E:\dev\estudos\tv-app
   dotnet restore
   dotnet build
   dotnet run --project TvApp
   ```

2. Na tela de login, informe:
   - **URL do servidor**: ex. `http://meuservidor.com:8080` (com porta, se
     houver)
   - **Usuario** e **Senha** do painel Xtream Codes
   - Marque "Lembrar minhas credenciais" para nao digitar de novo (a senha e
     salva protegida com DPAPI do Windows, so pode ser lida pelo mesmo
     usuario/maquina)

3. Depois de conectar, a tela principal mostra:
   - **Grupos** (categorias) na coluna da esquerda, com "⭐ Favoritos" fixo
     no topo
   - **Canais** do grupo selecionado na coluna do meio (busca por nome no
     campo de pesquisa do topo)
   - Clique na estrela para favoritar/desfavoritar um canal
   - **Duplo clique** em um canal para reproduzir
   - Player a direita com Parar / Mudo / Volume / Tela cheia (tambem F11 ou
     Esc)

## Observacoes / proximos passos sugeridos

- **VOD e Series**: hoje o app cobre apenas **TV ao vivo** (Live), como
  pedido. Para adicionar filmes/series, basta criar servicos analogos
  usando as acoes `get_vod_categories`/`get_vod_streams` e
  `get_series_categories`/`get_series`, seguindo o mesmo padrao de
  `XtreamService`.
- **EPG**: nao incluido nesta primeira versao (a pedido). Se quiser depois,
  a API expoe `get_short_epg` e `xmltv.php`.
- **Versoes dos pacotes NuGet**: fixei `LibVLCSharp`/`LibVLCSharp.WPF` em
  `3.9.1` e `VideoLAN.LibVLC.Windows` em `3.0.21`. Se o `dotnet restore`
  reclamar que a versao nao existe, abra o NuGet Package Manager no Visual
  Studio e atualize esses 3 pacotes para a versao estavel mais recente (a
  API usada nao muda entre versoes recentes).
- Multiplas contas salvas: a estrutura em `AccountStore` ja guarda uma
  lista de contas; hoje a UI só usa a "ultima usada" para preencher o
  login automaticamente, mas seria simples adicionar um seletor de perfis
  salvos na tela de login.
