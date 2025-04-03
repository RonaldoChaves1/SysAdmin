## SysAdmin

![Badge de status](https://img.shields.io/badge/status-Em_desenvolvimento-green)

### Sobre o SysAdmin
O objetivo desse aplicativo é disponibilizar uma plataforma que seja possível administrar os pools de aplicativos do IIS e serviços específicos do windows, com a intenção de parar e iniciar a partir de comandos acionados por um usuário autenticado no AD.

A criação dessa aplicação se deu pela necessidade de que vários usuários de uma empresa pudessem gerenciar os pools de forma autônoma, sem a necessidade de possuir permissão de acesso diretamente no servidor. Além disso, trata-se também de um estudo relacionado a observabilidade, onde através é possível rastrear os logs da aplicação através do ELK (Elasticsearch, Logstash e Kibana).

Ainda existem melhorias para essa aplicação, mas na versão inicial, já é possível gerenciar os pools e utilizar da API para conectar gerenciar os pools através da IC.

### Como executar
Para executar a aplicação, é necessário fazer o download do código fonte, restaurar os pacotes do nuget e executar. É possível também utilizar do docker para realizar o processo de instalação de ferramentas para auxiliar na observabilidade.
