# CSharp
Customized C# Code, shared project
# TCP Socket Communication Project

## Overview
이 프로젝트는 TCP 소켓 통신을 관리하기 위한 서버와 클라이언트 매니저를 제공합니다. `cSocketServerManager`와 `cSocketClientManager` 클래스를 사용하여 네트워크 기반의 어플리케이션을 쉽게 구축할 수 있습니다.

## Features
- **cSocketServerManager**: TCP 서버를 관리하며 클라이언트 연결 수락 및 데이터 송수신을 담당합니다.
- **cSocketClientManager**: TCP 클라이언트를 관리하며 서버에 연결하고 데이터를 송수신합니다.
- **cSocketManagerBase**: 서버와 클라이언트 매니저에 공통적인 기능을 제공합니다.

## Installation
프로젝트는 .NET Framework 또는 .NET Core 환경에서 작동하도록 설계되었습니다. 이 코드를 사용하기 위해 특별한 설치 과정은 필요하지 않습니다. 기존의 .NET 프로젝트에 클래스 파일들을 추가하기만 하면 됩니다.

## Usage
### Server
```csharp
var serverManager = new cSocketServerManager("127.0.0.1", 8080);
serverManager.StartServer();
```

### Client
```csharp
var clientManager = new cSocketClientManager("127.0.0.1", 8080);
clientManager.StartClient();
```

### Contributing
프로젝트에 기여하고 싶으시다면, Pull Request나 Issue를 통해 기여할 수 있습니다.

### License
이 프로젝트는 MIT License 하에 배포됩니다.


### 주의
이 README.md는 ChatGPT를 통해 작성되었습니다.
